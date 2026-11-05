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

    ' Runs once, right after the login window appears.
    Private Sub frmLogin_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        CheckFirstRun()
    End Sub

    ''' <summary>
    ''' If the users table is empty (new installation), ask the installer to create the Admin account.
    ''' If it already has users, nothing happens.
    ''' </summary>
    Private Sub CheckFirstRun()
        Try
            If UserRepository.CountUsers() > 0 Then Return
        Catch ex As Exception
            lblMessage.ForeColor = Color.Firebrick
            lblMessage.Text = "Unable to reach the database. Check that MySQL is running and appsettings.json is correct."
            Debug.WriteLine("FIRST-RUN CHECK ERROR: " & ex.ToString())
            Return
        End Try

        btnLogin.Enabled = False
        Using setup As New frmFirstRunSetup()
            If setup.ShowDialog(Me) = DialogResult.OK Then
                btnLogin.Enabled = True
                txtUsername.Text = setup.CreatedUsername
                lblMessage.ForeColor = Color.SeaGreen
                lblMessage.Text = "Admin account created. Please log in."
                txtPassword.Focus()
            Else
                lblMessage.ForeColor = Color.Firebrick
                lblMessage.Text = "No Admin account exists yet. Close DoughTrack and open it again to create one."
            End If
        End Using
    End Sub

    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        lblMessage.ForeColor = Color.Firebrick
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