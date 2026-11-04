Imports DoughTrack.Helpers
Imports DoughTrack.Models
Imports MySql.Data.MySqlClient

Namespace Data

    ''' <summary>
    ''' SQL for inventory_items (ingredients and supplies).
    ''' Items are never deleted. They are set Inactive instead, so recipes,
    ''' sales and stock-in records that point to them stay valid.
    ''' </summary>
    Public Module InventoryItemRepository

        Private Const SelectColumns As String =
            "inventory_item_id, item_name, category, quantity, unit, reorder_point, unit_cost, status"

        ''' <summary>Items matching the search text and category ("" = all categories).</summary>
        Public Function Search(searchText As String, category As String) As List(Of InventoryItem)
            Dim pattern As String = "%" & EscapeLike(If(searchText, "").Trim()) & "%"
            Const sql As String =
                "SELECT " & SelectColumns & " FROM inventory_items " &
                "WHERE item_name LIKE @search ESCAPE '\\' " &
                "AND (@category = '' OR category = @category) " &
                "ORDER BY item_name"
            Return ReadItems(sql,
                Param("@search", MySqlDbType.VarChar, pattern),
                Param("@category", MySqlDbType.VarChar, If(category, "").Trim()))
        End Function

        ''' <summary>Active items only. Used by the Stock-In item picker.</summary>
        Public Function GetActive() As List(Of InventoryItem)
            Const sql As String =
                "SELECT " & SelectColumns & " FROM inventory_items WHERE status = 'Active' ORDER BY item_name"
            Return ReadItems(sql)
        End Function

        Public Function GetById(inventoryItemId As Integer) As InventoryItem
            Const sql As String =
                "SELECT " & SelectColumns & " FROM inventory_items WHERE inventory_item_id = @id"
            Dim rows As List(Of InventoryItem) = ReadItems(sql, Param("@id", MySqlDbType.Int32, inventoryItemId))
            Return If(rows.Count > 0, rows(0), Nothing)
        End Function

        ''' <summary>Active items whose quantity is at or below the reorder point (includes out of stock).</summary>
        Public Function GetLowStock() As DataTable
            Const sql As String =
                "SELECT item_name AS `Item`, " &
                "category AS `Category`, " &
                "quantity AS `On hand`, " &
                "unit AS `Unit`, " &
                "reorder_point AS `Reorder point`, " &
                "CASE WHEN quantity <= 0 THEN 'Out of Stock' ELSE 'Low' END AS `Status` " &
                "FROM inventory_items " &
                "WHERE status = 'Active' AND quantity <= reorder_point " &
                "ORDER BY quantity, item_name"
            Return FillTable(sql)
        End Function

        ''' <summary>Every inventory row, for recipe pickers. Includes inactive records.</summary>
        Public Function GetForRecipe() As List(Of InventoryItem)
            Const sql As String =
                "SELECT " & SelectColumns & " FROM inventory_items ORDER BY status, item_name"
            Return ReadItems(sql)
        End Function

        ''' <summary>True when a recipe or a stock-in record uses this item.</summary>
        Public Function HasReferences(inventoryItemId As Integer) As Boolean
            Const sql As String =
                "SELECT (SELECT COUNT(*) FROM menu_item_ingredients WHERE inventory_item_id = @id) + " &
                "(SELECT COUNT(*) FROM stock_in_items WHERE inventory_item_id = @id)"
            Return ExecuteScalarInt(sql, Param("@id", MySqlDbType.Int32, inventoryItemId)) > 0
        End Function

        ''' <summary>Adds a new item and returns its id.</summary>
        Public Function Add(item As InventoryItem) As Integer
            Validate(item)
            Const sql As String =
                "INSERT INTO inventory_items (item_name, category, quantity, unit, reorder_point, unit_cost, status) " &
                "VALUES (@name, @category, @quantity, @unit, @reorder, @cost, @status)"
            Dim newId As Integer
            Try
                Using conn As MySqlConnection = CreateConnection()
                    conn.Open()
                    Using cmd As New MySqlCommand(sql, conn)
                        AddItemParameters(cmd, item)
                        cmd.ExecuteNonQuery()
                        newId = CInt(cmd.LastInsertedId)
                    End Using
                End Using
            Catch ex As MySqlException When ex.Number = 1062
                Throw New InvalidOperationException("An inventory item named """ & item.ItemName & """ already exists.")
            End Try
            Return newId
        End Function

        ''' <summary>Updates an existing item.</summary>
        Public Sub Update(item As InventoryItem)
            Validate(item)
            Dim current As InventoryItem = GetById(item.InventoryItemId)
            If current Is Nothing Then
                Throw New InvalidOperationException("This inventory item no longer exists.")
            End If
            ' Changing the unit would silently change what recipes and stock-in lines mean.
            If Not current.Unit.Equals(item.Unit, StringComparison.OrdinalIgnoreCase) AndAlso
               HasReferences(item.InventoryItemId) Then
                Throw New InvalidOperationException(
                    "The unit cannot be changed because this item is used in a recipe or a stock-in record.")
            End If

            Const sql As String =
                "UPDATE inventory_items SET item_name = @name, category = @category, quantity = @quantity, " &
                "unit = @unit, reorder_point = @reorder, unit_cost = @cost, status = @status " &
                "WHERE inventory_item_id = @id"
            Try
                Using conn As MySqlConnection = CreateConnection()
                    conn.Open()
                    Using cmd As New MySqlCommand(sql, conn)
                        AddItemParameters(cmd, item)
                        cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, item.InventoryItemId))
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
            Catch ex As MySqlException When ex.Number = 1062
                Throw New InvalidOperationException("An inventory item named """ & item.ItemName & """ already exists.")
            End Try
        End Sub

        ''' <summary>Sets Active or Inactive. This is the safe replacement for deleting.</summary>
        Public Sub SetStatus(inventoryItemId As Integer, status As String)
            If status <> "Active" AndAlso status <> "Inactive" Then
                Throw New InvalidOperationException("Status must be Active or Inactive.")
            End If
            Const sql As String = "UPDATE inventory_items SET status = @status WHERE inventory_item_id = @id"
            Using conn As MySqlConnection = CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.Add(Param("@status", MySqlDbType.VarChar, status))
                    cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, inventoryItemId))
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        Private Sub Validate(item As InventoryItem)
            If String.IsNullOrWhiteSpace(item.ItemName) Then
                Throw New InvalidOperationException("Item name is required.")
            End If
            If item.ItemName.Trim().Length > 100 Then
                Throw New InvalidOperationException("Item name must be 100 characters or fewer.")
            End If
            If item.Category <> "Ingredients" AndAlso item.Category <> "Supplies" Then
                Throw New InvalidOperationException("Choose Ingredients or Supplies.")
            End If
            If String.IsNullOrWhiteSpace(item.Unit) OrElse item.Unit.Trim().Length > 10 Then
                Throw New InvalidOperationException("Choose a unit.")
            End If
            If item.Quantity < 0D Then
                Throw New InvalidOperationException("Quantity cannot be negative.")
            End If
            If item.ReorderPoint < 0D Then
                Throw New InvalidOperationException("Reorder point cannot be negative.")
            End If
            If item.UnitCost < 0D Then
                Throw New InvalidOperationException("Unit cost cannot be negative.")
            End If
            If item.Status <> "Active" AndAlso item.Status <> "Inactive" Then
                Throw New InvalidOperationException("Status must be Active or Inactive.")
            End If
        End Sub

        Private Sub AddItemParameters(cmd As MySqlCommand, item As InventoryItem)
            cmd.Parameters.Add(Param("@name", MySqlDbType.VarChar, item.ItemName.Trim()))
            cmd.Parameters.Add(Param("@category", MySqlDbType.VarChar, item.Category))
            cmd.Parameters.Add(DecimalParam("@quantity", UnitConverter.RoundQuantity(item.Quantity), 3))
            cmd.Parameters.Add(Param("@unit", MySqlDbType.VarChar, item.Unit.Trim()))
            cmd.Parameters.Add(DecimalParam("@reorder", UnitConverter.RoundQuantity(item.ReorderPoint), 3))
            cmd.Parameters.Add(DecimalParam("@cost", UnitConverter.RoundCost(item.UnitCost), 4))
            cmd.Parameters.Add(Param("@status", MySqlDbType.VarChar, item.Status))
        End Sub

        Private Function ReadItems(sql As String, ParamArray parameters() As MySqlParameter) As List(Of InventoryItem)
            Dim items As New List(Of InventoryItem)
            Using conn As MySqlConnection = CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    If parameters IsNot Nothing AndAlso parameters.Length > 0 Then
                        cmd.Parameters.AddRange(parameters)
                    End If
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

        Private Function EscapeLike(value As String) As String
            Return value.Replace("\", "\\").Replace("%", "\%").Replace("_", "\_")
        End Function

    End Module

End Namespace
