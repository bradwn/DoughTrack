Imports System.IO
Imports DoughTrack.Data
Imports DoughTrack.Helpers
Imports DoughTrack.Models

''' <summary>
''' Point of sale. The menu is on the left and the unsaved order is on the right.
''' </summary>
Public Class SalesView
    Inherits UserControl

    Private _search As TextBox
    Private _category As ComboBox
    Private _menuHost As FlowLayoutPanel
    Private _menuMessage As Label
    Private _orderGrid As DataGridView
    Private _orderMessage As Label
    Private _totalLabel As Label
    Private _completeButton As Button
    Private ReadOnly _order As New List(Of SaleDraftLine)
    Private ReadOnly _thumbs As New List(Of Image)
    Private ReadOnly _blank As Image
    Private _offers As New Dictionary(Of Integer, MenuOffer)
    Private _loadingFilters As Boolean
    Private _saving As Boolean

    Public Sub New()
        Dock = DockStyle.Fill
        BackColor = UiTheme.Content
        Padding = New Padding(12)
        _blank = CreateBlank()

        Dim split As New SplitContainer With {
            .Dock = DockStyle.Fill,
            .SplitterWidth = 8,
            .BackColor = UiTheme.Content
        }

        split.Panel1.Controls.Add(BuildMenuPanel())
        split.Panel2.Controls.Add(BuildOrderPanel())
        Controls.Add(split)
        AddHandler HandleCreated, Sub() split.SplitterDistance = Math.Max(360, split.Width * 3 \ 5)
        AddHandler Disposed, Sub() ClearThumbs()

        LoadCategories()
        ReloadMenu()
        BindOrder()
    End Sub

    Private Function BuildMenuPanel() As Control
        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(12)
        }
        Dim tools As New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 44,
            .WrapContents = False,
            .FlowDirection = FlowDirection.LeftToRight
        }
        _search = New TextBox With {.Width = 180}
        _category = New ComboBox With {.Width = 150, .DropDownStyle = ComboBoxStyle.DropDownList}
        Dim searchButton As Button = UiTheme.MakeButton("Search")
        tools.Controls.Add(New Label With {.Text = "Search", .AutoSize = True, .Padding = New Padding(0, 8, 6, 0)})
        tools.Controls.Add(_search)
        tools.Controls.Add(searchButton)
        tools.Controls.Add(New Label With {.Text = "Category", .AutoSize = True, .Padding = New Padding(10, 8, 6, 0)})
        tools.Controls.Add(_category)

        _menuMessage = New Label With {
            .Dock = DockStyle.Bottom,
            .Height = 24,
            .ForeColor = UiTheme.TextMuted,
            .TextAlign = ContentAlignment.MiddleLeft
        }
        Dim scroller As New Panel With {.Dock = DockStyle.Fill, .AutoScroll = True}
        _menuHost = New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .AutoSize = True,
            .AutoSizeMode = AutoSizeMode.GrowAndShrink,
            .WrapContents = True,
            .FlowDirection = FlowDirection.LeftToRight,
            .BackColor = Color.White
        }
        scroller.Controls.Add(_menuHost)
        card.Controls.Add(tools)
        card.Controls.Add(_menuMessage)
        card.Controls.Add(scroller)
        card.Controls.SetChildIndex(scroller, 0)

        AddHandler searchButton.Click, Sub() ReloadMenu()
        AddHandler _search.KeyDown,
            Sub(sender, e)
                If e.KeyCode = Keys.Enter Then
                    ReloadMenu()
                    e.SuppressKeyPress = True
                End If
            End Sub
        AddHandler _category.SelectedIndexChanged,
            Sub()
                If Not _loadingFilters Then ReloadMenu()
            End Sub
        Return card
    End Function

    Private Function BuildOrderPanel() As Control
        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(12),
            .Margin = New Padding(8, 0, 0, 0)
        }
        Dim title As New Label With {
            .Dock = DockStyle.Top,
            .Height = 32,
            .Font = New Font("Segoe UI", 14.0!, FontStyle.Bold),
            .Text = "Current Order"
        }
        _orderGrid = New DataGridView With {.Dock = DockStyle.Fill}
        UiTheme.StyleGrid(_orderGrid)
        _orderGrid.ReadOnly = False
        _orderGrid.EditMode = DataGridViewEditMode.EditProgrammatically
        _orderGrid.RowTemplate.Height = 36
        _orderGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        _orderGrid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "Item", .HeaderText = "Item", .FillWeight = 140})
        _orderGrid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "Quantity", .HeaderText = "Quantity", .FillWeight = 55})
        Dim priceColumn As New DataGridViewTextBoxColumn With {.Name = "Price", .HeaderText = "Unit price", .FillWeight = 70}
        priceColumn.DefaultCellStyle.Format = "₱#,##0.00"
        _orderGrid.Columns.Add(priceColumn)
        Dim subtotalColumn As New DataGridViewTextBoxColumn With {.Name = "Subtotal", .HeaderText = "Subtotal", .FillWeight = 70}
        subtotalColumn.DefaultCellStyle.Format = "₱#,##0.00"
        _orderGrid.Columns.Add(subtotalColumn)
        _orderGrid.Columns.Add(New DataGridViewButtonColumn With {
            .Name = "Decrease", .HeaderText = "", .Text = "−", .UseColumnTextForButtonValue = True,
            .AutoSizeMode = DataGridViewAutoSizeColumnMode.None, .Width = 44, .FlatStyle = FlatStyle.Flat
        })
        _orderGrid.Columns.Add(New DataGridViewButtonColumn With {
            .Name = "Increase", .HeaderText = "", .Text = "+", .UseColumnTextForButtonValue = True,
            .AutoSizeMode = DataGridViewAutoSizeColumnMode.None, .Width = 44, .FlatStyle = FlatStyle.Flat
        })
        _orderGrid.Columns.Add(New DataGridViewButtonColumn With {
            .Name = "Remove", .HeaderText = "", .Text = "Remove", .UseColumnTextForButtonValue = True,
            .AutoSizeMode = DataGridViewAutoSizeColumnMode.None, .Width = 80, .FlatStyle = FlatStyle.Flat
        })
        For Each column As DataGridViewColumn In _orderGrid.Columns
            If TypeOf column Is DataGridViewTextBoxColumn Then column.ReadOnly = True
        Next

        _totalLabel = New Label With {
            .Dock = DockStyle.Bottom,
            .Height = 36,
            .Font = New Font("Segoe UI", 14.0!, FontStyle.Bold),
            .TextAlign = ContentAlignment.MiddleRight,
            .Text = "Total  " & UiTheme.FormatMoney(0D)
        }
        _orderMessage = New Label With {
            .Dock = DockStyle.Bottom,
            .Height = 40,
            .ForeColor = UiTheme.Danger
        }
        Dim buttons As New FlowLayoutPanel With {
            .Dock = DockStyle.Bottom,
            .Height = 46,
            .FlowDirection = FlowDirection.RightToLeft
        }
        _completeButton = UiTheme.MakeButton("Complete Sale")
        Dim clearButton As Button = UiTheme.MakeButton("Clear Order")
        buttons.Controls.Add(_completeButton)
        buttons.Controls.Add(clearButton)

        card.Controls.Add(title)
        card.Controls.Add(buttons)
        card.Controls.Add(_totalLabel)
        card.Controls.Add(_orderMessage)
        card.Controls.Add(_orderGrid)
        card.Controls.SetChildIndex(_orderGrid, 0)

        AddHandler _orderGrid.CellContentClick, AddressOf OrderClick
        AddHandler clearButton.Click, AddressOf ClearOrder
        AddHandler _completeButton.Click, AddressOf CompleteOrder
        Return card
    End Function

    Private Sub LoadCategories()
        _loadingFilters = True
        Dim selected As String = If(_category.SelectedIndex > 0, CStr(_category.SelectedItem), "")
        _category.Items.Clear()
        _category.Items.Add("All categories")
        Try
            For Each category As String In SaleRepository.GetActiveCategories()
                _category.Items.Add(category)
            Next
        Catch ex As Exception
            _menuMessage.Text = ex.Message
            _menuMessage.ForeColor = UiTheme.Danger
        End Try
        Dim index As Integer = _category.Items.IndexOf(selected)
        _category.SelectedIndex = If(index >= 0, index, 0)
        _loadingFilters = False
    End Sub

    Private Sub ReloadMenu()
        ClearThumbs()
        _menuHost.Controls.Clear()
        _offers.Clear()
        Dim category As String = If(_category.SelectedIndex > 0, CStr(_category.SelectedItem), "")
        Dim offers As List(Of MenuOffer)
        Try
            offers = SaleRepository.GetOffers(_search.Text, category)
        Catch ex As Exception
            _menuMessage.ForeColor = UiTheme.Danger
            _menuMessage.Text = ex.Message
            Return
        End Try

        For Each offer As MenuOffer In offers
            _offers(offer.MenuItemId) = offer
            _menuHost.Controls.Add(MakeCard(offer))
            Dim line As SaleDraftLine = FindLine(offer.MenuItemId)
            If line IsNot Nothing Then line.Available = offer.CanMake
        Next
        _menuHost.Width = Math.Max(280, _menuHost.Parent.ClientSize.Width - 8)
        _menuMessage.ForeColor = UiTheme.TextMuted
        _menuMessage.Text = If(offers.Count = 0, "No active menu items match this search.", offers.Count.ToString() & " menu items")
        BindOrder()
    End Sub

    Private Function MakeCard(offer As MenuOffer) As Control
        Dim card As New Panel With {
            .Width = 250,
            .Height = 168,
            .Margin = New Padding(0, 0, 8, 8),
            .BackColor = UiTheme.Content,
            .Padding = New Padding(8)
        }
        Dim picture As New PictureBox With {
            .Location = New Point(8, 8),
            .Size = New Size(64, 64),
            .SizeMode = PictureBoxSizeMode.Zoom,
            .Image = LoadThumb(offer.ImagePath)
        }
        Dim nameLabel As New Label With {
            .Location = New Point(80, 8),
            .Size = New Size(156, 22),
            .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
            .Text = offer.Name
        }
        Dim categoryLabel As New Label With {
            .Location = New Point(80, 30),
            .Size = New Size(156, 18),
            .ForeColor = UiTheme.TextMuted,
            .Text = offer.Category
        }
        Dim priceLabel As New Label With {
            .Location = New Point(80, 50),
            .Size = New Size(156, 20),
            .Text = UiTheme.FormatMoney(offer.SellingPrice)
        }
        Dim stockLabel As New Label With {
            .Location = New Point(8, 80),
            .Size = New Size(230, 20),
            .ForeColor = If(offer.IsAvailable, Color.FromArgb(30, 120, 60), UiTheme.Danger),
            .Text = If(offer.IsAvailable, "Available  ·  " & offer.CanMake.ToString() & " can be made", "Unavailable")
        }
        Dim quantity As New NumericUpDown With {
            .Location = New Point(8, 112),
            .Width = 70,
            .Minimum = 1D,
            .Maximum = Math.Max(1, offer.CanMake),
            .Value = 1D
        }
        Dim addButton As Button = UiTheme.MakeButton("Add to order")
        addButton.Location = New Point(88, 110)
        addButton.Enabled = offer.IsAvailable
        Dim current As MenuOffer = offer
        Dim stepper As NumericUpDown = quantity
        AddHandler addButton.Click, Sub() AddToOrder(current, Decimal.ToInt32(stepper.Value))
        card.Controls.Add(picture)
        card.Controls.Add(nameLabel)
        card.Controls.Add(categoryLabel)
        card.Controls.Add(priceLabel)
        card.Controls.Add(stockLabel)
        card.Controls.Add(quantity)
        card.Controls.Add(addButton)
        Return card
    End Function

    Private Sub AddToOrder(offer As MenuOffer, quantity As Integer)
        _orderMessage.ForeColor = UiTheme.Danger
        _orderMessage.Text = ""
        If quantity < 1 Then Return
        Dim line As SaleDraftLine = FindLine(offer.MenuItemId)
        Dim nextQuantity As Integer = quantity
        If line IsNot Nothing Then nextQuantity = line.Quantity + quantity
        If nextQuantity > offer.CanMake Then
            _orderMessage.Text = offer.Name & " only has stock for " & offer.CanMake.ToString() & "."
            Return
        End If
        If line Is Nothing Then
            _order.Add(New SaleDraftLine With {
                .MenuItemId = offer.MenuItemId,
                .ItemName = offer.Name,
                .UnitPrice = offer.SellingPrice,
                .Quantity = quantity,
                .Available = offer.CanMake
            })
        Else
            line.Quantity = nextQuantity
            line.Available = offer.CanMake
            line.UnitPrice = offer.SellingPrice
        End If
        BindOrder()
    End Sub

    Private Sub OrderClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 OrElse _saving Then Return
        Dim line As SaleDraftLine = TryCast(_orderGrid.Rows(e.RowIndex).Tag, SaleDraftLine)
        If line Is Nothing Then Return
        Dim columnName As String = _orderGrid.Columns(e.ColumnIndex).Name
        _orderMessage.Text = ""
        If columnName = "Remove" Then
            _order.Remove(line)
        ElseIf columnName = "Decrease" Then
            If line.Quantity <= 1 Then
                _order.Remove(line)
            Else
                line.Quantity -= 1
            End If
        ElseIf columnName = "Increase" Then
            If line.Quantity + 1 > line.Available Then
                _orderMessage.Text = line.ItemName & " only has stock for " & line.Available.ToString() & "."
                Return
            End If
            line.Quantity += 1
        Else
            Return
        End If
        BindOrder()
    End Sub

    Private Sub ClearOrder(sender As Object, e As EventArgs)
        If _saving OrElse _order.Count = 0 Then Return
        If MessageBox.Show("Clear the current order?", "DoughTrack", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
            Return
        End If
        _order.Clear()
        _orderMessage.Text = ""
        BindOrder()
    End Sub

    Private Sub CompleteOrder(sender As Object, e As EventArgs)
        _orderMessage.Text = ""
        If _order.Count = 0 Then
            _orderMessage.Text = "Add at least one item to the order."
            Return
        End If
        If Not Session.IsLoggedIn Then
            _orderMessage.Text = "Log in before completing a sale."
            Return
        End If

        _saving = True
        _completeButton.Enabled = False
        Try
            Dim result As CompletedSale = SaleRepository.CompleteSale(Session.CurrentUser.UserId, _order)
            _order.Clear()
            BindOrder()
            LoadCategories()
            ReloadMenu()
            _orderMessage.ForeColor = Color.FromArgb(30, 120, 60)
            _orderMessage.Text = "Sale " & result.SaleId.ToString() & " saved. Total " &
                UiTheme.FormatMoney(result.TotalAmount) & ". COGS " & UiTheme.FormatMoney(result.CogsAmount) & "."
        Catch ex As Exception
            _orderMessage.ForeColor = UiTheme.Danger
            _orderMessage.Text = ex.Message
            ReloadMenu()
        Finally
            _saving = False
            _completeButton.Enabled = True
        End Try
    End Sub

    Private Sub BindOrder()
        _orderGrid.Rows.Clear()
        Dim total As Decimal = 0D
        For Each line As SaleDraftLine In _order
            Dim rowIndex As Integer = _orderGrid.Rows.Add(line.ItemName, line.Quantity, line.UnitPrice, line.Subtotal)
            _orderGrid.Rows(rowIndex).Tag = line
            total += line.Subtotal
        Next
        _totalLabel.Text = "Total  " & UiTheme.FormatMoney(total)
    End Sub

    Private Function FindLine(menuItemId As Integer) As SaleDraftLine
        For Each line As SaleDraftLine In _order
            If line.MenuItemId = menuItemId Then Return line
        Next
        Return Nothing
    End Function

    Private Function LoadThumb(storedPath As String) As Image
        Dim fullPath As String = MenuImageStore.Resolve(storedPath)
        If fullPath = "" OrElse Not File.Exists(fullPath) Then Return _blank
        Try
            Using stream As New FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using source As Image = Image.FromStream(stream)
                    Dim copy As New Bitmap(source)
                    _thumbs.Add(copy)
                    Return copy
                End Using
            End Using
        Catch
            Return _blank
        End Try
    End Function

    Private Sub ClearThumbs()
        For Each thumb As Image In _thumbs
            thumb.Dispose()
        Next
        _thumbs.Clear()
    End Sub

    Private Shared Function CreateBlank() As Image
        Dim bitmap As New Bitmap(64, 64)
        Using graphics As Graphics = Graphics.FromImage(bitmap)
            graphics.Clear(Color.FromArgb(230, 232, 236))
        End Using
        Return bitmap
    End Function

End Class
