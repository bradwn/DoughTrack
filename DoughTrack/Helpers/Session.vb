Imports DoughTrack.Models

Namespace Helpers

    ''' <summary>
    ''' Remembers the currently logged-in user while the app is running.
    ''' </summary>
    Public Module Session

        Public Property CurrentUser As Models.User

        Public ReadOnly Property IsLoggedIn As Boolean
            Get
                Return CurrentUser IsNot Nothing
            End Get
        End Property

        Public Sub Logout()
            CurrentUser = Nothing
        End Sub

    End Module

End Namespace