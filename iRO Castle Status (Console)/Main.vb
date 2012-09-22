﻿
Imports System.IO
Imports SharpPcap
Imports iROCastleStatus

Module Main

    Public Sub Main()

        Console.WriteLine("iRO Castle Status {0}", Version.VersionString)
        Console.WriteLine("using SharpPcap {0}", SharpPcap.Version.VersionString)
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

        'WoE.iRO.ProcessBreakMessage(Now, "The [BalderGuild5] castle has been conquered by the [Valkyrie] guild.")
        'WoE.iRO.ProcessBreakMessage(Now, "The [Valkyrie Realms 5] castle has been conquered by the [Warrior Nation] guild.")
        'WoE.iRO.ProcessBreakMessage(Now, "The [Valkyrie] guild conquered the [Valfreyja 3] of Horn.")
        'WoE.iRO.ProcessBreakMessage(Now, "The [Warrior Nation] guild conquered the [Valfreyja 5] stronghold of Banadis.")
        'Console.ReadKey()

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

                    WoE.iRO.ProcessBreakMessage(time, text)

                End If 'Incoming global chat

            End If 'srcIp.BelongsToGravity

        End If

    End Sub

    Private Sub iRO_BreakOccurred(sender As Object, e As Castle.BreakEventArgs)
        'Console.WriteLine("[{0}:{1}] -- {5}{3} {4} -- {2}", e.Time.Hour, e.Time.Minute, e.NewOwningGuild, e.Realm.Name, e.Castle.Number, New String(" "c, If(e.Realm.Name.Length <= 10, 10 - e.Realm.Name.Length, 0)))

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
                Dim LogDirectoryName = String.Format("Log-{0}-{1}-{2}--{3}-{4}-{5}", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second)
                Dim LogDirectoryPath = Path.Combine("D:\iRO Castle Status\Packet Logs", LogDirectoryName)
                _Logger = New Logger(LogDirectoryPath, True)
            End If
            Return _Logger
#Else
            Return Nothing
#End If
        End Get
    End Property

End Module
