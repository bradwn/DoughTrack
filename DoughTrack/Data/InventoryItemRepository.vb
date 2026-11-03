Imports DoughTrack.Models
Imports MySql.Data.MySqlClient

Namespace Data

    ''' <summary>
    ''' SQL for inventory_items. Screen queries are added with the Inventory module.
    ''' </summary>
    Public Module InventoryItemRepository

        Public Function GetStocks() As DataTable
            Const sql As String =
                "SELECT item_name AS `Item`, " &
                "category AS `Category`, " &
                "quantity AS `On hand`, " &
                "unit AS `Unit`, " &
                "reorder_point AS `Reorder point`, " &
                "unit_cost AS `Unit cost`, " &
                "status AS `Status` " &
                "FROM inventory_items " &
                "ORDER BY item_name"
            Return FillTable(sql)
        End Function

        ''' <summary>Active items whose on-hand quantity is at or below the reorder point.</summary>
        Public Function GetLowStock() As DataTable
            Const sql As String =
                "SELECT item_name AS `Item`, " &
                "category AS `Category`, " &
                "quantity AS `On hand`, " &
                "unit AS `Unit`, " &
                "reorder_point AS `Reorder point` " &
                "FROM inventory_items " &
                "WHERE status = 'Active' AND quantity <= reorder_point " &
                "ORDER BY quantity, item_name"
            Return FillTable(sql)
        End Function

        ''' <summary>Every inventory row, for recipe pickers. Includes inactive records.</summary>
        Public Function GetForRecipe() As List(Of InventoryItem)
            Const sql As String =
                "SELECT inventory_item_id, item_name, category, quantity, unit, " &
                "reorder_point, unit_cost, status " &
                "FROM inventory_items ORDER BY item_name"
            Dim items As New List(Of InventoryItem)
            Using conn As MySqlConnection = CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            items.Add(New InventoryItem With {
                                .InventoryItemId = reader.GetInt32("inventory_item_id"),
                                .ItemName = reader.GetString("item_name"),
                                .Category = reader.GetString("category"),
                                .Quantity = reader.GetDecimal("quantity"),
                                .Unit = reader.GetString("unit"),
                                .ReorderPoint = reader.GetDecimal("reorder_point"),
                                .UnitCost = reader.GetDecimal("unit_cost"),
                                .Status = reader.GetString("status")
                            })
                        End While
                    End Using
                End Using
            End Using
            Return items
        End Function

    End Module

End Namespace
