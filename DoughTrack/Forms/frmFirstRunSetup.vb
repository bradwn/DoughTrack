Imports DoughTrack.Data
Imports DoughTrack.Helpers

''' <summary>
''' Shown once, on a brand-new installation (users table empty).
''' The person installing DoughTrack chooses the Admin username and password.
''' No default password exists anywhere in the program.
''' </summary>
Public Class frmFirstRunSetup
    Inherits Form

    Private ReadOnly _username As TextBox
    Private ReadOnly _fullName As TextBox
    Private ReadOnly _password As TextBox
    Private ReadOnly _confirm As TextBox
    Private ReadOnly _message As Label

    ''' <summary>The username that was created (valid after DialogResult.OK).</summary>
    <System.ComponentModel.Browsable(False),
     System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)>
    Public Property CreatedUsername As String = ""

    Public Sub New()
        Font = New Font("Segoe UI", 10.0!)
        Text = "DoughTrack - First-time setup"
        StartPosition = FormStartPosition.CenterParent
        MinimizeBox = False
        MaximizeBox = False
        ControlBox = False          ' must use Create or Cancel
        FormBorderStyle = FormBorderStyle.FixedDialog
        ClientSize = New Size(520, 430)
        BackColor = UiTheme.Content

        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(16)
        }

        Dim intro As New Label With {
            .Dock = DockStyle.Top,
            .Height = 70,
            .Text = "Welcome to DoughTrack." & vbCrLf &
                    "No Admin account exists yet. Choose the username and password you will use to log in. " &
                    "Keep the password somewhere safe."
        }

        Dim fields As New TableLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 180,
            .ColumnCount = 2,
            .RowCount = 4
        }
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150))
        fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        For row As Integer = 0 To 3
            fields.RowStyles.Add(New RowStyle(SizeType.Absolute, 44))
        Next

        _username = New TextBox With {.MaxLength = 50, .Dock = DockStyle.Fill, .Text = "admin"}
        _fullName = New TextBox With {.MaxLength = 100, .Dock = DockStyle.Fill, .Text = "Doughtown Administrator"}
        _password = New TextBox With {.MaxLength = 100, .Dock = DockStyle.Fill, .UseSystemPasswordChar = True}
        _confirm = New TextBox With {.MaxLength = 100, .Dock = DockStyle.Fill, .UseSystemPasswordChar = True}

        fields.Controls.Add(FieldLabel("Username"), 0, 0)
        fields.Controls.Add(_username, 1, 0)
        fields.Controls.Add(FieldLabel("Full name"), 0, 1)
        fields.Controls.Add(_fullName, 1, 1)
        fields.Controls.Add(FieldLabel("Password"), 0, 2)
        fields.Controls.Add(_password, 1, 2)
        fields.Controls.Add(FieldLabel("Confirm password"), 0, 3)
        fields.Controls.Add(_confirm, 1, 3)

        _message = New Label With {.Dock = DockStyle.Bottom, .Height = 44, .ForeColor = UiTheme.Danger}
        Dim buttons As New FlowLayoutPanel With {
            .Dock = DockStyle.Bottom,
            .Height = 46,
            .FlowDirection = FlowDirection.RightToLeft,
            .Padding = New Padding(0, 6, 0, 0)
        }
        Dim createButton As Button = UiTheme.MakeButton("Create Admin")
        Dim cancelButton As Button = UiTheme.MakeButton("Cancel")
        buttons.Controls.Add(createButton)
        buttons.Controls.Add(cancelButton)

        card.Controls.Add(_message)
        card.Controls.Add(buttons)
        card.Controls.Add(fields)
        card.Controls.Add(intro)
        card.Controls.SetChildIndex(intro, 0)
        card.Controls.SetChildIndex(fields, 1)
        Controls.Add(card)

        AcceptButton = createButton
        CancelButton = cancelButton
        cancelButton.DialogResult = DialogResult.Cancel
        AddHandler createButton.Click, AddressOf CreateAdmin
    End Sub

    Private Sub CreateAdmin(sender As Object, e As EventArgs)
        _message.Text = ""

        Dim username As String = _username.Text.Trim()
        Dim fullName As String = _fullName.Text.Trim()
        Dim password As String = _password.Text

        If username.Length < 3 Then
            _message.Text = "Username must be at least 3 characters."
            Return
        End If
        If username.Contains(" ") Then
            _message.Text = "Username cannot contain spaces."
            Return
        End If
        If fullName = "" Then
            _message.Text = "Full name is required."
            Return
        End If
        If password.Length < 8 Then
            _message.Text = "Password must be at least 8 characters."
            Return
        End If
        If password <> _confirm.Text Then
            _message.Text = "The two passwords do not match."
            _confirm.Clear()
            _confirm.Focus()
            Return
        End If

        Try
            Dim hashed As String = PasswordHelper.HashPassword(password)
            If UserRepository.CreateFirstAdmin(username, hashed, fullName) Then
                CreatedUsername = username
                DialogResult = DialogResult.OK
                Close()
            Else
                _message.Text = "An account already exists. Close this window and log in."
            End If
        Catch ex As Exception
            Debug.WriteLine("FIRST-RUN SETUP ERROR: " & ex.ToString())
            _message.Text = "Unable to create the account. Please check the database connection."
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
