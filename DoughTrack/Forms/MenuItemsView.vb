Imports System.IO
Imports DoughTrack.Data
Imports DoughTrack.Helpers
Imports DoughTrack.Models

''' <summary>
''' Menu list: search, category filter, add, edit, view, and activate or deactivate.
''' </summary>
Public Class MenuItemsView
    Inherits UserControl

    Private ReadOnly _search As TextBox
    Private ReadOnly _category As ComboBox
    Private ReadOnly _message As Label
    Private ReadOnly _grid As DataGridView
    Private ReadOnly _thumbs As New List(Of Image)
    Private ReadOnly _blank As Image
    Private _loadingFilters As Boolean

    Public Sub New()
        Dock = DockStyle.Fill
        BackColor = UiTheme.Content
        Padding = New Padding(16)
        _blank = CreateBlank()

        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(16)
        }

        Dim tools As New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 46,
            .WrapContents = False,
            .FlowDirection = FlowDirection.LeftToRight,
            .Padding = New Padding(0, 0, 0, 8)
        }
        _search = New TextBox With {.Width = 220}
        _category = New ComboBox With {
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Width = 180
        }
        Dim searchButton As Button = UiTheme.MakeButton("Search")
        Dim addButton As Button = UiTheme.MakeButton("Add Menu Item")
        tools.Controls.Add(New Label With {
            .Text = "Search",
            .AutoSize = True,
            .Padding = New Padding(0, 8, 6, 0)
        })
        tools.Controls.Add(_search)
        tools.Controls.Add(searchButton)
        tools.Controls.Add(New Label With {
            .Text = "Category",
            .AutoSize = True,
            .Padding = New Padding(12, 8, 6, 0)
        })
        tools.Controls.Add(_category)
        tools.Controls.Add(addButton)

        _message = New Label With {
            .Dock = DockStyle.Bottom,
            .Height = 24,
            .ForeColor = UiTheme.TextMuted,
            .TextAlign = ContentAlignment.MiddleLeft
        }

        _grid = New DataGridView With {.Dock = DockStyle.Fill}
        UiTheme.StyleGrid(_grid)
        _grid.ReadOnly = False
        _grid.EditMode = DataGridViewEditMode.EditProgrammatically
        _grid.RowTemplate.Height = 58
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        BuildColumns()

        card.Controls.Add(tools)
        card.Controls.Add(_message)
        card.Controls.Add(_grid)
        card.Controls.SetChildIndex(_grid, 0)
        Controls.Add(card)

        AddHandler searchButton.Click, Sub() ReloadGrid()
        AddHandler addButton.Click, AddressOf AddItem
        AddHandler _search.KeyDown,
            Sub(sender, e)
                If e.KeyCode = Keys.Enter Then
                    ReloadGrid()
                    e.SuppressKeyPress = True
                End If
            End Sub
        AddHandler _category.SelectedIndexChanged,
            Sub()
                If Not _loadingFilters Then ReloadGrid()
            End Sub
        AddHandler _grid.CellContentClick, AddressOf GridClick
        AddHandler Disposed, Sub() ClearThumbs()

        LoadCategories()
        ReloadGrid()
    End Sub

    Private Sub BuildColumns()
        _grid.Columns.Add(DataColumn("ID", "ID", 40))
        Dim imageColumn As New DataGridViewImageColumn With {
            .Name = "Image",
            .HeaderText = "Image",
            .ImageLayout = DataGridViewImageCellLayout.Zoom,
            .AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            .Width = 72
        }
        imageColumn.ReadOnly = True
        _grid.Columns.Add(imageColumn)
        _grid.Columns.Add(DataColumn("MenuItem", "Menu Item", 160))
        _grid.Columns.Add(DataColumn("Category", "Category", 90))
        Dim price As DataGridViewTextBoxColumn = DataColumn("Price", "Selling Price", 80)
        price.DefaultCellStyle.Format = "₱#,##0.00"
        price.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        _grid.Columns.Add(price)
        _grid.Columns.Add(DataColumn("Status", "Status", 70))
        _grid.Columns.Add(ActionColumn("View", "View"))
        _grid.Columns.Add(ActionColumn("Edit", "Edit"))
        _grid.Columns.Add(ActionColumn("StatusAction", "Deactivate"))
        _grid.Columns("View").HeaderText = "Actions"
        _grid.Columns("Edit").HeaderText = ""
        _grid.Columns("StatusAction").HeaderText = ""
    End Sub

    Private Sub LoadCategories()
        _loadingFilters = True
        Dim selected As String = ""
        If _category.SelectedIndex > 0 Then selected = CStr(_category.SelectedItem)
        _category.Items.Clear()
        _category.Items.Add("All categories")
        Try
            For Each category As String In MenuItemRepository.GetCategories()
                _category.Items.Add(category)
            Next
            _message.ForeColor = UiTheme.TextMuted
        Catch ex As Exception
            _message.ForeColor = UiTheme.Danger
            _message.Text = ex.Message
        End Try
        Dim index As Integer = _category.Items.IndexOf(selected)
        _category.SelectedIndex = If(index >= 0, index, 0)
        _loadingFilters = False
    End Sub

    Private Sub ReloadGrid()
        ClearThumbs()
        _grid.Rows.Clear()
        Dim category As String = ""
        If _category.SelectedIndex > 0 Then category = CStr(_category.SelectedItem)

        Dim items As List(Of MenuItem)
        Try
            items = MenuItemRepository.Search(_search.Text, category)
        Catch ex As Exception
            _message.ForeColor = UiTheme.Danger
            _message.Text = ex.Message
            Return
        End Try

        For Each item As MenuItem In items
            Dim thumb As Image = LoadThumb(item.ImagePath)
            Dim rowIndex As Integer = _grid.Rows.Add(
                item.MenuItemId,
                thumb,
                item.MenuItemName,
                item.Category,
                item.SellingPrice,
                item.Status,
                "View",
                "Edit",
                If(item.Status = "Active", "Deactivate", "Reactivate"))
            _grid.Rows(rowIndex).Tag = item
        Next

        _message.ForeColor = UiTheme.TextMuted
        If items.Count = 0 Then
            _message.Text = "No menu items match this search."
        Else
            _message.Text = items.Count.ToString() & If(items.Count = 1, " menu item", " menu items")
        End If
    End Sub

    Private Sub AddItem(sender As Object, e As EventArgs)
        Dim created As New MenuItem With {.Status = "Active"}
        Using editor As New frmMenuItemEdit(created)
            If editor.ShowDialog(FindForm()) = DialogResult.OK Then
                LoadCategories()
                ReloadGrid()
            End If
        End Using
    End Sub

    Private Sub GridClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 Then Return
        Dim item As MenuItem = TryCast(_grid.Rows(e.RowIndex).Tag, MenuItem)
        If item Is Nothing Then Return
        Dim columnName As String = _grid.Columns(e.ColumnIndex).Name

        If columnName = "View" Then
            Using details As New frmMenuItemDetails(item)
                details.ShowDialog(FindForm())
            End Using
            LoadCategories()
            ReloadGrid()
        ElseIf columnName = "Edit" Then
            Using editor As New frmMenuItemEdit(item)
                If editor.ShowDialog(FindForm()) = DialogResult.OK Then
                    LoadCategories()
                    ReloadGrid()
                End If
            End Using
        ElseIf columnName = "StatusAction" Then
            ToggleStatus(item)
        End If
    End Sub

    Private Sub ToggleStatus(item As MenuItem)
        Dim makeInactive As Boolean = item.Status = "Active"
        Dim question As String
        If makeInactive Then
            question = "Deactivate " & item.MenuItemName & "?" & vbCrLf &
                "It stays on past sales and cannot be sold while inactive."
        Else
            question = "Reactivate " & item.MenuItemName & "?"
        End If
        If MessageBox.Show(question, "DoughTrack", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
            Return
        End If

        Try
            MenuItemRepository.SetStatus(item.MenuItemId, If(makeInactive, "Inactive", "Active"))
            ReloadGrid()
        Catch ex As Exception
            _message.ForeColor = UiTheme.Danger
            _message.Text = ex.Message
        End Try
    End Sub

    Private Function LoadThumb(storedPath As String) As Image
        Dim fullPath As String = MenuImageStore.Resolve(storedPath)
        If fullPath = "" OrElse Not File.Exists(fullPath) Then Return _blank
        Try
            Using stream As New FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using source As Image = Image.FromStream(stream)
                    Dim copy As New Bitmap(source)
                    _thumbs.Add(copy)
                    Return copy
                End Using
            End Using
        Catch
            Return _blank
        End Try
    End Function

    Private Sub ClearThumbs()
        For Each thumb As Image In _thumbs
            thumb.Dispose()
        Next
        _thumbs.Clear()
    End Sub

    Private Shared Function CreateBlank() As Image
        Dim bitmap As New Bitmap(48, 48)
        Using graphics As Graphics = Graphics.FromImage(bitmap)
            graphics.Clear(Color.FromArgb(243, 244, 246))
        End Using
        Return bitmap
    End Function

    Private Shared Function DataColumn(name As String, header As String, weight As Single) As DataGridViewTextBoxColumn
        Return New DataGridViewTextBoxColumn With {
            .Name = name,
            .HeaderText = header,
            .ReadOnly = True,
            .FillWeight = weight
        }
    End Function

    Private Shared Function ActionColumn(name As String, text As String) As DataGridViewButtonColumn
        Return New DataGridViewButtonColumn With {
            .Name = name,
            .HeaderText = "Actions",
            .Text = text,
            .UseColumnTextForButtonValue = False,
            .AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            .Width = 100,
            .FlatStyle = FlatStyle.Flat
        }
    End Function

End Class
