
Imports System.IO
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
        Console.WriteLine("----------------------------------------------------")
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
        If LogPath IsNot Nothing Then
            Console.WriteLine("Packets will be logged to ""{0}"".", LogPath)
        End If
        Console.WriteLine("Ready. Press [Escape] to exit...")

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


            Dim payload = tcpPacket.PayloadData

            If payload IsNot Nothing AndAlso LogPath IsNot Nothing Then

                If Not Directory.Exists(LogPath) Then

                    Try
                        Directory.CreateDirectory(LogPath)
                    Catch ex As Exception
                        Console.WriteLine("Could not create log directory: {0}", ex.Message)
                    End Try

                End If

                Dim BaseLogFilePath = Path.Combine(LogPath, String.Format("packet-{0}-{1}-{2}--{3}-{4}-{5}--", time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second))
                Dim LogFilePath As String
                Dim i As Integer = 1

                Do
                    LogFilePath = BaseLogFilePath & i.ToString
                    i += 1
                Loop While File.Exists(LogFilePath)

                Try
                    Dim logfile = File.Create(LogFilePath)
                    logfile.Write(payload, 0, payload.Length)
                    logfile.Close()

                    Dim logfile2 = File.Create(LogFilePath & ".txt")
                    Dim writer = New IO.StreamWriter(logfile2)
                    writer.WriteLine("Time:          {0}", time)
                    writer.WriteLine("Source:        {0}:{1}", srcIp, srcPort)
                    writer.WriteLine("Destination:   {0}:{1}", dstIp, dstPort)
                    writer.WriteLine("Packet length: {0}", length)
                    writer.Close()
                Catch ex As Exception
                    Console.WriteLine("Could not write log file: {0}", ex.Message)
                End Try

            End If

            'Incoming global chat
            If payload IsNot Nothing AndAlso payload.Length >= 5 AndAlso payload(0) = &H8E Then

                'Chat data starts at the fourth byte, and is zero-terminated.

                Console.WriteLine("Incoming global chat: ""{0}""", System.Text.Encoding.ASCII.GetString(payload, 4, payload.Length - 5))

            End If

            'Dim ms As New MemoryStream(payload, False)
            'Dim reader As New IO.StreamReader(ms)
            'Console.WriteLine("{0}:{1}:{2},{3} Len={4} {5}:{6} -> {7}:{8}   payloaddata={9}   payloadpacket={10}{11}{12}",
            '        time.Hour, time.Minute, time.Second, time.Millisecond, length,
            '        srcIp, srcPort, dstIp, dstPort,
            '        tcpPacket.PayloadData, tcpPacket.PayloadPacket, Environment.NewLine,
            '        reader.ReadToEnd())


        End If

    End Sub

    Private ReadOnly Property LogPath As String
        Get
#If DEBUG Then
            Static _LogPath As String
            If _LogPath Is Nothing Then
                Dim now = DateTime.Now
                _LogPath = Path.Combine("D:\iRO Castle Watch\Packet Logs", String.Format("Log-{0}-{1}-{2}--{3}-{4}-{5}", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second))
            End If
            Return _LogPath
#Else
            Return Nothing
#End If
        End Get
    End Property

End Module
