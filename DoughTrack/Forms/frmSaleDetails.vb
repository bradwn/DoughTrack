Imports DoughTrack.Data
Imports DoughTrack.Helpers
Imports DoughTrack.Models

''' <summary>
''' One completed sale. Every amount and item name is read from the saved sale.
''' </summary>
Public Class frmSaleDetails
    Inherits Form

    Private ReadOnly _saleId As Integer
    Private ReadOnly _summary As Label
    Private ReadOnly _grid As DataGridView
    Private ReadOnly _totalLabel As Label
    Private ReadOnly _message As Label

    Public Sub New(saleId As Integer)
        _saleId = saleId
        Font = New Font("Segoe UI", 10.0!)
        Text = "Sale Details"
        StartPosition = FormStartPosition.CenterParent
        MinimizeBox = False
        MaximizeBox = False
        FormBorderStyle = FormBorderStyle.FixedDialog
        ClientSize = New Size(760, 520)
        BackColor = UiTheme.Content

        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Padding = New Padding(16)
        }
        _summary = New Label With {
            .Dock = DockStyle.Top,
            .Height = 72,
            .Text = "Transaction " & saleId.ToString()
        }
        _message = New Label With {
            .Dock = DockStyle.Top,
            .Height = 24,
            .ForeColor = UiTheme.TextMuted
        }
        _totalLabel = New Label With {
            .Dock = DockStyle.Bottom,
            .Height = 36,
            .Font = New Font("Segoe UI", 14.0!, FontStyle.Bold),
            .TextAlign = ContentAlignment.MiddleRight
        }
        Dim closeButton As Button = UiTheme.MakeButton("Close")
        Dim buttons As New FlowLayoutPanel With {
            .Dock = DockStyle.Bottom,
            .Height = 46,
            .FlowDirection = FlowDirection.RightToLeft,
            .Padding = New Padding(0, 6, 0, 0)
        }
        buttons.Controls.Add(closeButton)

        _grid = New DataGridView With {.Dock = DockStyle.Fill}
        UiTheme.StyleGrid(_grid)
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        BuildColumns()

        card.Controls.Add(_grid)
        card.Controls.Add(_totalLabel)
        card.Controls.Add(buttons)
        card.Controls.Add(_message)
        card.Controls.Add(_summary)
        Controls.Add(card)

        CancelButton = closeButton
        AddHandler closeButton.Click, Sub() Close()
        LoadSale()
    End Sub

    Private Sub BuildColumns()
        _grid.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "Item",
            .HeaderText = "Item",
            .FillWeight = 180
        })
        Dim quantity As New DataGridViewTextBoxColumn With {
            .Name = "Quantity",
            .HeaderText = "Quantity",
            .FillWeight = 70
        }
        quantity.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        _grid.Columns.Add(quantity)
        AddMoneyColumn("UnitPrice", "Unit price")
        AddMoneyColumn("Subtotal", "Subtotal")
    End Sub

    Private Sub AddMoneyColumn(name As String, header As String)
        Dim column As New DataGridViewTextBoxColumn With {
            .Name = name,
            .HeaderText = header,
            .FillWeight = 90
        }
        column.DefaultCellStyle.Format = "₱#,##0.00"
        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        _grid.Columns.Add(column)
    End Sub

    Private Sub LoadSale()
        Dim sale As Sale
        Try
            sale = SaleRepository.GetSale(_saleId)
        Catch ex As Exception
            _summary.Text = ex.Message
            _message.ForeColor = UiTheme.Danger
            _message.Text = "This sale could not be read."
            Return
        End Try

        If sale Is Nothing Then
            _summary.Text = "Transaction " & _saleId.ToString() & " is not in the database."
            _message.ForeColor = UiTheme.Danger
            _message.Text = "It may have been removed."
            Return
        End If

        _summary.Text =
            "Transaction ID  " & sale.SaleId.ToString() & vbCrLf &
            "Date and Time  " & sale.SaleDateTime.ToString("MMM d, yyyy h:mm tt")
        _totalLabel.Text = "Total amount  " & UiTheme.FormatMoney(sale.TotalAmount)

        For Each line As SaleItem In sale.Lines
            _grid.Rows.Add(LineName(line), line.Quantity, line.UnitPrice, line.Subtotal)
        Next

        If sale.Lines.Count = 0 Then
            _message.Text = "This sale has no saved items."
        Else
            _message.Text = sale.Lines.Count.ToString() & If(sale.Lines.Count = 1, " item", " items") &
                ". Prices are the ones saved on this sale."
        End If
    End Sub

    Private Shared Function LineName(line As SaleItem) As String
        Dim name As String = If(line.MenuItemName, "").Trim()
        If name = "" Then Return "Item " & line.MenuItemId.ToString()
        Return name
    End Function

End Class
