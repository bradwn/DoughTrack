Imports System.IO
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

    End Module

End Namespace