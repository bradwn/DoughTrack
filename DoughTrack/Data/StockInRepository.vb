Imports DoughTrack.Helpers
Imports DoughTrack.Models
Imports MySql.Data.MySqlClient

Namespace Data

    ''' <summary>
    ''' SQL for stock_ins and stock_in_items.
    ''' Save writes the header, the lines, and the inventory increase in ONE transaction.
    ''' If any step fails, nothing is saved.
    ''' </summary>
    Public Module StockInRepository

        ''' <summary>
        ''' Saves a stock-in and adds each line's quantity to inventory.
        ''' Lines hold quantities already converted to the inventory item's own unit.
        ''' Returns the new stock_in_id.
        ''' </summary>
        Public Function Save(stockInDate As Date, userId As Integer, lines As IList(Of StockInDraftLine)) As Integer
            ' ---- Validation (before touching the database) ----
            If lines Is Nothing OrElse lines.Count = 0 Then
                Throw New InvalidOperationException("Add at least one item to the stock-in.")
            End If
            If stockInDate.Date > Date.Today Then
                Throw New InvalidOperationException("The stock-in date cannot be in the future.")
            End If
            If userId <= 0 Then
                Throw New InvalidOperationException("You must be logged in to save a stock-in.")
            End If

            Dim total As Decimal = 0D
            For Each line As StockInDraftLine In lines
                If line.StoredQuantity <= 0D Then
                    Throw New InvalidOperationException("Quantity for " & line.ItemName & " must be greater than zero.")
                End If
                If line.StoredUnitCost < 0D Then
                    Throw New InvalidOperationException("Unit cost for " & line.ItemName & " cannot be negative.")
                End If
                total += line.Subtotal
            Next
            total = UnitConverter.RoundMoney(total)

            ' ---- One transaction for everything ----
            Dim stockInId As Integer = 0
            Try
                ExecuteInTransaction(
                    Sub(conn As MySqlConnection, transaction As MySqlTransaction)
                        stockInId = InsertHeader(conn, transaction, stockInDate, total, userId)
                        For Each line As StockInDraftLine In lines
                            AddToInventory(conn, transaction, line)
                            InsertLine(conn, transaction, stockInId, line)
                        Next
                    End Sub)
            Catch ex As InvalidOperationException
                Throw
            Catch ex As Exception
                Debug.WriteLine("STOCK-IN ERROR: " & ex.ToString())
                Throw New InvalidOperationException(
                    "Unable to save the stock-in. No changes were made. Please check the values and try again.")
            End Try
            Return stockInId
        End Function

        ''' <summary>Most recent stock-ins, newest first.</summary>
        Public Function GetRecent(maxRows As Integer) As DataTable
            Const sql As String =
                "SELECT h.stock_in_id AS `ID`, " &
                "h.stock_in_date AS `Date`, " &
                "h.total_cost AS `Total cost`, " &
                "u.full_name AS `Recorded by`, " &
                "COUNT(d.stock_in_item_id) AS `Lines` " &
                "FROM stock_ins h " &
                "INNER JOIN users u ON u.user_id = h.user_id " &
                "LEFT JOIN stock_in_items d ON d.stock_in_id = h.stock_in_id " &
                "GROUP BY h.stock_in_id, h.stock_in_date, h.total_cost, u.full_name " &
                "ORDER BY h.stock_in_date DESC, h.stock_in_id DESC " &
                "LIMIT @maxRows"
            Return FillTable(sql, Param("@maxRows", MySqlDbType.Int32, maxRows))
        End Function

        ''' <summary>The lines of one saved stock-in.</summary>
        Public Function GetLines(stockInId As Integer) As DataTable
            Const sql As String =
                "SELECT ii.item_name AS `Item`, " &
                "d.quantity AS `Quantity`, " &
                "ii.unit AS `Unit`, " &
                "d.unit_cost AS `Unit cost`, " &
                "d.subtotal AS `Subtotal` " &
                "FROM stock_in_items d " &
                "INNER JOIN inventory_items ii ON ii.inventory_item_id = d.inventory_item_id " &
                "WHERE d.stock_in_id = @id " &
                "ORDER BY d.stock_in_item_id"
            Return FillTable(sql, Param("@id", MySqlDbType.Int32, stockInId))
        End Function

        Private Function InsertHeader(conn As MySqlConnection, transaction As MySqlTransaction,
                                      stockInDate As Date, total As Decimal, userId As Integer) As Integer
            Const sql As String =
                "INSERT INTO stock_ins (stock_in_date, total_cost, user_id) VALUES (@date, @total, @userId)"
            Using cmd As New MySqlCommand(sql, conn, transaction)
                cmd.Parameters.Add(Param("@date", MySqlDbType.Date, stockInDate.Date))
                cmd.Parameters.Add(DecimalParam("@total", total, 2))
                cmd.Parameters.Add(Param("@userId", MySqlDbType.Int32, userId))
                cmd.ExecuteNonQuery()
                Return CInt(cmd.LastInsertedId)
            End Using
        End Function

        ''' <summary>
        ''' Locks the inventory row, adds the quantity, and updates the unit cost
        ''' as a weighted average of what is on hand and what just arrived.
        ''' </summary>
        Private Sub AddToInventory(conn As MySqlConnection, transaction As MySqlTransaction, line As StockInDraftLine)
            Const lockSql As String =
                "SELECT quantity, unit_cost, status FROM inventory_items " &
                "WHERE inventory_item_id = @id FOR UPDATE"

            Dim onHand As Decimal
            Dim onHandCost As Decimal
            Dim status As String

            Using cmd As New MySqlCommand(lockSql, conn, transaction)
                cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, line.InventoryItemId))
                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    If Not reader.Read() Then
                        Throw New InvalidOperationException(line.ItemName & " no longer exists in inventory.")
                    End If
                    onHand = reader.GetDecimal("quantity")
                    onHandCost = reader.GetDecimal("unit_cost")
                    status = reader.GetString("status")
                End Using
            End Using

            If status <> "Active" Then
                Throw New InvalidOperationException(line.ItemName & " is inactive. Reactivate it before adding stock.")
            End If

            Dim newCost As Decimal = UnitConverter.WeightedAverageCost(
                onHand, onHandCost, line.StoredQuantity, line.Subtotal)

            Const updateSql As String =
                "UPDATE inventory_items SET quantity = quantity + @quantity, unit_cost = @cost " &
                "WHERE inventory_item_id = @id"
            Using cmd As New MySqlCommand(updateSql, conn, transaction)
                cmd.Parameters.Add(DecimalParam("@quantity", line.StoredQuantity, 3))
                cmd.Parameters.Add(DecimalParam("@cost", newCost, 4))
                cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, line.InventoryItemId))
                If cmd.ExecuteNonQuery() <> 1 Then
                    Throw New InvalidOperationException("Could not update the quantity of " & line.ItemName & ".")
                End If
            End Using
        End Sub

        Private Sub InsertLine(conn As MySqlConnection, transaction As MySqlTransaction,
                               stockInId As Integer, line As StockInDraftLine)
            Const sql As String =
                "INSERT INTO stock_in_items (stock_in_id, inventory_item_id, quantity, unit_cost, subtotal) " &
                "VALUES (@stockInId, @itemId, @quantity, @cost, @subtotal)"
            Using cmd As New MySqlCommand(sql, conn, transaction)
                cmd.Parameters.Add(Param("@stockInId", MySqlDbType.Int32, stockInId))
                cmd.Parameters.Add(Param("@itemId", MySqlDbType.Int32, line.InventoryItemId))
                cmd.Parameters.Add(DecimalParam("@quantity", line.StoredQuantity, 3))
                cmd.Parameters.Add(DecimalParam("@cost", line.StoredUnitCost, 4))
                cmd.Parameters.Add(DecimalParam("@subtotal", line.Subtotal, 2))
                cmd.ExecuteNonQuery()
            End Using
        End Sub

    End Module

End Namespace
