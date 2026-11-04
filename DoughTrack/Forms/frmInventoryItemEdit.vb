Imports DoughTrack.Data
Imports DoughTrack.Helpers
Imports DoughTrack.Models

''' <summary>
''' Adds or edits one inventory item (an ingredient or a supply).
''' </summary>
Public Class frmInventoryItemEdit
    Inherits Form

    Private ReadOnly _item As InventoryItem
    Private ReadOnly _name As TextBox
    Private ReadOnly _category As ComboBox
    Private ReadOnly _unit As ComboBox
    Private ReadOnly _quantity As NumericUpDown
    Private ReadOnly _reorder As NumericUpDown
    Private ReadOnly _cost As NumericUpDown
    Private ReadOnly _status As ComboBox
    Private ReadOnly _hint As Label
    Private ReadOnly _message As Label

    Public Sub New(item As InventoryItem)
        _item = item
        Font = New Font("Segoe UI", 10.0!)
        Text = If(item.InventoryItemId = 0, "Add Inventory Item", "Edit Inventory Item")
        StartPosition = FormStartPosition.CenterParent
        MinimizeBox = False
        MaximizeBox = False
        FormBorderStyle = FormBorderStyle.FixedDialog
        ClientSize = New Size(500, 470)
        BackColor = UiTheme.Content

        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = UiTheme.Card,
            .Padding = New Padding(16)
        }

        Dim fields As New TableLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 330,
            .ColumnCount = 2,
            .RowCount = 8
        }
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150))
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        For row As Integer = 0 To 7
            fields.RowStyles.Add(New RowStyle(SizeType.Absolute, 40))
        Next

        _name = New TextBox With {.MaxLength = 100, .Dock = DockStyle.Fill}
        _category = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Left, .Width = 200}
        _category.Items.AddRange(New Object() {"Ingredients", "Supplies"})
        _unit = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Left, .Width = 120}
        _unit.Items.AddRange(New Object() {"kg", "g", "L", "ml", "pcs"})
        _quantity = MakeNumber(3, 999999999.999D)
        _reorder = MakeNumber(3, 999999999.999D)
        _cost = MakeNumber(4, 99999999.9999D)
        _status = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Left, .Width = 200}
        _status.Items.AddRange(New Object() {"Active", "Inactive"})

        _hint = New Label With {
            .Dock = DockStyle.Fill,
            .ForeColor = UiTheme.TextMuted,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Font = New Font("Segoe UI", 8.5!)
        }

        fields.Controls.Add(FieldLabel("Item name"), 0, 0)
        fields.Controls.Add(_name, 1, 0)
        fields.Controls.Add(FieldLabel("Category"), 0, 1)
        fields.Controls.Add(_category, 1, 1)
        fields.Controls.Add(FieldLabel("Unit"), 0, 2)
        fields.Controls.Add(_unit, 1, 2)
        fields.Controls.Add(_hint, 1, 3)
        fields.Controls.Add(FieldLabel("On hand quantity"), 0, 4)
        fields.Controls.Add(_quantity, 1, 4)
        fields.Controls.Add(FieldLabel("Reorder point"), 0, 5)
        fields.Controls.Add(_reorder, 1, 5)
        fields.Controls.Add(FieldLabel("Unit cost (per unit)"), 0, 6)
        fields.Controls.Add(_cost, 1, 6)
        fields.Controls.Add(FieldLabel("Status"), 0, 7)
        fields.Controls.Add(_status, 1, 7)

        _message = New Label With {
            .Dock = DockStyle.Bottom,
            .Height = 40,
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

        card.Controls.Add(_message)
        card.Controls.Add(buttons)
        card.Controls.Add(fields)
        card.Controls.SetChildIndex(fields, 0)
        Controls.Add(card)

        AcceptButton = saveButton
        CancelButton = cancelButton
        AddHandler saveButton.Click, AddressOf SaveItem
        AddHandler cancelButton.Click, Sub() Close()

        LoadFields()
    End Sub

    Private Sub LoadFields()
        _name.Text = If(_item.ItemName, "")
        _category.SelectedItem = If(_item.Category = "Supplies", "Supplies", "Ingredients")

        ' An old row may use a unit that is not in the list. Keep it so nothing is lost.
        Dim unitText As String = If(_item.Unit, "").Trim()
        If unitText <> "" AndAlso Not _unit.Items.Contains(unitText) Then
            _unit.Items.Add(unitText)
        End If
        If unitText <> "" Then
            _unit.SelectedItem = unitText
        Else
            _unit.SelectedItem = "pcs"
        End If

        SetValue(_quantity, _item.Quantity)
        SetValue(_reorder, _item.ReorderPoint)
        SetValue(_cost, _item.UnitCost)
        _status.SelectedItem = If(_item.Status = "Inactive", "Inactive", "Active")

        If _item.InventoryItemId > 0 Then
            Try
                If InventoryItemRepository.HasReferences(_item.InventoryItemId) Then
                    _unit.Enabled = False
                    _hint.Text = "Unit is locked: this item is used in a recipe or a stock-in."
                End If
            Catch ex As Exception
                Debug.WriteLine("INVENTORY EDIT ERROR: " & ex.ToString())
            End Try
        Else
            _hint.Text = "Pick the unit you count this item in (for example g for coffee beans)."
        End If
    End Sub

    Private Sub SaveItem(sender As Object, e As EventArgs)
        _message.Text = ""
        If _category.SelectedItem Is Nothing OrElse _unit.SelectedItem Is Nothing OrElse _status.SelectedItem Is Nothing Then
            _message.Text = "Choose a category, a unit, and a status."
            Return
        End If

        _item.ItemName = _name.Text.Trim()
        _item.Category = CStr(_category.SelectedItem)
        _item.Unit = CStr(_unit.SelectedItem)
        _item.Quantity = _quantity.Value
        _item.ReorderPoint = _reorder.Value
        _item.UnitCost = _cost.Value
        _item.Status = CStr(_status.SelectedItem)

        Try
            If _item.InventoryItemId = 0 Then
                InventoryItemRepository.Add(_item)
            Else
                InventoryItemRepository.Update(_item)
            End If
            DialogResult = DialogResult.OK
            Close()
        Catch ex As InvalidOperationException
            ' Our own validation messages are already written for the user.
            _message.Text = ex.Message
        Catch ex As Exception
            Debug.WriteLine("INVENTORY SAVE ERROR: " & ex.ToString())
            _message.Text = "Unable to save this item. Please check the database connection."
        End Try
    End Sub

    Private Shared Function MakeNumber(decimals As Integer, maximum As Decimal) As NumericUpDown
        Return New NumericUpDown With {
            .DecimalPlaces = decimals,
            .Minimum = 0D,
            .Maximum = maximum,
            .ThousandsSeparator = True,
            .Dock = DockStyle.Left,
            .Width = 200
        }
    End Function

    Private Shared Sub SetValue(box As NumericUpDown, value As Decimal)
        If value >= box.Minimum AndAlso value <= box.Maximum Then box.Value = value
    End Sub

    Private Shared Function FieldLabel(caption As String) As Label
        Return New Label With {
            .Text = caption,
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft
        }
    End Function

End Class
