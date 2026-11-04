Imports DoughTrack.Data
Imports DoughTrack.Helpers

''' <summary>
''' Inventory > Low Stock. Not a table: it is a live query of inventory_items
''' for Active items where quantity &lt;= reorder point (Out of Stock when quantity = 0).
''' </summary>
Public Class LowStockView
    Inherits UserControl

    Private ReadOnly _banner As Label
    Private ReadOnly _grid As DataGridView

    Public Sub New()
        Dock = DockStyle.Fill
        BackColor = UiTheme.Content
        Padding = New Padding(16)

        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = UiTheme.Card,
            .Padding = New Padding(16)
        }

        _banner = New Label With {
            .Dock = DockStyle.Top,
            .Height = 40,
            .Padding = New Padding(12, 0, 0, 0),
            .TextAlign = ContentAlignment.MiddleLeft,
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
        }
        Dim refreshButton As Button = UiTheme.MakeButton("Refresh")
        Dim top As New Panel With {.Dock = DockStyle.Top, .Height = 48}
        refreshButton.Dock = DockStyle.Right
        top.Controls.Add(_banner)
        top.Controls.Add(refreshButton)

        _grid = New DataGridView With {.Dock = DockStyle.Fill}
        UiTheme.StyleGrid(_grid)

        card.Controls.Add(_grid)
        card.Controls.Add(top)
        Controls.Add(card)

        AddHandler refreshButton.Click, Sub() LoadRows()
        UiTheme.StyleInputs(Me)
        LoadRows()
    End Sub

    Private Sub LoadRows()
        Try
            Dim rows As DataTable = InventoryItemRepository.GetLowStock()
            _grid.DataSource = rows
            UiTheme.ApplyColumnFormats(_grid, Array.Empty(Of String)(), {"On hand", "Reorder point"})

            If rows.Rows.Count = 0 Then
                _banner.Text = "All active items are above their reorder points."
                _banner.ForeColor = UiTheme.Success
                _banner.BackColor = UiTheme.SuccessWash
            Else
                _banner.Text = rows.Rows.Count.ToString() &
                    If(rows.Rows.Count = 1, " item needs", " items need") & " attention (low or out of stock)."
                _banner.ForeColor = UiTheme.Danger
                _banner.BackColor = UiTheme.DangerWash
            End If

            For Each gridRow As DataGridViewRow In _grid.Rows
                Dim statusText As String = Convert.ToString(gridRow.Cells("Status").Value)
                gridRow.Cells("Status").Style.ForeColor = StockFormat.StatusColor(statusText)
                gridRow.Cells("Status").Style.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
            Next
        Catch ex As Exception
            Debug.WriteLine("LOW STOCK ERROR: " & ex.ToString())
            _grid.DataSource = Nothing
            _banner.Text = "Unable to load low stock items. Please check the database connection."
            _banner.ForeColor = UiTheme.Danger
            _banner.BackColor = Color.FromArgb(252, 228, 224)
        End Try
    End Sub

End Class
