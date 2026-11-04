Imports DoughTrack.Data
Imports DoughTrack.Helpers
Imports DoughTrack.Models

''' <summary>
''' Completed sales. The grid reads sales and sale_items.
''' Item names and prices are the values stored on each line.
''' </summary>
Public Class SalesHistoryView
    Inherits UserControl

    Private ReadOnly _search As TextBox
    Private ReadOnly _category As ComboBox
    Private ReadOnly _fromDate As DateTimePicker
    Private ReadOnly _toDate As DateTimePicker
    Private ReadOnly _message As Label
    Private ReadOnly _grid As DataGridView
    Private _loadingFilters As Boolean

    Public Sub New()
        Dock = DockStyle.Fill
        BackColor = UiTheme.Content
        Padding = New Padding(16)

        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = UiTheme.Card,
            .Padding = New Padding(16)
        }

        Dim tools As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 84
        }
        Dim searchRow As New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 42,
            .WrapContents = False,
            .FlowDirection = FlowDirection.LeftToRight
        }
        _search = New TextBox With {.Width = 260}
        _category = New ComboBox With {
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Width = 180
        }
        Dim searchButton As Button = UiTheme.MakeButton("Search")
        searchRow.Controls.Add(_search)
        searchRow.Controls.Add(_category)
        searchRow.Controls.Add(searchButton)
        UiTheme.SetCue(_search, "Search sales...")

        Dim dateRow As New FlowLayoutPanel With {
            .Dock = DockStyle.Top,
            .Height = 42,
            .WrapContents = False,
            .FlowDirection = FlowDirection.LeftToRight
        }
        _fromDate = DateFilter()
        _toDate = DateFilter()
        dateRow.Controls.Add(FieldLabel("From"))
        dateRow.Controls.Add(_fromDate)
        dateRow.Controls.Add(FieldLabel("To"))
        dateRow.Controls.Add(_toDate)
        dateRow.Controls.Add(New Label With {
            .Text = "Check a date to limit the list.",
            .AutoSize = True,
            .ForeColor = UiTheme.TextMuted,
            .Padding = New Padding(8, 8, 0, 0)
        })
        tools.Controls.Add(dateRow)
        tools.Controls.Add(searchRow)

        _message = New Label With {
            .Dock = DockStyle.Bottom,
            .Height = 24,
            .ForeColor = UiTheme.TextMuted,
            .TextAlign = ContentAlignment.MiddleLeft
        }

        _grid = New DataGridView With {.Dock = DockStyle.Fill}
        UiTheme.StyleGrid(_grid)
        _grid.ReadOnly = False
        _grid.EditMode = DataGridViewEditMode.EditProgrammatically
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        BuildColumns()

        card.Controls.Add(_grid)
        card.Controls.Add(_message)
        card.Controls.Add(tools)
        Controls.Add(card)

        AddHandler searchButton.Click, Sub() ReloadGrid()
        AddHandler _search.KeyDown,
            Sub(sender, e)
                If e.KeyCode = Keys.Enter Then
                    ReloadGrid()
                    e.SuppressKeyPress = True
                End If
            End Sub
        AddHandler _category.SelectedIndexChanged,
            Sub()
                If Not _loadingFilters Then ReloadGrid()
            End Sub
        AddHandler _fromDate.ValueChanged, Sub() If Not _loadingFilters Then ReloadGrid()
        AddHandler _toDate.ValueChanged, Sub() If Not _loadingFilters Then ReloadGrid()
        AddHandler _fromDate.MouseUp, Sub() If Not _loadingFilters Then ReloadGrid()
        AddHandler _toDate.MouseUp, Sub() If Not _loadingFilters Then ReloadGrid()
        AddHandler _grid.CellContentClick, AddressOf GridClick
        AddHandler _grid.CellDoubleClick, AddressOf GridDoubleClick

        AddHandler _fromDate.HandleCreated, Sub() UncheckDate(_fromDate)
        AddHandler _toDate.HandleCreated, Sub() UncheckDate(_toDate)
        UiTheme.StyleInputs(Me)
        LoadCategories()
        ReloadGrid()
    End Sub

    Private Sub UncheckDate(picker As DateTimePicker)
        _loadingFilters = True
        picker.Checked = False
        _loadingFilters = False
    End Sub

    Private Sub BuildColumns()
        Dim idColumn As DataGridViewTextBoxColumn = DataColumn("SaleId", "Sale ID", 50)
        idColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        idColumn.Width = 90
        _grid.Columns.Add(idColumn)

        Dim whenColumn As DataGridViewTextBoxColumn = DataColumn("When", "Date and Time", 120)
        whenColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        whenColumn.Width = 180
        _grid.Columns.Add(whenColumn)

        _grid.Columns.Add(DataColumn("Items", "Items", 220))

        Dim totalColumn As DataGridViewTextBoxColumn = DataColumn("Total", "Total Amount", 80)
        totalColumn.DefaultCellStyle.Format = "₱#,##0.00"
        totalColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        totalColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None
        totalColumn.Width = 130
        _grid.Columns.Add(totalColumn)

        _grid.Columns.Add(New DataGridViewButtonColumn With {
            .Name = "Details",
            .HeaderText = "Action",
            .Text = "View Details",
            .UseColumnTextForButtonValue = True,
            .AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            .Width = 120,
            .FlatStyle = FlatStyle.Flat
        })
    End Sub

    Private Sub LoadCategories()
        _loadingFilters = True
        Dim selected As String = ""
        If _category.SelectedIndex > 0 Then selected = CStr(_category.SelectedItem)
        _category.Items.Clear()
        _category.Items.Add("All Categories")
        Try
            For Each category As String In SaleRepository.GetHistoryCategories()
                _category.Items.Add(category)
            Next
        Catch ex As Exception
            _message.ForeColor = UiTheme.Danger
            _message.Text = ex.Message
        End Try
        Dim index As Integer = _category.Items.IndexOf(selected)
        _category.SelectedIndex = If(index >= 0, index, 0)
        _fromDate.Checked = False
        _toDate.Checked = False
        _loadingFilters = False
    End Sub

    Private Sub ReloadGrid()
        _grid.Rows.Clear()
        Dim fromDate As Date? = Nothing
        Dim toDate As Date? = Nothing
        If _fromDate.Checked Then fromDate = _fromDate.Value.Date
        If _toDate.Checked Then toDate = _toDate.Value.Date
        If fromDate.HasValue AndAlso toDate.HasValue AndAlso fromDate.Value > toDate.Value Then
            _message.ForeColor = UiTheme.Danger
            _message.Text = "The start date is after the end date."
            Return
        End If

        Dim category As String = ""
        If _category.SelectedIndex > 0 Then category = CStr(_category.SelectedItem)

        Dim sales As List(Of Sale)
        Try
            sales = SaleRepository.SearchHistory(_search.Text, category, fromDate, toDate)
        Catch ex As Exception
            _message.ForeColor = UiTheme.Danger
            _message.Text = ex.Message
            Return
        End Try

        For Each sale As Sale In sales
            Dim rowIndex As Integer = _grid.Rows.Add(
                sale.SaleId,
                sale.SaleDateTime.ToString("MMM d, yyyy h:mm tt"),
                sale.ItemSummary,
                sale.TotalAmount,
                "View Details")
            _grid.Rows(rowIndex).Tag = sale.SaleId
        Next

        _message.ForeColor = UiTheme.TextMuted
        If sales.Count = 0 Then
            _message.Text = "No sales match this search."
        Else
            _message.Text = sales.Count.ToString() & If(sales.Count = 1, " sale", " sales") &
                ". Names and prices are the values saved with each sale."
        End If
    End Sub

    Private Sub GridClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return
        If _grid.Columns(e.ColumnIndex).Name <> "Details" Then Return
        OpenDetails(e.RowIndex)
    End Sub

    Private Sub GridDoubleClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return
        If _grid.Columns(e.ColumnIndex).Name = "Details" Then Return
        OpenDetails(e.RowIndex)
    End Sub

    Private Sub OpenDetails(rowIndex As Integer)
        Dim saleId As Integer = CInt(_grid.Rows(rowIndex).Tag)
        Using details As New frmSaleDetails(saleId)
            details.ShowDialog(FindForm())
        End Using
    End Sub

    Private Shared Function DateFilter() As DateTimePicker
        Return New DateTimePicker With {
            .Format = DateTimePickerFormat.Short,
            .ShowCheckBox = True,
            .Checked = False,
            .Width = 140
        }
    End Function

    Private Shared Function FieldLabel(caption As String) As Label
        Return New Label With {
            .Text = caption,
            .AutoSize = True,
            .Padding = New Padding(8, 8, 6, 0)
        }
    End Function

    Private Shared Function DataColumn(name As String, header As String, weight As Single) As DataGridViewTextBoxColumn
        Return New DataGridViewTextBoxColumn With {
            .Name = name,
            .HeaderText = header,
            .ReadOnly = True,
            .FillWeight = weight
        }
    End Function

End Class
