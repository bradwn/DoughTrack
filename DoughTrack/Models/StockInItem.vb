Namespace Models

    ''' <summary>
    ''' One row of the stock_in_items table.
    ''' </summary>
    Public Class StockInItem
        Public Property StockInItemId As Integer
        Public Property StockInId As Integer
        Public Property InventoryItemId As Integer
        Public Property Quantity As Decimal
        Public Property UnitCost As Decimal
        Public Property Subtotal As Decimal
    End Class

End Namespace
