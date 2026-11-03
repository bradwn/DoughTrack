Namespace Models

    ''' <summary>
    ''' One row of the inventory_items table.
    ''' </summary>
    Public Class InventoryItem
        Public Property InventoryItemId As Integer
        Public Property ItemName As String
        Public Property Category As String
        Public Property Quantity As Decimal
        Public Property Unit As String
        Public Property ReorderPoint As Decimal
        Public Property UnitCost As Decimal
        Public Property Status As String
    End Class

End Namespace
