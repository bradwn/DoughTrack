Namespace Models

    ''' <summary>
    ''' One row of the expenses table.
    ''' </summary>
    Public Class Expense
        Public Property ExpenseId As Integer
        Public Property ExpenseDate As Date
        Public Property Category As String
        Public Property Amount As Decimal
        Public Property Description As String
    End Class

End Namespace
