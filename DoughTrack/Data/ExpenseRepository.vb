Namespace Data

    ''' <summary>
    ''' SQL for expenses. Screen queries are added with the Expenses module.
    ''' </summary>
    Public Module ExpenseRepository

        Public Function GetExpenses() As DataTable
            Const sql As String =
                "SELECT expense_date AS `Date`, " &
                "category AS `Category`, " &
                "amount AS `Amount`, " &
                "IFNULL(description, '') AS `Description` " &
                "FROM expenses " &
                "ORDER BY expense_date DESC, expense_id DESC"
            Return FillTable(sql)
        End Function

    End Module

End Namespace
