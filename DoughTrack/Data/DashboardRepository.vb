Imports MySql.Data.MySqlClient

Namespace Data

    ''' <summary>
    ''' Dashboard figures from sales, sale items, recipes, inventory, and expenses.
    ''' There is no dashboard table.
    ''' Every row in sales is a completed sale; that table has no status column.
    ''' </summary>
    Public Module DashboardRepository

        Public Function GetTodaysSales(day As Date) As (Total As Decimal, Count As Integer)
            Const sql As String =
                "SELECT COALESCE(SUM(total_amount), 0) " &
                "FROM sales " &
                "WHERE sale_date_time >= @start AND sale_date_time < @end"

            Const countSql As String =
                "SELECT COUNT(*) " &
                "FROM sales " &
                "WHERE sale_date_time >= @start AND sale_date_time < @end"

            Return (ExecuteScalarDecimal(sql, DayParameters(day)),
                    ExecuteScalarInt(countSql, DayParameters(day)))
        End Function

        ''' <summary>
        ''' Today's sales revenue minus estimated cost of goods sold.
        ''' Cost is sale quantity × recipe quantity × the inventory item's current unit cost.
        ''' Items without a recipe contribute no cost.
        ''' </summary>
        Public Function GetTodaysGrossProfit(day As Date) As (Profit As Decimal, Revenue As Decimal, Cost As Decimal)
            Const costSql As String =
                "SELECT COALESCE(SUM(si.quantity * mii.quantity_required * ii.unit_cost), 0) " &
                "FROM sale_items si " &
                "INNER JOIN sales s ON s.sale_id = si.sale_id " &
                "INNER JOIN menu_item_ingredients mii ON mii.menu_item_id = si.menu_item_id " &
                "INNER JOIN inventory_items ii ON ii.inventory_item_id = mii.inventory_item_id " &
                "WHERE s.sale_date_time >= @start AND s.sale_date_time < @end"

            Dim revenue As Decimal = GetTodaysSales(day).Total
            Dim cost As Decimal = ExecuteScalarDecimal(costSql, DayParameters(day))
            Return (revenue - cost, revenue, cost)
        End Function

        Public Function GetTodaysExpenses(day As Date) As (Total As Decimal, Count As Integer)
            Const sql As String =
                "SELECT COALESCE(SUM(amount), 0) FROM expenses WHERE expense_date = @day"
            Const countSql As String =
                "SELECT COUNT(*) FROM expenses WHERE expense_date = @day"
            Dim dayParameter As MySqlParameter = Param("@day", MySqlDbType.Date, day.Date)
            Dim dayParameterForCount As MySqlParameter = Param("@day", MySqlDbType.Date, day.Date)
            Return (ExecuteScalarDecimal(sql, dayParameter),
                    ExecuteScalarInt(countSql, dayParameterForCount))
        End Function

        ''' <summary>
        ''' Menu items ranked by quantity sold on completed sales. Limited to the top 10.
        ''' </summary>
        Public Function GetFastMovingItems() As DataTable
            Const sql As String =
                "SELECT mi.menu_item_name AS `Item`, " &
                "SUM(si.quantity) AS `Quantity sold`, " &
                "SUM(si.subtotal) AS `Revenue` " &
                "FROM sale_items si " &
                "INNER JOIN menu_items mi ON mi.menu_item_id = si.menu_item_id " &
                "GROUP BY mi.menu_item_id, mi.menu_item_name " &
                "ORDER BY `Quantity sold` DESC, mi.menu_item_name " &
                "LIMIT 10"
            Return FillTable(sql)
        End Function

        Private Function DayParameters(day As Date) As MySqlParameter()
            Return {
                Param("@start", MySqlDbType.DateTime, day.Date),
                Param("@end", MySqlDbType.DateTime, day.Date.AddDays(1))
            }
        End Function

    End Module

End Namespace
