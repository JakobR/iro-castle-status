
Imports System.ComponentModel

Public Class Castle
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

    Public Event BreakOccurred As EventHandler(Of BreakEventArgs)

    Private _Number As Integer

    Public Property Number As Integer
        Get
            Return _Number
        End Get
        Private Set(value As Integer)
            _Number = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Number"))
        End Set
    End Property

    Private _IsEnabled As Boolean

    Public Property IsEnabled As Boolean
        Get
            Return _IsEnabled
        End Get
        Set(value As Boolean)
            _IsEnabled = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsEnabled"))
        End Set
    End Property

    Private _Realm As Realm

    Public Property Realm As Realm
        Get
            Return _Realm
        End Get
        Friend Set(value As Realm)
            _Realm = value
        End Set
    End Property

    Private _Breaks As New SortedSet(Of Break)(New Break.Comparer)

    Public ReadOnly Property Breaks As IEnumerable(Of Break)
        Get
#If DEBUG Then
            For Each b In _Breaks
                Debug.Assert(b.Castle Is Me)
            Next
#End If
            Return _Breaks
        End Get
    End Property

    Private _OwningGuild As String

    Public Property OwningGuild As String
        Get
            If _OwningGuild IsNot Nothing Then
                Return _OwningGuild
            End If

            If Breaks.Count = 0 Then
                Return Nothing
            End If

            Return Breaks.Last.BreakingGuild
        End Get
        Set(value As String)
            _OwningGuild = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("OwningGuild"))
        End Set
    End Property

    Public Sub New(Number As Integer, Optional IsEnabled As Boolean = True)
        Me.Number = Number
        Me.IsEnabled = IsEnabled
    End Sub

    ' Only call this method from the UI thread!
    ' (MainWindow is binding to the Breaks and OwningGuild properties).
    ' (Actually, all updates to the WoE, Realm, and Castle classes should be done in the UI thread.)
    Public Sub AddBreak(Time As DateTime, BreakingGuild As String)

        ' Only add the break if there wasn't already another one less than five seconds ago
        If Breaks.Count > 0 Then
            Dim LastBreak = Breaks.Last
            Dim diff = Time - LastBreak.Time

            If Math.Abs(diff.TotalSeconds) <= 5 Then
                Debug.Print("Drop break by {0} at {1:00}:{2:00} in {3} {4}.", BreakingGuild, Time.Hour, Time.Minute, Realm.Name, Number)
                Exit Sub
            End If
        End If

        _OwningGuild = Nothing
        _Breaks.Add(New Break(Me, Time, BreakingGuild))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Breaks"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("OwningGuild"))
        RaiseEvent BreakOccurred(Me, New BreakEventArgs With {.Castle = Me, .Time = Time, .NewOwningGuild = BreakingGuild})
    End Sub



    Public Class Break

        Private _Castle As Castle

        Public ReadOnly Property Castle As Castle
            Get
                Return _Castle
            End Get
        End Property

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

        Public Sub New(Castle As Castle, Time As DateTime, BreakingGuild As String)
            _Castle = Castle
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
