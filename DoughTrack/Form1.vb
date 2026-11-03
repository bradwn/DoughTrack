Imports DoughTrack.Data
Imports DoughTrack.Helpers

Public Class Form1

    Private Sub btnTestConnection_Click(sender As Object, e As EventArgs) _
        Handles btnTestConnection.Click

        Dim errorMessage As String = ""

        If DatabaseHelper.TestConnection(errorMessage) Then
            MessageBox.Show("Connected to doughtrack_db successfully!",
                            "DoughTrack", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            MessageBox.Show("Connection failed:" & vbCrLf & errorMessage,
                            "DoughTrack", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    ' TEMPORARY: creates the first Admin account. Delete after use.
    Private Sub btnCreateAdmin_Click(sender As Object, e As EventArgs) _
        Handles btnCreateAdmin.Click

        Try
            If UserRepository.GetByUsername("admin") IsNot Nothing Then
                MessageBox.Show("The admin account already exists.")
                Return
            End If

            ' Change this password to your own. Don't reuse a real password.
            Dim hashed As String = PasswordHelper.HashPassword("Darwin20")
            UserRepository.CreateUser("admin", hashed, "Doughtown Administrator")

            MessageBox.Show("Admin account created.")
        Catch ex As Exception
            MessageBox.Show("Failed: " & ex.Message)
        End Try
    End Sub

End Class