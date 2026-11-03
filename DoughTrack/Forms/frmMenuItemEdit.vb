Imports DoughTrack.Data
Imports DoughTrack.Helpers
Imports DoughTrack.Models

''' <summary>
''' Adds or edits a menu item and the recipe saved with it.
''' </summary>
Public Class frmMenuItemEdit
    Inherits Form

    Private ReadOnly _item As MenuItem
    Private ReadOnly _name As TextBox
    Private ReadOnly _category As ComboBox
    Private ReadOnly _price As NumericUpDown
    Private ReadOnly _status As ComboBox
    Private ReadOnly _imagePathLabel As Label
    Private ReadOnly _preview As PictureBox
    Private ReadOnly _message As Label
    Private ReadOnly _recipe As RecipeEditor
    Private _imagePath As String = ""

    Public Sub New(item As MenuItem)
        _item = item
        Font = New Font("Segoe UI", 10.0!)
        Text = If(item.MenuItemId = 0, "Add Menu Item", "Edit Menu Item")
        StartPosition = FormStartPosition.CenterParent
        MinimizeBox = False
        MaximizeBox = False
        FormBorderStyle = FormBorderStyle.FixedDialog
        ClientSize = New Size(860, 680)
        BackColor = UiTheme.Content

        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(16)
        }

        Dim fields As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 4,
            .RowCount = 4
        }
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110))
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 160))
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90))
        For row As Integer = 0 To 3
            fields.RowStyles.Add(New RowStyle(SizeType.Absolute, 36))
        Next

        _name = New TextBox With {.MaxLength = 100, .Dock = DockStyle.Fill}
        _category = New ComboBox With {.Dock = DockStyle.Fill, .DropDownStyle = ComboBoxStyle.DropDown}
        _price = New NumericUpDown With {
            .DecimalPlaces = 2,
            .Minimum = 0D,
            .Maximum = 99999999.99D,
            .ThousandsSeparator = True,
            .Dock = DockStyle.Left,
            .Width = 140
        }
        _status = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Fill}
        _status.Items.AddRange(New Object() {"Active", "Inactive"})
        _imagePathLabel = New Label With {
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft,
            .ForeColor = UiTheme.TextMuted,
            .AutoEllipsis = True,
            .Text = "No image"
        }
        Dim browse As Button = UiTheme.MakeButton("Choose image")
        Dim clearImage As Button = UiTheme.MakeButton("Clear")

        fields.Controls.Add(FieldLabel("Name"), 0, 0)
        fields.Controls.Add(_name, 1, 0)
        fields.SetColumnSpan(_name, 3)
        fields.Controls.Add(FieldLabel("Category"), 0, 1)
        fields.Controls.Add(_category, 1, 1)
        fields.SetColumnSpan(_category, 3)
        fields.Controls.Add(FieldLabel("Price"), 0, 2)
        fields.Controls.Add(_price, 1, 2)
        fields.Controls.Add(FieldLabel("Status"), 2, 2)
        fields.Controls.Add(_status, 3, 2)
        fields.Controls.Add(FieldLabel("Image"), 0, 3)
        fields.Controls.Add(_imagePathLabel, 1, 3)
        Dim imageButtons As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False
        }
        imageButtons.Controls.Add(browse)
        imageButtons.Controls.Add(clearImage)
        fields.Controls.Add(imageButtons, 2, 3)
        fields.SetColumnSpan(imageButtons, 2)

        _preview = New PictureBox With {
            .Dock = DockStyle.Right,
            .Width = 120,
            .Height = 120,
            .SizeMode = PictureBoxSizeMode.Zoom,
            .BackColor = UiTheme.Content,
            .Margin = New Padding(12)
        }
        Dim header As New Panel With {.Dock = DockStyle.Top, .Height = 220}
        header.Controls.Add(_preview)
        header.Controls.Add(fields)
        header.Controls.SetChildIndex(fields, 0)

        Dim recipeTitle As New Label With {
            .Dock = DockStyle.Top,
            .Height = 28,
            .Font = New Font(Font, FontStyle.Bold),
            .Text = "Recipe — ingredients and supplies"
        }
        _recipe = New RecipeEditor()
        _message = New Label With {
            .Dock = DockStyle.Bottom,
            .Height = 24,
            .ForeColor = UiTheme.Danger
        }
        Dim buttons As New FlowLayoutPanel With {
            .Dock = DockStyle.Bottom,
            .Height = 46,
            .FlowDirection = FlowDirection.RightToLeft,
            .Padding = New Padding(0, 6, 0, 0)
        }
        Dim saveButton As Button = UiTheme.MakeButton("Save")
        Dim cancelButton As Button = UiTheme.MakeButton("Cancel")
        buttons.Controls.Add(saveButton)
        buttons.Controls.Add(cancelButton)

        card.Controls.Add(header)
        card.Controls.Add(recipeTitle)
        card.Controls.Add(buttons)
        card.Controls.Add(_message)
        card.Controls.Add(_recipe)
        card.Controls.SetChildIndex(_recipe, 0)
        Controls.Add(card)

        CancelButton = cancelButton
        AddHandler browse.Click, AddressOf ChooseImage
        AddHandler clearImage.Click, AddressOf ClearChosenImage
        AddHandler saveButton.Click, AddressOf SaveItem
        AddHandler cancelButton.Click, Sub() Close()

        LoadFields()
    End Sub

    Private Sub LoadFields()
        For Each category As String In MenuItemRepository.GetCategories()
            _category.Items.Add(category)
        Next
        _name.Text = _item.MenuItemName
        _category.Text = _item.Category
        If _item.SellingPrice >= 0D AndAlso _item.SellingPrice <= _price.Maximum Then
            _price.Value = _item.SellingPrice
        End If
        _status.SelectedItem = If(_item.Status = "Inactive", "Inactive", "Active")
        _imagePath = If(_item.ImagePath, "")
        ShowImage()

        Dim lines As New List(Of RecipeLine)
        If _item.MenuItemId > 0 Then
            lines = MenuItemIngredientRepository.GetLines(_item.MenuItemId)
        End If
        _recipe.Configure(_item.MenuItemId, False, lines)
    End Sub

    Private Sub ChooseImage(sender As Object, e As EventArgs)
        Using dialog As New OpenFileDialog()
            dialog.Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp"
            dialog.Title = "Menu item image"
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            Try
                _imagePath = MenuImageStore.SaveCopy(dialog.FileName)
                ShowImage()
                _message.Text = ""
            Catch ex As Exception
                _message.Text = ex.Message
            End Try
        End Using
    End Sub

    Private Sub ClearChosenImage(sender As Object, e As EventArgs)
        _imagePath = ""
        ShowImage()
    End Sub

    Private Sub ShowImage()
        Dim previous As Image = _preview.Image
        _preview.Image = Nothing
        If previous IsNot Nothing Then previous.Dispose()
        If _imagePath = "" Then
            _imagePathLabel.Text = "No image"
            Return
        End If
        _imagePathLabel.Text = _imagePath
        Dim fullPath As String = MenuImageStore.Resolve(_imagePath)
        If Not IO.File.Exists(fullPath) Then Return
        Using stream As New IO.FileStream(fullPath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.ReadWrite)
            Using source As Image = Image.FromStream(stream)
                _preview.Image = New Bitmap(source)
            End Using
        End Using
    End Sub

    Private Sub SaveItem(sender As Object, e As EventArgs)
        _message.Text = ""
        _item.MenuItemName = _name.Text.Trim()
        _item.Category = _category.Text.Trim()
        _item.SellingPrice = _price.Value
        _item.Status = CStr(_status.SelectedItem)
        _item.ImagePath = _imagePath
        Try
            MenuItemRepository.SaveWithRecipe(_item, _recipe.CurrentLines())
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            _message.Text = ex.Message
        End Try
    End Sub

    Private Shared Function FieldLabel(caption As String) As Label
        Return New Label With {
            .Text = caption,
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft
        }
    End Function

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        Dim preview As Image = _preview.Image
        _preview.Image = Nothing
        If preview IsNot Nothing Then preview.Dispose()
        MyBase.OnFormClosed(e)
    End Sub

End Class
