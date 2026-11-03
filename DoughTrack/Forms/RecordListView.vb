Imports DoughTrack.Helpers

''' <summary>
''' Shared list layout for module screens. Rows come from the database.
''' </summary>
Public Class RecordListView
    Inherits UserControl

    Private ReadOnly _statusLabel As Label
    Private ReadOnly _grid As DataGridView

    Public Sub New(subtitle As String, rows As DataTable, moneyColumns As String(), quantityColumns As String())
        Dock = DockStyle.Fill
        BackColor = UiTheme.Content
        Padding = New Padding(16)

        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(16)
        }

        Dim subtitleLabel As New Label With {
            .Dock = DockStyle.Top,
            .Height = 28,
            .Text = subtitle,
            .ForeColor = UiTheme.TextMuted,
            .TextAlign = ContentAlignment.MiddleLeft
        }

        _statusLabel = New Label With {
            .Dock = DockStyle.Top,
            .Height = 24,
            .ForeColor = UiTheme.TextMuted,
            .TextAlign = ContentAlignment.MiddleLeft
        }

        _grid = New DataGridView With {.Dock = DockStyle.Fill}
        UiTheme.StyleGrid(_grid)

        card.Controls.Add(_grid)
        card.Controls.Add(_statusLabel)
        card.Controls.Add(subtitleLabel)
        Controls.Add(card)

        Bind(rows, moneyColumns, quantityColumns)
    End Sub

    Private Sub Bind(rows As DataTable, moneyColumns As String(), quantityColumns As String())
        _grid.DataSource = rows
        UiTheme.ApplyColumnFormats(_grid, moneyColumns, quantityColumns)
        If rows.Rows.Count = 0 Then
            _statusLabel.Text = "No records yet."
        Else
            _statusLabel.Text = rows.Rows.Count.ToString() & If(rows.Rows.Count = 1, " record", " records")
        End If
    End Sub

    Public Sub ShowError(message As String)
        _statusLabel.Text = message
        _statusLabel.ForeColor = UiTheme.Danger
        _grid.DataSource = Nothing
    End Sub

End Class
