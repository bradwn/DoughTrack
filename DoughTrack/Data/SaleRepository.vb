Imports DoughTrack.Helpers
Imports DoughTrack.Models
Imports MySql.Data.MySqlClient

Namespace Data

    ''' <summary>
    ''' Sales and sale lines. Completing a sale writes the sale, its lines,
    ''' the cost snapshot, and the inventory deduction in one transaction.
    ''' </summary>
    Public Module SaleRepository

        Private ReadOnly SnapshotRequiredMessage As String =
            "Sales cannot be saved yet. In MySQL Workbench, select doughtrack_db and run " &
            "Database/Migrations/002_sale_cogs_snapshot.sql as an administrator. " &
            "That script only adds columns. It does not delete data."

        Public Function GetActiveCategories() As List(Of String)
            Const sql As String =
                "SELECT DISTINCT category FROM menu_items WHERE status = 'Active' ORDER BY category"
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

        ''' <summary>Active menu items and how many servings on-hand stock can make.</summary>
        Public Function GetOffers(nameSearch As String, category As String) As List(Of MenuOffer)
            Dim pattern As String = "%" & EscapeLike(If(nameSearch, "").Trim()) & "%"
            Const sql As String =
                "SELECT mi.menu_item_id, mi.menu_item_name, mi.category, mi.selling_price, " &
                "IFNULL(mi.image_path, '') AS image_path, " &
                "mii.quantity_required, mii.unit AS recipe_unit, " &
                "ii.inventory_item_id, ii.item_name, ii.quantity AS on_hand, ii.unit AS stock_unit " &
                "FROM menu_items mi " &
                "LEFT JOIN menu_item_ingredients mii ON mii.menu_item_id = mi.menu_item_id " &
                "LEFT JOIN inventory_items ii ON ii.inventory_item_id = mii.inventory_item_id " &
                "WHERE mi.status = 'Active' " &
                "AND mi.menu_item_name LIKE @search ESCAPE '\\' " &
                "AND (@category = '' OR mi.category = @category) " &
                "ORDER BY mi.menu_item_name, ii.item_name"

            Dim grouped As New Dictionary(Of Integer, OfferBuilder)
            Using conn As MySqlConnection = CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.Add(Param("@search", MySqlDbType.VarChar, pattern))
                    cmd.Parameters.Add(Param("@category", MySqlDbType.VarChar, If(category, "").Trim()))
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim id As Integer = reader.GetInt32("menu_item_id")
                            Dim builder As OfferBuilder = Nothing
                            If Not grouped.TryGetValue(id, builder) Then
                                builder = New OfferBuilder With {
                                    .Offer = New MenuOffer With {
                                        .MenuItemId = id,
                                        .Name = reader.GetString("menu_item_name"),
                                        .Category = reader.GetString("category"),
                                        .SellingPrice = reader.GetDecimal("selling_price"),
                                        .ImagePath = reader.GetString("image_path"),
                                        .CanMake = 0
                                    }
                                }
                                grouped.Add(id, builder)
                            End If
                            If reader.IsDBNull(reader.GetOrdinal("quantity_required")) Then Continue While
                            builder.HasRecipe = True
                            builder.Parts.Add(ReadPart(reader))
                        End While
                    End Using
                End Using
            End Using

            Dim offers As New List(Of MenuOffer)
            For Each builder As OfferBuilder In grouped.Values
                builder.Offer.CanMake = ServingsAvailable(builder)
                offers.Add(builder.Offer)
            Next
            Return offers
        End Function

        Public Function GetTodaysSaleItems(day As Date) As DataTable
            Dim hasName As Boolean = ColumnExists("sale_items", "menu_item_name")
            Dim sql As String =
                "SELECT s.sale_date_time AS `Sold at`, " &
                ItemLabelSql("si", hasName) & " AS `Item`, " &
                "si.quantity AS `Quantity`, " &
                "si.unit_price AS `Unit price`, " &
                "si.subtotal AS `Subtotal` " &
                "FROM sale_items si " &
                "INNER JOIN sales s ON s.sale_id = si.sale_id " &
                "WHERE s.sale_date_time >= @start AND s.sale_date_time < @end " &
                "ORDER BY s.sale_date_time DESC, si.sale_item_id DESC"
            Return FillTable(sql,
                Param("@start", MySqlDbType.DateTime, day.Date),
                Param("@end", MySqlDbType.DateTime, day.Date.AddDays(1)))
        End Function

        ''' <summary>
        ''' Categories currently stored on menu items that appear in a sale.
        ''' Sale lines do not keep their own category, so this list can change if a menu category is edited.
        ''' Names and prices on the sale do not.
        ''' </summary>
        Public Function GetHistoryCategories() As List(Of String)
            Const sql As String =
                "SELECT DISTINCT mi.category " &
                "FROM sale_items si " &
                "INNER JOIN menu_items mi ON mi.menu_item_id = si.menu_item_id " &
                "ORDER BY mi.category"
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

        ''' <summary>
        ''' Completed sales. Item text, unit price, and subtotal come from sale_items.
        ''' A later edit to the menu item does not change those saved values.
        ''' </summary>
        Public Function SearchHistory(searchText As String, category As String, fromDate As Date?, toDate As Date?) As List(Of Sale)
            Dim hasName As Boolean = ColumnExists("sale_items", "menu_item_name")
            Dim label As String = ItemLabelSql("si", hasName)
            Dim searchLabel As String = ItemLabelSql("sis", hasName)
            Dim trimmed As String = If(searchText, "").Trim()
            Dim sql As String =
                "SELECT s.sale_id, s.sale_date_time, s.total_amount, s.user_id, " &
                "COALESCE(GROUP_CONCAT(CONCAT(si.quantity, ' x ', " & label & ") " &
                "ORDER BY si.sale_item_id SEPARATOR ', '), '') AS item_summary " &
                "FROM sales s " &
                "LEFT JOIN sale_items si ON si.sale_id = s.sale_id " &
                "WHERE (@fromDate IS NULL OR s.sale_date_time >= @fromDate) " &
                "AND (@toDate IS NULL OR s.sale_date_time < @toDate) " &
                "AND (@category = '' OR EXISTS (" &
                "SELECT 1 FROM sale_items sic " &
                "INNER JOIN menu_items mi ON mi.menu_item_id = sic.menu_item_id " &
                "WHERE sic.sale_id = s.sale_id AND mi.category = @category)) " &
                "AND (@hasSearch = 0 OR CAST(s.sale_id AS CHAR) LIKE @search ESCAPE '\\' " &
                "OR DATE_FORMAT(s.sale_date_time, '%Y-%m-%d') LIKE @search ESCAPE '\\' " &
                "OR DATE_FORMAT(s.sale_date_time, '%b %e, %Y') LIKE @search ESCAPE '\\' " &
                "OR DATE_FORMAT(s.sale_date_time, '%l:%i %p') LIKE @search ESCAPE '\\' " &
                "OR EXISTS (SELECT 1 FROM sale_items sis WHERE sis.sale_id = s.sale_id " &
                "AND " & searchLabel & " LIKE @search ESCAPE '\\')) " &
                "GROUP BY s.sale_id, s.sale_date_time, s.total_amount, s.user_id " &
                "ORDER BY s.sale_date_time DESC, s.sale_id DESC"

            Dim sales As New List(Of Sale)
            Using conn As MySqlConnection = CreateConnection()
                conn.Open()
                Using limitCmd As New MySqlCommand("SET SESSION group_concat_max_len = 8192", conn)
                    limitCmd.ExecuteNonQuery()
                End Using
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.Add(NullableDate("@fromDate", fromDate))
                    Dim exclusiveEnd As Date? = Nothing
                    If toDate.HasValue Then exclusiveEnd = toDate.Value.Date.AddDays(1)
                    cmd.Parameters.Add(NullableDate("@toDate", exclusiveEnd))
                    cmd.Parameters.Add(Param("@category", MySqlDbType.VarChar, If(category, "").Trim()))
                    cmd.Parameters.Add(Param("@hasSearch", MySqlDbType.Int32, If(trimmed = "", 0, 1)))
                    cmd.Parameters.Add(Param("@search", MySqlDbType.VarChar, "%" & EscapeLike(trimmed) & "%"))
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            sales.Add(New Sale With {
                                .SaleId = reader.GetInt32("sale_id"),
                                .SaleDateTime = reader.GetDateTime("sale_date_time"),
                                .TotalAmount = reader.GetDecimal("total_amount"),
                                .UserId = reader.GetInt32("user_id"),
                                .ItemSummary = reader.GetString("item_summary")
                            })
                        End While
                    End Using
                End Using
            End Using
            Return sales
        End Function

        ''' <summary>
        ''' One saved sale and its lines. Returns Nothing when the sale id is not in the database.
        ''' Line name, unit price, and subtotal are the stored snapshots.
        ''' </summary>
        Public Function GetSale(saleId As Integer) As Sale
            Const headerSql As String =
                "SELECT sale_id, sale_date_time, total_amount, user_id FROM sales WHERE sale_id = @id"
            Dim hasName As Boolean = ColumnExists("sale_items", "menu_item_name")
            Dim nameSql As String = If(hasName, "COALESCE(menu_item_name, '')", "''")
            Dim lineSql As String =
                "SELECT sale_item_id, sale_id, menu_item_id, " & nameSql & " AS menu_item_name, " &
                "quantity, unit_price, subtotal " &
                "FROM sale_items WHERE sale_id = @id ORDER BY sale_item_id"

            Using conn As MySqlConnection = CreateConnection()
                conn.Open()
                Dim sale As Sale = Nothing
                Using cmd As New MySqlCommand(headerSql, conn)
                    cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, saleId))
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If Not reader.Read() Then Return Nothing
                        sale = New Sale With {
                            .SaleId = reader.GetInt32("sale_id"),
                            .SaleDateTime = reader.GetDateTime("sale_date_time"),
                            .TotalAmount = reader.GetDecimal("total_amount"),
                            .UserId = reader.GetInt32("user_id")
                        }
                    End Using
                End Using

                Using cmd As New MySqlCommand(lineSql, conn)
                    cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, saleId))
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            sale.Lines.Add(New SaleItem With {
                                .SaleItemId = reader.GetInt32("sale_item_id"),
                                .SaleId = reader.GetInt32("sale_id"),
                                .MenuItemId = reader.GetInt32("menu_item_id"),
                                .MenuItemName = reader.GetString("menu_item_name"),
                                .Quantity = reader.GetInt32("quantity"),
                                .UnitPrice = reader.GetDecimal("unit_price"),
                                .Subtotal = reader.GetDecimal("subtotal")
                            })
                        End While
                    End Using
                End Using
                Return sale
            End Using
        End Function

        Public Function TodaysCogs(day As Date) As Decimal
            If Not ColumnExists("sale_items", "cogs_amount") Then Return -1D
            Const sql As String =
                "SELECT COALESCE(SUM(si.cogs_amount), 0) " &
                "FROM sale_items si INNER JOIN sales s ON s.sale_id = si.sale_id " &
                "WHERE s.sale_date_time >= @start AND s.sale_date_time < @end"
            Return ExecuteScalarDecimal(sql,
                Param("@start", MySqlDbType.DateTime, day.Date),
                Param("@end", MySqlDbType.DateTime, day.Date.AddDays(1)))
        End Function

        ''' <summary>
        ''' Saves one sale. Any failure rolls back the sale, its lines, and the stock deduction.
        ''' </summary>
        Public Function CompleteSale(userId As Integer, lines As IList(Of SaleDraftLine)) As CompletedSale
            If userId <= 0 Then Throw New InvalidOperationException("Log in before completing a sale.")
            Dim order As List(Of SaleDraftLine) = MergeLines(lines)
            If order.Count = 0 Then Throw New InvalidOperationException("Add at least one item to the order.")
            RequireSnapshotColumns()

            Dim completed As New CompletedSale()
            Try
                ExecuteInTransaction(
                    Sub(conn, transaction)
                        Dim locked As New Dictionary(Of Integer, LockedMenu)
                        For Each line As SaleDraftLine In order
                            locked.Add(line.MenuItemId, LockMenu(conn, transaction, line))
                        Next

                        Dim recipes As New Dictionary(Of Integer, List(Of RecipePart))
                        Dim inventoryIds As New HashSet(Of Integer)
                        For Each line As SaleDraftLine In order
                            Dim parts As List(Of RecipePart) = LockRecipe(conn, transaction, line.MenuItemId)
                            If parts.Count = 0 Then
                                Throw New InvalidOperationException(locked(line.MenuItemId).Name & " has no recipe, so it cannot be sold.")
                            End If
                            recipes.Add(line.MenuItemId, parts)
                            For Each part As RecipePart In parts
                                inventoryIds.Add(part.InventoryItemId)
                            Next
                        Next

                        Dim stock As Dictionary(Of Integer, StockRow) = LockStock(conn, transaction, inventoryIds)
                        Dim usage As New Dictionary(Of Integer, Decimal)
                        Dim total As Decimal = 0D
                        Dim cogs As Decimal = 0D
                        Dim prepared As New List(Of PreparedLine)

                        For Each line As SaleDraftLine In order
                            Dim menu As LockedMenu = locked(line.MenuItemId)
                            Dim lineCogs As Decimal = 0D
                            For Each part As RecipePart In recipes(line.MenuItemId)
                                Dim row As StockRow = Nothing
                                If Not stock.TryGetValue(part.InventoryItemId, row) Then
                                    Throw New InvalidOperationException("A recipe ingredient no longer exists.")
                                End If
                                Dim perServing As Decimal = ToStockQuantity(part.QuantityRequired, part.RecipeUnit, row.Unit)
                                Dim needed As Decimal = ToStockQuantity(perServing * line.Quantity, row.Unit, row.Unit)
                                If needed <= 0D Then
                                    Throw New InvalidOperationException(row.ItemName & " has a recipe quantity that is too small to deduct.")
                                End If
                                Dim already As Decimal = 0D
                                usage.TryGetValue(row.InventoryItemId, already)
                                usage(row.InventoryItemId) = already + needed
                                lineCogs += needed * row.UnitCost
                            Next
                            Dim preparedLine As New PreparedLine With {
                                .MenuItemId = line.MenuItemId,
                                .Name = menu.Name,
                                .Quantity = line.Quantity,
                                .UnitPrice = menu.Price,
                                .Subtotal = UnitConverter.RoundMoney(menu.Price * line.Quantity),
                                .CogsAmount = UnitConverter.RoundMoney(lineCogs)
                            }
                            prepared.Add(preparedLine)
                            total += preparedLine.Subtotal
                            cogs += preparedLine.CogsAmount
                        Next

                        For Each pair As KeyValuePair(Of Integer, Decimal) In usage
                            DeductStock(conn, transaction, stock(pair.Key), pair.Value)
                        Next

                        completed.SaleId = InsertSale(conn, transaction, userId, total, cogs)
                        For Each preparedLine As PreparedLine In prepared
                            InsertSaleItem(conn, transaction, completed.SaleId, preparedLine)
                        Next
                        completed.TotalAmount = total
                        completed.CogsAmount = cogs
                    End Sub)
            Catch ex As Exception
                Throw Friendly(ex)
            End Try
            Return completed
        End Function

        Private Sub RequireSnapshotColumns()
            If Not ColumnExists("sales", "cogs_amount") OrElse
               Not ColumnExists("sale_items", "menu_item_name") OrElse
               Not ColumnExists("sale_items", "cogs_amount") Then
                Throw New InvalidOperationException(SnapshotRequiredMessage)
            End If
        End Sub

        Private Function MergeLines(lines As IList(Of SaleDraftLine)) As List(Of SaleDraftLine)
            Dim merged As New Dictionary(Of Integer, SaleDraftLine)
            If lines Is Nothing Then Return New List(Of SaleDraftLine)
            For Each line As SaleDraftLine In lines
                If line Is Nothing OrElse line.Quantity <= 0 Then
                    Throw New InvalidOperationException("Every order quantity must be at least 1.")
                End If
                Dim existing As SaleDraftLine = Nothing
                If merged.TryGetValue(line.MenuItemId, existing) Then
                    If UnitConverter.RoundMoney(existing.UnitPrice) <> UnitConverter.RoundMoney(line.UnitPrice) Then
                        Throw New InvalidOperationException(line.ItemName & " is in the order at two different prices.")
                    End If
                    existing.Quantity += line.Quantity
                Else
                    merged.Add(line.MenuItemId, New SaleDraftLine With {
                        .MenuItemId = line.MenuItemId,
                        .ItemName = line.ItemName,
                        .UnitPrice = line.UnitPrice,
                        .Quantity = line.Quantity
                    })
                End If
            Next
            Return merged.Values.OrderBy(Function(line) line.MenuItemId).ToList()
        End Function

        Private Function LockMenu(conn As MySqlConnection, transaction As MySqlTransaction, line As SaleDraftLine) As LockedMenu
            Const sql As String =
                "SELECT menu_item_name, selling_price, status FROM menu_items " &
                "WHERE menu_item_id = @id FOR UPDATE"
            Using cmd As New MySqlCommand(sql, conn, transaction)
                cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, line.MenuItemId))
                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    If Not reader.Read() Then
                        Throw New InvalidOperationException(line.ItemName & " is no longer on the menu.")
                    End If
                    Dim status As String = reader.GetString("status")
                    Dim name As String = reader.GetString("menu_item_name")
                    Dim price As Decimal = reader.GetDecimal("selling_price")
                    If status <> "Active" Then
                        Throw New InvalidOperationException(name & " is inactive and cannot be sold.")
                    End If
                    If UnitConverter.RoundMoney(price) <> UnitConverter.RoundMoney(line.UnitPrice) Then
                        Throw New InvalidOperationException("The price of " & name & " changed. Remove it from the order and add it again.")
                    End If
                    Return New LockedMenu With {.Name = name, .Price = price}
                End Using
            End Using
        End Function

        Private Function LockRecipe(conn As MySqlConnection, transaction As MySqlTransaction, menuItemId As Integer) As List(Of RecipePart)
            Const sql As String =
                "SELECT inventory_item_id, quantity_required, unit FROM menu_item_ingredients " &
                "WHERE menu_item_id = @id ORDER BY inventory_item_id FOR UPDATE"
            Dim parts As New List(Of RecipePart)
            Using cmd As New MySqlCommand(sql, conn, transaction)
                cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, menuItemId))
                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        parts.Add(New RecipePart With {
                            .InventoryItemId = reader.GetInt32("inventory_item_id"),
                            .QuantityRequired = reader.GetDecimal("quantity_required"),
                            .RecipeUnit = reader.GetString("unit")
                        })
                    End While
                End Using
            End Using
            Return parts
        End Function

        Private Function LockStock(conn As MySqlConnection, transaction As MySqlTransaction, ids As IEnumerable(Of Integer)) As Dictionary(Of Integer, StockRow)
            Dim ordered As List(Of Integer) = ids.OrderBy(Function(id) id).ToList()
            Dim rows As New Dictionary(Of Integer, StockRow)
            If ordered.Count = 0 Then Return rows
            Dim names As New List(Of String)
            Using cmd As New MySqlCommand()
                cmd.Connection = conn
                cmd.Transaction = transaction
                For index As Integer = 0 To ordered.Count - 1
                    Dim parameterName As String = "@id" & index.ToString()
                    names.Add(parameterName)
                    cmd.Parameters.Add(Param(parameterName, MySqlDbType.Int32, ordered(index)))
                Next
                cmd.CommandText =
                    "SELECT inventory_item_id, item_name, quantity, unit, unit_cost FROM inventory_items " &
                    "WHERE inventory_item_id IN (" & String.Join(", ", names) & ") " &
                    "ORDER BY inventory_item_id FOR UPDATE"
                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim row As New StockRow With {
                            .InventoryItemId = reader.GetInt32("inventory_item_id"),
                            .ItemName = reader.GetString("item_name"),
                            .OnHand = reader.GetDecimal("quantity"),
                            .Unit = reader.GetString("unit"),
                            .UnitCost = reader.GetDecimal("unit_cost")
                        }
                        rows.Add(row.InventoryItemId, row)
                    End While
                End Using
            End Using
            Return rows
        End Function

        Private Sub DeductStock(conn As MySqlConnection, transaction As MySqlTransaction, row As StockRow, needed As Decimal)
            If needed > row.OnHand Then
                Throw New InvalidOperationException("Not enough " & row.ItemName & " to complete this sale.")
            End If
            Const sql As String =
                "UPDATE inventory_items SET quantity = quantity - @needed " &
                "WHERE inventory_item_id = @id AND quantity >= @needed"
            Using cmd As New MySqlCommand(sql, conn, transaction)
                cmd.Parameters.Add(DecimalParam("@needed", needed, 3))
                cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, row.InventoryItemId))
                If cmd.ExecuteNonQuery() <> 1 Then
                    Throw New InvalidOperationException("Not enough " & row.ItemName & " to complete this sale.")
                End If
            End Using
        End Sub

        Private Function InsertSale(conn As MySqlConnection, transaction As MySqlTransaction, userId As Integer, total As Decimal, cogs As Decimal) As Integer
            Const sql As String =
                "INSERT INTO sales (total_amount, cogs_amount, user_id) VALUES (@total, @cogs, @userId)"
            Using cmd As New MySqlCommand(sql, conn, transaction)
                cmd.Parameters.Add(DecimalParam("@total", total, 2))
                cmd.Parameters.Add(DecimalParam("@cogs", cogs, 2))
                cmd.Parameters.Add(Param("@userId", MySqlDbType.Int32, userId))
                cmd.ExecuteNonQuery()
                Return CInt(cmd.LastInsertedId)
            End Using
        End Function

        Private Sub InsertSaleItem(conn As MySqlConnection, transaction As MySqlTransaction, saleId As Integer, line As PreparedLine)
            Const sql As String =
                "INSERT INTO sale_items (sale_id, menu_item_id, menu_item_name, quantity, unit_price, subtotal, cogs_amount) " &
                "VALUES (@saleId, @menuId, @name, @quantity, @price, @subtotal, @cogs)"
            Using cmd As New MySqlCommand(sql, conn, transaction)
                cmd.Parameters.Add(Param("@saleId", MySqlDbType.Int32, saleId))
                cmd.Parameters.Add(Param("@menuId", MySqlDbType.Int32, line.MenuItemId))
                cmd.Parameters.Add(Param("@name", MySqlDbType.VarChar, line.Name))
                cmd.Parameters.Add(Param("@quantity", MySqlDbType.Int32, line.Quantity))
                cmd.Parameters.Add(DecimalParam("@price", line.UnitPrice, 2))
                cmd.Parameters.Add(DecimalParam("@subtotal", line.Subtotal, 2))
                cmd.Parameters.Add(DecimalParam("@cogs", line.CogsAmount, 2))
                cmd.ExecuteNonQuery()
            End Using
        End Sub

        Private Function ServingsAvailable(builder As OfferBuilder) As Integer
            If Not builder.HasRecipe OrElse builder.Parts.Count = 0 Then Return 0
            Dim lowest As Integer = Integer.MaxValue
            For Each part As RecipePart In builder.Parts
                If part.InventoryItemId <= 0 Then Return 0
                Dim perServing As Decimal
                Try
                    perServing = ToStockQuantity(part.QuantityRequired, part.RecipeUnit, part.StockUnit)
                Catch
                    Return 0
                End Try
                If perServing <= 0D OrElse part.OnHand < perServing Then Return 0
                Dim raw As Decimal = Decimal.Truncate(part.OnHand / perServing)
                If raw > 9999D Then raw = 9999D
                Dim servings As Integer = Decimal.ToInt32(raw)
                If servings < lowest Then lowest = servings
            Next
            If lowest = Integer.MaxValue Then Return 0
            Return lowest
        End Function

        Private Function ToStockQuantity(amount As Decimal, fromUnit As String, toUnit As String) As Decimal
            Dim source As String = If(fromUnit, "").Trim()
            Dim target As String = If(toUnit, "").Trim()
            Dim converted As Decimal
            If source.Equals(target, StringComparison.OrdinalIgnoreCase) Then
                converted = amount
            ElseIf UnitConverter.AreCompatible(source, target) Then
                converted = UnitConverter.ConvertQuantity(amount, source, target)
            Else
                Throw New InvalidOperationException(
                    "Cannot convert " & source & " to " & target & ". Match the recipe unit to the inventory unit.")
            End If
            Dim rounded As Decimal = UnitConverter.RoundQuantity(converted)
            If converted > 0D AndAlso rounded <= 0D Then
                Throw New InvalidOperationException("A recipe quantity is too small to deduct from inventory.")
            End If
            Return rounded
        End Function

        Private Function ReadPart(reader As MySqlDataReader) As RecipePart
            Dim onHand As Decimal = 0D
            Dim stockUnit As String = ""
            If Not reader.IsDBNull(reader.GetOrdinal("on_hand")) Then onHand = reader.GetDecimal("on_hand")
            If Not reader.IsDBNull(reader.GetOrdinal("stock_unit")) Then stockUnit = reader.GetString("stock_unit")
            Return New RecipePart With {
                .InventoryItemId = If(reader.IsDBNull(reader.GetOrdinal("inventory_item_id")), 0, reader.GetInt32("inventory_item_id")),
                .QuantityRequired = reader.GetDecimal("quantity_required"),
                .RecipeUnit = reader.GetString("recipe_unit"),
                .OnHand = onHand,
                .StockUnit = stockUnit
            }
        End Function

        ''' <summary>Saved line name, or "Item {id}" when the snapshot is missing or blank.</summary>
        Private Function ItemLabelSql(tableAlias As String, hasName As Boolean) As String
            Dim idLabel As String = "CONCAT('Item ', " & tableAlias & ".menu_item_id)"
            If Not hasName Then Return idLabel
            Return "COALESCE(NULLIF(" & tableAlias & ".menu_item_name, ''), " & idLabel & ")"
        End Function

        Private Function NullableDate(name As String, value As Date?) As MySqlParameter
            Dim parameter As MySqlParameter = Param(name, MySqlDbType.DateTime, DBNull.Value)
            If value.HasValue Then parameter.Value = value.Value
            Return parameter
        End Function

        Private Function EscapeLike(value As String) As String
            Return value.Replace("\", "\\").Replace("%", "\%").Replace("_", "\_")
        End Function

        Private Function Friendly(ex As Exception) As Exception
            If TypeOf ex Is InvalidOperationException Then Return ex
            Dim sqlError = TryCast(ex, MySqlException)
            If sqlError Is Nothing Then Return ex
            If sqlError.Number = 3819 OrElse sqlError.Number = 1690 Then
                Return New InvalidOperationException("This sale would reduce inventory below zero.")
            End If
            If sqlError.Number = 1054 Then Return New InvalidOperationException(SnapshotRequiredMessage)
            Return ex
        End Function

        Private Class OfferBuilder
            Public Property Offer As MenuOffer
            Public Property HasRecipe As Boolean
            Public ReadOnly Property Parts As New List(Of RecipePart)
        End Class

        Private Structure RecipePart
            Public InventoryItemId As Integer
            Public QuantityRequired As Decimal
            Public RecipeUnit As String
            Public OnHand As Decimal
            Public StockUnit As String
        End Structure

        Private Structure LockedMenu
            Public Name As String
            Public Price As Decimal
        End Structure

        Private Structure StockRow
            Public InventoryItemId As Integer
            Public ItemName As String
            Public OnHand As Decimal
            Public Unit As String
            Public UnitCost As Decimal
        End Structure

        Private Structure PreparedLine
            Public MenuItemId As Integer
            Public Name As String
            Public Quantity As Integer
            Public UnitPrice As Decimal
            Public Subtotal As Decimal
            Public CogsAmount As Decimal
        End Structure

    End Module

End Namespace
