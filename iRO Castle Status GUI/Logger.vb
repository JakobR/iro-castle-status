
Imports System.IO

Public Class Logger

    Private _LogDirectoryPath As String

    Public Property LogDirectoryPath As String
        Get
            Return _LogDirectoryPath
        End Get
        Private Set(value As String)
            _LogDirectoryPath = value
        End Set
    End Property

    Public Property FailSilently As Boolean


    Public Sub New(LogDirectoryPath As String, Optional FailSilently As Boolean = False)

        Me.LogDirectoryPath = Path.GetFullPath(LogDirectoryPath)
        Me.FailSilently = FailSilently

    End Sub


    Public Sub LogTcpPacket(TcpPacket As PacketDotNet.TcpPacket, IpPacket As PacketDotNet.IpPacket, Time As Date, Optional FullPacketLength As Integer = Integer.MinValue, Optional LogCategory As String = Nothing)

        Dim DirectoryPath = LogDirectoryPath

        If LogCategory IsNot Nothing Then
            DirectoryPath = Path.Combine(DirectoryPath, LogCategory)
        End If

        If Not Directory.Exists(DirectoryPath) Then

            Try
                Directory.CreateDirectory(DirectoryPath)
            Catch ex As Exception
                If Not FailSilently Then
                    Throw New Exception("Could create log directory.", ex)
                Else
                    Return
                End If
            End Try

        End If

        Dim BaseFilename = String.Format("packet-{0}-{1}-{2}--{3}-{4}-{5}--", Time.Year, Time.Month, Time.Day, Time.Hour, Time.Minute, Time.Second)
        Dim BaseLogFilePath = Path.Combine(DirectoryPath, BaseFilename)
        Dim LogFilePath As String
        Dim i As Integer = 1

        ' Find an unused filename (so we don't overwrite files for packets which arrived in the same second, but earlier than this one)
        Do
            LogFilePath = BaseLogFilePath & i.ToString
            i += 1
        Loop While (File.Exists(LogFilePath & ".data") Or File.Exists(LogFilePath & ".txt"))


        Dim Payload = TcpPacket.PayloadData

        Try
            If Payload IsNot Nothing Then
                Dim LogFilePayload = File.Create(LogFilePath & ".data")
                LogFilePayload.Write(Payload, 0, Payload.Length)
                LogFilePayload.Close()
            End If

            Dim LogFilePacketInfo = File.Create(LogFilePath & ".txt")
            Dim writer = New IO.StreamWriter(LogFilePacketInfo)
            writer.WriteLine("Time:          {0}", Time)
            writer.WriteLine("Source:        {0}:{1}", IpPacket.SourceAddress, TcpPacket.SourcePort)
            writer.WriteLine("Destination:   {0}:{1}", IpPacket.DestinationAddress, TcpPacket.DestinationPort)
            writer.WriteLine("Packet length: {0}", If(FullPacketLength = Integer.MinValue, "n/a", FullPacketLength.ToString))
            writer.Close()

        Catch ex As Exception
            If Not FailSilently Then
                Throw New Exception("Could not write log file.", ex)
            Else
                Return
            End If
        End Try

    End Sub

End Class
