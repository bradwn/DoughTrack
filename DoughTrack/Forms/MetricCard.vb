Public Class MetricCard
    Inherits Panel

    Private ReadOnly _header As Panel
    Private ReadOnly _titleLabel As Label
    Private ReadOnly _valueLabel As Label
    Private ReadOnly _noteLabel As Label

    Public Sub New(title As String, note As String)
        BackColor = Helpers.UiTheme.Card
        Margin = New Padding(8)
        DoubleBuffered = True

        _header = New Panel With {
            .BackColor = Helpers.UiTheme.CardHeader,
            .Dock = DockStyle.Top,
            .Height = 36
        }
        _titleLabel = New Label With {
            .Text = title,
            .ForeColor = Color.White,
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Padding = New Padding(12, 0, 8, 0)
        }
        _header.Controls.Add(_titleLabel)

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
        Controls.Add(_header)
    End Sub

    Public Sub SetValue(value As String)
        _valueLabel.Text = value
    End Sub

    Public Sub SetNote(note As String)
        _noteLabel.Text = note
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        If _valueLabel Is Nothing Then Return
        Dim top As Integer = 48
        _valueLabel.SetBounds(16, top, Math.Max(0, Width - 32), 38)
        _noteLabel.SetBounds(16, top + 40, Math.Max(0, Width - 32), 36)
    End Sub

End Class
