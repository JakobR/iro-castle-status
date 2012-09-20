
Imports SharpPcap

Module Main

    Sub Main()

        Console.WriteLine("iRO Castle Watch {0}", Version.VersionString)
        Console.WriteLine("using SharpPcap {0}", SharpPcap.Version.VersionString)
        Console.WriteLine()

        'Retrieve device list
        Dim devices = CaptureDeviceList.Instance

        If devices.Count < 1 Then
            Console.WriteLine("No network devices found on this machine.")
            Return
        End If

        Console.WriteLine("The following devices are available on this machine:")
        Console.WriteLine("----------------------------------------------------")
        Console.WriteLine()

        Dim i As Integer = 1

        For Each device In devices
            Console.WriteLine("{0}. {1}{3}   {2}", i, device.Name, device.Description, Environment.NewLine)
            i += 1
        Next

        Console.WriteLine()




        Console.ReadKey()

    End Sub

End Module
