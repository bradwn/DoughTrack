Namespace Data

    ''' <summary>
    ''' SQL for stock_ins and stock_in_items. Save a stock-in with DatabaseHelper.ExecuteInTransaction.
    ''' </summary>
    Public Module StockInRepository

        Public Function GetStockIns() As DataTable
            Const sql As String =
                "SELECT h.stock_in_date AS `Date`, " &
                "h.total_cost AS `Total cost`, " &
                "u.full_name AS `Recorded by`, " &
                "COUNT(d.stock_in_item_id) AS `Lines` " &
                "FROM stock_ins h " &
                "INNER JOIN users u ON u.user_id = h.user_id " &
                "LEFT JOIN stock_in_items d ON d.stock_in_id = h.stock_in_id " &
                "GROUP BY h.stock_in_id, h.stock_in_date, h.total_cost, u.full_name " &
                "ORDER BY h.stock_in_date DESC, h.stock_in_id DESC"
            Return FillTable(sql)
        End Function

    End Module

End Namespace
