Imports MySql.Data.MySqlClient

Namespace Data

    ''' <summary>
    ''' SQL for sales and sale_items. Save a sale with DatabaseHelper.ExecuteInTransaction.
    ''' A sale may include only items MenuItemRepository.CanBeSold accepts.
    ''' Inactive menu items stay on these history queries.
    ''' </summary>
    Public Module SaleRepository

        ''' <summary>Line items from completed sales whose sale time is on this day.</summary>
        Public Function GetTodaysSaleItems(day As Date) As DataTable
            Const sql As String =
                "SELECT s.sale_date_time AS `Sold at`, " &
                "mi.menu_item_name AS `Item`, " &
                "si.quantity AS `Quantity`, " &
                "si.unit_price AS `Unit price`, " &
                "si.subtotal AS `Subtotal` " &
                "FROM sale_items si " &
                "INNER JOIN sales s ON s.sale_id = si.sale_id " &
                "INNER JOIN menu_items mi ON mi.menu_item_id = si.menu_item_id " &
                "WHERE s.sale_date_time >= @start AND s.sale_date_time < @end " &
                "ORDER BY s.sale_date_time DESC, si.sale_item_id DESC"
            Return FillTable(sql,
                Param("@start", MySqlDbType.DateTime, day.Date),
                Param("@end", MySqlDbType.DateTime, day.Date.AddDays(1)))
        End Function

        ''' <summary>Every completed sale. The sales table has no status column.</summary>
        Public Function GetSalesHistory() As DataTable
            Const sql As String =
                "SELECT s.sale_date_time AS `Sold at`, " &
                "s.total_amount AS `Total`, " &
                "u.full_name AS `Recorded by` " &
                "FROM sales s " &
                "INNER JOIN users u ON u.user_id = s.user_id " &
                "ORDER BY s.sale_date_time DESC, s.sale_id DESC"
            Return FillTable(sql)
        End Function

    End Module

End Namespace
