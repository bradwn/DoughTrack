Imports MySql.Data.MySqlClient
Imports DoughTrack.Models

Namespace Data

    ''' <summary>
    ''' All SQL that touches the users table lives here.
    ''' </summary>
    Public Module UserRepository

        ''' <summary>
        ''' Finds a user by username. Returns Nothing if not found.
        ''' </summary>
        Public Function GetByUsername(username As String) As User
            Const sql As String =
                "SELECT user_id, username, password_hash, full_name " &
                "FROM users WHERE username = @username"

            Using conn As MySqlConnection = DatabaseHelper.CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@username", username)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Return New User With {
                                .UserId = reader.GetInt32("user_id"),
                                .Username = reader.GetString("username"),
                                .PasswordHash = reader.GetString("password_hash"),
                                .FullName = reader.GetString("full_name")
                            }
                        End If
                    End Using
                End Using
            End Using

            Return Nothing
        End Function

        ''' <summary>
        ''' Adds a new user. The password must already be hashed.
        ''' </summary>
        Public Sub CreateUser(username As String, passwordHash As String, fullName As String)
            Const sql As String =
                "INSERT INTO users (username, password_hash, full_name) " &
                "VALUES (@username, @passwordHash, @fullName)"

            Using conn As MySqlConnection = DatabaseHelper.CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@username", username)
                    cmd.Parameters.AddWithValue("@passwordHash", passwordHash)
                    cmd.Parameters.AddWithValue("@fullName", fullName)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        ''' <summary>
        ''' How many user accounts exist. Zero means this is a brand-new installation.
        ''' </summary>
        Public Function CountUsers() As Integer
            Using conn As MySqlConnection = DatabaseHelper.CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand("SELECT COUNT(*) FROM users", conn)
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        End Function

        ''' <summary>
        ''' Creates the very first Admin account, but ONLY if the users table is still empty.
        ''' The check and the insert are one SQL statement, so this cannot be used to add
        ''' a second account. Returns True if the account was created, False if a user
        ''' already existed. The password must already be hashed.
        ''' </summary>
        Public Function CreateFirstAdmin(username As String, passwordHash As String, fullName As String) As Boolean
            Const sql As String =
                "INSERT INTO users (username, password_hash, full_name) " &
                "SELECT @username, @passwordHash, @fullName FROM DUAL " &
                "WHERE NOT EXISTS (SELECT 1 FROM users)"

            Using conn As MySqlConnection = DatabaseHelper.CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@username", username)
                    cmd.Parameters.AddWithValue("@passwordHash", passwordHash)
                    cmd.Parameters.AddWithValue("@fullName", fullName)
                    Return cmd.ExecuteNonQuery() = 1
                End Using
            End Using
        End Function

        ''' <summary>
        ''' Saves a new password hash for one user. The password must already be hashed.
        ''' </summary>
        Public Sub UpdatePasswordHash(userId As Integer, passwordHash As String)
            Const sql As String =
                "UPDATE users SET password_hash = @passwordHash WHERE user_id = @userId"

            Using conn As MySqlConnection = DatabaseHelper.CreateConnection()
                conn.Open()
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@passwordHash", passwordHash)
                    cmd.Parameters.AddWithValue("@userId", userId)
                    If cmd.ExecuteNonQuery() <> 1 Then
                        Throw New InvalidOperationException("Unable to find your account to update.")
                    End If
                End Using
            End Using
        End Sub

    End Module

End Namespace
