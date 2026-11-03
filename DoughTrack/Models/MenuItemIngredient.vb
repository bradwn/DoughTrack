Namespace Models

    ''' <summary>
    ''' One recipe line from menu_item_ingredients.
    ''' </summary>
    Public Class MenuItemIngredient
        Public Property MenuItemIngredientId As Integer
        Public Property MenuItemId As Integer
        Public Property InventoryItemId As Integer
        Public Property QuantityRequired As Decimal
        Public Property Unit As String
    End Class

End Namespace
