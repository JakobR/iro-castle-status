
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

    Private Shared Function iRO_CreateRealms() As IEnumerable(Of Realm)

        ' TODO: Read this stuff from an xml file or so...

        Return {
                Realm.Create("Balder", Type.WoE1,
                                {
                                    New Castle(1),
                                    New Castle(2, IsEnabled:=False),
                                    New Castle(3),
                                    New Castle(4),
                                    New Castle(5)
                                }),
                Realm.Create("Britoniah", Type.WoE1,
                                {
                                    New Castle(1, IsEnabled:=False),
                                    New Castle(2),
                                    New Castle(3),
                                    New Castle(4),
                                    New Castle(5)
                                }),
                Realm.Create("Luina", Type.WoE1,
                                {
                                    New Castle(1),
                                    New Castle(2),
                                    New Castle(3),
                                    New Castle(4, IsEnabled:=False),
                                    New Castle(5)
                                }),
                Realm.Create("Valkyrie", Type.WoE1,
                                {
                                    New Castle(1, IsEnabled:=False),
                                    New Castle(2),
                                    New Castle(3),
                                    New Castle(4),
                                    New Castle(5)
                                }),
                Realm.Create("Nithafjoll", Type.WoE2,
                                {
                                    New Castle(1),
                                    New Castle(2),
                                    New Castle(3),
                                    New Castle(4),
                                    New Castle(5)
                                }),
                Realm.Create("Valfreyja", Type.WoE2,
                                {
                                    New Castle(1),
                                    New Castle(2),
                                    New Castle(3),
                                    New Castle(4),
                                    New Castle(5)
                                })
        }

    End Function


End Class
