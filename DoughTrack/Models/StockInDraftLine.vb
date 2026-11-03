Namespace Models

    ''' <summary>
    ''' One unsaved stock-in line. Quantity is entered in EnteredUnit and stored in the item's unit.
    ''' </summary>
    Public Class StockInDraftLine
        Public Property InventoryItemId As Integer
        Public Property ItemName As String
        Public Property ItemUnit As String
        Public Property EnteredQuantity As Decimal
        Public Property EnteredUnit As String
        Public Property EnteredUnitCost As Decimal
        Public Property StoredQuantity As Decimal
        Public Property StoredUnitCost As Decimal
        Public Property Subtotal As Decimal
    End Class

End Namespace
