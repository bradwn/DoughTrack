Public Class MetricCard
    Inherits Panel

    Private ReadOnly _titleLabel As Label
    Private ReadOnly _valueLabel As Label
    Private ReadOnly _noteLabel As Label

    Public Sub New(title As String, note As String)
        BackColor = Color.White
        Margin = New Padding(8)
        DoubleBuffered = True

        Dim accent As New Panel With {
            .BackColor = Helpers.UiTheme.Accent,
            .Height = 4,
            .Dock = DockStyle.Top
        }

        _titleLabel = New Label With {
            .Text = title,
            .ForeColor = Helpers.UiTheme.TextMuted,
            .Font = New Font("Segoe UI", 9.0!, FontStyle.Bold),
            .AutoSize = False
        }
        _valueLabel = New Label With {
            .Text = "—",
            .ForeColor = Helpers.UiTheme.TextDark,
            .Font = New Font("Segoe UI", 20.0!, FontStyle.Bold),
            .AutoSize = False
        }
        _noteLabel = New Label With {
            .Text = note,
            .ForeColor = Helpers.UiTheme.TextMuted,
            .Font = New Font("Segoe UI", 8.5!),
            .AutoSize = False
        }

        Controls.Add(_noteLabel)
        Controls.Add(_valueLabel)
        Controls.Add(_titleLabel)
        Controls.Add(accent)
    End Sub

    Public Sub SetValue(value As String)
        _valueLabel.Text = value
    End Sub

    Public Sub SetNote(note As String)
        _noteLabel.Text = note
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        If _titleLabel Is Nothing Then Return
        _titleLabel.SetBounds(16, 16, Math.Max(0, Width - 32), 20)
        _valueLabel.SetBounds(16, 40, Math.Max(0, Width - 32), 38)
        _noteLabel.SetBounds(16, Math.Max(82, Height - 40), Math.Max(0, Width - 32), 32)
    End Sub

End Class
