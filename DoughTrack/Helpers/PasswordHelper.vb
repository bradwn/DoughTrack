Imports System.Security.Cryptography

Namespace Helpers

    ''' <summary>
    ''' Hashes and verifies passwords using PBKDF2 with a random salt.
    ''' Stored format:  iterations.salt.hash   (salt and hash are Base64 text)
    ''' </summary>
    Public Module PasswordHelper

        Private Const SaltSize As Integer = 16
        Private Const HashSize As Integer = 32
        Private Const DefaultIterations As Integer = 100000

        ''' <summary>Turns a plain password into a storable hash string.</summary>
        Public Function HashPassword(password As String) As String
            Dim salt As Byte() = RandomNumberGenerator.GetBytes(SaltSize)
            Dim hash As Byte() = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, DefaultIterations, HashAlgorithmName.SHA256, HashSize)

            Return DefaultIterations.ToString() & "." &
                   System.Convert.ToBase64String(salt) & "." &
                   System.Convert.ToBase64String(hash)
        End Function

        ''' <summary>
        ''' Checks a typed password against a stored hash.
        ''' Returns True only if they match.
        ''' </summary>
        Public Function VerifyPassword(password As String, storedHash As String) As Boolean
            Try
                Dim parts As String() = storedHash.Split("."c)
                If parts.Length <> 3 Then Return False

                Dim storedIterations As Integer = Integer.Parse(parts(0))
                Dim salt As Byte() = System.Convert.FromBase64String(parts(1))
                Dim expectedHash As Byte() = System.Convert.FromBase64String(parts(2))

                Dim actualHash As Byte() = Rfc2898DeriveBytes.Pbkdf2(
                    password, salt, storedIterations, HashAlgorithmName.SHA256, expectedHash.Length)

                Return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash)
            Catch
                Return False
            End Try
        End Function

    End Module

End Namespace