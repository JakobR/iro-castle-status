
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

    Public ReadOnly Property Enabled As Boolean
        Get
            Return True
        End Get
    End Property

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

    Private Sub New(Name As String, Castles As IEnumerable(Of Castle))
        Me.Name = Name
        _Castles = New List(Of Castle)(Castles)
    End Sub

    Public Shared Function Create(Name As String, Castles As IEnumerable(Of Castle)) As Realm
        Dim r = New Realm(Name, Castles)

        For Each c In r.Castles
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
        RaiseEvent BreakOccurred(Me, New Castle.BreakEventArgs() With {.Realm = Me, .Castle = e.Castle, .NewOwningGuild = e.NewOwningGuild, .Time = e.Time})
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("HasAtLeastOneBreak"))
    End Sub

End Class
