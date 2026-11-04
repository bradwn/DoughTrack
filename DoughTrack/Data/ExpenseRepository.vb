Imports MySql.Data.MySqlClient
Imports DoughTrack.Helpers
Imports DoughTrack.Models

Namespace Data

    ''' <summary>
    ''' SQL for the expenses table. Deletes remove one expense row and nothing else.
    ''' </summary>
    Public Module ExpenseRepository

        Private ReadOnly SuggestedCategories As String() = {
            "Ingredients", "Rent", "Salaries", "Supplies", "Utilities"
        }

        Public Function Search(textSearch As String, category As String, fromDate As Date?, toDate As Date?) As List(Of Expense)
            Dim pattern As String = "%" & EscapeLike(If(textSearch, "").Trim()) & "%"
            Const sql As String =
                "SELECT expense_id, expense_date, category, amount, description " &
                "FROM expenses " &
                "WHERE (CAST(expense_id AS CHAR) LIKE @search ESCAPE '\\' " &
                "OR category LIKE @search ESCAPE '\\' " &
                "OR IFNULL(description, '') LIKE @search ESCAPE '\\' " &
                "OR DATE_FORMAT(expense_date, '%Y-%m-%d') LIKE @search ESCAPE '\\') " &
                "AND (@category = '' OR category = @category) " &
                "AND (@useFrom = 0 OR expense_date >= @fromDate) " &
                "AND (@useTo = 0 OR expense_date <= @toDate) " &
                "ORDER BY expense_date DESC, expense_id DESC"

            Dim expenses As New List(Of Expense)
            Using conn As MySqlConnection = CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    AddFilterParameters(cmd, pattern, category, fromDate, toDate)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            expenses.Add(ReadExpense(reader))
                        End While
                    End Using
                End Using
            End Using
            Return expenses
        End Function

        Public Function CountAll() As Integer
            Return ExecuteScalarInt("SELECT COUNT(*) FROM expenses")
        End Function

        ''' <summary>Categories already stored on expenses.</summary>
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

        ''' <summary>Saved categories plus a short list so the first expense still has choices.</summary>
        Public Function GetCategoryChoices() As List(Of String)
            Dim categories As New SortedSet(Of String)(StringComparer.CurrentCultureIgnoreCase)
            For Each suggested As String In SuggestedCategories
                categories.Add(suggested)
            Next
            For Each category As String In GetCategories()
                categories.Add(category)
            Next
            Return New List(Of String)(categories)
        End Function

        Public Function GetById(expenseId As Integer) As Expense
            Const sql As String =
                "SELECT expense_id, expense_date, category, amount, description " &
                "FROM expenses WHERE expense_id = @id"
            Using conn As MySqlConnection = CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, expenseId))
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then Return ReadExpense(reader)
                    End Using
                End Using
            End Using
            Return Nothing
        End Function

        Public Sub Save(expense As Expense)
            Validate(expense)
            Try
                ExecuteInTransaction(
                    Sub(conn, tx)
                        If expense.ExpenseId = 0 Then
                            expense.ExpenseId = InsertExpense(conn, tx, expense)
                        Else
                            UpdateExpense(conn, tx, expense)
                        End If
                    End Sub)
            Catch ex As Exception
                Throw Friendly(ex)
            End Try
        End Sub

        ''' <summary>
        ''' Removes one expense. No other table references expenses, so this cannot change sales or inventory.
        ''' </summary>
        Public Sub Delete(expenseId As Integer)
            If expenseId <= 0 Then Throw New InvalidOperationException("Choose an expense to delete.")
            Const sql As String = "DELETE FROM expenses WHERE expense_id = @id"
            Try
                ExecuteInTransaction(
                    Sub(conn, tx)
                        Using cmd As New MySqlCommand(sql, conn, tx)
                            cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, expenseId))
                            If cmd.ExecuteNonQuery() <> 1 Then
                                Throw New InvalidOperationException("That expense no longer exists.")
                            End If
                        End Using
                    End Sub)
            Catch ex As Exception
                Throw Friendly(ex)
            End Try
        End Sub

        Private Function InsertExpense(conn As MySqlConnection, tx As MySqlTransaction, expense As Expense) As Integer
            Const sql As String =
                "INSERT INTO expenses (expense_date, category, amount, description) " &
                "VALUES (@expenseDate, @category, @amount, @description)"
            Using cmd As New MySqlCommand(sql, conn, tx)
                AddExpenseParameters(cmd, expense)
                cmd.ExecuteNonQuery()
                Return CInt(cmd.LastInsertedId)
            End Using
        End Function

        Private Sub UpdateExpense(conn As MySqlConnection, tx As MySqlTransaction, expense As Expense)
            Const sql As String =
                "UPDATE expenses SET expense_date = @expenseDate, category = @category, " &
                "amount = @amount, description = @description WHERE expense_id = @id"
            Using cmd As New MySqlCommand(sql, conn, tx)
                AddExpenseParameters(cmd, expense)
                cmd.Parameters.Add(Param("@id", MySqlDbType.Int32, expense.ExpenseId))
                If cmd.ExecuteNonQuery() <> 1 Then
                    Throw New InvalidOperationException("That expense no longer exists.")
                End If
            End Using
        End Sub

        Private Sub AddExpenseParameters(cmd As MySqlCommand, expense As Expense)
            cmd.Parameters.Add(Param("@expenseDate", MySqlDbType.Date, expense.ExpenseDate.Date))
            cmd.Parameters.Add(Param("@category", MySqlDbType.VarChar, expense.Category))
            cmd.Parameters.Add(DecimalParam("@amount", expense.Amount, 2))
            Dim descriptionValue As Object = DBNull.Value
            If expense.Description <> "" Then descriptionValue = expense.Description
            cmd.Parameters.Add(Param("@description", MySqlDbType.VarChar, descriptionValue))
        End Sub

        Private Sub AddFilterParameters(cmd As MySqlCommand, pattern As String, category As String, fromDate As Date?, toDate As Date?)
            cmd.Parameters.Add(Param("@search", MySqlDbType.VarChar, pattern))
            cmd.Parameters.Add(Param("@category", MySqlDbType.VarChar, If(category, "").Trim()))
            cmd.Parameters.Add(Param("@useFrom", MySqlDbType.Int32, If(fromDate.HasValue, 1, 0)))
            cmd.Parameters.Add(Param("@fromDate", MySqlDbType.Date, If(fromDate, Date.Today)))
            cmd.Parameters.Add(Param("@useTo", MySqlDbType.Int32, If(toDate.HasValue, 1, 0)))
            cmd.Parameters.Add(Param("@toDate", MySqlDbType.Date, If(toDate, Date.Today)))
        End Sub

        Private Sub Validate(expense As Expense)
            If expense Is Nothing Then Throw New InvalidOperationException("Expense is missing.")
            If expense.ExpenseDate = Date.MinValue Then
                Throw New InvalidOperationException("Date is required.")
            End If
            expense.Category = If(expense.Category, "").Trim()
            expense.Description = If(expense.Description, "").Trim()
            If expense.Category = "" Then Throw New InvalidOperationException("Category is required.")
            If expense.Category.Length > 50 Then
                Throw New InvalidOperationException("Category must be 50 characters or less.")
            End If
            expense.Amount = UnitConverter.RoundMoney(expense.Amount)
            If expense.Amount <= 0D Then
                Throw New InvalidOperationException("Amount must be greater than zero.")
            End If
            If expense.Amount > 99999999.99D Then
                Throw New InvalidOperationException("Amount must be 99,999,999.99 or less.")
            End If
            If expense.Description.Length > 255 Then
                Throw New InvalidOperationException("Description must be 255 characters or less.")
            End If
        End Sub

        Private Function ReadExpense(reader As MySqlDataReader) As Expense
            Dim description As String = ""
            If Not reader.IsDBNull(reader.GetOrdinal("description")) Then
                description = reader.GetString("description")
            End If
            Return New Expense With {
                .ExpenseId = reader.GetInt32("expense_id"),
                .ExpenseDate = reader.GetDateTime("expense_date").Date,
                .Category = reader.GetString("category"),
                .Amount = reader.GetDecimal("amount"),
                .Description = description
            }
        End Function

        Private Function EscapeLike(value As String) As String
            Return value.Replace("\", "\\").Replace("%", "\%").Replace("_", "\_")
        End Function

        Private Function Friendly(ex As Exception) As Exception
            If TypeOf ex Is InvalidOperationException Then Return ex
            Dim sqlError = TryCast(ex, MySqlException)
            If sqlError IsNot Nothing AndAlso sqlError.Number = 3819 Then
                Return New InvalidOperationException("Amount must be greater than zero.")
            End If
            If sqlError IsNot Nothing AndAlso sqlError.Number = 1451 Then
                Return New InvalidOperationException("That expense is still used by another record, so it was not deleted.")
            End If
            Return ex
        End Function

    End Module

End Namespace
