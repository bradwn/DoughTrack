Imports DoughTrack.Data
Imports DoughTrack.Helpers

Public Class frmLogin

    Private Sub frmLogin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim logo As Image = UiTheme.LogoImage()
        If logo Is Nothing Then
            picLogo.Visible = False
            lblLogoFallback.Visible = True
            Return
        End If
        picLogo.Image = logo
        picLogo.Visible = True
        lblLogoFallback.Visible = False
    End Sub

    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        lblMessage.Text = ""

        Dim username As String = txtUsername.Text.Trim()
        Dim password As String = txtPassword.Text

        ' 1. Validate blank fields
        If username = "" OrElse password = "" Then
            lblMessage.Text = "Please enter your username and password."
            Return
        End If

        Try
            ' 2. Look up the user and check the password
            Dim foundUser = UserRepository.GetByUsername(username)

            If foundUser Is Nothing OrElse
               Not PasswordHelper.VerifyPassword(password, foundUser.PasswordHash) Then
                ' Same message for both cases, so nobody can guess which usernames exist.
                lblMessage.Text = "Incorrect username or password."
                txtPassword.Clear()
                txtPassword.Focus()
                Return
            End If

            ' 3. Success: remember who logged in and open the main window.
            Session.CurrentUser = foundUser
            Hide()
            Try
                Using mainForm As New frmMain()
                    mainForm.ShowDialog()
                End Using
            Finally
                txtPassword.Clear()
                lblMessage.Text = ""
                Show()
                Activate()
                txtPassword.Focus()
            End Try

        Catch ex As Exception
            ' Friendly message for the user...
            lblMessage.Text = "Unable to log in. Please check the database connection."
            ' ...technical details for you, shown in Visual Studio's Output window.
            Debug.WriteLine("LOGIN ERROR: " & ex.ToString())
        End Try
    End Sub

    Private Sub btnExit_Click(sender As Object, e As EventArgs) Handles btnExit.Click
        Application.Exit()
    End Sub

End Class