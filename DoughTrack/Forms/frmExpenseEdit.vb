Imports DoughTrack.Data
Imports DoughTrack.Helpers
Imports DoughTrack.Models

Public Enum ExpenseFormMode
    AddNew
    EditExisting
    ViewOnly
End Enum

''' <summary>
''' Adds, edits, or shows one expense.
''' Rules: date required, category required, amount greater than zero, description optional.
''' </summary>
Public Class frmExpenseEdit
    Inherits Form

    Private ReadOnly _mode As ExpenseFormMode
    Private ReadOnly _expense As Expense
    Private ReadOnly _date As DateTimePicker
    Private ReadOnly _category As ComboBox
    Private ReadOnly _amount As NumericUpDown
    Private ReadOnly _description As TextBox
    Private ReadOnly _message As Label

    Public Sub New(mode As ExpenseFormMode, expense As Expense)
        _mode = mode
        _expense = expense
        Font = New Font("Segoe UI", 10.0!)
        Select Case mode
            Case ExpenseFormMode.AddNew
                Text = "Add Expense"
            Case ExpenseFormMode.EditExisting
                Text = "Edit Expense"
            Case Else
                Text = "Expense Details"
        End Select
        StartPosition = FormStartPosition.CenterParent
        MinimizeBox = False
        MaximizeBox = False
        FormBorderStyle = FormBorderStyle.FixedDialog
        ClientSize = New Size(500, 380)
        BackColor = UiTheme.Content

        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = UiTheme.Card,
            .Padding = New Padding(16)
        }

        Dim fields As New TableLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 230,
            .ColumnCount = 2,
            .RowCount = 4
        }
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110))
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        fields.RowStyles.Add(New RowStyle(SizeType.Absolute, 40))
        fields.RowStyles.Add(New RowStyle(SizeType.Absolute, 40))
        fields.RowStyles.Add(New RowStyle(SizeType.Absolute, 40))
        fields.RowStyles.Add(New RowStyle(SizeType.Absolute, 100))

        _date = New DateTimePicker With {.Format = DateTimePickerFormat.Short, .Width = 150, .Dock = DockStyle.Left}
        _category = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDown, .MaxLength = 50, .Dock = DockStyle.Fill}
        _amount = New NumericUpDown With {
            .DecimalPlaces = 2, .Minimum = 0D, .Maximum = 99999999.99D,
            .ThousandsSeparator = True, .Width = 150, .Dock = DockStyle.Left
        }
        _description = New TextBox With {.Multiline = True, .MaxLength = 255, .Dock = DockStyle.Fill,
                                         .ScrollBars = ScrollBars.Vertical}

        fields.Controls.Add(FieldLabel("Date *"), 0, 0)
        fields.Controls.Add(_date, 1, 0)
        fields.Controls.Add(FieldLabel("Category *"), 0, 1)
        fields.Controls.Add(_category, 1, 1)
        fields.Controls.Add(FieldLabel("Amount *"), 0, 2)
        fields.Controls.Add(_amount, 1, 2)
        fields.Controls.Add(FieldLabel("Description"), 0, 3)
        fields.Controls.Add(_description, 1, 3)

        _message = New Label With {.Dock = DockStyle.Bottom, .Height = 40, .ForeColor = UiTheme.Danger}
        Dim buttons As New FlowLayoutPanel With {
            .Dock = DockStyle.Bottom,
            .Height = 46,
            .FlowDirection = FlowDirection.RightToLeft,
            .Padding = New Padding(0, 6, 0, 0)
        }
        Dim cancelButton As Button = UiTheme.MakeButton(If(mode = ExpenseFormMode.ViewOnly, "Close", "Cancel"))
        Dim saveButton As Button = Nothing
        If mode <> ExpenseFormMode.ViewOnly Then
            saveButton = UiTheme.MakeButton("Save Expense")
            buttons.Controls.Add(saveButton)
        End If
        buttons.Controls.Add(cancelButton)

        card.Controls.Add(_message)
        card.Controls.Add(buttons)
        card.Controls.Add(fields)
        card.Controls.SetChildIndex(fields, 0)
        Controls.Add(card)

        CancelButton = cancelButton
        AddHandler cancelButton.Click, Sub() Close()
        If saveButton IsNot Nothing Then
            AcceptButton = saveButton
            AddHandler saveButton.Click, AddressOf SaveExpense
        End If

        LoadFields()
    End Sub

    Private Sub LoadFields()
        Try
            For Each category As String In ExpenseRepository.GetCategories()
                _category.Items.Add(category)
            Next
        Catch ex As Exception
            Debug.WriteLine("EXPENSE CATEGORY ERROR: " & ex.ToString())
        End Try
        ' Starter suggestions for a new database. The admin can still type any category.
        For Each suggestion As String In {"Rent", "Utilities", "Supplies", "Transportation", "Maintenance", "Marketing", "Miscellaneous"}
            If Not _category.Items.Contains(suggestion) Then _category.Items.Add(suggestion)
        Next

        If _mode = ExpenseFormMode.AddNew Then
            _date.Value = Date.Today
        Else
            _date.Value = _expense.ExpenseDate
            _category.Text = If(_expense.Category, "")
            If _expense.Amount >= _amount.Minimum AndAlso _expense.Amount <= _amount.Maximum Then
                _amount.Value = _expense.Amount
            End If
            _description.Text = If(_expense.Description, "")
        End If

        If _mode = ExpenseFormMode.ViewOnly Then
            _date.Enabled = False
            _category.Enabled = False
            _amount.Enabled = False
            _description.ReadOnly = True
        End If
    End Sub

    Private Sub SaveExpense(sender As Object, e As EventArgs)
        _message.Text = ""
        Dim target As Expense = If(_mode = ExpenseFormMode.AddNew, New Expense(), _expense)
        target.ExpenseDate = _date.Value.Date
        target.Category = _category.Text.Trim()
        target.Amount = _amount.Value
        target.Description = _description.Text.Trim()

        Try
            If _mode = ExpenseFormMode.AddNew Then
                ExpenseRepository.Add(target)
            Else
                ExpenseRepository.Update(target)
            End If
            DialogResult = DialogResult.OK
            Close()
        Catch ex As InvalidOperationException
            _message.Text = ex.Message
        Catch ex As Exception
            Debug.WriteLine("EXPENSE SAVE ERROR: " & ex.ToString())
            _message.Text = "Unable to save this expense. Please check the database connection."
        End Try
    End Sub

    Private Shared Function FieldLabel(caption As String) As Label
        Return New Label With {
            .Text = caption,
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft
        }
    End Function

End Class
