Imports DoughTrack.Data
Imports DoughTrack.Helpers
Imports DoughTrack.Models

''' <summary>
''' Inventory > Stocks: search, category and status filters, add, edit, and activate/deactivate.
''' Items are never deleted because recipes, sales and stock-ins point to them.
''' </summary>
Public Class StocksView
    Inherits UserControl

    Private ReadOnly _search As TextBox
    Private ReadOnly _category As ComboBox
    Private ReadOnly _status As ComboBox
    Private ReadOnly _message As Label
    Private ReadOnly _grid As DataGridView

    Public Sub New()
        Dock = DockStyle.Fill
        BackColor = UiTheme.Content
        Padding = New Padding(16)

        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(16)
        }

        Dim tools As New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 46,
            .WrapContents = False,
            .FlowDirection = FlowDirection.LeftToRight
        }
        _search = New TextBox With {.Width = 200}
        _category = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Width = 150}
        _category.Items.AddRange(New Object() {"All categories", "Ingredients", "Supplies"})
        _category.SelectedIndex = 0
        _status = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Width = 130}
        _status.Items.AddRange(New Object() {"All status", "Normal", "Low", "Out of Stock", "Inactive"})
        _status.SelectedIndex = 0
        Dim searchButton As Button = UiTheme.MakeButton("Search")
        Dim addButton As Button = UiTheme.MakeButton("Add Item")
        tools.Controls.Add(New Label With {.Text = "Search", .AutoSize = True, .Padding = New Padding(0, 8, 6, 0)})
        tools.Controls.Add(_search)
        tools.Controls.Add(searchButton)
        tools.Controls.Add(New Label With {.Text = "Filter", .AutoSize = True, .Padding = New Padding(12, 8, 6, 0)})
        tools.Controls.Add(_category)
        tools.Controls.Add(_status)
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
        AddHandler _category.SelectedIndexChanged, Sub() ReloadGrid()
        AddHandler _status.SelectedIndexChanged, Sub() ReloadGrid()
        AddHandler _grid.CellContentClick, AddressOf GridClick

        ReloadGrid()
    End Sub

    Private Sub BuildColumns()
        _grid.Columns.Add(TextColumn("Code", "ID", 60))
        _grid.Columns.Add(TextColumn("ItemName", "Item Name", 150))
        _grid.Columns.Add(TextColumn("Category", "Category", 90))
        _grid.Columns.Add(RightColumn("Quantity", "Quantity", 90))
        _grid.Columns.Add(RightColumn("Reorder", "Reorder Point", 90))
        _grid.Columns.Add(TextColumn("Status", "Status", 80))
        _grid.Columns.Add(ActionColumn("Edit", "Edit", "Actions"))
        _grid.Columns.Add(ActionColumn("StatusAction", "Deactivate", ""))
    End Sub

    Private Sub ReloadGrid()
        _grid.Rows.Clear()
        Dim category As String = ""
        If _category.SelectedIndex > 0 Then category = CStr(_category.SelectedItem)
        Dim wantedStatus As String = ""
        If _status.SelectedIndex > 0 Then wantedStatus = CStr(_status.SelectedItem)

        Dim items As List(Of InventoryItem)
        Try
            items = InventoryItemRepository.Search(_search.Text, category)
        Catch ex As Exception
            Debug.WriteLine("STOCKS LOAD ERROR: " & ex.ToString())
            _message.ForeColor = UiTheme.Danger
            _message.Text = "Unable to load inventory. Please check the database connection."
            Return
        End Try

        Dim shown As Integer = 0
        For Each item As InventoryItem In items
            Dim statusText As String = StockFormat.StatusText(item)
            If wantedStatus <> "" AndAlso statusText <> wantedStatus Then Continue For

            Dim rowIndex As Integer = _grid.Rows.Add(
                StockFormat.ItemCode(item),
                item.ItemName,
                item.Category,
                StockFormat.FormatQuantity(item.Quantity, item.Unit),
                StockFormat.FormatQuantity(item.ReorderPoint, item.Unit),
                statusText,
                "Edit",
                If(item.Status = "Active", "Deactivate", "Reactivate"))
            _grid.Rows(rowIndex).Tag = item
            _grid.Rows(rowIndex).Cells("Status").Style.ForeColor = StockFormat.StatusColor(statusText)
            _grid.Rows(rowIndex).Cells("Status").Style.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
            If item.Status <> "Active" Then
                _grid.Rows(rowIndex).DefaultCellStyle.ForeColor = UiTheme.TextMuted
            End If
            shown += 1
        Next

        _message.ForeColor = UiTheme.TextMuted
        If shown = 0 Then
            _message.Text = "No inventory items match this search."
        Else
            _message.Text = shown.ToString() & If(shown = 1, " item", " items")
        End If
    End Sub

    Private Sub AddItem(sender As Object, e As EventArgs)
        Dim created As New InventoryItem With {.Status = "Active", .Category = "Ingredients", .Unit = "pcs"}
        Using editor As New frmInventoryItemEdit(created)
            If editor.ShowDialog(FindForm()) = DialogResult.OK Then ReloadGrid()
        End Using
    End Sub

    Private Sub GridClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 Then Return
        Dim item As InventoryItem = TryCast(_grid.Rows(e.RowIndex).Tag, InventoryItem)
        If item Is Nothing Then Return
        Dim columnName As String = _grid.Columns(e.ColumnIndex).Name

        If columnName = "Edit" Then
            ' Edit a copy so a cancelled dialog does not change the row shown.
            Dim copy As New InventoryItem With {
                .InventoryItemId = item.InventoryItemId,
                .ItemName = item.ItemName,
                .Category = item.Category,
                .Quantity = item.Quantity,
                .Unit = item.Unit,
                .ReorderPoint = item.ReorderPoint,
                .UnitCost = item.UnitCost,
                .Status = item.Status
            }
            Using editor As New frmInventoryItemEdit(copy)
                If editor.ShowDialog(FindForm()) = DialogResult.OK Then ReloadGrid()
            End Using
        ElseIf columnName = "StatusAction" Then
            ToggleStatus(item)
        End If
    End Sub

    Private Sub ToggleStatus(item As InventoryItem)
        Dim makeInactive As Boolean = (item.Status = "Active")
        Dim question As String
        If makeInactive Then
            question = "Deactivate """ & item.ItemName & """?" & vbCrLf & vbCrLf &
                       "It stays in the database for recipes and past records, but it can no longer receive new stock-in and will not appear in Low Stock."
        Else
            question = "Reactivate """ & item.ItemName & """?"
        End If
        If MessageBox.Show(FindForm(), question, "DoughTrack", MessageBoxButtons.YesNo,
                           MessageBoxIcon.Question) <> DialogResult.Yes Then Return
        Try
            InventoryItemRepository.SetStatus(item.InventoryItemId, If(makeInactive, "Inactive", "Active"))
            ReloadGrid()
        Catch ex As Exception
            Debug.WriteLine("INVENTORY STATUS ERROR: " & ex.ToString())
            MessageBox.Show(FindForm(), "Unable to change the status of this item.", "DoughTrack",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Shared Function TextColumn(name As String, header As String, weight As Single) As DataGridViewTextBoxColumn
        Return New DataGridViewTextBoxColumn With {
            .Name = name, .HeaderText = header, .ReadOnly = True, .FillWeight = weight
        }
    End Function

    Private Shared Function RightColumn(name As String, header As String, weight As Single) As DataGridViewTextBoxColumn
        Dim column As DataGridViewTextBoxColumn = TextColumn(name, header, weight)
        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        Return column
    End Function

    Private Shared Function ActionColumn(name As String, text As String, header As String) As DataGridViewButtonColumn
        Return New DataGridViewButtonColumn With {
            .Name = name,
            .HeaderText = header,
            .Text = text,
            .UseColumnTextForButtonValue = False,
            .AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            .Width = 100,
            .FlatStyle = FlatStyle.Flat
        }
    End Function

End Class
