Imports DoughTrack.Models

Namespace Helpers

    ''' <summary>
    ''' Small display helpers shared by the inventory screens.
    ''' </summary>
    Public Module StockFormat

        ''' <summary>Example: 2,500 g</summary>
        Public Function FormatQuantity(quantity As Decimal, unit As String) As String
            Return quantity.ToString("#,##0.###") & " " & unit
        End Function

        ''' <summary>Display code such as ING-001 or SUP-012. Not stored in the database.</summary>
        Public Function ItemCode(item As InventoryItem) As String
            Dim prefix As String = If(item.Category = "Supplies", "SUP", "ING")
            Return prefix & "-" & item.InventoryItemId.ToString("000")
        End Function

        ''' <summary>Inactive, Out of Stock, Low, or Normal.</summary>
        Public Function StatusText(item As InventoryItem) As String
            If item.Status = "Inactive" Then Return "Inactive"
            Return UnitConverter.StockLevel(item.Quantity, item.ReorderPoint)
        End Function

        Public Function StatusColor(statusText As String) As Color
            Select Case statusText
                Case "Out of Stock", "Low"
                    Return UiTheme.Danger
                Case "Normal"
                    Return UiTheme.Success
                Case Else
                    Return UiTheme.TextMuted
            End Select
        End Function

    End Module

End Namespace
