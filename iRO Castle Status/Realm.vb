
Imports System.ComponentModel

Public Class Realm
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

    Public Event BreakOccurred As EventHandler(Of Castle.BreakEventArgs)

    ' Castle with number i+1 must be stored at index i
    Private _Castles As List(Of Castle)

    Public ReadOnly Property Castles As IEnumerable(Of Castle)
        Get
#If DEBUG Then
            For i = 0 To _Castles.Count - 1
                Debug.Assert(_Castles(i).Number = i + 1)
            Next

            For Each c In _Castles
                Debug.Assert(c.Realm Is Me)
            Next
#End If

            Return _Castles.AsReadOnly
        End Get
    End Property

    ' Requires valid castle number
    Public Function GetCastleWithNumber(CastleNumber As Integer) As Castle
        Debug.Assert(CastleNumber >= 1)
        Debug.Assert(CastleNumber <= Castles.Count)

        Dim c = Castles(CastleNumber - 1)

        Debug.Assert(c.Number = CastleNumber)

        Return c
    End Function

    Private _Name As String

    Public Property Name As String
        Get
            Return _Name
        End Get
        Set(value As String)
            _Name = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Name"))
        End Set
    End Property

    Private _Type As WoE.Type

    Public Property Type As WoE.Type
        Get
            Return _Type
        End Get
        Set(value As WoE.Type)
            _Type = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Type"))
        End Set
    End Property

    Private Sub New()
    End Sub

    Public Shared Function Create(Name As String, Type As WoE.Type, Castles As IEnumerable(Of Castle)) As Realm
        Dim r = New Realm

        r.Name = Name
        r.Type = Type

        ' Only iterate over the 'Castles' parameter once.
        ' For every iteration, the iterator function will be called (in case of WoE.iRO's method of creating the realms), which means that new Castle objects are created.
        r._Castles = New List(Of Castle)(Castles)

        For Each c In r._Castles
            c.Realm = r
        Next

        ' Can't add event handlers in constructor (they won't work)
        ' That's why Realm.Create has to be used instead of New Realm.
        For Each c In r._Castles
            AddHandler c.BreakOccurred, AddressOf r.Castle_BreakOccurred
        Next

        Return r
    End Function

    Public ReadOnly Property HasAtLeastOneBreak As Boolean
        Get
            For Each c In Castles
                If c.Breaks.Count > 0 Then
                    Return True
                End If
            Next

            Return False
        End Get
    End Property

    Private Sub Castle_BreakOccurred(sender As Object, e As Castle.BreakEventArgs)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("HasAtLeastOneBreak"))
        RaiseEvent BreakOccurred(Me, New Castle.BreakEventArgs() With {.Realm = Me, .Castle = e.Castle, .NewOwningGuild = e.NewOwningGuild, .Time = e.Time})
    End Sub

End Class
