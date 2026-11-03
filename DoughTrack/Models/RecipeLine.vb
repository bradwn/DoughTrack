Namespace Models

    ''' <summary>
    ''' One recipe row joined to the inventory item it uses.
    ''' </summary>
    Public Class RecipeLine
        Public Property MenuItemIngredientId As Integer
        Public Property MenuItemId As Integer
        Public Property InventoryItemId As Integer
        Public Property ItemName As String
        Public Property InventoryCategory As String
        Public Property QuantityRequired As Decimal
        Public Property Unit As String
    End Class

End Namespace
