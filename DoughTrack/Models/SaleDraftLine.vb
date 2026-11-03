Imports DoughTrack.Helpers

Namespace Models

    ''' <summary>
    ''' One line in the order, before it is saved.
    ''' </summary>
    Public Class SaleDraftLine
        Public Property MenuItemId As Integer
        Public Property ItemName As String
        Public Property UnitPrice As Decimal
        Public Property Quantity As Integer
        Public Property Available As Integer

        Public ReadOnly Property Subtotal As Decimal
            Get
                Return UnitConverter.RoundMoney(UnitPrice * Quantity)
            End Get
        End Property
    End Class

    ''' <summary>
    ''' An active menu item plus how many servings current stock can make.
    ''' </summary>
    Public Class MenuOffer
        Public Property MenuItemId As Integer
        Public Property Name As String
        Public Property Category As String
        Public Property SellingPrice As Decimal
        Public Property ImagePath As String
        Public Property CanMake As Integer

        Public ReadOnly Property IsAvailable As Boolean
            Get
                Return CanMake > 0
            End Get
        End Property
    End Class

    ''' <summary>
    ''' A sale that was committed, including the saved cost snapshot.
    ''' </summary>
    Public Class CompletedSale
        Public Property SaleId As Integer
        Public Property TotalAmount As Decimal
        Public Property CogsAmount As Decimal
    End Class

End Namespace
