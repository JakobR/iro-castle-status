Class MainWindow 

    Private Sub ShowWoE1_Click(sender As Object, e As RoutedEventArgs) Handles ShowWoE1.Click
        ShowWoE(WoE.Type.WoE1)
    End Sub

    Private Sub ShowWoE2_Click(sender As Object, e As RoutedEventArgs) Handles ShowWoE2.Click
        ShowWoE(WoE.Type.WoE2)
    End Sub

    Private Sub ShowWoE(Type As WoE.Type)
        ' ...
    End Sub

    Private Sub DebugBreakButton_Click(sender As Object, e As RoutedEventArgs) Handles DebugBreakButton.Click
        WoE.iRO.ProcessBreakMessage(Now, "The [BalderGuild5] castle has been conquered by the [Valkyrie] guild.")
        WoE.iRO.ProcessBreakMessage(Now, "The [Valkyrie Realms 5] castle has been conquered by the [Warrior Nation] guild.")
        WoE.iRO.ProcessBreakMessage(Now, "The [Valkyrie] guild conquered the [Valfreyja 3] of Horn.")
        WoE.iRO.ProcessBreakMessage(Now, "The [Warrior Nation] guild conquered the [Valfreyja 5] stronghold of Banadis.")
        WoE.iRO.ProcessBreakMessage(Now, "The [Nithafjoll 1] stronghold of Himinn is occupied by the [Volkiba] Guild.")
        'BreakLogListView.ScrollIntoView(WoE.iRO.AllCastleBreaks.Last)
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
#If DEBUG Then
        DebugBreakButton.Visibility = Windows.Visibility.Visible
#End If

        AddHandler WoE.iRO.BreakOccurred, AddressOf WoE_iRO_BreakOccurred
    End Sub

    Private Sub WoE_iRO_BreakOccurred(sender As Object, e As Castle.BreakEventArgs)
        ' not working
        'BreakLogListView.UpdateLayout()
        'BreakLogListView.ScrollIntoView(WoE.iRO.AllCastleBreaks.Last)
    End Sub

End Class
