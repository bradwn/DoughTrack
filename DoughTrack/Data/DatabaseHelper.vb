Imports System.Data
Imports Microsoft.Extensions.Configuration
Imports MySql.Data.MySqlClient

Namespace Data

    ''' <summary>
    ''' Creates connections to the DoughTrack MySQL database.
    ''' Settings are read from appsettings.json (not hardcoded).
    ''' </summary>
    Public Module DatabaseHelper

        ' Holds the connection string after it is built once.
        Private _connectionString As String = Nothing

        ''' <summary>
        ''' Builds the connection string the first time it is needed.
        ''' </summary>
        Private Function GetConnectionString() As String
            If _connectionString Is Nothing Then
                Dim config As IConfigurationRoot = New ConfigurationBuilder() _
                    .SetBasePath(AppContext.BaseDirectory) _
                    .AddJsonFile("appsettings.json", optional:=False) _
                    .Build()

                Dim builder As New MySqlConnectionStringBuilder()
                builder.Server = config("Database:Server")
                builder.Port = CUInt(config("Database:Port"))
                builder.Database = config("Database:Name")
                builder.UserID = config("Database:User")
                builder.Password = config("Database:Password")
                builder.CharacterSet = "utf8mb4"

                _connectionString = builder.ConnectionString
            End If

            Return _connectionString
        End Function

        ''' <summary>
        ''' Returns a NEW, unopened connection. The caller opens it and
        ''' closes it (use the Using keyword so it closes automatically).
        ''' </summary>
        Public Function CreateConnection() As MySqlConnection
            Return New MySqlConnection(GetConnectionString())
        End Function

        ''' <summary>
        ''' Tests whether the database can be reached.
        ''' Returns True if successful. errorMessage explains any failure.
        ''' </summary>
        Public Function TestConnection(ByRef errorMessage As String) As Boolean
            Try
                Using conn As MySqlConnection = CreateConnection()
                    conn.Open()
                End Using
                errorMessage = ""
                Return True
            Catch ex As Exception
                errorMessage = ex.Message
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Runs work inside one transaction. Commits when work finishes.
        ''' Rolls back and rethrows when work fails.
        ''' </summary>
        Public Sub ExecuteInTransaction(work As Action(Of MySqlConnection, MySqlTransaction))
            Using conn As MySqlConnection = CreateConnection()
                conn.Open()
                Using transaction As MySqlTransaction = conn.BeginTransaction()
                    Try
                        work(conn, transaction)
                        transaction.Commit()
                    Catch
                        Try
                            transaction.Rollback()
                        Catch
                            ' Keep the original failure from work.
                        End Try
                        Throw
                    End Try
                End Using
            End Using
        End Sub

        ''' <summary>Creates a named parameter. Callers do not concatenate values into SQL.</summary>
        Public Function Param(name As String, dbType As MySqlDbType, value As Object) As MySqlParameter
            Return New MySqlParameter(name, dbType) With {.Value = value}
        End Function

        ''' <summary>True when the connected database already has this column.</summary>
        Public Function ColumnExists(tableName As String, columnName As String) As Boolean
            Const sql As String =
                "SELECT COUNT(*) FROM information_schema.COLUMNS " &
                "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @tableName AND COLUMN_NAME = @columnName"
            Return ExecuteScalarInt(sql,
                Param("@tableName", MySqlDbType.VarChar, tableName),
                Param("@columnName", MySqlDbType.VarChar, columnName)) = 1
        End Function

        ''' <summary>Decimal parameter with an explicit scale so MySQL does not truncate it.</summary>
        Public Function DecimalParam(name As String, value As Decimal, scale As Byte) As MySqlParameter
            Return New MySqlParameter(name, MySqlDbType.Decimal) With {
                .Precision = 18,
                .Scale = scale,
                .Value = value
            }
        End Function

        ''' <summary>Runs a parameterized query and returns the rows.</summary>
        Public Function FillTable(sql As String, ParamArray parameters() As MySqlParameter) As DataTable
            Using conn As MySqlConnection = CreateConnection()
                Using cmd As New MySqlCommand(sql, conn)
                    If parameters IsNot Nothing AndAlso parameters.Length > 0 Then
                        cmd.Parameters.AddRange(parameters)
                    End If
                    Using adapter As New MySqlDataAdapter(cmd)
                        Dim table As New DataTable()
                        adapter.Fill(table)
                        Return table
                    End Using
                End Using
            End Using
        End Function

        ''' <summary>Runs a parameterized query that returns one number. Null becomes 0.</summary>
        Public Function ExecuteScalarDecimal(sql As String, ParamArray parameters() As MySqlParameter) As Decimal
            Using conn As MySqlConnection = CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    If parameters IsNot Nothing AndAlso parameters.Length > 0 Then
                        cmd.Parameters.AddRange(parameters)
                    End If
                    Dim value As Object = cmd.ExecuteScalar()
                    If value Is Nothing OrElse IsDBNull(value) Then Return 0D
                    Return Convert.ToDecimal(value)
                End Using
            End Using
        End Function

        ''' <summary>Runs a parameterized query that returns one count. Null becomes 0.</summary>
        Public Function ExecuteScalarInt(sql As String, ParamArray parameters() As MySqlParameter) As Integer
            Using conn As MySqlConnection = CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    If parameters IsNot Nothing AndAlso parameters.Length > 0 Then
                        cmd.Parameters.AddRange(parameters)
                    End If
                    Dim value As Object = cmd.ExecuteScalar()
                    If value Is Nothing OrElse IsDBNull(value) Then Return 0
                    Return Convert.ToInt32(value)
                End Using
            End Using
        End Function

    End Module

End Namespace