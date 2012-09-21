
Imports System.Text.RegularExpressions

Public Class WoE

    Private _Realms As New List(Of Realm)

    Public ReadOnly Property Realms As IEnumerable(Of Realm)
        Get
            Return _Realms.AsReadOnly
        End Get
    End Property

    Public Sub ProcessBreakMessage(Time As DateTime, Message As String)

        Dim woe1regex = "\AThe \[(?<realm>.+)(?<number>\d)\] castle has been conquered by the \[(?<guild>.+)\] guild.\z"
        Dim woe2regex = "\AThe \[(?<guild>.+)\] guild conquered the \[(?<realm>.+)(?<number>\d)\] (stronghold )?of (?<castle>\w+)\.\z"

        'Use combined regex
        Dim regex = New Regex(String.Format("({0}|{1})", woe1regex, woe2regex))

        Dim match = regex.Match(Message)

        If match.Success Then

            Dim info = New With {
                                    .RealmName = match.Groups("realm").Value.TrimStart(),
                                    .CastleNumber = Integer.Parse(match.Groups("number").Value),
                                    .GuildName = match.Groups("guild").Value
                                }

            For Each Realm In Realms
                If info.RealmName.StartsWith(Realm.Name) AndAlso
                   info.CastleNumber >= 1 AndAlso
                   info.CastleNumber <= Realm.Castles.Count Then

                    Realm.GetCastleWithNumber(info.CastleNumber).AddBreak(Time, info.GuildName)

                End If
            Next

        End If

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
