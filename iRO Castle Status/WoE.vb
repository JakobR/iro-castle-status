
Imports System.Text.RegularExpressions

Public Class WoE

    Public Event BreakOccurred As EventHandler(Of Castle.BreakEventArgs)

    Private _Realms As List(Of Realm)

    Public ReadOnly Property Realms As IEnumerable(Of Realm)
        Get
            Return _Realms.AsReadOnly
        End Get
    End Property

    Private Sub New(Realms As IEnumerable(Of Realm))
        _Realms = New List(Of Realm)(Realms)
    End Sub

    Public Shared Function Create(Realms As IEnumerable(Of Realm)) As WoE
        Dim w = New WoE(Realms)

        For Each r In w.Realms
            AddHandler r.BreakOccurred, AddressOf w.Realm_BreakOccurred
        Next

        Return w
    End Function

    Public Sub ProcessBreakMessage(Time As DateTime, Message As String)

        'Use combined regex
        Dim regex = New Regex(String.Format("({0}|{1})", My.Resources.WoE1Regex, My.Resources.WoE2Regex))

        Dim match = regex.Match(Message)

        If Not match.Success Then
            Exit Sub
        End If

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
                Exit Sub

            End If
        Next

    End Sub

    Private Sub Realm_BreakOccurred(sender As Object, e As Castle.BreakEventArgs)
        RaiseEvent BreakOccurred(Me, New Castle.BreakEventArgs() With {.Realm = e.Realm, .Castle = e.Castle, .NewOwningGuild = e.NewOwningGuild, .Time = e.Time})
    End Sub


    Public Shared ReadOnly Property iRO As WoE
        Get
            Static _iRO As WoE

            If _iRO Is Nothing Then
                _iRO = WoE.Create(iRO_CreateRealms)
            End If

            Return _iRO
        End Get
    End Property

    Private Shared Iterator Function iRO_CreateRealms() As IEnumerable(Of Realm)

        ' TODO: Read this stuff from an xml file or so...

        Yield Realm.Create("Balder",
                            Iterator Function() As IEnumerable(Of Castle)
                                Yield New Castle(1)
                                Yield New Castle(2, Enabled:=False)
                                Yield New Castle(3)
                                Yield New Castle(4)
                                Yield New Castle(5)
                            End Function())

        Yield Realm.Create("Britoniah",
                          Iterator Function() As IEnumerable(Of Castle)
                              Yield New Castle(1, Enabled:=False)
                              Yield New Castle(2)
                              Yield New Castle(3)
                              Yield New Castle(4)
                              Yield New Castle(5)
                          End Function())

        Yield Realm.Create("Luina",
                         Iterator Function() As IEnumerable(Of Castle)
                             Yield New Castle(1)
                             Yield New Castle(2)
                             Yield New Castle(3)
                             Yield New Castle(4, Enabled:=False)
                             Yield New Castle(5)
                         End Function())

        Yield Realm.Create("Valkyrie",
                            Iterator Function() As IEnumerable(Of Castle)
                                Yield New Castle(1, Enabled:=False)
                                Yield New Castle(2)
                                Yield New Castle(3)
                                Yield New Castle(4)
                                Yield New Castle(5)
                            End Function())

        Yield Realm.Create("Nithafjoll",
                            Iterator Function() As IEnumerable(Of Castle)
                                Yield New Castle(1)
                                Yield New Castle(2)
                                Yield New Castle(3)
                                Yield New Castle(4)
                                Yield New Castle(5)
                            End Function())

        Yield Realm.Create("Valfreyja",
                           Iterator Function() As IEnumerable(Of Castle)
                               Yield New Castle(1)
                               Yield New Castle(2)
                               Yield New Castle(3)
                               Yield New Castle(4)
                               Yield New Castle(5)
                           End Function())

    End Function


End Class
