Imports DoughTrack.Data
Imports DoughTrack.Helpers
Imports DoughTrack.Models

''' <summary>
''' Adds or edits one expense. Description may be left blank.
''' </summary>
Public Class frmExpenseEdit
    Inherits Form

    Private ReadOnly _expense As Expense
    Private ReadOnly _date As DateTimePicker
    Private ReadOnly _category As ComboBox
    Private ReadOnly _amount As NumericUpDown
    Private ReadOnly _description As TextBox
    Private ReadOnly _message As Label

    Public Sub New(expense As Expense)
        _expense = expense
        Font = New Font("Segoe UI", 10.0!)
        Text = If(expense.ExpenseId = 0, "Add Expenses", "Edit Expense")
        StartPosition = FormStartPosition.CenterParent
        MinimizeBox = False
        MaximizeBox = False
        FormBorderStyle = FormBorderStyle.FixedDialog
        ClientSize = New Size(560, 340)
        BackColor = UiTheme.Content

        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(16)
        }
        Dim fields As New TableLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 210,
            .ColumnCount = 2,
            .RowCount = 4
        }
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 110))
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        fields.RowStyles.Add(New RowStyle(SizeType.Absolute, 40))
        fields.RowStyles.Add(New RowStyle(SizeType.Absolute, 40))
        fields.RowStyles.Add(New RowStyle(SizeType.Absolute, 40))
        fields.RowStyles.Add(New RowStyle(SizeType.Absolute, 72))

        _date = New DateTimePicker With {
            .Format = DateTimePickerFormat.Short,
            .Width = 160,
            .ShowCheckBox = True
        }
        _category = New ComboBox With {
            .Dock = DockStyle.Fill,
            .DropDownStyle = ComboBoxStyle.DropDown,
            .MaxLength = 50
        }
        _amount = New NumericUpDown With {
            .DecimalPlaces = 2,
            .Minimum = 0D,
            .Maximum = 99999999.99D,
            .ThousandsSeparator = True,
            .Width = 160
        }
        _description = New TextBox With {
            .Dock = DockStyle.Fill,
            .MaxLength = 255,
            .Multiline = True
        }

        fields.Controls.Add(FieldLabel("Date"), 0, 0)
        fields.Controls.Add(_date, 1, 0)
        fields.Controls.Add(FieldLabel("Category"), 0, 1)
        fields.Controls.Add(_category, 1, 1)
        fields.Controls.Add(FieldLabel("Amount"), 0, 2)
        fields.Controls.Add(_amount, 1, 2)
        fields.Controls.Add(FieldLabel("Description"), 0, 3)
        fields.Controls.Add(_description, 1, 3)

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
        Dim saveButton As Button = UiTheme.MakeButton("Save Expense")
        Dim cancelButton As Button = UiTheme.MakeButton("Cancel")
        buttons.Controls.Add(saveButton)
        buttons.Controls.Add(cancelButton)

        card.Controls.Add(fields)
        card.Controls.Add(_message)
        card.Controls.Add(buttons)
        Controls.Add(card)

        AcceptButton = saveButton
        CancelButton = cancelButton
        AddHandler saveButton.Click, AddressOf SaveExpense
        AddHandler cancelButton.Click, Sub() Close()
        LoadFields()
    End Sub

    Private Sub LoadFields()
        Try
            For Each category As String In ExpenseRepository.GetCategoryChoices()
                _category.Items.Add(category)
            Next
        Catch ex As Exception
            _message.Text = ex.Message
        End Try
        If _expense.ExpenseDate = Date.MinValue Then
            _date.Value = Date.Today
            _date.Checked = False
        Else
            _date.Value = _expense.ExpenseDate
            _date.Checked = True
        End If
        _category.Text = If(_expense.Category, "")
        If _expense.Amount >= _amount.Minimum AndAlso _expense.Amount <= _amount.Maximum Then
            _amount.Value = _expense.Amount
        End If
        _description.Text = If(_expense.Description, "")
    End Sub

    Private Sub SaveExpense(sender As Object, e As EventArgs)
        _message.Text = ""
        If Not _date.Checked Then
            _message.Text = "Date is required."
            Return
        End If
        _expense.ExpenseDate = _date.Value.Date
        _expense.Category = _category.Text.Trim()
        _expense.Amount = _amount.Value
        _expense.Description = _description.Text.Trim()
        If _expense.Category = "" Then
            _message.Text = "Category is required."
            Return
        End If
        If _expense.Amount <= 0D Then
            _message.Text = "Amount must be greater than zero."
            Return
        End If
        Try
            ExpenseRepository.Save(_expense)
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

End Class
