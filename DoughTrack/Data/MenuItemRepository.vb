Imports MySql.Data.MySqlClient
Imports DoughTrack.Models

Namespace Data

    ''' <summary>
    ''' SQL for menu_items. Recipe lines saved with an item use the same transaction.
    ''' </summary>
    Public Module MenuItemRepository

        Public Function Search(nameSearch As String, category As String) As List(Of MenuItem)
            Dim pattern As String = "%" & EscapeLike(If(nameSearch, "").Trim()) & "%"
            Const sql As String =
                "SELECT menu_item_id, menu_item_name, category, selling_price, image_path, status " &
                "FROM menu_items " &
                "WHERE menu_item_name LIKE @search ESCAPE '\\' " &
                "AND (@category = '' OR category = @category) " &
                "ORDER BY menu_item_name"

            Dim items As New List(Of MenuItem)
            Using conn As MySqlConnection = CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.Add(Param("@search", MySqlDbType.VarChar, pattern))
                    cmd.Parameters.Add(Param("@category", MySqlDbType.VarChar, If(category, "").Trim()))
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            items.Add(ReadItem(reader))
                        End While
                    End Using
                End Using
            End Using
            Return items
        End Function

        Public Function GetCategories() As List(Of String)
            Const sql As String = "SELECT DISTINCT category FROM menu_items ORDER BY category"
            Dim categories As New List(Of String)
            Using conn As MySqlConnection = CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            categories.Add(reader.GetString(0))
                        End While
                    End Using
                End Using
            End Using
            Return categories
        End Function

        Public Function GetById(menuItemId As Integer) As MenuItem
            Const sql As String =
                "SELECT menu_item_id, menu_item_name, category, selling_price, image_path, status " &
                "FROM menu_items WHERE menu_item_id = @id"
            Using conn As MySqlConnection = CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, menuItemId))
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then Return ReadItem(reader)
                    End Using
                End Using
            End Using
            Return Nothing
        End Function

        ''' <summary>Inactive items stay in the menu list and in old sales, but they cannot be sold.</summary>
        Public Function CanBeSold(menuItemId As Integer) As Boolean
            Const sql As String =
                "SELECT COUNT(*) FROM menu_items WHERE menu_item_id = @id AND status = 'Active'"
            Return ExecuteScalarInt(sql, Param("@id", MySqlDbType.Int32, menuItemId)) = 1
        End Function

        Public Function CountSoldInLast30Days(menuItemId As Integer) As Integer
            Const sql As String =
                "SELECT COALESCE(SUM(si.quantity), 0) " &
                "FROM sale_items si " &
                "INNER JOIN sales s ON s.sale_id = si.sale_id " &
                "WHERE si.menu_item_id = @id AND s.sale_date_time >= @start"
            Return ExecuteScalarInt(sql,
                Param("@id", MySqlDbType.Int32, menuItemId),
                Param("@start", MySqlDbType.DateTime, Date.Today.AddDays(-30)))
        End Function

        Public Sub SetStatus(menuItemId As Integer, status As String)
            status = NormalizeStatus(status)
            Const sql As String = "UPDATE menu_items SET status = @status WHERE menu_item_id = @id"
            Try
                ExecuteInTransaction(
                    Sub(conn, tx)
                        Using cmd As New MySqlCommand(sql, conn, tx)
                            cmd.Parameters.Add(Param("@status", MySqlDbType.VarChar, status))
                            cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, menuItemId))
                            If cmd.ExecuteNonQuery() <> 1 Then
                                Throw New InvalidOperationException("That menu item no longer exists.")
                            End If
                        End Using
                    End Sub)
            Catch ex As Exception
                Throw Friendly(ex)
            End Try
        End Sub

        ''' <summary>
        ''' Inserts or updates the menu item and its recipe lines together.
        ''' Removes only recipe rows the admin took off this item. Never deletes the menu item.
        ''' </summary>
        Public Sub SaveWithRecipe(item As MenuItem, lines As IList(Of RecipeLine))
            ValidateItem(item)
            Try
                ExecuteInTransaction(
                    Sub(conn, tx)
                        If NameTaken(conn, tx, item.MenuItemName, item.MenuItemId) Then
                            Throw New InvalidOperationException("A menu item with that name already exists.")
                        End If
                        If item.MenuItemId = 0 Then
                            item.MenuItemId = InsertItem(conn, tx, item)
                        Else
                            UpdateItem(conn, tx, item)
                        End If
                        MenuItemIngredientRepository.ReplaceLines(conn, tx, item.MenuItemId, lines)
                    End Sub)
            Catch ex As Exception
                Throw Friendly(ex)
            End Try
        End Sub

        Private Function InsertItem(conn As MySqlConnection, tx As MySqlTransaction, item As MenuItem) As Integer
            Const sql As String =
                "INSERT INTO menu_items (menu_item_name, category, selling_price, image_path, status) " &
                "VALUES (@name, @category, @price, @image, @status)"
            Using cmd As New MySqlCommand(sql, conn, tx)
                AddItemParameters(cmd, item)
                cmd.ExecuteNonQuery()
                Return CInt(cmd.LastInsertedId)
            End Using
        End Function

        Private Sub UpdateItem(conn As MySqlConnection, tx As MySqlTransaction, item As MenuItem)
            Const sql As String =
                "UPDATE menu_items SET menu_item_name = @name, category = @category, " &
                "selling_price = @price, image_path = @image, status = @status " &
                "WHERE menu_item_id = @id"
            Using cmd As New MySqlCommand(sql, conn, tx)
                AddItemParameters(cmd, item)
                cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, item.MenuItemId))
                If cmd.ExecuteNonQuery() <> 1 Then
                    Throw New InvalidOperationException("That menu item no longer exists.")
                End If
            End Using
        End Sub

        Private Sub AddItemParameters(cmd As MySqlCommand, item As MenuItem)
            cmd.Parameters.Add(Param("@name", MySqlDbType.VarChar, item.MenuItemName))
            cmd.Parameters.Add(Param("@category", MySqlDbType.VarChar, item.Category))
            cmd.Parameters.Add(Param("@price", MySqlDbType.Decimal, item.SellingPrice))
            Dim imageValue As Object = DBNull.Value
            If Not String.IsNullOrWhiteSpace(item.ImagePath) Then imageValue = item.ImagePath
            cmd.Parameters.Add(Param("@image", MySqlDbType.VarChar, imageValue))
            cmd.Parameters.Add(Param("@status", MySqlDbType.VarChar, item.Status))
        End Sub

        Private Function NameTaken(conn As MySqlConnection, tx As MySqlTransaction, name As String, exceptId As Integer) As Boolean
            Const sql As String =
                "SELECT COUNT(*) FROM menu_items " &
                "WHERE menu_item_name = @name AND menu_item_id <> @id"
            Using cmd As New MySqlCommand(sql, conn, tx)
                cmd.Parameters.Add(Param("@name", MySqlDbType.VarChar, name))
                cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, exceptId))
                Return Convert.ToInt32(cmd.ExecuteScalar()) > 0
            End Using
        End Function

        Private Sub ValidateItem(item As MenuItem)
            If item Is Nothing Then Throw New InvalidOperationException("Menu item is missing.")
            item.MenuItemName = If(item.MenuItemName, "").Trim()
            item.Category = If(item.Category, "").Trim()
            item.Status = NormalizeStatus(item.Status)
            If item.MenuItemName = "" Then Throw New InvalidOperationException("Enter a menu item name.")
            If item.MenuItemName.Length > 100 Then Throw New InvalidOperationException("Name must be 100 characters or less.")
            If item.Category = "" Then Throw New InvalidOperationException("Enter a category.")
            If item.Category.Length > 50 Then Throw New InvalidOperationException("Category must be 50 characters or less.")
            If item.SellingPrice < 0D OrElse item.SellingPrice > 99999999.99D Then
                Throw New InvalidOperationException("Selling price must be zero or greater.")
            End If
            If item.ImagePath IsNot Nothing AndAlso item.ImagePath.Length > 255 Then
                Throw New InvalidOperationException("The image path is too long.")
            End If
        End Sub

        Private Function NormalizeStatus(status As String) As String
            If status = "Active" OrElse status = "Inactive" Then Return status
            Throw New InvalidOperationException("Status must be Active or Inactive.")
        End Function

        Private Function ReadItem(reader As MySqlDataReader) As MenuItem
            Dim imagePath As String = ""
            If Not reader.IsDBNull(reader.GetOrdinal("image_path")) Then
                imagePath = reader.GetString("image_path")
            End If
            Return New MenuItem With {
                .MenuItemId = reader.GetInt32("menu_item_id"),
                .MenuItemName = reader.GetString("menu_item_name"),
                .Category = reader.GetString("category"),
                .SellingPrice = reader.GetDecimal("selling_price"),
                .ImagePath = imagePath,
                .Status = reader.GetString("status")
            }
        End Function

        Private Function EscapeLike(value As String) As String
            Return value.Replace("\", "\\").Replace("%", "\%").Replace("_", "\_")
        End Function

        Private Function Friendly(ex As Exception) As Exception
            If TypeOf ex Is InvalidOperationException Then Return ex
            Dim sqlError = TryCast(ex, MySqlException)
            If sqlError IsNot Nothing AndAlso sqlError.Number = 1062 Then
                Return New InvalidOperationException("That name is already used, or that ingredient is already on the recipe.")
            End If
            If sqlError IsNot Nothing AndAlso sqlError.Number = 3819 Then
                Return New InvalidOperationException("Check the price and the recipe quantities.")
            End If
            Return ex
        End Function

    End Module

End Namespace
