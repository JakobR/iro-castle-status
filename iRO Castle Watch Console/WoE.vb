
Public Class WoE

    Private _Realms As New List(Of Realm)

    Public ReadOnly Property Realms As IEnumerable(Of Realm)
        Get
            Return _Realms.AsReadOnly
        End Get
    End Property


    Private Sub New()

    End Sub

    Public Shared ReadOnly Property iRO As WoE
        Get
            Static _iRO As WoE
            If _iRO Is Nothing Then
                _iRO = New WoE

                _iRO._Realms.AddRange(iRO_CreateRealms)
            End If

            Return _iRO
        End Get
    End Property

    Private Shared Iterator Function iRO_CreateRealms() As IEnumerable(Of Realm)

        ' TODO: Read this stuff from an xml file or so...

        Yield New Realm("Balder",
                        Iterator Function() As IEnumerable(Of Castle)
                            Yield New Castle(1)
                            Yield New Castle(2, Enabled:=False)
                            Yield New Castle(3)
                            Yield New Castle(4)
                            Yield New Castle(5)
                        End Function())

        Yield New Realm("Britoniah",
                        Iterator Function() As IEnumerable(Of Castle)
                            Yield New Castle(1, Enabled:=False)
                            Yield New Castle(2)
                            Yield New Castle(3)
                            Yield New Castle(4)
                            Yield New Castle(5)
                        End Function())

        Yield New Realm("Luina",
                        Iterator Function() As IEnumerable(Of Castle)
                            Yield New Castle(1)
                            Yield New Castle(2)
                            Yield New Castle(3)
                            Yield New Castle(4, Enabled:=False)
                            Yield New Castle(5)
                        End Function())

        Yield New Realm("Valkyrie",
                        Iterator Function() As IEnumerable(Of Castle)
                            Yield New Castle(1, Enabled:=False)
                            Yield New Castle(2)
                            Yield New Castle(3)
                            Yield New Castle(4)
                            Yield New Castle(5)
                        End Function())

        Yield New Realm("Nithafjoll",
                        Iterator Function() As IEnumerable(Of Castle)
                            Yield New Castle(1)
                            Yield New Castle(2)
                            Yield New Castle(3)
                            Yield New Castle(4)
                            Yield New Castle(5)
                        End Function())

        Yield New Realm("Valfreyja",
                        Iterator Function() As IEnumerable(Of Castle)
                            Yield New Castle(1)
                            Yield New Castle(2)
                            Yield New Castle(3)
                            Yield New Castle(4)
                            Yield New Castle(5)
                        End Function())

    End Function


End Class
