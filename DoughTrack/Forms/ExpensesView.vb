Imports DoughTrack.Data
Imports DoughTrack.Helpers
Imports DoughTrack.Models

''' <summary>
''' Expenses: search, category and date filters, add, view, edit, and delete.
''' The Dashboard's Today's Expense reads the same expenses table.
''' </summary>
Public Class ExpensesView
    Inherits UserControl

    Private ReadOnly _search As TextBox
    Private ReadOnly _category As ComboBox
    Private ReadOnly _from As DateTimePicker
    Private ReadOnly _to As DateTimePicker
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

        Dim tools As New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 88,
            .WrapContents = True,
            .FlowDirection = FlowDirection.LeftToRight
        }
        _search = New TextBox With {.Width = 200}
        _category = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Width = 170}
        ' ShowCheckBox: the date filter is off until the box is ticked.
        _from = New DateTimePicker With {.Format = DateTimePickerFormat.Short, .ShowCheckBox = True, .Checked = False, .Width = 130}
        _to = New DateTimePicker With {.Format = DateTimePickerFormat.Short, .ShowCheckBox = True, .Checked = False, .Width = 130}
        Dim searchButton As Button = UiTheme.MakeButton("Search")
        Dim clearButton As Button = UiTheme.MakeButton("Clear Filters")
        Dim addButton As Button = UiTheme.MakeButton("Add Expense")

        tools.Controls.Add(New Label With {.Text = "Search", .AutoSize = True, .Padding = New Padding(0, 8, 6, 0)})
        tools.Controls.Add(_search)
        tools.Controls.Add(searchButton)
        tools.Controls.Add(New Label With {.Text = "Category", .AutoSize = True, .Padding = New Padding(12, 8, 6, 0)})
        tools.Controls.Add(_category)
        tools.Controls.Add(addButton)
        tools.SetFlowBreak(addButton, True)
        tools.Controls.Add(New Label With {.Text = "From", .AutoSize = True, .Padding = New Padding(0, 8, 6, 0)})
        tools.Controls.Add(_from)
        tools.Controls.Add(New Label With {.Text = "To", .AutoSize = True, .Padding = New Padding(12, 8, 6, 0)})
        tools.Controls.Add(_to)
        tools.Controls.Add(clearButton)

        _message = New Label With {
            .Dock = DockStyle.Bottom,
            .Height = 26,
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
        AddHandler addButton.Click, AddressOf AddExpense
        AddHandler clearButton.Click, AddressOf ClearFilters
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
        AddHandler _from.ValueChanged, Sub() ReloadGrid()
        AddHandler _to.ValueChanged, Sub() ReloadGrid()
        AddHandler _grid.CellContentClick, AddressOf GridClick

        LoadCategories()
        ReloadGrid()
    End Sub

    Private Sub BuildColumns()
        _grid.Columns.Add(TextColumn("ExpenseId", "ID", 40, False))
        _grid.Columns.Add(TextColumn("ExpenseDate", "Date", 80, False))
        _grid.Columns.Add(TextColumn("Category", "Category", 90, False))
        _grid.Columns.Add(TextColumn("Amount", "Amount", 80, True))
        _grid.Columns.Add(TextColumn("Description", "Description", 160, False))
        _grid.Columns.Add(ActionColumn("View", "View", "Actions"))
        _grid.Columns.Add(ActionColumn("Edit", "Edit", ""))
        _grid.Columns.Add(ActionColumn("Delete", "Delete", ""))
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
        Catch ex As Exception
            Debug.WriteLine("EXPENSE FILTER ERROR: " & ex.ToString())
        End Try
        Dim index As Integer = _category.Items.IndexOf(selected)
        _category.SelectedIndex = If(index >= 0, index, 0)
        _loadingFilters = False
    End Sub

    Private Sub ReloadGrid()
        _grid.Rows.Clear()

        Dim category As String = ""
        If _category.SelectedIndex > 0 Then category = CStr(_category.SelectedItem)
        Dim fromDate As Date? = If(_from.Checked, CType(_from.Value.Date, Date?), Nothing)
        Dim toDate As Date? = If(_to.Checked, CType(_to.Value.Date, Date?), Nothing)

        If fromDate.HasValue AndAlso toDate.HasValue AndAlso fromDate.Value > toDate.Value Then
            _message.ForeColor = UiTheme.Danger
            _message.Text = "The From date must not be after the To date."
            Return
        End If

        Dim expenses As List(Of Expense)
        Try
            expenses = ExpenseRepository.Search(_search.Text, category, fromDate, toDate)
        Catch ex As Exception
            Debug.WriteLine("EXPENSE LOAD ERROR: " & ex.ToString())
            _message.ForeColor = UiTheme.Danger
            _message.Text = "Unable to load expenses. Please check the database connection."
            Return
        End Try

        Dim total As Decimal = 0D
        For Each expense As Expense In expenses
            Dim rowIndex As Integer = _grid.Rows.Add(
                expense.ExpenseId,
                expense.ExpenseDate.ToString("MMM d, yyyy"),
                expense.Category,
                UiTheme.FormatMoney(expense.Amount),
                expense.Description,
                "View", "Edit", "Delete")
            _grid.Rows(rowIndex).Tag = expense
            total += expense.Amount
        Next

        _message.ForeColor = UiTheme.TextMuted
        If expenses.Count = 0 Then
            _message.Text = "No expenses match this search."
        Else
            _message.Text = expenses.Count.ToString() & If(expenses.Count = 1, " expense", " expenses") &
                            "  |  Total " & UiTheme.FormatMoney(total)
        End If
    End Sub

    Private Sub ClearFilters(sender As Object, e As EventArgs)
        _search.Clear()
        _from.Checked = False
        _to.Checked = False
        _loadingFilters = True
        _category.SelectedIndex = 0
        _loadingFilters = False
        ReloadGrid()
    End Sub

    Private Sub AddExpense(sender As Object, e As EventArgs)
        Using editor As New frmExpenseEdit(ExpenseFormMode.AddNew, New Expense())
            If editor.ShowDialog(FindForm()) = DialogResult.OK Then
                LoadCategories()
                ReloadGrid()
            End If
        End Using
    End Sub

    Private Sub GridClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 Then Return
        Dim expense As Expense = TryCast(_grid.Rows(e.RowIndex).Tag, Expense)
        If expense Is Nothing Then Return
        Dim columnName As String = _grid.Columns(e.ColumnIndex).Name

        If columnName = "View" Then
            Using details As New frmExpenseEdit(ExpenseFormMode.ViewOnly, expense)
                details.ShowDialog(FindForm())
            End Using
        ElseIf columnName = "Edit" Then
            ' Edit a copy so a cancelled dialog leaves the row unchanged.
            Dim copy As New Expense With {
                .ExpenseId = expense.ExpenseId,
                .ExpenseDate = expense.ExpenseDate,
                .Category = expense.Category,
                .Amount = expense.Amount,
                .Description = expense.Description
            }
            Using editor As New frmExpenseEdit(ExpenseFormMode.EditExisting, copy)
                If editor.ShowDialog(FindForm()) = DialogResult.OK Then
                    LoadCategories()
                    ReloadGrid()
                End If
            End Using
        ElseIf columnName = "Delete" Then
            DeleteExpense(expense)
        End If
    End Sub

    Private Sub DeleteExpense(expense As Expense)
        Dim question As String =
            "Delete this expense?" & vbCrLf & vbCrLf &
            expense.ExpenseDate.ToString("MMM d, yyyy") & "  |  " & expense.Category & "  |  " &
            UiTheme.FormatMoney(expense.Amount) & vbCrLf & vbCrLf &
            "This cannot be undone."
        If MessageBox.Show(FindForm(), question, "DoughTrack", MessageBoxButtons.YesNo,
                           MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
        Try
            ExpenseRepository.Delete(expense.ExpenseId)
            LoadCategories()
            ReloadGrid()
        Catch ex As InvalidOperationException
            MessageBox.Show(FindForm(), ex.Message, "DoughTrack", MessageBoxButtons.OK, MessageBoxIcon.Information)
            ReloadGrid()
        Catch ex As Exception
            Debug.WriteLine("EXPENSE DELETE ERROR: " & ex.ToString())
            MessageBox.Show(FindForm(), "Unable to delete this expense.", "DoughTrack",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Shared Function TextColumn(name As String, header As String, weight As Single, rightAligned As Boolean) As DataGridViewTextBoxColumn
        Dim column As New DataGridViewTextBoxColumn With {
            .Name = name, .HeaderText = header, .ReadOnly = True, .FillWeight = weight
        }
        If rightAligned Then column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        Return column
    End Function

    Private Shared Function ActionColumn(name As String, text As String, header As String) As DataGridViewButtonColumn
        Return New DataGridViewButtonColumn With {
            .Name = name,
            .HeaderText = header,
            .Text = text,
            .UseColumnTextForButtonValue = False,
            .AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            .Width = 80,
            .FlatStyle = FlatStyle.Flat
        }
    End Function

End Class
