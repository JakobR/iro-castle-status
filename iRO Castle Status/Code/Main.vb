
Imports System.IO
Imports SharpPcap
Imports iROCastleStatus
Imports System.Collections.Concurrent
Imports System.Threading
Imports System.Windows.Threading
Imports System.Text.RegularExpressions

Module Main

#Const DEBUG_ENABLE_LOGGING = False
#Const DEBUG_VERBOSE = False

    Private OptionShowConsole As Boolean = False
    Private OptionStatistics As Boolean = False
    Private OptionVerbose As Boolean = False
    Private OptionNoGUI As Boolean = False

    Private MainThread As Thread

    Public ReadOnly Property GUIDispatcher As Dispatcher
        Get
            Return Dispatcher.FromThread(MainThread)
        End Get
    End Property

    <STAThread()>
    Public Sub Main(Args() As String)

        ' Parse command line options
        ' Available options:
        '   --console: Show the console (always active in #DEBUG mode)
        '   --statistics: Show device statistics (always active in #DEBUG mode)
        '   --verbose: Show verbose packet information.
        '   --nogui:   Don't show the WPF Window. Implies --console.


        For Each arg In Args
            If "--console".Equals(arg) Then
                OptionShowConsole = True
            ElseIf "--statistics".Equals(arg) Then
                OptionStatistics = True
            ElseIf "--verbose".Equals(arg) Then
                OptionVerbose = True
            ElseIf "--nogui".Equals(arg) Then
                OptionNoGUI = True
            Else
                MessageBox.Show("Invalid command line switch. Valid switches are ""--console"", ""--statistics"", ""--verbose"", and ""--nogui"". More information about this is not available.", "iRO Castle Status")
                Exit Sub
            End If
        Next

        If OptionNoGUI Then
            OptionShowConsole = True
        End If

#If DEBUG Then
        OptionShowConsole = True
        OptionStatistics = True
#If DEBUG_VERBOSE Then
        OptionVerbose = True
#End If
#End If

        If OptionShowConsole Then
            ConsoleManager.Show()

            Dim w = Math.Min(200, Console.LargestWindowWidth)
            Dim h = Math.Min(30, Console.LargestWindowHeight)

            Console.SetWindowPosition(0, 0)
            Console.SetWindowSize(w, h)
            Console.SetBufferSize(w, 5000)
        End If

        MainThread = Thread.CurrentThread
        Debug.Assert(MainThread.GetApartmentState() = ApartmentState.STA)
        'MessageBox.Show(String.Format("Thread Apartment: {0}", MainThread.GetApartmentState))

        Console.WriteLine("iRO Castle Status {0}", Version)
        Console.WriteLine("using SharpPcap {0}", SharpPcap.Version.VersionString)
        If Args.Length > 0 Then
            Console.WriteLine("Command line arguments: " & String.Join(" ", Args))
        End If
        Console.WriteLine()

        AddHandler WoE.iRO.BreakOccurred, AddressOf iRO_BreakOccurred

        ' Retrieve device list
        Dim devices = CaptureDeviceList.Instance

        If devices.Count < 1 Then
            Console.WriteLine("No network devices found on this machine.")
            Return
        End If

        Console.WriteLine("The following devices are available on this machine:")
        Console.WriteLine()

        ' Enumerate devices and display info
        Dim i As Integer = 1
        For Each device In devices
            Console.WriteLine("{0}. {1}{3}   {2}", i, device.Name, device.Description, Environment.NewLine)
            i += 1
        Next

        Console.WriteLine()
        Console.WriteLine("This application will capture TCP/IP packets from all devices.")

        ' Start listening to all devices
        Dim CapturedDevices As New List(Of ICaptureDevice)
        For Each device In devices
            LastStatisticsOutput.Add(device, DateTime.Now)

            AddHandler device.OnPacketArrival, New PacketArrivalEventHandler(AddressOf device_OnPacketArrival)

            device.Open(DeviceMode.Promiscuous, 1000)
            device.Filter = "ip and tcp"

            device.StartCapture()
            CapturedDevices.Add(device)

            Console.WriteLine("-- Listening on {0}...", device.Description)
        Next

        Console.WriteLine()
        If PacketLogger IsNot Nothing Then
            Console.WriteLine("Packet info and content will be logged to ""{0}"".", PacketLogger.LogDirectoryPath)
        Else
            Console.WriteLine("Packet info and content will not be logged.")
        End If

        Console.WriteLine()
        If OptionVerbose Then
            Console.WriteLine("Packet info will be shown verbosely in the console window.")
        Else
            Console.WriteLine("Packet info in the console window:")
            Console.WriteLine("  'f' -- Packet from Gravity's servers (these will be interpreted for WoE break messages)")
            Console.WriteLine("  't' -- Packet from this computer to Gravity's servers")
            Console.WriteLine("  'o' -- Other packets, uninteresting for this application (Teamspeak, etc.)")
        End If

        ' Start background thread to process packets
        Task.Factory.StartNew(AddressOf ProcessPackets)

        Console.WriteLine()
        If OptionNoGUI Then
            Debug.Assert(OptionShowConsole)
            Debug.Assert(ConsoleManager.HasConsole)

            Console.WriteLine("Ready. Press [Escape] to exit...{0}", Environment.NewLine)

            Do Until (Console.ReadKey.Key = ConsoleKey.Escape)
            Loop
        Else
            Console.WriteLine("Ready.{0}", Environment.NewLine)

            Dim w = New MainWindow
            w.ShowDialog()
        End If

        Console.WriteLine("{0}Closing devices...", Environment.NewLine)

        For Each device In CapturedDevices
            Console.WriteLine("{2}-- Closing {0}.{2}   * Statistics: {1}", device.Description, device.Statistics, Environment.NewLine)
            device.StopCapture()
            device.Close()
        Next

        ' Complete packet queue
        ' This *must* be done after stopping all capturing (adding to the collection when completed will throw an exception)
        ' If we don't complete it, the loop in ProcessPackets won't stop -- which actually doesn't matter much in this case, since the application terminates when Sub Main is done.
        PacketQueue.CompleteAdding()

#If DEBUG Then
        System.Threading.Thread.Sleep(250)

        ' Flush input buffer
        Do While Console.KeyAvailable
            Console.ReadKey()
        Loop

        Console.WriteLine("{0}Press any key to exit...", Environment.NewLine)
        Console.ReadKey()
#End If

    End Sub


    Private LastStatisticsOutput As New Dictionary(Of ICaptureDevice, DateTime)
    Private StatisticsOutputInterval As New TimeSpan(0, 0, 30)

    Private PacketQueue As New BlockingCollection(Of RawCapture)

    ' Careful: This will be called from a (various?) background thread(s).
    Private Sub device_OnPacketArrival(sender As Object, e As CaptureEventArgs)

        Debug.Assert(PacketQueue IsNot Nothing)

        If OptionStatistics Then
            Debug.Assert(LastStatisticsOutput.ContainsKey(e.Device))

            Dim now = DateTime.Now
            Dim interval = now - LastStatisticsOutput(e.Device)
            If interval >= StatisticsOutputInterval Then
                Console.WriteLine("{0}Statistics for {1}:{0}  {2}", Environment.NewLine, e.Device.Name, e.Device.Statistics)
                LastStatisticsOutput(e.Device) = now
            End If
        End If

        PacketQueue.Add(e.Packet)

    End Sub

    ' Call this in its own background thread.
    Private Sub ProcessPackets()

        For Each RawPacket In PacketQueue.GetConsumingEnumerable()

            ProcessPacket(RawPacket)

        Next

    End Sub

    Private Sub ProcessPacket(RawPacket As RawCapture)

        Dim time = RawPacket.Timeval.Date
        Dim length = RawPacket.Data.Length

        Dim packet = PacketDotNet.Packet.ParsePacket(RawPacket.LinkLayerType, RawPacket.Data)
        Dim tcpPacket = PacketDotNet.TcpPacket.GetEncapsulated(packet)

        If tcpPacket IsNot Nothing Then

            Dim ipPacket = DirectCast(tcpPacket.ParentPacket, PacketDotNet.IpPacket)

            Dim srcIp = ipPacket.SourceAddress
            Dim srcPort = tcpPacket.SourcePort

            Dim dstIp = ipPacket.DestinationAddress
            Dim dstPort = tcpPacket.DestinationPort

            If OptionVerbose Then
                Console.WriteLine("{0:00}:{1:00}:{2:00},{3:000} Len={4} {5}:{6} -> {7}:{8}  payloaddata={9} bytes   payloadpacket={10}",
                        time.Hour, time.Minute, time.Second, time.Millisecond, length,
                        srcIp, srcPort, dstIp, dstPort,
                        If(tcpPacket.PayloadData Is Nothing, "none", tcpPacket.PayloadData.Length.ToString), tcpPacket.PayloadPacket)
            Else
                If srcIp.BelongsToGravity Then
                    Console.Write("f"c)
                ElseIf dstIp.BelongsToGravity Then
                    Console.Write("t"c)
                Else
                    Console.Write("o"c)
                End If
            End If

            If PacketLogger IsNot Nothing Then
                Dim category As String = Nothing

                If srcIp.BelongsToGravity Then
                    category = "From Gravity"
                ElseIf dstIp.BelongsToGravity Then
                    category = "To Gravity"
                End If

                If category IsNot Nothing Then
                    PacketLogger.LogTcpPacket(tcpPacket, ipPacket, time, length, category)
                End If
            End If


            'Only process packets from the iRO servers
            If srcIp.BelongsToGravity Then
                Dim payload = tcpPacket.PayloadData

                If payload IsNot Nothing Then
                    ' very cheap processing. just "convert" to ASCII and the regex will do the rest...
                    Dim text = System.Text.Encoding.ASCII.GetString(payload, 0, payload.Length)
                    ProcessBreakMessage(time, text)
                End If

            End If 'srcIp.BelongsToGravity

        End If

    End Sub

    Private ReadOnly Property WoEMessageRegex As Regex
        Get
            Static _regex As Regex

            If _regex Is Nothing Then
                ' need to use the singleline options, so that "." matches all characters.
                ' (the &H9A byte seems to be lost in the 'cheap' conversion to ASCII, i.e. &H9A is converted to '?' -- probably another encoding would help but it's not really a problem in this case)
                Dim packet_start = ".\x00.."
                Dim packet_end = "\x00"

                Dim realm_regex = "\[(?<realm>\w+?)\s?(Guild\s?|Realms\s?|)(?<number>\d)\]"
                Dim castlename_regex = "(?<castle>\w+?)"
                Dim guild_regex = "\[(?<guild>[^\x00\n\r]+?)\]"

                Dim woe1regex = String.Format("{2}(ssss)?The {0} castle has been conquered by the {1} guild\.{3}", realm_regex, guild_regex, packet_start, packet_end)
                Dim woe2regex = String.Format("{3}The {0} guild conquered the {1} (stronghold )?of {2}\.{4}", guild_regex, realm_regex, castlename_regex, packet_start, packet_end)
                Dim woe2regex2 = String.Format("{3}The {0} (stronghold )?of {1} is occupied by the {2} Guild\.{4}", realm_regex, castlename_regex, guild_regex, packet_start, packet_end)

                'Use combined regex
                _regex = New Regex(String.Format("({0}|{1}|{2})", woe1regex, woe2regex, woe2regex2), RegexOptions.ExplicitCapture And RegexOptions.Singleline And RegexOptions.Compiled)
            End If

            Return _regex
        End Get
    End Property

    Public Sub ProcessBreakMessage(Time As DateTime, Message As String)

        For Each match As Match In WoEMessageRegex.Matches(Message)

            Debug.Assert(match.Success)
            If Not match.Success Then
                Continue For
            End If

            Dim info = New With {
                                    .RealmName = match.Groups("realm").Value.TrimStart(),
                                    .CastleNumber = Integer.Parse(match.Groups("number").Value),
                                    .GuildName = match.Groups("guild").Value
                                }

            Dim realm = Aggregate r In WoE.iRO.Realms Where info.RealmName.StartsWith(r.Name) Into FirstOrDefault()

            If realm IsNot Nothing AndAlso info.CastleNumber >= 1 AndAlso info.CastleNumber <= realm.Castles.Count Then

                Dim castle = realm.GetCastleWithNumber(info.CastleNumber)

                ' Invoke this in main thread!
                ' (Otherwise the binding to the Breaks collection might throw an exception)
                GUIDispatcher.BeginInvoke(New Action(Of DateTime, String)(AddressOf castle.AddBreak), Time, info.GuildName)

            End If

        Next

    End Sub

    Private Sub iRO_BreakOccurred(sender As Object, e As Castle.BreakEventArgs)
        Dim output = New System.Text.StringBuilder()
        output.AppendFormat("{2}{2}{2}{2}{2}Castle status as of {0:00}:{1:00}:{2}", e.Time.Hour, e.Time.Minute, Environment.NewLine)

        For Each r In WoE.iRO.Realms
            If r.HasAtLeastOneBreak Then
                For Each c In r.Castles
                    output.AppendFormat("{1,10} {2} -- {0}{3}", c.OwningGuild, r.Name, c.Number, Environment.NewLine)
                Next
                output.AppendLine()
            End If
        Next

        Console.WriteLine(output.ToString)
    End Sub


    Private ReadOnly Property PacketLogger As Logger
        Get
            Static _Logger As Logger
#If DEBUG Then
#If DEBUG_ENABLE_LOGGING Then
            If _Logger Is Nothing Then
                Dim now = DateTime.Now
                Dim LogDirectoryName = String.Format("Log-{0:0000}-{1:00}-{2:00}--{3:00}-{4:00}-{5:00}", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second)
                Dim LogDirectoryPath = Path.Combine("D:\iRO Castle Status", LogDirectoryName)
                _Logger = New Logger(LogDirectoryPath, True)
            End If
#End If
#End If
            Return _Logger
        End Get
    End Property

    Public ReadOnly Property Version As String
        Get
            Try
                Dim assembly = System.Reflection.Assembly.GetExecutingAssembly()
                Dim versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location)

                Return String.Format("{0}", versionInfo.ProductVersion)
            Catch ex As Exception
                Debug.Fail("Error while reading assemby version info!", "This exception was thrown:" & Environment.NewLine & ex.ToString)
                Return "[version information unavailable]"
            End Try
        End Get
    End Property

End Module
