Imports DoughTrack.Data
Imports DoughTrack.Helpers

''' <summary>
''' Settings page. Shows the logged-in username and lets the admin change the password.
''' Rules: current password must be correct, new password at least 8 characters,
''' confirmation must match, new password must differ from the current one.
''' </summary>
Public Class SettingsView
    Inherits UserControl

    Private Const MinPasswordLength As Integer = 8

    Private ReadOnly _username As TextBox
    Private ReadOnly _currentPassword As TextBox
    Private ReadOnly _newPassword As TextBox
    Private ReadOnly _confirmPassword As TextBox
    Private ReadOnly _message As Label

    Public Sub New()
        Font = New Font("Segoe UI", 10.0!)
        BackColor = UiTheme.Content
        Padding = New Padding(24)

        Dim card As New Panel With {
            .Location = New Point(24, 24),
            .Size = New Size(560, 430),
            .BackColor = Color.White,
            .Padding = New Padding(20)
        }

        Dim layout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 8
        }
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 170))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 44))
        For i As Integer = 1 To 4
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 46))
        Next
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 44))
        layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 46))

        Dim heading As New Label With {
            .Text = "Account Settings",
            .Font = New Font("Segoe UI", 13.0!, FontStyle.Bold),
            .ForeColor = UiTheme.TextDark,
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft
        }
        layout.Controls.Add(heading, 0, 0)
        layout.SetColumnSpan(heading, 2)

        _username = New TextBox With {
            .ReadOnly = True,
            .TabStop = False,
            .Anchor = AnchorStyles.Left Or AnchorStyles.Right
        }
        _currentPassword = MakePasswordBox()
        _newPassword = MakePasswordBox()
        _confirmPassword = MakePasswordBox()

        layout.Controls.Add(FieldLabel("Username"), 0, 1)
        layout.Controls.Add(_username, 1, 1)
        layout.Controls.Add(FieldLabel("Current password *"), 0, 2)
        layout.Controls.Add(_currentPassword, 1, 2)
        layout.Controls.Add(FieldLabel("New password *"), 0, 3)
        layout.Controls.Add(_newPassword, 1, 3)
        layout.Controls.Add(FieldLabel("Confirm new password *"), 0, 4)
        layout.Controls.Add(_confirmPassword, 1, 4)

        Dim hint As New Label With {
            .Text = "Use at least " & MinPasswordLength.ToString() & " characters.",
            .ForeColor = UiTheme.TextMuted,
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft
        }
        layout.Controls.Add(hint, 1, 5)

        _message = New Label With {
            .Dock = DockStyle.Fill,
            .ForeColor = UiTheme.Danger,
            .TextAlign = ContentAlignment.MiddleLeft
        }
        layout.Controls.Add(_message, 0, 6)
        layout.SetColumnSpan(_message, 2)

        Dim saveButton As Button = UiTheme.MakeButton("Change Password")
        saveButton.Anchor = AnchorStyles.Left
        layout.Controls.Add(saveButton, 1, 7)
        AddHandler saveButton.Click, AddressOf SavePassword

        card.Controls.Add(layout)
        Controls.Add(card)

        If Session.IsLoggedIn Then
            _username.Text = Session.CurrentUser.Username
        End If
    End Sub

    Private Shared Function MakePasswordBox() As TextBox
        Return New TextBox With {
            .UseSystemPasswordChar = True,
            .MaxLength = 100,
            .Anchor = AnchorStyles.Left Or AnchorStyles.Right
        }
    End Function

    Private Shared Function FieldLabel(caption As String) As Label
        Return New Label With {
            .Text = caption,
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft,
            .ForeColor = UiTheme.TextDark
        }
    End Function

    Private Sub ShowMessage(text As String, isError As Boolean)
        _message.ForeColor = If(isError, UiTheme.Danger, Color.FromArgb(46, 125, 50))
        _message.Text = text
    End Sub

    Private Sub SavePassword(sender As Object, e As EventArgs)
        ShowMessage("", False)

        If Not Session.IsLoggedIn Then
            ShowMessage("You must be logged in to change the password.", True)
            Return
        End If

        Dim currentPw As String = _currentPassword.Text
        Dim newPw As String = _newPassword.Text

        If currentPw = "" Then
            ShowMessage("Enter your current password.", True)
            _currentPassword.Focus()
            Return
        End If
        If newPw.Length < MinPasswordLength Then
            ShowMessage("The new password must be at least " & MinPasswordLength.ToString() & " characters.", True)
            _newPassword.Focus()
            Return
        End If
        If newPw <> _confirmPassword.Text Then
            ShowMessage("The new password and the confirmation do not match.", True)
            _confirmPassword.Focus()
            Return
        End If
        If newPw = currentPw Then
            ShowMessage("The new password must be different from the current password.", True)
            _newPassword.Focus()
            Return
        End If

        Try
            Dim found = UserRepository.GetByUsername(Session.CurrentUser.Username)
            If found Is Nothing OrElse Not PasswordHelper.VerifyPassword(currentPw, found.PasswordHash) Then
                ShowMessage("The current password is incorrect.", True)
                _currentPassword.Clear()
                _currentPassword.Focus()
                Return
            End If

            Dim newHash As String = PasswordHelper.HashPassword(newPw)
            UserRepository.UpdatePasswordHash(found.UserId, newHash)
            Session.CurrentUser.PasswordHash = newHash

            _currentPassword.Clear()
            _newPassword.Clear()
            _confirmPassword.Clear()
            ShowMessage("Password changed. Use the new password the next time you log in.", False)
        Catch ex As InvalidOperationException
            ShowMessage(ex.Message, True)
        Catch ex As Exception
            Debug.WriteLine("CHANGE PASSWORD ERROR: " & ex.ToString())
            ShowMessage("Unable to change the password. Please check the database connection.", True)
        End Try
    End Sub

End Class
