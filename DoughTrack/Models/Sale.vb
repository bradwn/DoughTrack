Namespace Models

    ''' <summary>
    ''' One row of the sales table.
    ''' </summary>
    Public Class Sale
        Public Property SaleId As Integer
        Public Property SaleDateTime As DateTime
        Public Property TotalAmount As Decimal
        Public Property UserId As Integer

        ''' <summary>Line summary built from the quantities and names saved on the sale.</summary>
        Public Property ItemSummary As String

        Public Property Lines As New List(Of SaleItem)
    End Class

End Namespace
