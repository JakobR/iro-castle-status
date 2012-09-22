
'From http://blogs.msdn.com/b/jpricket/archive/2008/08/05/wpf-a-stretching-treeview.aspx
'Explanation of the problem: http://leecampbell.blogspot.co.at/2009/01/horizontal-stretch-on-treeviewitems.html

Public Class StretchingTreeView
    Inherits TreeView

    Protected Overrides Function GetContainerForItemOverride() As DependencyObject
        Return New StretchingTreeViewItem()
    End Function

    Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
        Return TypeOf item Is StretchingTreeViewItem
    End Function

End Class

Public Class StretchingTreeViewItem
    Inherits TreeViewItem

    Public Sub New()
        AddHandler Me.Loaded, New RoutedEventHandler(AddressOf StretchingTreeViewItem_Loaded)
    End Sub

    Private Sub StretchingTreeViewItem_Loaded(sender As Object, e As RoutedEventArgs)
        ' The purpose of this code is to stretch the Header Content all the way accross the TreeView. 
        If Me.VisualChildrenCount > 0 Then
            Dim grid As Grid = TryCast(Me.GetVisualChild(0), Grid)

            If grid IsNot Nothing AndAlso grid.ColumnDefinitions.Count = 3 Then
                ' Remove the middle column which is set to Auto and let it get replaced with the 
                ' last column that is set to Star.
                grid.ColumnDefinitions.RemoveAt(1)
            End If
        End If
    End Sub

    Protected Overrides Function GetContainerForItemOverride() As DependencyObject
        Return New StretchingTreeViewItem()
    End Function

    Protected Overrides Function IsItemItsOwnContainerOverride(item As Object) As Boolean
        Return TypeOf item Is StretchingTreeViewItem
    End Function

End Class
