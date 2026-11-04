Imports DoughTrack.Data
Imports DoughTrack.Helpers
Imports DoughTrack.Models

''' <summary>
''' One expense, read again from the expenses table when the window opens.
''' </summary>
Public Class frmExpenseDetails
    Inherits Form

    Private ReadOnly _expenseId As Integer
    Private ReadOnly _summary As Label
    Private _expense As Expense

    Public Sub New(expenseId As Integer)
        _expenseId = expenseId
        Font = New Font("Segoe UI", 10.0!)
        Text = "Expense Details"
        StartPosition = FormStartPosition.CenterParent
        MinimizeBox = False
        MaximizeBox = False
        FormBorderStyle = FormBorderStyle.FixedDialog
        ClientSize = New Size(520, 280)
        BackColor = UiTheme.Content

        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(16)
        }
        _summary = New Label With {
            .Dock = DockStyle.Fill,
            .Text = "Expense " & expenseId.ToString()
        }
        Dim closeButton As Button = UiTheme.MakeButton("Close")
        Dim editButton As Button = UiTheme.MakeButton("Edit")
        Dim buttons As New FlowLayoutPanel With {
            .Dock = DockStyle.Bottom,
            .Height = 46,
            .FlowDirection = FlowDirection.RightToLeft,
            .Padding = New Padding(0, 6, 0, 0)
        }
        buttons.Controls.Add(closeButton)
        buttons.Controls.Add(editButton)

        card.Controls.Add(buttons)
        card.Controls.Add(_summary)
        Controls.Add(card)

        CancelButton = closeButton
        AddHandler closeButton.Click, Sub() Close()
        AddHandler editButton.Click, AddressOf EditExpense
        LoadExpense()
    End Sub

    Private Sub LoadExpense()
        Try
            _expense = ExpenseRepository.GetById(_expenseId)
        Catch ex As Exception
            _summary.Text = ex.Message
            _expense = Nothing
            Return
        End Try
        If _expense Is Nothing Then
            _summary.Text = "This expense no longer exists."
            Return
        End If
        Dim description As String = _expense.Description
        If description = "" Then description = "—"
        _summary.Text =
            "Expense ID" & vbTab & _expense.ExpenseId.ToString() & vbCrLf & vbCrLf &
            "Date" & vbTab & vbTab & _expense.ExpenseDate.ToString("MMM d, yyyy") & vbCrLf & vbCrLf &
            "Category" & vbTab & _expense.Category & vbCrLf & vbCrLf &
            "Amount" & vbTab & vbTab & UiTheme.FormatMoney(_expense.Amount) & vbCrLf & vbCrLf &
            "Description" & vbTab & description
    End Sub

    Private Sub EditExpense(sender As Object, e As EventArgs)
        If _expense Is Nothing Then
            _summary.Text = "This expense no longer exists."
            Return
        End If
        Using editor As New frmExpenseEdit(_expense)
            If editor.ShowDialog(Me) = DialogResult.OK Then LoadExpense()
        End Using
    End Sub

End Class
