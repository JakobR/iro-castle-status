﻿
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

            Console.WriteLine("{0}:{1}:{2},{3} Len={4} {5}:{6} -> {7}:{8}   payloaddata={9}   payloadpacket={10}",
                    time.Hour, time.Minute, time.Second, time.Millisecond, length,
                    srcIp, srcPort, dstIp, dstPort,
                    tcpPacket.PayloadData, tcpPacket.PayloadPacket)

        End If

    End Sub

End Module
