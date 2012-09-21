
Public Class Realm

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

    Public Property Name As String

    Public Sub New(Name As String, Castles As IEnumerable(Of Castle))
        _Castles = New List(Of Castle)(Castles)
        Me.Name = Name
    End Sub

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

End Class
