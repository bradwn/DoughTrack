Imports DoughTrack.Data
Imports DoughTrack.Helpers
Imports DoughTrack.Models

''' <summary>
''' Inventory > Stock-In. The admin builds a list of incoming items in memory,
''' then Save Stock-In writes the record and increases inventory in one transaction.
''' Quantities can be entered in a compatible unit (for example kg for an item counted in g);
''' they are converted to the item's own unit before saving.
''' </summary>
Public Class StockInView
    Inherits UserControl

    Private ReadOnly _date As DateTimePicker
    Private ReadOnly _item As ComboBox
    Private ReadOnly _quantity As NumericUpDown
    Private ReadOnly _unit As ComboBox
    Private ReadOnly _unitCost As NumericUpDown
    Private ReadOnly _lines As DataGridView
    Private ReadOnly _recent As DataGridView
    Private ReadOnly _total As Label
    Private ReadOnly _message As Label

    Private ReadOnly _items As New List(Of InventoryItem)
    Private ReadOnly _draft As New List(Of StockInDraftLine)

    Public Sub New()
        Dock = DockStyle.Fill
        BackColor = UiTheme.Content
        Padding = New Padding(16)

        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(16)
        }

        Dim root As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 6
        }
        root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 42))   ' date
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 70))   ' entry row
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 55.0F)) ' draft lines
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 70))   ' total + buttons + message
        root.RowStyles.Add(New RowStyle(SizeType.Absolute, 28))   ' recent title
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 45.0F)) ' recent grid

        ' ---- Row 0: date ----
        _date = New DateTimePicker With {
            .Format = DateTimePickerFormat.Short,
            .MaxDate = Date.Today,
            .Value = Date.Today,
            .Width = 140
        }
        Dim dateRow As New FlowLayoutPanel With {.Dock = DockStyle.Fill, .WrapContents = False}
        dateRow.Controls.Add(New Label With {.Text = "Date *", .AutoSize = True, .Padding = New Padding(0, 6, 8, 0),
                                             .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)})
        dateRow.Controls.Add(_date)
        root.Controls.Add(dateRow, 0, 0)

        ' ---- Row 1: add-item entry ----
        _item = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Fill}
        _quantity = New NumericUpDown With {
            .DecimalPlaces = 3, .Minimum = 0D, .Maximum = 999999999.999D,
            .ThousandsSeparator = True, .Dock = DockStyle.Fill
        }
        _unit = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Fill}
        _unitCost = New NumericUpDown With {
            .DecimalPlaces = 2, .Minimum = 0D, .Maximum = 99999999.99D,
            .ThousandsSeparator = True, .Dock = DockStyle.Fill
        }
        Dim addButton As Button = UiTheme.MakeButton("Add Item")
        addButton.Anchor = AnchorStyles.Left

        Dim entry As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 5, .RowCount = 2}
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130))
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90))
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 130))
        entry.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110))
        entry.RowStyles.Add(New RowStyle(SizeType.Absolute, 24))
        entry.RowStyles.Add(New RowStyle(SizeType.Absolute, 34))
        entry.Controls.Add(SmallLabel("Item *"), 0, 0)
        entry.Controls.Add(SmallLabel("Quantity *"), 1, 0)
        entry.Controls.Add(SmallLabel("Unit"), 2, 0)
        entry.Controls.Add(SmallLabel("Unit cost (per unit) *"), 3, 0)
        entry.Controls.Add(_item, 0, 1)
        entry.Controls.Add(_quantity, 1, 1)
        entry.Controls.Add(_unit, 2, 1)
        entry.Controls.Add(_unitCost, 3, 1)
        entry.Controls.Add(addButton, 4, 1)
        root.Controls.Add(entry, 0, 1)

        ' ---- Row 2: draft lines ----
        _lines = New DataGridView With {.Dock = DockStyle.Fill}
        UiTheme.StyleGrid(_lines)
        _lines.ReadOnly = False
        _lines.EditMode = DataGridViewEditMode.EditProgrammatically
        _lines.Columns.Add(TextColumn("LineItem", "Item", 150))
        _lines.Columns.Add(RightColumn("LineQty", "Quantity", 90))
        _lines.Columns.Add(RightColumn("LineCost", "Unit cost", 90))
        _lines.Columns.Add(RightColumn("LineSubtotal", "Subtotal", 90))
        _lines.Columns.Add(RightColumn("LineStored", "Added to stock", 110))
        _lines.Columns.Add(New DataGridViewButtonColumn With {
            .Name = "Remove", .HeaderText = "", .Text = "Remove",
            .AutoSizeMode = DataGridViewAutoSizeColumnMode.None, .Width = 90, .FlatStyle = FlatStyle.Flat
        })
        root.Controls.Add(_lines, 0, 2)

        ' ---- Row 3: total, buttons, message ----
        _total = New Label With {
            .Dock = DockStyle.Left,
            .AutoSize = False,
            .Width = 260,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Font = New Font("Segoe UI", 12.0!, FontStyle.Bold),
            .Text = "Total: " & UiTheme.FormatMoney(0D)
        }
        Dim saveButton As Button = UiTheme.MakeButton("Save Stock-In")
        Dim cancelButton As Button = UiTheme.MakeButton("Cancel")
        Dim buttons As New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.RightToLeft,
            .WrapContents = False
        }
        buttons.Controls.Add(saveButton)
        buttons.Controls.Add(cancelButton)
        Dim footerTop As New Panel With {.Dock = DockStyle.Top, .Height = 40}
        footerTop.Controls.Add(buttons)
        footerTop.Controls.Add(_total)
        _message = New Label With {.Dock = DockStyle.Fill, .ForeColor = UiTheme.Danger}
        Dim footer As New Panel With {.Dock = DockStyle.Fill}
        footer.Controls.Add(_message)
        footer.Controls.Add(footerTop)
        root.Controls.Add(footer, 0, 3)

        ' ---- Rows 4-5: recent stock-ins ----
        root.Controls.Add(New Label With {
            .Text = "Recent stock-ins (double-click a row to see its items)",
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.BottomLeft,
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
        }, 0, 4)
        _recent = New DataGridView With {.Dock = DockStyle.Fill}
        UiTheme.StyleGrid(_recent)
        root.Controls.Add(_recent, 0, 5)

        card.Controls.Add(root)
        Controls.Add(card)

        AddHandler _item.SelectedIndexChanged, AddressOf ItemChanged
        AddHandler addButton.Click, AddressOf AddLine
        AddHandler saveButton.Click, AddressOf SaveStockIn
        AddHandler cancelButton.Click, AddressOf CancelDraft
        AddHandler _lines.CellContentClick, AddressOf LineClick
        AddHandler _recent.CellDoubleClick, AddressOf ShowRecentLines

        LoadItems()
        LoadRecent()
    End Sub

    ' ------------------------------------------------------------ loading

    Private Sub LoadItems()
        _items.Clear()
        _item.Items.Clear()
        Try
            _items.AddRange(InventoryItemRepository.GetActive())
            For Each inventoryItem As InventoryItem In _items
                _item.Items.Add(inventoryItem.ItemName & " (" & inventoryItem.Unit & ")")
            Next
        Catch ex As Exception
            Debug.WriteLine("STOCK-IN ITEMS ERROR: " & ex.ToString())
            _message.Text = "Unable to load inventory items. Please check the database connection."
        End Try
        _unit.Items.Clear()
    End Sub

    Private Sub LoadRecent()
        Try
            _recent.DataSource = StockInRepository.GetRecent(15)
            UiTheme.ApplyColumnFormats(_recent, {"Total cost"}, Array.Empty(Of String)())
            If _recent.Columns.Contains("ID") Then _recent.Columns("ID").FillWeight = 30
        Catch ex As Exception
            Debug.WriteLine("STOCK-IN RECENT ERROR: " & ex.ToString())
        End Try
    End Sub

    Private Sub ItemChanged(sender As Object, e As EventArgs)
        _unit.Items.Clear()
        If _item.SelectedIndex < 0 Then Return
        Dim selected As InventoryItem = _items(_item.SelectedIndex)
        For Each choice As String In UnitConverter.UnitChoices(selected.Unit)
            _unit.Items.Add(choice)
        Next
        ' Default to the item's own unit.
        For index As Integer = 0 To _unit.Items.Count - 1
            If CStr(_unit.Items(index)).Equals(selected.Unit, StringComparison.OrdinalIgnoreCase) Then
                _unit.SelectedIndex = index
                Return
            End If
        Next
        If _unit.Items.Count > 0 Then _unit.SelectedIndex = 0
    End Sub

    ' ------------------------------------------------------------ draft lines

    Private Sub AddLine(sender As Object, e As EventArgs)
        _message.Text = ""
        If _item.SelectedIndex < 0 Then
            _message.Text = "Select an item."
            Return
        End If
        If _unit.SelectedItem Is Nothing Then
            _message.Text = "Select a unit."
            Return
        End If
        If _quantity.Value <= 0D Then
            _message.Text = "Quantity must be greater than zero."
            Return
        End If

        Dim selected As InventoryItem = _items(_item.SelectedIndex)
        Dim enteredUnit As String = CStr(_unit.SelectedItem)
        Dim storedQuantity As Decimal
        Try
            If enteredUnit.Equals(selected.Unit, StringComparison.OrdinalIgnoreCase) Then
                storedQuantity = _quantity.Value
            Else
                storedQuantity = UnitConverter.ConvertQuantity(_quantity.Value, enteredUnit, selected.Unit)
            End If
        Catch ex As InvalidOperationException
            _message.Text = ex.Message
            Return
        End Try
        storedQuantity = UnitConverter.RoundQuantity(storedQuantity)
        If storedQuantity <= 0D Then
            _message.Text = "That quantity is too small for the item's unit."
            Return
        End If

        Dim subtotal As Decimal = UnitConverter.RoundMoney(_quantity.Value * _unitCost.Value)
        Dim storedCost As Decimal = UnitConverter.RoundCost(_quantity.Value * _unitCost.Value / storedQuantity)

        _draft.Add(New StockInDraftLine With {
            .InventoryItemId = selected.InventoryItemId,
            .ItemName = selected.ItemName,
            .ItemUnit = selected.Unit,
            .EnteredQuantity = _quantity.Value,
            .EnteredUnit = enteredUnit,
            .EnteredUnitCost = _unitCost.Value,
            .StoredQuantity = storedQuantity,
            .StoredUnitCost = storedCost,
            .Subtotal = subtotal
        })
        RenderLines()

        ' Ready for the next item.
        _item.SelectedIndex = -1
        _quantity.Value = 0D
        _unitCost.Value = 0D
        _unit.Items.Clear()
        _item.Focus()
    End Sub

    Private Sub RenderLines()
        _lines.Rows.Clear()
        Dim total As Decimal = 0D
        For Each line As StockInDraftLine In _draft
            Dim rowIndex As Integer = _lines.Rows.Add(
                line.ItemName,
                StockFormat.FormatQuantity(line.EnteredQuantity, line.EnteredUnit),
                UiTheme.FormatMoney(line.EnteredUnitCost) & " / " & line.EnteredUnit,
                UiTheme.FormatMoney(line.Subtotal),
                "+" & StockFormat.FormatQuantity(line.StoredQuantity, line.ItemUnit),
                "Remove")
            _lines.Rows(rowIndex).Tag = line
            total += line.Subtotal
        Next
        _total.Text = "Total: " & UiTheme.FormatMoney(UnitConverter.RoundMoney(total))
    End Sub

    Private Sub LineClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 Then Return
        If _lines.Columns(e.ColumnIndex).Name <> "Remove" Then Return
        Dim line As StockInDraftLine = TryCast(_lines.Rows(e.RowIndex).Tag, StockInDraftLine)
        If line Is Nothing Then Return
        _draft.Remove(line)
        RenderLines()
    End Sub

    ' ------------------------------------------------------------ save / cancel

    Private Sub SaveStockIn(sender As Object, e As EventArgs)
        _message.Text = ""
        If _draft.Count = 0 Then
            _message.Text = "Add at least one item before saving."
            Return
        End If
        If Not Session.IsLoggedIn Then
            _message.Text = "You must be logged in to save a stock-in."
            Return
        End If

        Try
            Dim newId As Integer = StockInRepository.Save(_date.Value.Date, Session.CurrentUser.UserId, _draft)
            MessageBox.Show(FindForm(),
                            "Stock-In #" & newId.ToString("000000") & " saved." & vbCrLf & "Inventory quantities were updated.",
                            "DoughTrack", MessageBoxButtons.OK, MessageBoxIcon.Information)
            _draft.Clear()
            RenderLines()
            LoadItems()
            LoadRecent()
        Catch ex As InvalidOperationException
            _message.Text = ex.Message
        Catch ex As Exception
            Debug.WriteLine("STOCK-IN SAVE ERROR: " & ex.ToString())
            _message.Text = "Unable to save the stock-in. Please check the database connection."
        End Try
    End Sub

    Private Sub CancelDraft(sender As Object, e As EventArgs)
        If _draft.Count > 0 Then
            If MessageBox.Show(FindForm(), "Discard this stock-in? Nothing has been saved yet.", "DoughTrack",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
        End If
        _draft.Clear()
        RenderLines()
        _message.Text = ""
        _date.Value = Date.Today
    End Sub

    Private Sub ShowRecentLines(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 OrElse Not _recent.Columns.Contains("ID") Then Return
        Try
            Dim stockInId As Integer = Convert.ToInt32(_recent.Rows(e.RowIndex).Cells("ID").Value)
            Dim rows As DataTable = StockInRepository.GetLines(stockInId)
            Dim text As New System.Text.StringBuilder()
            text.AppendLine("Stock-In #" & stockInId.ToString("000000"))
            text.AppendLine()
            For Each row As DataRow In rows.Rows
                text.AppendLine(
                    CStr(row("Item")) & "  +" &
                    Convert.ToDecimal(row("Quantity")).ToString("#,##0.###") & " " & CStr(row("Unit")) &
                    "  @ " & UiTheme.FormatMoney(Convert.ToDecimal(row("Unit cost"))) &
                    "  = " & UiTheme.FormatMoney(Convert.ToDecimal(row("Subtotal"))))
            Next
            MessageBox.Show(FindForm(), text.ToString(), "Stock-In details", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            Debug.WriteLine("STOCK-IN DETAILS ERROR: " & ex.ToString())
            MessageBox.Show(FindForm(), "Unable to load this stock-in.", "DoughTrack",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ------------------------------------------------------------ small builders

    Private Shared Function SmallLabel(caption As String) As Label
        Return New Label With {
            .Text = caption,
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.BottomLeft,
            .ForeColor = UiTheme.TextMuted
        }
    End Function

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

End Class
