Imports DoughTrack.Data
Imports DoughTrack.Helpers

''' <summary>
''' Main window. The sidebar stays the same for every module.
''' </summary>
Public Class frmMain
    Inherits Form

    Private ReadOnly _navFlow As FlowLayoutPanel
    Private ReadOnly _contentHost As Panel
    Private ReadOnly _titleLabel As Label
    Private ReadOnly _userLabel As Label
    Private ReadOnly _navButtons As New List(Of Button)
    Private _dashboardButton As Button

    Public Sub New()
        Font = New Font("Segoe UI", 10.0!)
        Text = "DoughTrack — Doughtown Cafe"
        StartPosition = FormStartPosition.CenterScreen
        MinimumSize = New Size(1100, 700)
        ClientSize = New Size(1200, 760)
        BackColor = UiTheme.Content

        Dim sidebar As New Panel With {
            .Dock = DockStyle.Left,
            .Width = 250,
            .BackColor = UiTheme.Sidebar
        }
        sidebar.Controls.Add(BuildBrand())
        sidebar.Controls.Add(BuildLogout())

        _navFlow = New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.TopDown,
            .WrapContents = False,
            .AutoScroll = True,
            .BackColor = UiTheme.Sidebar,
            .Padding = New Padding(12, 8, 12, 8)
        }
        sidebar.Controls.Add(_navFlow)
        ' Fill is docked after the top and bottom bars, so it keeps the space between them.
        sidebar.Controls.SetChildIndex(_navFlow, 0)

        BuildNavigation()
        AddHandler _navFlow.SizeChanged, Sub() SizeNavItems()

        Dim header As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 76,
            .BackColor = Color.White
        }
        Dim headerLine As New Panel With {
            .Dock = DockStyle.Bottom,
            .Height = 1,
            .BackColor = UiTheme.Line
        }
        _titleLabel = New Label With {
            .AutoSize = False,
            .Font = New Font("Segoe UI", 18.0!, FontStyle.Bold),
            .ForeColor = UiTheme.TextDark,
            .Text = "Dashboard"
        }
        _userLabel = New Label With {
            .AutoSize = False,
            .TextAlign = ContentAlignment.MiddleRight,
            .ForeColor = UiTheme.TextMuted
        }
        header.Controls.Add(headerLine)
        header.Controls.Add(_titleLabel)
        header.Controls.Add(_userLabel)
        AddHandler header.Resize, Sub() LayoutHeader()

        _contentHost = New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = UiTheme.Content
        }

        Dim workspace As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = UiTheme.Content
        }
        workspace.Controls.Add(_contentHost)
        workspace.Controls.Add(header)

        Controls.Add(workspace)
        Controls.Add(sidebar)
    End Sub

    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not Session.IsLoggedIn Then
            Close()
            Return
        End If
        _userLabel.Text = Session.CurrentUser.FullName
        LayoutHeader()
        SizeNavItems()
        ShowDashboard()
    End Sub

    Private Sub frmMain_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Session.Logout()
    End Sub

    Private Function BuildBrand() As Panel
        Dim brand As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 112,
            .BackColor = UiTheme.Sidebar
        }
        Dim accent As New Panel With {
            .BackColor = UiTheme.Accent
        }
        Dim nameLabel As New Label With {
            .Text = "Doughtown Cafe",
            .ForeColor = Color.White,
            .Font = New Font("Segoe UI", 16.0!, FontStyle.Bold),
            .AutoSize = False
        }
        Dim productLabel As New Label With {
            .Text = "DoughTrack",
            .ForeColor = UiTheme.Accent,
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Bold),
            .AutoSize = False
        }
        brand.Controls.Add(nameLabel)
        brand.Controls.Add(productLabel)
        brand.Controls.Add(accent)
        AddHandler brand.Resize,
            Sub()
                accent.SetBounds(24, 28, 36, 3)
                nameLabel.SetBounds(24, 40, brand.Width - 36, 32)
                productLabel.SetBounds(24, 72, brand.Width - 36, 22)
            End Sub
        Return brand
    End Function

    Private Function BuildLogout() As Button
        Dim logout As Button = MakeNavButton("Logout", 16)
        logout.Dock = DockStyle.Bottom
        logout.Height = 52
        logout.Margin = New Padding(0)
        logout.FlatAppearance.MouseOverBackColor = Color.FromArgb(92, 56, 48)
        AddHandler logout.Click, AddressOf Logout_Click
        Return logout
    End Function

    Private Sub BuildNavigation()
        _dashboardButton = AddNav("Dashboard", 16, AddressOf ShowDashboard)
        AddNav("Sales", 16, AddressOf ShowSales)
        AddNav("Sales History", 16, AddressOf ShowSalesHistory)
        AddNav("Menu Items", 16, AddressOf ShowMenu)
        AddSectionLabel("INVENTORY")
        AddNav("Stocks", 32, AddressOf ShowStocks)
        AddNav("Stock-In", 32, AddressOf ShowStockIn)
        AddNav("Low Stock", 32, AddressOf ShowLowStock)
        AddNav("Expenses", 16, AddressOf ShowExpenses)
    End Sub

    Private Function AddNav(caption As String, leftPadding As Integer, onClick As EventHandler) As Button
        Dim button As Button = MakeNavButton(caption, leftPadding)
        button.Margin = New Padding(0, 2, 0, 2)
        button.Height = 42
        AddHandler button.Click, onClick
        AddHandler button.Click, Sub() SelectNav(button)
        _navButtons.Add(button)
        _navFlow.Controls.Add(button)
        Return button
    End Function

    Private Sub AddSectionLabel(caption As String)
        Dim label As New Label With {
            .Text = caption,
            .ForeColor = UiTheme.Accent,
            .Font = New Font("Segoe UI", 8.5!, FontStyle.Bold),
            .Height = 32,
            .TextAlign = ContentAlignment.BottomLeft,
            .Margin = New Padding(8, 12, 0, 2),
            .Padding = New Padding(8, 0, 0, 4)
        }
        _navFlow.Controls.Add(label)
    End Sub

    Private Shared Function MakeNavButton(caption As String, leftPadding As Integer) As Button
        Dim button As New Button With {
            .Text = caption,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Padding = New Padding(leftPadding, 0, 8, 0),
            .FlatStyle = FlatStyle.Flat,
            .ForeColor = Color.White,
            .BackColor = UiTheme.Sidebar,
            .Cursor = Cursors.Hand,
            .UseVisualStyleBackColor = False
        }
        button.FlatAppearance.BorderSize = 0
        button.FlatAppearance.MouseOverBackColor = UiTheme.SidebarHover
        button.FlatAppearance.MouseDownBackColor = UiTheme.SidebarSelected
        Return button
    End Function

    Private Sub SizeNavItems()
        Dim width As Integer = _navFlow.ClientSize.Width - _navFlow.Padding.Horizontal
        If width < 80 Then Return
        For Each item As Control In _navFlow.Controls
            item.Width = width
        Next
    End Sub

    Private Sub LayoutHeader()
        If _titleLabel Is Nothing OrElse _titleLabel.Parent Is Nothing Then Return
        Dim header As Control = _titleLabel.Parent
        _titleLabel.SetBounds(24, 18, header.Width - 280, 40)
        _userLabel.SetBounds(header.Width - 260, 18, 236, 40)
    End Sub

    Private Sub SelectNav(selected As Button)
        For Each button As Button In _navButtons
            button.BackColor = UiTheme.Sidebar
            button.ForeColor = Color.White
            button.FlatAppearance.MouseOverBackColor = UiTheme.SidebarHover
        Next
        selected.BackColor = UiTheme.SidebarSelected
        selected.ForeColor = UiTheme.Accent
        selected.FlatAppearance.MouseOverBackColor = UiTheme.SidebarSelected
    End Sub

    Private Sub ShowContent(title As String, view As Control)
        _titleLabel.Text = title
        For index As Integer = _contentHost.Controls.Count - 1 To 0 Step -1
            Dim oldView As Control = _contentHost.Controls(index)
            _contentHost.Controls.RemoveAt(index)
            oldView.Dispose()
        Next
        view.Dock = DockStyle.Fill
        _contentHost.Controls.Add(view)
    End Sub

    Private Sub ShowDashboard()
        SelectNav(_dashboardButton)
        Dim dashboard As New DashboardView()
        ShowContent("Dashboard", dashboard)
        dashboard.LoadFigures()
    End Sub

    Private Sub ShowSales()
        ShowContent("Sales", New SalesView())
    End Sub

    Private Sub ShowSalesHistory()
        ShowContent("Sales History", New SalesHistoryView())
    End Sub

    Private Sub ShowMenu()
        ShowContent("Menu Items", New MenuItemsView())
    End Sub

    Private Sub ShowStocks()
        ShowContent("Stocks", LoadList(
            "Current inventory quantities.",
            Function() InventoryItemRepository.GetStocks(),
            {"Unit cost"},
            {"On hand", "Reorder point"}))
    End Sub

    Private Sub ShowStockIn()
        ShowContent("Stock-In", LoadList(
            "Stock-in records.",
            Function() StockInRepository.GetStockIns(),
            {"Total cost"},
            {"Lines"}))
    End Sub

    Private Sub ShowLowStock()
        ShowContent("Low Stock", LoadList(
            "Active items at or below the reorder point.",
            Function() InventoryItemRepository.GetLowStock(),
            Array.Empty(Of String)(),
            {"On hand", "Reorder point"}))
    End Sub

    Private Sub ShowExpenses()
        ShowContent("Expenses", New ExpensesView())
    End Sub

    Private Shared Function LoadList(subtitle As String, load As Func(Of DataTable), moneyColumns As String(), quantityColumns As String()) As RecordListView
        Try
            Return New RecordListView(subtitle, load(), moneyColumns, quantityColumns)
        Catch ex As Exception
            Dim view As New RecordListView(subtitle, New DataTable(), moneyColumns, quantityColumns)
            view.ShowError(ex.Message)
            Return view
        End Try
    End Function

    Private Sub Logout_Click(sender As Object, e As EventArgs)
        Close()
    End Sub

End Class
