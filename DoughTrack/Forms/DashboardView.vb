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

        _errorLabel = New Label With {
            .Dock = DockStyle.Top,
            .Height = 0,
            .ForeColor = UiTheme.Danger,
            .Visible = False
        }

        _salesCard = New MetricCard("Today's Sales", "Completed sales today")
        _profitCard = New MetricCard("Est. Gross Profit", "Revenue minus cost of goods sold")
        _expenseCard = New MetricCard("Today's Expense", "Expenses dated today")

        _fastGrid = New DataGridView()
        _fastStatus = New Label()
        _lowGrid = New DataGridView()
        _lowStatus = New Label()

        Dim topRow As New TableLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 188,
            .ColumnCount = 3,
            .RowCount = 1,
            .BackColor = UiTheme.Content
        }
        topRow.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.33F))
        topRow.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.33F))
        topRow.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.34F))
        topRow.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        topRow.Controls.Add(_salesCard, 0, 0)
        topRow.Controls.Add(_expenseCard, 0, 0)
        topRow.Controls.Add(_profitCard, 2, 0)
        _salesCard.Dock = DockStyle.Fill
        _profitCard.Dock = DockStyle.Fill

        Dim bottomRow As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 1,
            .BackColor = UiTheme.Content,
            .Padding = New Padding(0, 4, 0, 0)
        }
        bottomRow.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        bottomRow.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
        bottomRow.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))

        bottomRow.Controls.Add(MakeListCard("Fast Moving Items", _fastGrid, _fastStatus), 0, 0)
        bottomRow.Controls.Add(MakeListCard("Low Stocks", _lowGrid, _lowStatus), 1, 0)
        _expenseCard.Dock = DockStyle.Fill

        Controls.Add(bottomRow)
        Controls.Add(topRow)
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
            For Each gridRow As DataGridViewRow In _lowGrid.Rows
                If _lowGrid.Columns.Contains("Status") Then
                    gridRow.Cells("Status").Style.ForeColor = UiTheme.Danger
                    gridRow.Cells("Status").Style.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
                End If
                If _lowGrid.Columns.Contains("On hand") Then
                    gridRow.Cells("On hand").Style.ForeColor = UiTheme.Danger
                End If
            Next
        Catch ex As Exception
            _lowStatus.Text = ex.Message
            _lowStatus.ForeColor = UiTheme.Danger
            ShowError(ex.Message)
        End Try
    End Sub

    Private Shared Function MakeListCard(title As String, grid As DataGridView, status As Label) As Panel
        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = UiTheme.Card,
            .Margin = New Padding(8)
        }
        Dim header As Panel = UiTheme.MakeSectionHeader(title)
        Dim body As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = UiTheme.Card,
            .Padding = New Padding(8, 4, 8, 8)
        }
        status.Dock = DockStyle.Top
        status.Height = 20
        status.ForeColor = UiTheme.TextMuted
        grid.Dock = DockStyle.Fill
        UiTheme.StyleGrid(grid)
        body.Controls.Add(grid)
        body.Controls.Add(status)
        card.Controls.Add(body)
        card.Controls.Add(header)
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

    Private Sub InitializeComponent()
        SuspendLayout()
        ' 
        ' DashboardView
        ' 
        Name = "DashboardView"
        ResumeLayout(False)

    End Sub

    Private Sub HideError()
        _errorLabel.Text = ""
        _errorLabel.Height = 0
        _errorLabel.Visible = False
    End Sub

    Private Sub DashboardView_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub
End Class
