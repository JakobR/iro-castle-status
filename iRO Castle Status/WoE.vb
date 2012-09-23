
Imports System.Text.RegularExpressions
Imports System.ComponentModel

Public Class WoE
    Implements INotifyPropertyChanged

    Public Enum Type
        WoE1 = 1
        WoE2 = 2
    End Enum

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

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

    Private ReadOnly Property WoEMessageRegex As Regex
        Get
            Static _regex As Regex

            If _regex Is Nothing Then

                Dim packet_start = ".\x00.." ' need to use the singleline options, so that "." matches all characters.
                Dim packet_end = "\x00"

                Dim realm_regex = "\[(?<realm>\w+?)\s?(Guild\s?|Realms\s?|)(?<number>\d)\]"
                Dim castlename_regex = "(?<castle>\w+?)"
                Dim guild_regex = "\[(?<guild>[^\x00\n\r]+?)\]"

                Dim woe1regex = String.Format("{2}(ssss)?The {0} castle has been conquered by the {1} guild\.{3}", realm_regex, guild_regex, packet_start, packet_end)
                Dim woe2regex = String.Format("{3}The {0} guild conquered the {1} (stronghold )?of {2}\.{4}", guild_regex, realm_regex, castlename_regex, packet_start, packet_end)
                Dim woe2regex2 = String.Format("{3}The {0} (stronghold )?of {1} is occupied by the {2} Guild\.{4}", realm_regex, castlename_regex, guild_regex, packet_start, packet_end)

                'Use combined regex
                _regex = New Regex(String.Format("({0}|{1}|{2})", woe1regex, woe2regex, woe2regex2), RegexOptions.ExplicitCapture And RegexOptions.Singleline And RegexOptions.Compiled)
            End If

            Return _regex
        End Get
    End Property

    Public Sub ProcessBreakMessage(Time As DateTime, Message As String)

        For Each match As Match In WoEMessageRegex.Matches(Message)

            Debug.Assert(match.Success)
            If Not match.Success Then
                Continue For
            End If

            Dim info = New With {
                                    .RealmName = match.Groups("realm").Value.TrimStart(),
                                    .CastleNumber = Integer.Parse(match.Groups("number").Value),
                                    .GuildName = match.Groups("guild").Value
                                }

            Dim realm = Aggregate r In Realms Where info.RealmName.StartsWith(r.Name) Into FirstOrDefault()

            If realm IsNot Nothing AndAlso info.CastleNumber >= 1 AndAlso info.CastleNumber <= realm.Castles.Count Then

                realm.GetCastleWithNumber(info.CastleNumber).AddBreak(Time, info.GuildName)


            End If

        Next

    End Sub

    Public ReadOnly Property AllCastleBreaks() As IEnumerable(Of Castle.Break)
        Get
            Return Realms.SelectMany(Function(r) r.Castles.SelectMany(Function(c) c.Breaks)).OrderBy(Function(b) b.Time)
        End Get
    End Property

    Private Sub Realm_BreakOccurred(sender As Object, e As Castle.BreakEventArgs)
        RaiseEvent BreakOccurred(Me, New Castle.BreakEventArgs() With {.Realm = e.Realm, .Castle = e.Castle, .NewOwningGuild = e.NewOwningGuild, .Time = e.Time})
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("AllCastleBreaks"))
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

        Yield Realm.Create("Balder", Type.WoE1,
                            Iterator Function()
                                Yield New Castle(1)
                                Yield New Castle(2, IsEnabled:=False)
                                Yield New Castle(3)
                                Yield New Castle(4)
                                Yield New Castle(5)
                            End Function())

        Yield Realm.Create("Britoniah", Type.WoE1,
                          Iterator Function()
                              Yield New Castle(1, IsEnabled:=False)
                              Yield New Castle(2)
                              Yield New Castle(3)
                              Yield New Castle(4)
                              Yield New Castle(5)
                          End Function())

        Yield Realm.Create("Luina", Type.WoE1,
                         Iterator Function()
                             Yield New Castle(1)
                             Yield New Castle(2)
                             Yield New Castle(3)
                             Yield New Castle(4, IsEnabled:=False)
                             Yield New Castle(5)
                         End Function())

        Yield Realm.Create("Valkyrie", Type.WoE1,
                            Iterator Function()
                                Yield New Castle(1, IsEnabled:=False)
                                Yield New Castle(2)
                                Yield New Castle(3)
                                Yield New Castle(4)
                                Yield New Castle(5)
                            End Function())

        Yield Realm.Create("Nithafjoll", Type.WoE2,
                            Iterator Function()
                                Yield New Castle(1)
                                Yield New Castle(2)
                                Yield New Castle(3)
                                Yield New Castle(4)
                                Yield New Castle(5)
                            End Function())

        Yield Realm.Create("Valfreyja", Type.WoE2,
                           Iterator Function()
                               Yield New Castle(1)
                               Yield New Castle(2)
                               Yield New Castle(3)
                               Yield New Castle(4)
                               Yield New Castle(5)
                           End Function())

    End Function


End Class
