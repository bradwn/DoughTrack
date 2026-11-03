Namespace Models

    ''' <summary>
    ''' One row of the sale_items table.
    ''' </summary>
    Public Class SaleItem
        Public Property SaleItemId As Integer
        Public Property SaleId As Integer
        Public Property MenuItemId As Integer
        Public Property Quantity As Integer
        Public Property UnitPrice As Decimal
        Public Property Subtotal As Decimal
    End Class

End Namespace
