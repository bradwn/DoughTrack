Imports DoughTrack.Data
Imports DoughTrack.Helpers
Imports DoughTrack.Models

''' <summary>
''' Expense list. Rows come from the expenses table.
''' </summary>
Public Class ExpensesView
    Inherits UserControl

    Private ReadOnly _search As TextBox
    Private ReadOnly _category As ComboBox
    Private ReadOnly _fromDate As DateTimePicker
    Private ReadOnly _toDate As DateTimePicker
    Private ReadOnly _message As Label
    Private ReadOnly _grid As DataGridView
    Private _loadingFilters As Boolean

    Public Sub New()
        Dock = DockStyle.Fill
        BackColor = UiTheme.Content
        Padding = New Padding(16)

        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(16)
        }

        Dim tools As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 84
        }
        Dim searchRow As New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 42,
            .WrapContents = False,
            .FlowDirection = FlowDirection.LeftToRight
        }
        _search = New TextBox With {.Width = 220}
        _category = New ComboBox With {
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Width = 180
        }
        Dim searchButton As Button = UiTheme.MakeButton("Search")
        Dim addButton As Button = UiTheme.MakeButton("Add Expenses")
        searchRow.Controls.Add(FieldLabel("Search"))
        searchRow.Controls.Add(_search)
        searchRow.Controls.Add(searchButton)
        searchRow.Controls.Add(FieldLabel("Category"))
        searchRow.Controls.Add(_category)
        searchRow.Controls.Add(addButton)

        Dim dateRow As New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 42,
            .WrapContents = False,
            .FlowDirection = FlowDirection.LeftToRight
        }
        _fromDate = DateFilter()
        _toDate = DateFilter()
        dateRow.Controls.Add(FieldLabel("From"))
        dateRow.Controls.Add(_fromDate)
        dateRow.Controls.Add(FieldLabel("To"))
        dateRow.Controls.Add(_toDate)
        dateRow.Controls.Add(New Label With {
            .Text = "Check a date to limit the list.",
            .AutoSize = True,
            .ForeColor = UiTheme.TextMuted,
            .Padding = New Padding(8, 8, 0, 0)
        })
        tools.Controls.Add(dateRow)
        tools.Controls.Add(searchRow)

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
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        BuildColumns()

        card.Controls.Add(_grid)
        card.Controls.Add(_message)
        card.Controls.Add(tools)
        Controls.Add(card)

        AddHandler searchButton.Click, Sub() ReloadGrid()
        AddHandler addButton.Click, AddressOf AddExpense
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
        AddHandler _fromDate.ValueChanged, Sub() If Not _loadingFilters Then ReloadGrid()
        AddHandler _toDate.ValueChanged, Sub() If Not _loadingFilters Then ReloadGrid()
        AddHandler _fromDate.MouseUp, Sub() If Not _loadingFilters Then ReloadGrid()
        AddHandler _toDate.MouseUp, Sub() If Not _loadingFilters Then ReloadGrid()
        AddHandler _grid.CellContentClick, AddressOf GridClick
        AddHandler _grid.CellDoubleClick, AddressOf GridDoubleClick
        AddHandler _fromDate.HandleCreated, Sub() UncheckDate(_fromDate)
        AddHandler _toDate.HandleCreated, Sub() UncheckDate(_toDate)

        LoadCategories()
        ReloadGrid()
    End Sub

    Private Sub UncheckDate(picker As DateTimePicker)
        _loadingFilters = True
        picker.Checked = False
        _loadingFilters = False
    End Sub

    Private Sub BuildColumns()
        Dim idColumn As DataGridViewTextBoxColumn = DataColumn("ExpenseId", "Expense ID", 50)
        idColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        idColumn.Width = 100
        _grid.Columns.Add(idColumn)

        Dim dateColumn As DataGridViewTextBoxColumn = DataColumn("ExpenseDate", "Date", 90)
        dateColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        dateColumn.Width = 140
        _grid.Columns.Add(dateColumn)

        _grid.Columns.Add(DataColumn("Category", "Category", 90))

        Dim amountColumn As DataGridViewTextBoxColumn = DataColumn("Amount", "Amount", 70)
        amountColumn.DefaultCellStyle.Format = "₱#,##0.00"
        amountColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        amountColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        amountColumn.Width = 120
        _grid.Columns.Add(amountColumn)

        _grid.Columns.Add(DataColumn("Description", "Description", 180))
        _grid.Columns.Add(ActionColumn("View", "View"))
        _grid.Columns.Add(ActionColumn("Edit", "Edit"))
        _grid.Columns.Add(ActionColumn("Delete", "Delete"))
        _grid.Columns("View").HeaderText = "Actions"
        _grid.Columns("Edit").HeaderText = ""
        _grid.Columns("Delete").HeaderText = ""
    End Sub

    Private Sub LoadCategories()
        _loadingFilters = True
        Dim selected As String = ""
        If _category.SelectedIndex > 0 Then selected = CStr(_category.SelectedItem)
        _category.Items.Clear()
        _category.Items.Add("All categories")
        Try
            For Each category As String In ExpenseRepository.GetCategories()
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
        _grid.Rows.Clear()
        Dim fromDate As Date? = Nothing
        Dim toDate As Date? = Nothing
        If _fromDate.Checked Then fromDate = _fromDate.Value.Date
        If _toDate.Checked Then toDate = _toDate.Value.Date
        If fromDate.HasValue AndAlso toDate.HasValue AndAlso fromDate.Value > toDate.Value Then
            _message.ForeColor = UiTheme.Danger
            _message.Text = "The start date is after the end date."
            Return
        End If

        Dim category As String = ""
        If _category.SelectedIndex > 0 Then category = CStr(_category.SelectedItem)

        Dim expenses As List(Of Expense)
        Dim total As Integer
        Try
            expenses = ExpenseRepository.Search(_search.Text, category, fromDate, toDate)
            total = ExpenseRepository.CountAll()
        Catch ex As Exception
            _message.ForeColor = UiTheme.Danger
            _message.Text = ex.Message
            Return
        End Try

        Dim visibleTotal As Decimal = 0D
        For Each expense As Expense In expenses
            Dim description As String = expense.Description
            If description = "" Then description = "—"
            Dim rowIndex As Integer = _grid.Rows.Add(
                expense.ExpenseId,
                expense.ExpenseDate.ToString("MMM d, yyyy"),
                expense.Category,
                expense.Amount,
                description,
                "View",
                "Edit",
                "Delete")
            _grid.Rows(rowIndex).Tag = expense.ExpenseId
            visibleTotal += expense.Amount
        Next

        _message.ForeColor = UiTheme.TextMuted
        If total = 0 Then
            _message.Text = "No expenses yet."
        ElseIf expenses.Count = 0 Then
            _message.Text = "Showing 0 of " & total.ToString() & " expenses."
        Else
            _message.Text = "Showing 1–" & expenses.Count.ToString() & " of " & total.ToString() &
                If(total = 1, " expense", " expenses") &
                ". Visible total " & UiTheme.FormatMoney(visibleTotal) & "."
        End If
    End Sub

    Private Sub AddExpense(sender As Object, e As EventArgs)
        Dim created As New Expense With {.ExpenseDate = Date.Today}
        Using editor As New frmExpenseEdit(created)
            If editor.ShowDialog(FindForm()) = DialogResult.OK Then
                LoadCategories()
                ReloadGrid()
            End If
        End Using
    End Sub

    Private Sub GridClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return
        Dim expenseId As Integer = CInt(_grid.Rows(e.RowIndex).Tag)
        Dim columnName As String = _grid.Columns(e.ColumnIndex).Name
        If columnName = "View" Then
            OpenDetails(expenseId)
        ElseIf columnName = "Edit" Then
            EditExpense(expenseId)
        ElseIf columnName = "Delete" Then
            DeleteExpense(expenseId)
        End If
    End Sub

    Private Sub GridDoubleClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return
        Dim columnName As String = _grid.Columns(e.ColumnIndex).Name
        If columnName = "View" OrElse columnName = "Edit" OrElse columnName = "Delete" Then Return
        OpenDetails(CInt(_grid.Rows(e.RowIndex).Tag))
    End Sub

    Private Sub OpenDetails(expenseId As Integer)
        Using details As New frmExpenseDetails(expenseId)
            details.ShowDialog(FindForm())
        End Using
        LoadCategories()
        ReloadGrid()
    End Sub

    Private Sub EditExpense(expenseId As Integer)
        Dim expense As Expense
        Try
            expense = ExpenseRepository.GetById(expenseId)
        Catch ex As Exception
            _message.ForeColor = UiTheme.Danger
            _message.Text = ex.Message
            Return
        End Try
        If expense Is Nothing Then
            _message.ForeColor = UiTheme.Danger
            _message.Text = "That expense no longer exists."
            ReloadGrid()
            Return
        End If
        Using editor As New frmExpenseEdit(expense)
            If editor.ShowDialog(FindForm()) = DialogResult.OK Then
                LoadCategories()
                ReloadGrid()
            End If
        End Using
    End Sub

    Private Sub DeleteExpense(expenseId As Integer)
        Dim expense As Expense
        Try
            expense = ExpenseRepository.GetById(expenseId)
        Catch ex As Exception
            _message.ForeColor = UiTheme.Danger
            _message.Text = ex.Message
            Return
        End Try
        If expense Is Nothing Then
            _message.ForeColor = UiTheme.Danger
            _message.Text = "That expense no longer exists."
            ReloadGrid()
            Return
        End If

        Dim question As String =
            "Delete expense " & expense.ExpenseId.ToString() & " (" &
            expense.ExpenseDate.ToString("MMM d, yyyy") & ", " & expense.Category & ", " &
            UiTheme.FormatMoney(expense.Amount) & ")?" & vbCrLf &
            "This removes only this expense. Sales, inventory, and other records stay as they are."
        If MessageBox.Show(question, "DoughTrack", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
            Return
        End If

        Try
            ExpenseRepository.Delete(expense.ExpenseId)
            LoadCategories()
            ReloadGrid()
        Catch ex As Exception
            _message.ForeColor = UiTheme.Danger
            _message.Text = ex.Message
        End Try
    End Sub

    Private Shared Function DateFilter() As DateTimePicker
        Return New DateTimePicker With {
            .Format = DateTimePickerFormat.Short,
            .ShowCheckBox = True,
            .Checked = False,
            .Width = 140
        }
    End Function

    Private Shared Function FieldLabel(caption As String) As Label
        Return New Label With {
            .Text = caption,
            .AutoSize = True,
            .Padding = New Padding(8, 8, 6, 0)
        }
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
            .UseColumnTextForButtonValue = True,
            .AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            .Width = 90,
            .FlatStyle = FlatStyle.Flat
        }
    End Function

End Class
