Imports DoughTrack.Data

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

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles btnTestConnection.Click

    End Sub
End Class