Imports DoughTrack.Helpers

''' <summary>
''' Small confirmation and message dialog styled like the rest of DoughTrack.
''' </summary>
Public Class frmNotice
    Inherits Form

    Public Sub New(message As String, isQuestion As Boolean, Optional title As String = "DoughTrack")
        Font = New Font("Segoe UI", 10.0!)
        Text = title
        FormBorderStyle = FormBorderStyle.FixedDialog
        StartPosition = FormStartPosition.CenterParent
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        Dim textHeight As Integer = Math.Min(280, Math.Max(72, 28 + (message.Length \ 46) * 18))
        ClientSize = New Size(460, textHeight + 108)
        BackColor = UiTheme.Content

        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = UiTheme.Card
        }
        Dim header As Panel = UiTheme.MakeSectionHeader(If(isQuestion, "Please confirm", title))
        Dim body As New Label With {
            .Dock = DockStyle.Fill,
            .Text = message,
            .ForeColor = UiTheme.TextDark,
            .Padding = New Padding(16, 12, 16, 8)
        }
        Dim buttons As New FlowLayoutPanel With {
            .Dock = DockStyle.Bottom,
            .Height = 52,
            .FlowDirection = FlowDirection.RightToLeft,
            .Padding = New Padding(12, 8, 12, 8),
            .BackColor = UiTheme.Card
        }
        If isQuestion Then
            Dim yesButton As Button = UiTheme.MakePrimaryButton("Yes")
            Dim noButton As Button = UiTheme.MakeSecondaryButton("No")
            yesButton.DialogResult = DialogResult.Yes
            noButton.DialogResult = DialogResult.No
            buttons.Controls.Add(yesButton)
            buttons.Controls.Add(noButton)
            AcceptButton = yesButton
            CancelButton = noButton
        Else
            Dim okButton As Button = UiTheme.MakePrimaryButton("OK")
            okButton.DialogResult = DialogResult.OK
            buttons.Controls.Add(okButton)
            AcceptButton = okButton
            CancelButton = okButton
        End If

        card.Controls.Add(body)
        card.Controls.Add(buttons)
        card.Controls.Add(header)
        Controls.Add(card)
    End Sub

End Class
