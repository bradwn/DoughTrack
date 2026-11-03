Imports DoughTrack.Data
Imports DoughTrack.Helpers
Imports DoughTrack.Models

''' <summary>
''' Shows one menu item, its sales count, and its recipe.
''' Recipe adds, quantity edits, and removals are saved immediately.
''' </summary>
Public Class frmMenuItemDetails
    Inherits Form

    Private ReadOnly _item As MenuItem
    Private ReadOnly _summary As Label
    Private ReadOnly _preview As PictureBox
    Private ReadOnly _recipe As RecipeEditor

    Public Sub New(item As MenuItem)
        _item = item
        Font = New Font("Segoe UI", 10.0!)
        Text = "Menu Item Details"
        StartPosition = FormStartPosition.CenterParent
        MinimizeBox = False
        MaximizeBox = False
        FormBorderStyle = FormBorderStyle.FixedDialog
        ClientSize = New Size(860, 640)
        BackColor = UiTheme.Content

        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(16)
        }
        _preview = New PictureBox With {
            .Dock = DockStyle.Left,
            .Width = 140,
            .SizeMode = PictureBoxSizeMode.Zoom,
            .BackColor = UiTheme.Content
        }
        _summary = New Label With {
            .Dock = DockStyle.Fill,
            .Padding = New Padding(16, 0, 0, 0)
        }
        Dim header As New Panel With {.Dock = DockStyle.Top, .Height = 150}
        header.Controls.Add(_preview)
        header.Controls.Add(_summary)
        header.Controls.SetChildIndex(_summary, 0)

        Dim title As New Label With {
            .Dock = DockStyle.Top,
            .Height = 28,
            .Font = New Font(Font, FontStyle.Bold),
            .Text = "Recipe"
        }
        _recipe = New RecipeEditor()
        Dim closeButton As Button = UiTheme.MakeButton("Close")
        Dim editButton As Button = UiTheme.MakeButton("Edit menu item")
        Dim buttons As New FlowLayoutPanel With {
            .Dock = DockStyle.Bottom,
            .Height = 46,
            .FlowDirection = FlowDirection.RightToLeft,
            .Padding = New Padding(0, 6, 0, 0)
        }
        buttons.Controls.Add(closeButton)
        buttons.Controls.Add(editButton)

        card.Controls.Add(header)
        card.Controls.Add(title)
        card.Controls.Add(buttons)
        card.Controls.Add(_recipe)
        card.Controls.SetChildIndex(_recipe, 0)
        Controls.Add(card)

        CancelButton = closeButton
        AddHandler closeButton.Click, Sub() Close()
        AddHandler editButton.Click, AddressOf EditItem
        LoadItem()
    End Sub

    Private Sub LoadItem()
        Dim current As MenuItem = MenuItemRepository.GetById(_item.MenuItemId)
        If current Is Nothing Then
            _summary.Text = "This menu item no longer exists."
            Return
        End If
        _item.MenuItemName = current.MenuItemName
        _item.Category = current.Category
        _item.SellingPrice = current.SellingPrice
        _item.ImagePath = current.ImagePath
        _item.Status = current.Status

        Dim sold As Integer = MenuItemRepository.CountSoldInLast30Days(_item.MenuItemId)
        Dim saleRule As String = If(_item.Status = "Active",
            "This item can be sold.",
            "Inactive items stay on past sales and cannot be sold.")
        _summary.Text =
            _item.MenuItemName & vbCrLf &
            "ID " & _item.MenuItemId.ToString() & "    " & _item.Category & vbCrLf &
            UiTheme.FormatMoney(_item.SellingPrice) & "    " & _item.Status & vbCrLf &
            "Sold in the last 30 days: " & sold.ToString() & vbCrLf &
            saleRule
        ShowImage()
        _recipe.Configure(_item.MenuItemId, True, MenuItemIngredientRepository.GetLines(_item.MenuItemId))
    End Sub

    Private Sub EditItem(sender As Object, e As EventArgs)
        Using editor As New frmMenuItemEdit(_item)
            If editor.ShowDialog(Me) = DialogResult.OK Then LoadItem()
        End Using
    End Sub

    Private Sub ShowImage()
        Dim previous As Image = _preview.Image
        _preview.Image = Nothing
        If previous IsNot Nothing Then previous.Dispose()
        Dim fullPath As String = MenuImageStore.Resolve(_item.ImagePath)
        If fullPath = "" OrElse Not IO.File.Exists(fullPath) Then Return
        Using stream As New IO.FileStream(fullPath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite)
            Using source As Image = Image.FromStream(stream)
                _preview.Image = New Bitmap(source)
            End Using
        End Using
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Dim preview As Image = _preview.Image
        _preview.Image = Nothing
        If preview IsNot Nothing Then preview.Dispose()
        MyBase.OnFormClosed(e)
    End Sub

End Class
