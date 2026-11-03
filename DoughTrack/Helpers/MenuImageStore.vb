Imports System.IO

Namespace Helpers

    ''' <summary>
    ''' Copies menu photos into MenuImages and resolves the stored path.
    ''' </summary>
    Public Module MenuImageStore

        Private ReadOnly AllowedExtensions As String() = {".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp"}

        Public Function SaveCopy(sourcePath As String) As String
            If String.IsNullOrWhiteSpace(sourcePath) OrElse Not File.Exists(sourcePath) Then
                Throw New IOException("Choose an image file that exists.")
            End If

            Dim extension As String = Path.GetExtension(sourcePath).ToLowerInvariant()
            If Not AllowedExtensions.Contains(extension) Then
                Throw New IOException("Use a jpg, png, bmp, gif, or webp image.")
            End If

            Dim folder As String = Path.Combine(AppContext.BaseDirectory, "MenuImages")
            Directory.CreateDirectory(folder)
            Dim fileName As String = DateTime.Now.ToString("yyyyMMddHHmmssfff") & extension
            File.Copy(sourcePath, Path.Combine(folder, fileName), False)

            Dim stored As String = Path.Combine("MenuImages", fileName)
            If stored.Length > 255 Then
                Throw New IOException("The image path is too long to store.")
            End If
            Return stored
        End Function

        Public Function Resolve(storedPath As String) As String
            If String.IsNullOrWhiteSpace(storedPath) Then Return ""
            If Path.IsPathRooted(storedPath) Then Return storedPath
            Return Path.Combine(AppContext.BaseDirectory, storedPath)
        End Function

    End Module

End Namespace
