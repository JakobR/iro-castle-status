
Module Version

    Public ReadOnly Property VersionString() As String
        Get
            Dim assembly = System.Reflection.Assembly.GetExecutingAssembly()

            Return assembly.GetName.Version.ToString()
        End Get
    End Property

    Public ReadOnly Property FileVersionString() As String
        Get
            Dim assembly = System.Reflection.Assembly.GetExecutingAssembly()

            Dim versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location)

            Return versionInfo.FileVersion.ToString
        End Get
    End Property

End Module
