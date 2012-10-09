
Imports iROCastleStatus.Main
Imports System.ComponentModel

#Const DEBUG_VERBOSE = False
#Const DEBUG_NOIPCHECK = False

Public Class CommandLineOptions
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private Sub New()
    End Sub

    Public Shared Function Parse(Args() As String) As CommandLineOptions

        Dim o = New CommandLineOptions

        ' Available options:
        '   --console:    Show the console (always active in #DEBUG mode)
        '   --statistics: Show device statistics (always active in #DEBUG mode)
        '   --verbose:    Show verbose packet information.
        '   --nogui:      Don't show the WPF Window. Implies --console.
        '   --noipcheck:  Reads all packets, not just those from Gravity's servers. (might be needed if you use lowerping)

        For Each arg In Args
            If "--console".Equals(arg) Then
                o.ShowConsole = True
            ElseIf "--statistics".Equals(arg) Then
                o.Statistics = True
            ElseIf "--verbose".Equals(arg) Then
                o.Verbose = True
            ElseIf "--nogui".Equals(arg) Then
                o.NoGUI = True
            ElseIf "--noipcheck".Equals(arg) Then
                o.NoIPCheck = True
            Else
                MessageBox.Show("Invalid command line switch. Valid switches are ""--console"", ""--statistics"", ""--verbose"", ""--nogui"", and ""--noipcheck"". Read the source code for more information.", "iRO Castle Status")
                Return Nothing
            End If
        Next

        If o.NoGUI Then
            o.ShowConsole = True
        End If

#If DEBUG Then
        o.ShowConsole = True
        o.Statistics = True
#If DEBUG_VERBOSE Then
        o.Verbose = True
#End If
#If DEBUG_NOIPCHECK Then
        o.NoIPCheck = True
#End If
#End If

        Return o
    End Function

    Private _ShowConsole As Boolean = False
    Public Property ShowConsole As Boolean
        Get
            Return _ShowConsole
        End Get
        Private Set(value As Boolean)
            _ShowConsole = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ShowConsole"))
        End Set
    End Property

    Private _Statistics As Boolean = False
    Public Property Statistics As Boolean
        Get
            Return _Statistics
        End Get
        Private Set(value As Boolean)
            _Statistics = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Statistics"))
        End Set
    End Property

    Private _Verbose As Boolean = False
    Public Property Verbose As Boolean
        Get
            Return _Verbose
        End Get
        Private Set(value As Boolean)
            _Verbose = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Verbose"))
        End Set
    End Property

    Private _NoGUI As Boolean = False
    Public Property NoGUI As Boolean
        Get
            Return _NoGUI
        End Get
        Private Set(value As Boolean)
            _NoGUI = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("NoGUI"))
        End Set
    End Property

    Private _NoIPCheck As Boolean = False
    Public Property NoIPCheck As Boolean
        Get
            Return _NoIPCheck
        End Get
        Private Set(value As Boolean)
            _NoIPCheck = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("NoIPCheck"))
        End Set
    End Property

End Class
