Imports DoughTrack.Helpers

''' <summary>
''' Main window. The sidebar stays the same for every module.
''' </summary>
Public Class frmMain
    Inherits Form

    Private ReadOnly _navFlow As FlowLayoutPanel
    Private ReadOnly _footer As FlowLayoutPanel
    Private ReadOnly _contentHost As Panel
    Private ReadOnly _titleLabel As Label
    Private ReadOnly _userLabel As Label
    Private ReadOnly _adminChip As Panel
    Private ReadOnly _bell As BellIcon
    Private ReadOnly _navButtons As New List(Of Button)
    Private _dashboardButton As Button
    Private _settingsButton As Button

    Public Sub New()
        Font = New Font("Segoe UI", 10.0!)
        Text = "DoughTrack — Doughtown Cafe"
        StartPosition = FormStartPosition.CenterScreen
        MinimumSize = New Size(1100, 700)
        ClientSize = New Size(1280, 800)
        BackColor = UiTheme.Content
        DoubleBuffered = True

        Dim sidebar As New Panel With {
            .Dock = DockStyle.Left,
            .Width = 220,
            .BackColor = UiTheme.Sidebar
        }
        sidebar.Controls.Add(BuildBrand())

        _footer = New FlowLayoutPanel With {
            .Dock = DockStyle.Bottom,
            .Height = 108,
            .FlowDirection = FlowDirection.TopDown,
            .WrapContents = False,
            .BackColor = UiTheme.Sidebar,
            .Padding = New Padding(0, 6, 0, 8)
        }
        _settingsButton = MakeNavButton("Settings", 18)
        _settingsButton.Height = 42
        _settingsButton.Margin = New Padding(0, 2, 0, 2)
        AddHandler _settingsButton.Click, AddressOf ShowSettings
        AddHandler _settingsButton.Click, Sub() SelectNav(_settingsButton)
        _navButtons.Add(_settingsButton)

        Dim logout As Button = MakeNavButton("Logout", 18)
        logout.Height = 42
        logout.Margin = New Padding(0, 2, 0, 2)
        AddHandler logout.Click, AddressOf Logout_Click
        _footer.Controls.Add(_settingsButton)
        _footer.Controls.Add(logout)
        sidebar.Controls.Add(_footer)

        _navFlow = New FlowLayoutPanel With {
            .Dock = DockStyle.Fill,
            .FlowDirection = FlowDirection.TopDown,
            .WrapContents = False,
            .AutoScroll = True,
            .BackColor = UiTheme.Sidebar,
            .Padding = New Padding(0, 4, 0, 8)
        }
        sidebar.Controls.Add(_navFlow)
        sidebar.Controls.SetChildIndex(_navFlow, 0)

        BuildNavigation()
        AddHandler sidebar.Resize, Sub() SizeNavItems()
        AddHandler _navFlow.SizeChanged, Sub() SizeNavItems()

        Dim header As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 64,
            .BackColor = UiTheme.Header
        }
        _titleLabel = New Label With {
            .AutoSize = False,
            .Font = New Font("Segoe UI", 16.0!, FontStyle.Bold),
            .ForeColor = Color.White,
            .Text = "Dashboard",
            .TextAlign = ContentAlignment.MiddleLeft
        }
        _bell = New BellIcon()
        _adminChip = New Panel With {
            .BackColor = UiTheme.PrimaryButton
        }
        _userLabel = New Label With {
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleCenter,
            .ForeColor = Color.White,
            .Font = New Font("Segoe UI", 9.5!, FontStyle.Bold),
            .Text = "Admin"
        }
        _adminChip.Controls.Add(_userLabel)
        header.Controls.Add(_titleLabel)
        header.Controls.Add(_bell)
        header.Controls.Add(_adminChip)
        AddHandler header.Resize, Sub() LayoutHeader()

        _contentHost = New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = UiTheme.Content,
            .Padding = New Padding(16)
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
        Dim name As String = Session.CurrentUser.FullName
        If String.IsNullOrWhiteSpace(name) Then name = Session.CurrentUser.Username
        If String.IsNullOrWhiteSpace(name) Then name = "Admin"
        _userLabel.Text = name
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
            .Height = 118,
            .BackColor = UiTheme.Sidebar
        }
        Dim badge As New LogoBadge()
        brand.Controls.Add(badge)
        AddHandler brand.Resize,
            Sub()
                badge.Location = New Point(Math.Max(0, (brand.Width - badge.Width) \ 2), 16)
            End Sub
        Return brand
    End Function

    Private Sub BuildNavigation()
        _dashboardButton = AddNav("Dashboard", 18, AddressOf ShowDashboard)
        AddNav("Sales", 18, AddressOf ShowSales)
        AddNav("Sales History", 18, AddressOf ShowSalesHistory)
        AddNav("Menu Items", 18, AddressOf ShowMenu)
        AddSectionLabel("INVENTORY")
        AddNav("Stocks", 32, AddressOf ShowStocks)
        AddNav("Stock-In", 32, AddressOf ShowStockIn)
        AddNav("Low Stock", 32, AddressOf ShowLowStock)
        AddNav("Expenses", 18, AddressOf ShowExpenses)
    End Sub

    Private Function AddNav(caption As String, leftPadding As Integer, onClick As EventHandler) As Button
        Dim button As Button = MakeNavButton(caption, leftPadding)
        button.Margin = New Padding(0, 1, 0, 1)
        button.Height = 40
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
            .Font = New Font("Segoe UI", 8.0!, FontStyle.Bold),
            .Height = 28,
            .TextAlign = ContentAlignment.BottomLeft,
            .Margin = New Padding(0, 10, 0, 2),
            .Padding = New Padding(18, 0, 0, 2)
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
            .Font = New Font("Segoe UI", 10.0!),
            .Cursor = Cursors.Hand,
            .UseVisualStyleBackColor = False
        }
        button.FlatAppearance.BorderSize = 0
        button.FlatAppearance.MouseOverBackColor = UiTheme.SidebarHover
        button.FlatAppearance.MouseDownBackColor = UiTheme.SidebarSelected
        Return button
    End Function

    Private Sub SizeNavItems()
        Dim width As Integer = _navFlow.ClientSize.Width
        If width < 80 Then Return
        For Each item As Control In _navFlow.Controls
            item.Width = width
        Next
        Dim footerWidth As Integer = _footer.ClientSize.Width
        If footerWidth < 80 Then Return
        For Each item As Control In _footer.Controls
            item.Width = footerWidth
        Next
    End Sub

    Private Sub LayoutHeader()
        If _titleLabel Is Nothing OrElse _titleLabel.Parent Is Nothing Then Return
        Dim header As Control = _titleLabel.Parent
        _adminChip.SetBounds(header.Width - 168, 15, 148, 34)
        _bell.SetBounds(header.Width - 208, 16, 32, 32)
        _titleLabel.SetBounds(20, 12, Math.Max(120, header.Width - 240), 40)
    End Sub

    Private Sub SelectNav(selected As Button)
        For Each button As Button In _navButtons
            button.BackColor = UiTheme.Sidebar
            button.ForeColor = Color.White
            button.Font = New Font("Segoe UI", 10.0!)
            button.FlatAppearance.MouseOverBackColor = UiTheme.SidebarHover
        Next
        selected.BackColor = UiTheme.SidebarSelected
        selected.ForeColor = Color.White
        selected.Font = New Font("Segoe UI", 10.0!, FontStyle.Bold)
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
        ShowContent("Stocks", New StocksView())
    End Sub

    Private Sub ShowStockIn()
        ShowContent("Stock-In", New StockInView())
    End Sub

    Private Sub ShowLowStock()
        ShowContent("Low Stock", New LowStockView())
    End Sub

    Private Sub ShowExpenses()
        ShowContent("Expenses", New ExpensesView())
    End Sub

    Private Sub ShowSettings()
        ShowContent("Settings", New SettingsView())
    End Sub

    Private Sub Logout_Click(sender As Object, e As EventArgs)
        Close()
    End Sub

End Class
