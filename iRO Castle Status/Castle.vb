
Public Class Castle

    Public Event BreakOccurred As EventHandler(Of BreakEventArgs)

    Private _Number As Integer

    Public Property Number As Integer
        Get
            Return _Number
        End Get
        Private Set(value As Integer)
            _Number = value
        End Set
    End Property

    Public Property Enabled As Boolean

    Private _Breaks As New SortedSet(Of Break)(New Break.Comparer)

    Public ReadOnly Property Breaks As IEnumerable(Of Break)
        Get
            Return _Breaks
        End Get
    End Property

    Public ReadOnly Property OwningGuild As String
        Get
            If Breaks.Count = 0 Then
                Return Nothing
            End If

            Return Breaks.Last.BreakingGuild
        End Get
    End Property

    Public Sub New(Number As Integer, Optional Enabled As Boolean = True)
        Me.Number = Number
        Me.Enabled = Enabled
    End Sub

    Public Sub AddBreak(Time As DateTime, BreakingGuild As String)
        _Breaks.Add(New Break(Time, BreakingGuild))
        RaiseEvent BreakOccurred(Me, New BreakEventArgs With {.Castle = Me, .Time = Time, .NewOwningGuild = BreakingGuild})
    End Sub



    Public Class Break

        Private _Time As DateTime

        Public ReadOnly Property Time As DateTime
            Get
                Return _Time
            End Get
        End Property

        Private _BreakingGuild As String

        Public ReadOnly Property BreakingGuild As String
            Get
                Return _BreakingGuild
            End Get
        End Property

        Public Sub New(Time As DateTime, BreakingGuild As String)
            _Time = Time
            _BreakingGuild = BreakingGuild
        End Sub

        Public Class Comparer
            Implements IComparer(Of Break)

            Public Function Compare(x As Break, y As Break) As Integer Implements IComparer(Of Break).Compare
                Return x.Time.CompareTo(y.Time)
            End Function
        End Class

    End Class

    Public Class BreakEventArgs
        Inherits EventArgs

        Public Property Castle As Castle
        Public Property Realm As Realm
        Public Property NewOwningGuild As String
        Public Property Time As DateTime

    End Class

End Class
