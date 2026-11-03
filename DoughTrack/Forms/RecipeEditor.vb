Imports DoughTrack.Data
Imports DoughTrack.Helpers
Imports DoughTrack.Models

''' <summary>
''' Adds, edits, and removes recipe lines. Immediate mode writes each change.
''' Buffered mode keeps the lines until the menu item is saved.
''' </summary>
Public Class RecipeEditor
    Inherits UserControl

    Private ReadOnly _inventory As ComboBox
    Private ReadOnly _quantity As NumericUpDown
    Private ReadOnly _unitLabel As Label
    Private ReadOnly _message As Label
    Private ReadOnly _grid As DataGridView
    Private ReadOnly _addButton As Button
    Private ReadOnly _updateButton As Button
    Private ReadOnly _removeButton As Button
    Private _menuItemId As Integer
    Private _immediate As Boolean

    Public Sub New()
        Dock = DockStyle.Fill
        BackColor = Color.White

        Dim tools As New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 44,
            .FlowDirection = FlowDirection.LeftToRight,
            .WrapContents = False,
            .Padding = New Padding(0, 4, 0, 4)
        }
        _inventory = New ComboBox With {
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Width = 280
        }
        _quantity = New NumericUpDown With {
            .DecimalPlaces = 3,
            .Minimum = 0.001D,
            .Maximum = 999999.999D,
            .Increment = 1D,
            .Value = 1D,
            .Width = 90,
            .ThousandsSeparator = True
        }
        _unitLabel = New Label With {
            .AutoSize = True,
            .Padding = New Padding(0, 8, 8, 0),
            .ForeColor = UiTheme.TextMuted,
            .Text = ""
        }
        _addButton = UiTheme.MakeButton("Add to recipe")
        _updateButton = UiTheme.MakeButton("Update quantity")
        _removeButton = UiTheme.MakeButton("Remove")
        tools.Controls.Add(_inventory)
        tools.Controls.Add(_quantity)
        tools.Controls.Add(_unitLabel)
        tools.Controls.Add(_addButton)
        tools.Controls.Add(_updateButton)
        tools.Controls.Add(_removeButton)

        _message = New Label With {
            .Dock = DockStyle.Top,
            .Height = 22,
            .ForeColor = UiTheme.Danger,
            .TextAlign = ContentAlignment.MiddleLeft
        }

        _grid = New DataGridView With {.Dock = DockStyle.Fill}
        UiTheme.StyleGrid(_grid)
        _grid.ReadOnly = True
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        _grid.Columns.Add(TextColumn("Item", 160))
        _grid.Columns.Add(TextColumn("Type", 80))
        Dim quantityColumn As DataGridViewTextBoxColumn = TextColumn("Quantity", 70)
        quantityColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        quantityColumn.DefaultCellStyle.Format = "#,##0.###"
        _grid.Columns.Add(quantityColumn)
        _grid.Columns.Add(TextColumn("Unit", 50))

        Controls.Add(tools)
        Controls.Add(_message)
        Controls.Add(_grid)
        Controls.SetChildIndex(_grid, 0)

        AddHandler _inventory.SelectedIndexChanged, AddressOf InventoryChanged
        AddHandler _addButton.Click, AddressOf AddLine
        AddHandler _updateButton.Click, AddressOf UpdateLine
        AddHandler _removeButton.Click, AddressOf RemoveLine
        AddHandler _grid.SelectionChanged, AddressOf GridSelectionChanged
        LoadInventory()
    End Sub

    Public Sub Configure(menuItemId As Integer, immediate As Boolean, lines As IList(Of RecipeLine))
        _menuItemId = menuItemId
        _immediate = immediate
        ShowLines(lines)
    End Sub

    Public Function CurrentLines() As List(Of RecipeLine)
        Dim lines As New List(Of RecipeLine)
        For Each row As DataGridViewRow In _grid.Rows
            Dim line As RecipeLine = TryCast(row.Tag, RecipeLine)
            If line IsNot Nothing Then lines.Add(CloneLine(line))
        Next
        Return lines
    End Function

    Private Sub LoadInventory()
        _inventory.Items.Clear()
        Try
            For Each item As InventoryItem In InventoryItemRepository.GetForRecipe()
                _inventory.Items.Add(New InventoryPick(item))
            Next
        Catch ex As Exception
            ShowError(ex.Message)
        End Try

        Dim hasItems As Boolean = _inventory.Items.Count > 0
        _addButton.Enabled = hasItems
        _inventory.Enabled = hasItems
        If hasItems Then
            _inventory.SelectedIndex = 0
        Else
            ShowError("No inventory records yet. Add ingredients or supplies before building a recipe.")
        End If
    End Sub

    Private Sub ShowLines(lines As IList(Of RecipeLine))
        _grid.Rows.Clear()
        If lines Is Nothing Then Return
        For Each line As RecipeLine In lines
            Dim rowIndex As Integer = _grid.Rows.Add(line.ItemName, line.InventoryCategory, line.QuantityRequired, line.Unit)
            _grid.Rows(rowIndex).Tag = CloneLine(line)
        Next
        If _grid.Rows.Count > 0 Then _grid.Rows(0).Selected = True
    End Sub

    Private Sub AddLine(sender As Object, e As EventArgs)
        ClearMessage()
        Dim pick As InventoryPick = TryCast(_inventory.SelectedItem, InventoryPick)
        If pick Is Nothing Then
            ShowError("Select an inventory item.")
            Return
        End If
        If ContainsItem(pick.Item.InventoryItemId) Then
            ShowError("That item is already on this recipe.")
            Return
        End If

        Dim line As New RecipeLine With {
            .MenuItemId = _menuItemId,
            .InventoryItemId = pick.Item.InventoryItemId,
            .ItemName = pick.Item.ItemName,
            .InventoryCategory = pick.Item.Category,
            .QuantityRequired = _quantity.Value,
            .Unit = pick.Item.Unit
        }

        Try
            If _immediate Then
                If _menuItemId <= 0 Then
                    ShowError("Save the menu item before adding a recipe.")
                    Return
                End If
                MenuItemIngredientRepository.AddLine(_menuItemId, line.InventoryItemId, line.QuantityRequired)
                ShowLines(MenuItemIngredientRepository.GetLines(_menuItemId))
            Else
                Dim rowIndex As Integer = _grid.Rows.Add(line.ItemName, line.InventoryCategory, line.QuantityRequired, line.Unit)
                _grid.Rows(rowIndex).Tag = line
                _grid.Rows(rowIndex).Selected = True
            End If
        Catch ex As Exception
            ShowError(ex.Message)
        End Try
    End Sub

    Private Sub UpdateLine(sender As Object, e As EventArgs)
        ClearMessage()
        Dim line As RecipeLine = SelectedLine()
        If line Is Nothing Then
            ShowError("Select a recipe line to update.")
            Return
        End If

        Try
            If _immediate Then
                MenuItemIngredientRepository.UpdateQuantity(_menuItemId, line.MenuItemIngredientId, _quantity.Value)
                ShowLines(MenuItemIngredientRepository.GetLines(_menuItemId))
            Else
                line.QuantityRequired = _quantity.Value
                Dim row As DataGridViewRow = _grid.SelectedRows(0)
                row.Cells("Quantity").Value = line.QuantityRequired
            End If
        Catch ex As Exception
            ShowError(ex.Message)
        End Try
    End Sub

    Private Sub RemoveLine(sender As Object, e As EventArgs)
        ClearMessage()
        Dim line As RecipeLine = SelectedLine()
        If line Is Nothing Then
            ShowError("Select a recipe line to remove.")
            Return
        End If
        Dim answer As DialogResult = MessageBox.Show(
            "Remove " & line.ItemName & " from this recipe?",
            "DoughTrack", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If answer <> DialogResult.Yes Then Return

        Try
            If _immediate Then
                MenuItemIngredientRepository.RemoveLine(_menuItemId, line.MenuItemIngredientId)
                ShowLines(MenuItemIngredientRepository.GetLines(_menuItemId))
            Else
                _grid.Rows.Remove(_grid.SelectedRows(0))
            End If
        Catch ex As Exception
            ShowError(ex.Message)
        End Try
    End Sub

    Private Sub InventoryChanged(sender As Object, e As EventArgs)
        Dim pick As InventoryPick = TryCast(_inventory.SelectedItem, InventoryPick)
        _unitLabel.Text = If(pick Is Nothing, "", pick.Item.Unit)
    End Sub

    Private Sub GridSelectionChanged(sender As Object, e As EventArgs)
        Dim line As RecipeLine = SelectedLine()
        If line Is Nothing Then Return
        Dim quantity As Decimal = line.QuantityRequired
        If quantity < _quantity.Minimum Then quantity = _quantity.Minimum
        If quantity > _quantity.Maximum Then quantity = _quantity.Maximum
        _quantity.Value = quantity
    End Sub

    Private Function SelectedLine() As RecipeLine
        If _grid.SelectedRows.Count = 0 Then Return Nothing
        Return TryCast(_grid.SelectedRows(0).Tag, RecipeLine)
    End Function

    Private Function ContainsItem(inventoryItemId As Integer) As Boolean
        For Each line As RecipeLine In CurrentLines()
            If line.InventoryItemId = inventoryItemId Then Return True
        Next
        Return False
    End Function

    Private Shared Function CloneLine(line As RecipeLine) As RecipeLine
        Return New RecipeLine With {
            .MenuItemIngredientId = line.MenuItemIngredientId,
            .MenuItemId = line.MenuItemId,
            .InventoryItemId = line.InventoryItemId,
            .ItemName = line.ItemName,
            .InventoryCategory = line.InventoryCategory,
            .QuantityRequired = line.QuantityRequired,
            .Unit = line.Unit
        }
    End Function

    Private Shared Function TextColumn(name As String, weight As Single) As DataGridViewTextBoxColumn
        Return New DataGridViewTextBoxColumn With {
            .Name = name,
            .HeaderText = name,
            .FillWeight = weight
        }
    End Function

    Private Sub ShowError(message As String)
        _message.Text = message
    End Sub

    Private Sub ClearMessage()
        _message.Text = ""
    End Sub

    Private Class InventoryPick
        Public ReadOnly Item As InventoryItem

        Public Sub New(item As InventoryItem)
            Me.Item = item
        End Sub

        Public Overrides Function ToString() As String
            Dim state As String = If(Item.Status = "Active", "", " · inactive")
            Return Item.ItemName & " · " & Item.Category & " · " & Item.Unit & state
        End Function
    End Class

End Class
