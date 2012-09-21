
Imports System.IO
Imports System.Text.RegularExpressions
Imports SharpPcap

Module Main

    Public Sub Main()

        Console.WriteLine("iRO Castle Watch {0}", Version.VersionString)
        Console.WriteLine("using SharpPcap {0}", SharpPcap.Version.VersionString)
        Console.WriteLine()

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
        Console.WriteLine("This application will capture packets from all devices.")

        ' Start listening to all devices
        Dim CapturedDevices As New List(Of ICaptureDevice)
        For Each device In devices
            AddHandler device.OnPacketArrival, New PacketArrivalEventHandler(AddressOf device_OnPacketArrival)

            device.Open(DeviceMode.Promiscuous, 1000)

            device.Filter = "ip and tcp"

            Console.WriteLine("-- Listening on {0}...", device.Description)

            device.StartCapture()
            CapturedDevices.Add(device)
        Next

        Console.WriteLine()
        If PacketLogger IsNot Nothing Then
            Console.WriteLine("Packets will be logged to ""{0}"".", PacketLogger.LogDirectoryPath)
        End If
        Console.WriteLine("Ready. Press [Escape] to exit...")
        Console.WriteLine()

        Do
            Dim key = Console.ReadKey

            If key.Key = ConsoleKey.Escape Then
                Exit Do
            End If
        Loop

        Console.WriteLine()
        Console.WriteLine("Closing devices...")

        For Each device In CapturedDevices
            Console.WriteLine("-- Closing {0}.", device.Description)
            device.StopCapture()
            device.Close()
        Next

    End Sub

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

            Console.WriteLine("{0}:{1}:{2},{3} Len={4} {5}:{6} -> {7}:{8}  payloaddata={9} bytes   payloadpacket={10}",
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

                'Incoming global chat
                If payload IsNot Nothing AndAlso payload.Length >= 5 AndAlso payload(0) = &H8E Then

                    'Chat data starts at the fourth byte, and is zero-terminated (so chop off one byte at the end).
                    Dim text = System.Text.Encoding.ASCII.GetString(payload, 4, payload.Length - 5)

                    Console.WriteLine("Incoming global chat: ""{0}""", text)


                    'Check for castle break
                    Dim regex As New Regex("\AThe \[(?<realm>.*)(?<number>\d)\] castle has been conquered by the \[(?<guild>.+)\] guild.\z")

                    Dim match = regex.Match(text)

                    Dim RealmName = match.Groups("realm").Value.TrimStart()
                    Dim CastleNumber = 0
                    Integer.TryParse(match.Groups("number").Value, CastleNumber)
                    Dim GuildName = match.Groups("guild").Value

                    For Each Realm In WoE.iRO.Realms

                        If RealmName.StartsWith(Realm.Name) Then

                            If CastleNumber >= 1 And CastleNumber <= Realm.Castles.Count Then

                                Dim Castle = Realm.GetCastleWithNumber(CastleNumber)

                                Castle.AddBreak(time, GuildName)

                                Console.WriteLine("[{0} {1}] broken by [{2}]!", Realm.Name, Castle.Number, Castle.OwningGuild)

                            End If

                        End If

                    Next

                End If 'Incoming global chat

            End If 'srcIp.BelongsToGravity

        End If

    End Sub

    Private ReadOnly Property PacketLogger As Logger
        Get
#If DEBUG Then
            Static _Logger As Logger
            If _Logger Is Nothing Then
                Dim now = DateTime.Now
                Dim LogDirectoryName = String.Format("Log-{0}-{1}-{2}--{3}-{4}-{5}", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second)
                Dim LogDirectoryPath = Path.Combine("D:\iRO Castle Watch\Packet Logs", LogDirectoryName)
                _Logger = New Logger(LogDirectoryPath, True)
            End If
            Return _Logger
#Else
            Return Nothing
#End If
        End Get
    End Property

    ''' <summary>
    ''' Checks if the IP address is in Gravity's Network, which seems to be 128.241.92.0/23.
    ''' </summary>
    ''' <param name="IP">The IP address to check.</param>
    <System.Runtime.CompilerServices.Extension()>
    Private Function BelongsToGravity(IP As Net.IPAddress) As Boolean

        Debug.Assert(IP IsNot Nothing)

        Dim b = IP.GetAddressBytes

        Return b(0) = 128 AndAlso b(1) = 241 AndAlso (b(3) = 92 OrElse b(3) = 93)

    End Function

End Module
