
Imports System.IO
Imports SharpPcap
Imports iROCastleStatus
Imports System.Threading
Imports System.Windows.Threading
Imports System.Text.RegularExpressions

Module Main

    Private MainThread As Thread

    Public Sub Main(Args() As String)

        ' Parse command line options
        ' Available options:
        '   --console: Show the console (always active in #DEBUG mode)
        '   --nogui:   Don't show the WPF Window. Implies --console.

        Dim OptionShowConsole As Boolean = False
        Dim OptionNoGUI As Boolean = False

        For Each arg In Args
            If "--console".Equals(arg) Then
                OptionShowConsole = True
            ElseIf "--nogui".Equals(arg) Then
                OptionNoGUI = True
            Else
                MessageBox.Show("Invalid command line option. Valid options are ""--console"" and ""--nogui"". More information about this is not available.", "iRO Castle Status")
                Exit Sub
            End If
        Next

        If OptionNoGUI Then
            OptionShowConsole = True
        End If

#If DEBUG Then
        OptionShowConsole = True
#End If

        If OptionShowConsole Then
            ConsoleManager.Show()

            Console.WindowWidth = 200
            Console.WindowHeight = 30
            Console.BufferWidth = 200
            Console.BufferHeight = 1000
        End If

        MainThread = Thread.CurrentThread

        Console.WriteLine("iRO Castle Status")
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
            AddHandler device.OnPacketArrival, New PacketArrivalEventHandler(AddressOf device_OnPacketArrival)

            device.Open(DeviceMode.Promiscuous, 1000)
            device.Filter = "ip and tcp"

            device.StartCapture()
            CapturedDevices.Add(device)

            Console.WriteLine("-- Listening on {0}...", device.Description)
        Next

        Console.WriteLine()
        If PacketLogger IsNot Nothing Then
            Console.WriteLine("Packets will be logged to ""{0}"".", PacketLogger.LogDirectoryPath)
        End If

        If OptionNoGUI Then
            Debug.Assert(OptionShowConsole)
            Debug.Assert(ConsoleManager.HasConsole)

            Console.WriteLine("Ready. Press [Escape] to exit...")
            Console.WriteLine()

            Do Until (Console.ReadKey.Key = ConsoleKey.Escape)
            Loop
        Else
            Console.WriteLine("Ready.")
            Console.WriteLine()

            Dim w = New MainWindow
            w.ShowDialog()
        End If

        Console.WriteLine()
        Console.WriteLine("Closing devices...")

        For Each device In CapturedDevices
            Console.WriteLine("-- Closing {0}.", device.Description)
            device.StopCapture()
            device.Close()
        Next

    End Sub

    ' Careful: This will be called from a background thread.
    Private Sub device_OnPacketArrival(sender As Object, e As CaptureEventArgs)
        Dim device = DirectCast(sender, ICaptureDevice)

        Dim time = e.Packet.Timeval.Date
        Dim length = e.Packet.Data.Length

        Dim packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data)
        Dim tcpPacket = PacketDotNet.TcpPacket.GetEncapsulated(packet)

        If tcpPacket IsNot Nothing Then

            Dim ipPacket = DirectCast(tcpPacket.ParentPacket, PacketDotNet.IpPacket)

            Dim srcIp = ipPacket.SourceAddress
            Dim srcPort = tcpPacket.SourcePort

            Dim dstIp = ipPacket.DestinationAddress
            Dim dstPort = tcpPacket.DestinationPort

            Console.WriteLine("{0:00}:{1:00}:{2:00},{3:000} Len={4} {5}:{6} -> {7}:{8}  payloaddata={9} bytes   payloadpacket={10}",
                    time.Hour, time.Minute, time.Second, time.Millisecond, length,
                    srcIp, srcPort, dstIp, dstPort,
                    If(tcpPacket.PayloadData Is Nothing, "none", tcpPacket.PayloadData.Length.ToString), tcpPacket.PayloadPacket)

            If PacketLogger IsNot Nothing Then
                Dim category As String
                If srcIp.BelongsToGravity Then
                    category = "From Gravity"
                ElseIf dstIp.BelongsToGravity Then
                    category = "To Gravity"
                Else
                    category = "Other"
                End If

                PacketLogger.LogTcpPacket(tcpPacket, ipPacket, time, length, category)
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
                Dim d As Dispatcher = Dispatcher.FromThread(MainThread)

                ' Invoke this in main thread!
                ' (Otherwise the binding to the Breaks collection might throw an exception)
                d.BeginInvoke(New Action(Of DateTime, String)(AddressOf castle.AddBreak), Time, info.GuildName)

            End If

        Next

    End Sub

    Private Sub iRO_BreakOccurred(sender As Object, e As Castle.BreakEventArgs)
        Console.WriteLine()
        Console.WriteLine()
        Console.WriteLine()
        Console.WriteLine()
        Console.WriteLine()
        Console.WriteLine("Castle status as of {0:00}:{1:00}:", e.Time.Hour, e.Time.Minute)
        Console.WriteLine()

        For Each r In WoE.iRO.Realms
            If r.HasAtLeastOneBreak Then
                For Each c In r.Castles
                    Console.WriteLine("{1,10} {2} -- {0}", c.OwningGuild, r.Name, c.Number)
                Next
                Console.WriteLine()
            End If
        Next
    End Sub


    Private ReadOnly Property PacketLogger As Logger
        Get
#If DEBUG Then
            Static _Logger As Logger
            If _Logger Is Nothing Then
                Dim now = DateTime.Now
                Dim LogDirectoryName = String.Format("Log-{0:0000}-{1:00}-{2:00}--{3:00}-{4:00}-{5:00}", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second)
                Dim LogDirectoryPath = Path.Combine("D:\iRO Castle Status", LogDirectoryName)
                _Logger = New Logger(LogDirectoryPath, True)
            End If
            Return _Logger
#Else
                Return Nothing
#End If
        End Get
    End Property

End Module
