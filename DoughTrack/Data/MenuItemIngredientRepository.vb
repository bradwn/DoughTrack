Imports MySql.Data.MySqlClient
Imports DoughTrack.Models

Namespace Data

    ''' <summary>
    ''' SQL for menu_item_ingredients. A menu item can have many ingredients and supplies.
    ''' </summary>
    Public Module MenuItemIngredientRepository

        Public Function GetLines(menuItemId As Integer) As List(Of RecipeLine)
            Const sql As String =
                "SELECT mii.menu_item_ingredient_id, mii.menu_item_id, mii.inventory_item_id, " &
                "ii.item_name, ii.category, mii.quantity_required, mii.unit " &
                "FROM menu_item_ingredients mii " &
                "INNER JOIN inventory_items ii ON ii.inventory_item_id = mii.inventory_item_id " &
                "WHERE mii.menu_item_id = @id " &
                "ORDER BY ii.item_name"
            Dim lines As New List(Of RecipeLine)
            Using conn As MySqlConnection = CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, menuItemId))
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            lines.Add(ReadLine(reader))
                        End While
                    End Using
                End Using
            End Using
            Return lines
        End Function

        Public Sub AddLine(menuItemId As Integer, inventoryItemId As Integer, quantity As Decimal)
            RequireQuantity(quantity)
            Try
                ExecuteInTransaction(
                    Sub(conn, tx)
                        InsertLine(conn, tx, menuItemId, inventoryItemId, quantity)
                    End Sub)
            Catch ex As Exception
                Throw Friendly(ex)
            End Try
        End Sub

        Public Sub UpdateQuantity(menuItemId As Integer, lineId As Integer, quantity As Decimal)
            RequireQuantity(quantity)
            Const sql As String =
                "UPDATE menu_item_ingredients SET quantity_required = @qty " &
                "WHERE menu_item_ingredient_id = @id AND menu_item_id = @menuId"
            Try
                ExecuteInTransaction(
                    Sub(conn, tx)
                        Using cmd As New MySqlCommand(sql, conn, tx)
                            cmd.Parameters.Add(Param("@qty", MySqlDbType.Decimal, quantity))
                            cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, lineId))
                            cmd.Parameters.Add(Param("@menuId", MySqlDbType.Int32, menuItemId))
                            If cmd.ExecuteNonQuery() <> 1 Then
                                Throw New InvalidOperationException("That recipe line no longer exists.")
                            End If
                        End Using
                    End Sub)
            Catch ex As Exception
                Throw Friendly(ex)
            End Try
        End Sub

        Public Sub RemoveLine(menuItemId As Integer, lineId As Integer)
            Const sql As String =
                "DELETE FROM menu_item_ingredients " &
                "WHERE menu_item_ingredient_id = @id AND menu_item_id = @menuId"
            Try
                ExecuteInTransaction(
                    Sub(conn, tx)
                        Using cmd As New MySqlCommand(sql, conn, tx)
                            cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, lineId))
                            cmd.Parameters.Add(Param("@menuId", MySqlDbType.Int32, menuItemId))
                            If cmd.ExecuteNonQuery() <> 1 Then
                                Throw New InvalidOperationException("That recipe line no longer exists.")
                            End If
                        End Using
                    End Sub)
            Catch ex As Exception
                Throw Friendly(ex)
            End Try
        End Sub

        ''' <summary>Replaces the recipe inside a transaction that also saves the menu item.</summary>
        Friend Sub ReplaceLines(conn As MySqlConnection, tx As MySqlTransaction, menuItemId As Integer, lines As IList(Of RecipeLine))
            Dim incoming As List(Of RecipeLine) = If(lines, New List(Of RecipeLine)).ToList()
            Dim seen As New HashSet(Of Integer)
            For Each line As RecipeLine In incoming
                RequireQuantity(line.QuantityRequired)
                If line.InventoryItemId <= 0 Then
                    Throw New InvalidOperationException("Choose an inventory item for every recipe line.")
                End If
                If Not seen.Add(line.InventoryItemId) Then
                    Throw New InvalidOperationException("Each inventory item can appear only once on a recipe.")
                End If
            Next

            Dim existing As HashSet(Of Integer) = LoadLineIds(conn, tx, menuItemId)
            Dim keep As New HashSet(Of Integer)
            For Each line As RecipeLine In incoming
                If line.MenuItemIngredientId = 0 Then
                    line.MenuItemIngredientId = InsertLine(conn, tx, menuItemId, line.InventoryItemId, line.QuantityRequired)
                Else
                    If Not existing.Contains(line.MenuItemIngredientId) Then
                        Throw New InvalidOperationException("A recipe line on this item has changed. Close the form and open it again.")
                    End If
                    UpdateLine(conn, tx, menuItemId, line)
                End If
                keep.Add(line.MenuItemIngredientId)
            Next

            For Each oldId As Integer In existing
                If Not keep.Contains(oldId) Then
                    DeleteLine(conn, tx, menuItemId, oldId)
                End If
            Next
        End Sub

        Private Function InsertLine(conn As MySqlConnection, tx As MySqlTransaction, menuItemId As Integer, inventoryItemId As Integer, quantity As Decimal) As Integer
            Dim unit As String = RequireInventoryUnit(conn, tx, inventoryItemId)
            Const sql As String =
                "INSERT INTO menu_item_ingredients (menu_item_id, inventory_item_id, quantity_required, unit) " &
                "VALUES (@menuId, @itemId, @qty, @unit)"
            Using cmd As New MySqlCommand(sql, conn, tx)
                cmd.Parameters.Add(Param("@menuId", MySqlDbType.Int32, menuItemId))
                cmd.Parameters.Add(Param("@itemId", MySqlDbType.Int32, inventoryItemId))
                cmd.Parameters.Add(Param("@qty", MySqlDbType.Decimal, quantity))
                cmd.Parameters.Add(Param("@unit", MySqlDbType.VarChar, unit))
                cmd.ExecuteNonQuery()
                Return CInt(cmd.LastInsertedId)
            End Using
        End Function

        Private Sub UpdateLine(conn As MySqlConnection, tx As MySqlTransaction, menuItemId As Integer, line As RecipeLine)
            Dim unit As String = RequireInventoryUnit(conn, tx, line.InventoryItemId)
            Const sql As String =
                "UPDATE menu_item_ingredients SET quantity_required = @qty, unit = @unit, inventory_item_id = @itemId " &
                "WHERE menu_item_ingredient_id = @id AND menu_item_id = @menuId"
            Using cmd As New MySqlCommand(sql, conn, tx)
                cmd.Parameters.Add(Param("@qty", MySqlDbType.Decimal, line.QuantityRequired))
                cmd.Parameters.Add(Param("@unit", MySqlDbType.VarChar, unit))
                cmd.Parameters.Add(Param("@itemId", MySqlDbType.Int32, line.InventoryItemId))
                cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, line.MenuItemIngredientId))
                cmd.Parameters.Add(Param("@menuId", MySqlDbType.Int32, menuItemId))
                If cmd.ExecuteNonQuery() <> 1 Then
                    Throw New InvalidOperationException("That recipe line no longer exists.")
                End If
            End Using
        End Sub

        Private Sub DeleteLine(conn As MySqlConnection, tx As MySqlTransaction, menuItemId As Integer, lineId As Integer)
            Const sql As String =
                "DELETE FROM menu_item_ingredients " &
                "WHERE menu_item_ingredient_id = @id AND menu_item_id = @menuId"
            Using cmd As New MySqlCommand(sql, conn, tx)
                cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, lineId))
                cmd.Parameters.Add(Param("@menuId", MySqlDbType.Int32, menuItemId))
                cmd.ExecuteNonQuery()
            End Using
        End Sub

        Private Function LoadLineIds(conn As MySqlConnection, tx As MySqlTransaction, menuItemId As Integer) As HashSet(Of Integer)
            Const sql As String =
                "SELECT menu_item_ingredient_id FROM menu_item_ingredients WHERE menu_item_id = @id"
            Dim ids As New HashSet(Of Integer)
            Using cmd As New MySqlCommand(sql, conn, tx)
                cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, menuItemId))
                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        ids.Add(reader.GetInt32(0))
                    End While
                End Using
            End Using
            Return ids
        End Function

        Private Function RequireInventoryUnit(conn As MySqlConnection, tx As MySqlTransaction, inventoryItemId As Integer) As String
            Const sql As String = "SELECT unit FROM inventory_items WHERE inventory_item_id = @id"
            Using cmd As New MySqlCommand(sql, conn, tx)
                cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, inventoryItemId))
                Dim value As Object = cmd.ExecuteScalar()
                If value Is Nothing OrElse IsDBNull(value) Then
                    Throw New InvalidOperationException("Choose an inventory item that already exists.")
                End If
                Dim unit As String = Convert.ToString(value)
                If unit = "" OrElse unit.Length > 10 Then
                    Throw New InvalidOperationException("That inventory item has a unit DoughTrack cannot store on a recipe.")
                End If
                Return unit
            End Using
        End Function

        Private Sub RequireQuantity(quantity As Decimal)
            If quantity <= 0D OrElse quantity > 999999.999D Then
                Throw New InvalidOperationException("Recipe quantity must be greater than zero.")
            End If
        End Sub

        Private Function ReadLine(reader As MySqlDataReader) As RecipeLine
            Return New RecipeLine With {
                .MenuItemIngredientId = reader.GetInt32("menu_item_ingredient_id"),
                .MenuItemId = reader.GetInt32("menu_item_id"),
                .InventoryItemId = reader.GetInt32("inventory_item_id"),
                .ItemName = reader.GetString("item_name"),
                .InventoryCategory = reader.GetString("category"),
                .QuantityRequired = reader.GetDecimal("quantity_required"),
                .Unit = reader.GetString("unit")
            }
        End Function

        Private Function Friendly(ex As Exception) As Exception
            If TypeOf ex Is InvalidOperationException Then Return ex
            Dim sqlError = TryCast(ex, MySqlException)
            If sqlError IsNot Nothing AndAlso sqlError.Number = 1062 Then
                Return New InvalidOperationException("That inventory item is already on this recipe.")
            End If
            If sqlError IsNot Nothing AndAlso sqlError.Number = 3819 Then
                Return New InvalidOperationException("Recipe quantity must be greater than zero.")
            End If
            Return ex
        End Function

    End Module

End Namespace
