
Imports System.Windows.Threading

Class MainWindow

    Private Sub InvertRealmEnabled(sender As Object, e As RoutedEventArgs)
        For Each r In WoE.iRO.Realms
            r.IsEnabled = Not r.IsEnabled
        Next
    End Sub

    Private Sub DebugBreakButton_Click(sender As Object, e As RoutedEventArgs) Handles DebugBreakButton.Click
        Dim packet_start = Chr(&H9A) & Chr(0) & Chr(80) & Chr(0)
        Dim packet_end = Chr(0)
        ProcessBreakMessage(Now, packet_start & "The [BalderGuild5] castle has been conquered by the [Valkyrie] guild." & packet_end)
        ProcessBreakMessage((Now - TimeSpan.FromMinutes(4)), packet_start & "The [Valkyrie Realms 5] castle has been conquered by the [blah] guild." & packet_end)
        ProcessBreakMessage(Now, packet_start & "The [Valkyrie Realms 5] castle has been conquered by the [Warrior Nation] guild." & packet_end)
        ProcessBreakMessage(Now, packet_start & "The [Valkyrie] guild conquered the [Valfreyja 3] of Horn." & packet_end)
        ProcessBreakMessage(Now, packet_start & "The [Warrior Nation] guild conquered the [Valfreyja 5] stronghold of Banadis." & packet_end)
        ProcessBreakMessage(Now, packet_start & "The [Nithafjoll 1] stronghold of Himinn is occupied by the [Revelations] Guild." & packet_end)
        ProcessBreakMessage(Now, packet_start & "f3498sag3" & packet_start & "The [Nithafjoll 2] stronghold of Himinn is occupied by the [Revelations] Guild." & packet_end & "sdkfj83gq93gn" & packet_start & "The [Nithafjoll 3] stronghold of Himinn is occupied by the [Revelations] Guild." & packet_end & "afrpokf09gk4hgrjtssss" & packet_start & "The [Warrior Nation] guild conquered the [Valfreyja 2] stronghold of asdfsg." & packet_end & "s" & packet_start & "The [Luina Guild 3] castle has been conquered by the [Valkyrie] guild." & packet_end & "asdfergv")
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
#If DEBUG Then
        DebugBreakButton.Visibility = Windows.Visibility.Visible
#End If

        For Each r In WoE.iRO.Realms
            r.IsEnabled = (r.Type = WoE.Type.WoE1)
        Next

        AddHandler WoE.iRO.BreakOccurred, AddressOf WoE_iRO_BreakOccurred

        Me.Top = My.Settings.VerticalLayout_WindowTop
        Me.Left = My.Settings.VerticalLayout_WindowLeft
        Me.Width = My.Settings.VerticalLayout_WindowWidth
        Me.Height = My.Settings.VerticalLayout_WindowHeight
        BreakLogColumn.Width = My.Settings.VerticalLayout_BreakLogWidth

        If My.Settings.UseHorizontalLayout Then
            HorizontalLayoutCheckBox.IsChecked = True
        End If

    End Sub

    Private Sub WoE_iRO_BreakOccurred(sender As Object, e As Castle.BreakEventArgs)

        ' not a good solution, but better than nothing
        Static _timer As DispatcherTimer
        If _timer Is Nothing Then
            _timer = New DispatcherTimer
            _timer.Interval = TimeSpan.FromMilliseconds(1)
            AddHandler _timer.Tick, Sub(s2, e2)
                                        _timer.Stop()
                                        Dim last = WoE.iRO.AllCastleBreaks.Last
                                        BreakLogListView.ScrollIntoView(last)
                                        BreakLogListView.SelectedItem = last
                                    End Sub
        End If
        _timer.Stop()
        _timer.Start()

        ' not working
        'BreakLogListView.UpdateLayout()
        'BreakLogListView.GetBindingExpression(ListView.ItemsSourceProperty).UpdateTarget()
        'BreakLogListView.ScrollIntoView(WoE.iRO.AllCastleBreaks.Last)
        ' this line does select the last item though, even if it's not yet in the list
        'BreakLogListView.SelectedItem = WoE.iRO.AllCastleBreaks.Last
    End Sub

    Private Sub HorizontalLayoutCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles HorizontalLayoutCheckBox.Checked

        My.Settings.VerticalLayout_WindowTop = Me.Top
        My.Settings.VerticalLayout_WindowLeft = Me.Left
        My.Settings.VerticalLayout_WindowWidth = Me.Width
        My.Settings.VerticalLayout_WindowHeight = Me.Height
        My.Settings.VerticalLayout_BreakLogWidth = BreakLogColumn.Width

        Me.Top = My.Settings.HorizontalLayout_WindowTop
        Me.Left = My.Settings.HorizontalLayout_WindowLeft
        Me.Width = My.Settings.HorizontalLayout_WindowWidth
        Me.Height = My.Settings.HorizontalLayout_WindowHeight
        BreakLogColumn.Width = My.Settings.HorizontalLayout_BreakLogWidth

        My.Settings.UseHorizontalLayout = True

        My.Settings.Save()

    End Sub

    Private Sub HorizontalLayoutCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles HorizontalLayoutCheckBox.Unchecked

        My.Settings.HorizontalLayout_WindowTop = Me.Top
        My.Settings.HorizontalLayout_WindowLeft = Me.Left
        My.Settings.HorizontalLayout_WindowWidth = Me.Width
        My.Settings.HorizontalLayout_WindowHeight = Me.Height
        My.Settings.HorizontalLayout_BreakLogWidth = BreakLogColumn.Width

        Me.Top = My.Settings.VerticalLayout_WindowTop
        Me.Left = My.Settings.VerticalLayout_WindowLeft
        Me.Width = My.Settings.VerticalLayout_WindowWidth
        Me.Height = My.Settings.VerticalLayout_WindowHeight
        BreakLogColumn.Width = My.Settings.VerticalLayout_BreakLogWidth

        My.Settings.UseHorizontalLayout = False

        My.Settings.Save()

    End Sub

    Private Sub MainWindow_Closing(sender As Object, e As ComponentModel.CancelEventArgs) Handles Me.Closing

        If HorizontalLayoutCheckBox.IsChecked Then
            My.Settings.HorizontalLayout_WindowTop = Me.Top
            My.Settings.HorizontalLayout_WindowLeft = Me.Left
            My.Settings.HorizontalLayout_WindowWidth = Me.Width
            My.Settings.HorizontalLayout_WindowHeight = Me.Height
            My.Settings.HorizontalLayout_BreakLogWidth = BreakLogColumn.Width
        Else
            My.Settings.VerticalLayout_WindowTop = Me.Top
            My.Settings.VerticalLayout_WindowLeft = Me.Left
            My.Settings.VerticalLayout_WindowWidth = Me.Width
            My.Settings.VerticalLayout_WindowHeight = Me.Height
            My.Settings.VerticalLayout_BreakLogWidth = BreakLogColumn.Width
        End If

        My.Settings.Save()

    End Sub

End Class
