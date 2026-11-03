Imports DoughTrack.Data
Imports DoughTrack.Helpers

''' <summary>
''' Daily dashboard. Every figure is queried from sales, expenses, or inventory.
''' </summary>
Public Class DashboardView
    Inherits UserControl

    Private ReadOnly _errorLabel As Label
    Private ReadOnly _salesCard As MetricCard
    Private ReadOnly _profitCard As MetricCard
    Private ReadOnly _expenseCard As MetricCard
    Private ReadOnly _fastGrid As DataGridView
    Private ReadOnly _fastStatus As Label
    Private ReadOnly _lowGrid As DataGridView
    Private ReadOnly _lowStatus As Label

    Public Sub New()
        Dock = DockStyle.Fill
        BackColor = UiTheme.Content
        Padding = New Padding(12)

        _errorLabel = New Label With {
            .Dock = DockStyle.Top,
            .Height = 0,
            .ForeColor = UiTheme.Danger,
            .Visible = False
        }

        _salesCard = New MetricCard("Today's Sale", "Completed sales today")
        _profitCard = New MetricCard("Estimated Gross Profit", "Today's revenue minus cost of goods sold")
        _expenseCard = New MetricCard("Today's Expense", "Expenses dated today")

        Dim metrics As New TableLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 156,
            .ColumnCount = 3,
            .RowCount = 1,
            .BackColor = UiTheme.Content
        }
        metrics.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.33F))
        metrics.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.33F))
        metrics.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.34F))
        metrics.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        metrics.Controls.Add(_salesCard, 0, 0)
        metrics.Controls.Add(_profitCard, 1, 0)
        metrics.Controls.Add(_expenseCard, 2, 0)
        _salesCard.Dock = DockStyle.Fill
        _profitCard.Dock = DockStyle.Fill
        _expenseCard.Dock = DockStyle.Fill

        _fastGrid = New DataGridView()
        _fastStatus = New Label()
        _lowGrid = New DataGridView()
        _lowStatus = New Label()

        Dim lists As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 1,
            .BackColor = UiTheme.Content,
            .Padding = New Padding(0, 4, 0, 0)
        }
        lists.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        lists.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        lists.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        lists.Controls.Add(MakeListCard("Fast Moving Items", "Ranked by quantity sold", _fastGrid, _fastStatus), 0, 0)
        lists.Controls.Add(MakeListCard("Low Stocks", "Active items at or below the reorder point", _lowGrid, _lowStatus), 1, 0)

        Controls.Add(lists)
        Controls.Add(metrics)
        Controls.Add(_errorLabel)
    End Sub

    Public Sub LoadFigures()
        HideError()
        Dim today As Date = Date.Today

        Try
            Dim sales = DashboardRepository.GetTodaysSales(today)
            _salesCard.SetValue(UiTheme.FormatMoney(sales.Total))
            _salesCard.SetNote(CountNote(sales.Count, "completed sale", "completed sales") & " today")
        Catch ex As Exception
            _salesCard.SetValue("—")
            _salesCard.SetNote("Could not read today's sales")
            ShowError(ex.Message)
        End Try

        Try
            Dim profit = DashboardRepository.GetTodaysGrossProfit(today)
            _profitCard.SetValue(UiTheme.FormatMoney(profit.Profit))
            _profitCard.SetNote("Revenue " & UiTheme.FormatMoney(profit.Revenue) &
                                " − COGS " & UiTheme.FormatMoney(profit.Cost))
        Catch ex As Exception
            _profitCard.SetValue("—")
            _profitCard.SetNote("Could not estimate gross profit")
            ShowError(ex.Message)
        End Try

        Try
            Dim expenses = DashboardRepository.GetTodaysExpenses(today)
            _expenseCard.SetValue(UiTheme.FormatMoney(expenses.Total))
            _expenseCard.SetNote(CountNote(expenses.Count, "expense", "expenses") & " dated today")
        Catch ex As Exception
            _expenseCard.SetValue("—")
            _expenseCard.SetNote("Could not read today's expenses")
            ShowError(ex.Message)
        End Try

        Try
            BindList(_fastGrid, _fastStatus, DashboardRepository.GetFastMovingItems(),
                     {"Revenue"}, {"Quantity sold"})
        Catch ex As Exception
            _fastStatus.Text = ex.Message
            _fastStatus.ForeColor = UiTheme.Danger
            ShowError(ex.Message)
        End Try

        Try
            BindList(_lowGrid, _lowStatus, InventoryItemRepository.GetLowStock(),
                     Array.Empty(Of String)(), {"On hand", "Reorder point"})
        Catch ex As Exception
            _lowStatus.Text = ex.Message
            _lowStatus.ForeColor = UiTheme.Danger
            ShowError(ex.Message)
        End Try
    End Sub

    Private Shared Function MakeListCard(title As String, subtitle As String, grid As DataGridView, status As Label) As Panel
        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Margin = New Padding(8),
            .Padding = New Padding(16, 12, 16, 12)
        }
        Dim titleLabel As New Label With {
            .Dock = DockStyle.Top,
            .Height = 26,
            .Text = title,
            .Font = New Font("Segoe UI", 12.0!, FontStyle.Bold),
            .ForeColor = UiTheme.TextDark
        }
        Dim subtitleLabel As New Label With {
            .Dock = DockStyle.Top,
            .Height = 22,
            .Text = subtitle,
            .ForeColor = UiTheme.TextMuted
        }
        status.Dock = DockStyle.Top
        status.Height = 22
        status.ForeColor = UiTheme.TextMuted
        grid.Dock = DockStyle.Fill
        UiTheme.StyleGrid(grid)

        card.Controls.Add(grid)
        card.Controls.Add(status)
        card.Controls.Add(subtitleLabel)
        card.Controls.Add(titleLabel)
        Return card
    End Function

    Private Shared Sub BindList(grid As DataGridView, status As Label, rows As DataTable, moneyColumns As String(), quantityColumns As String())
        grid.DataSource = rows
        UiTheme.ApplyColumnFormats(grid, moneyColumns, quantityColumns)
        status.ForeColor = UiTheme.TextMuted
        If rows.Rows.Count = 0 Then
            status.Text = "No records yet."
        Else
            status.Text = rows.Rows.Count.ToString() & If(rows.Rows.Count = 1, " record", " records")
        End If
    End Sub

    Private Shared Function CountNote(count As Integer, singular As String, plural As String) As String
        If count = 0 Then Return "No " & plural
        If count = 1 Then Return "1 " & singular
        Return count.ToString() & " " & plural
    End Function

    Private Sub ShowError(message As String)
        _errorLabel.Text = message
        _errorLabel.Height = 36
        _errorLabel.Visible = True
    End Sub

    Private Sub HideError()
        _errorLabel.Text = ""
        _errorLabel.Height = 0
        _errorLabel.Visible = False
    End Sub

End Class
