Imports DoughTrack.Helpers
Imports DoughTrack.Models
Imports MySql.Data.MySqlClient

Namespace Data

    ''' <summary>
    ''' SQL for the expenses table. Today's Expense on the Dashboard reads this same table.
    ''' Nothing else points to an expense row, so deleting one is safe.
    ''' </summary>
    Public Module ExpenseRepository

        Private Const MaxAmount As Decimal = 99999999.99D

        ''' <summary>
        ''' Expenses matching the search text (category, description, or id), an optional category,
        ''' and an optional date range. Newest first.
        ''' </summary>
        Public Function Search(searchText As String, category As String,
                               fromDate As Date?, toDate As Date?) As List(Of Expense)
            Dim text As String = If(searchText, "").Trim()
            Const sql As String =
                "SELECT expense_id, expense_date, category, amount, description " &
                "FROM expenses " &
                "WHERE (@search = '' " &
                "       OR category LIKE @pattern ESCAPE '\\' " &
                "       OR IFNULL(description, '') LIKE @pattern ESCAPE '\\' " &
                "       OR CAST(expense_id AS CHAR) = @search) " &
                "AND (@category = '' OR category = @category) " &
                "AND (@fromDate IS NULL OR expense_date >= @fromDate) " &
                "AND (@toDate IS NULL OR expense_date <= @toDate) " &
                "ORDER BY expense_date DESC, expense_id DESC"

            Dim results As New List(Of Expense)
            Using conn As MySqlConnection = CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.Add(Param("@search", MySqlDbType.VarChar, text))
                    cmd.Parameters.Add(Param("@pattern", MySqlDbType.VarChar, "%" & EscapeLike(text) & "%"))
                    cmd.Parameters.Add(Param("@category", MySqlDbType.VarChar, If(category, "").Trim()))
                    cmd.Parameters.Add(NullableDate("@fromDate", fromDate))
                    cmd.Parameters.Add(NullableDate("@toDate", toDate))
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            results.Add(ReadExpense(reader))
                        End While
                    End Using
                End Using
            End Using
            Return results
        End Function

        ''' <summary>Distinct categories already used, for the filter and the add/edit drop-down.</summary>
        Public Function GetCategories() As List(Of String)
            Const sql As String = "SELECT DISTINCT category FROM expenses ORDER BY category"
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

        Public Function Add(expense As Expense) As Integer
            Validate(expense)
            Const sql As String =
                "INSERT INTO expenses (expense_date, category, amount, description) " &
                "VALUES (@date, @category, @amount, @description)"
            Using conn As MySqlConnection = CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    AddExpenseParameters(cmd, expense)
                    cmd.ExecuteNonQuery()
                    Return CInt(cmd.LastInsertedId)
                End Using
            End Using
        End Function

        Public Sub Update(expense As Expense)
            Validate(expense)
            Const sql As String =
                "UPDATE expenses SET expense_date = @date, category = @category, " &
                "amount = @amount, description = @description " &
                "WHERE expense_id = @id"
            Using conn As MySqlConnection = CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    AddExpenseParameters(cmd, expense)
                    cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, expense.ExpenseId))
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        ''' <summary>Deletes one expense. Safe because no other table references expenses.</summary>
        Public Sub Delete(expenseId As Integer)
            Const sql As String = "DELETE FROM expenses WHERE expense_id = @id"
            Using conn As MySqlConnection = CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, expenseId))
                    If cmd.ExecuteNonQuery() = 0 Then
                        Throw New InvalidOperationException("This expense no longer exists.")
                    End If
                End Using
            End Using
        End Sub

        ''' <summary>Business rules: date required, category required, amount greater than zero.</summary>
        Private Sub Validate(expense As Expense)
            If expense.ExpenseDate = Date.MinValue Then
                Throw New InvalidOperationException("Date is required.")
            End If
            If String.IsNullOrWhiteSpace(expense.Category) Then
                Throw New InvalidOperationException("Category is required.")
            End If
            If expense.Category.Trim().Length > 50 Then
                Throw New InvalidOperationException("Category must be 50 characters or fewer.")
            End If
            If expense.Amount <= 0D Then
                Throw New InvalidOperationException("Amount must be greater than zero.")
            End If
            If expense.Amount > MaxAmount Then
                Throw New InvalidOperationException("Amount is too large.")
            End If
            If If(expense.Description, "").Trim().Length > 255 Then
                Throw New InvalidOperationException("Description must be 255 characters or fewer.")
            End If
        End Sub

        Private Sub AddExpenseParameters(cmd As MySqlCommand, expense As Expense)
            Dim description As String = If(expense.Description, "").Trim()
            cmd.Parameters.Add(Param("@date", MySqlDbType.Date, expense.ExpenseDate.Date))
            cmd.Parameters.Add(Param("@category", MySqlDbType.VarChar, expense.Category.Trim()))
            cmd.Parameters.Add(DecimalParam("@amount", UnitConverter.RoundMoney(expense.Amount), 2))
            cmd.Parameters.Add(Param("@description", MySqlDbType.VarChar,
                                     If(description = "", CObj(DBNull.Value), CObj(description))))
        End Sub

        Private Function ReadExpense(reader As MySqlDataReader) As Expense
            Return New Expense With {
                .ExpenseId = reader.GetInt32("expense_id"),
                .ExpenseDate = reader.GetDateTime("expense_date"),
                .Category = reader.GetString("category"),
                .Amount = reader.GetDecimal("amount"),
                .Description = If(reader.IsDBNull(reader.GetOrdinal("description")), "", reader.GetString("description"))
            }
        End Function

        Private Function NullableDate(name As String, value As Date?) As MySqlParameter
            Return Param(name, MySqlDbType.Date,
                         If(value.HasValue, CObj(value.Value.Date), CObj(DBNull.Value)))
        End Function

        Private Function EscapeLike(value As String) As String
            Return value.Replace("\", "\\").Replace("%", "\%").Replace("_", "\_")
        End Function

    End Module

End Namespace
