Namespace Helpers

    ''' <summary>
    ''' Converts kg/g, L/ml, and pcs. Other families are rejected.
    ''' </summary>
    Public Module UnitConverter

        Public Function TryNormalize(raw As String, ByRef canonical As String) As Boolean
            Select Case If(raw, "").Trim().ToLowerInvariant()
                Case "kg", "kilogram", "kilograms"
                    canonical = "kg"
                Case "g", "gram", "grams"
                    canonical = "g"
                Case "l", "liter", "litre", "liters", "litres"
                    canonical = "L"
                Case "ml", "milliliter", "millilitre", "milliliters", "millilitres"
                    canonical = "ml"
                Case "pc", "pcs", "piece", "pieces"
                    canonical = "pcs"
                Case Else
                    canonical = ""
                    Return False
            End Select
            Return True
        End Function

        Public Function Normalize(raw As String) As String
            Dim canonical As String = ""
            If Not TryNormalize(raw, canonical) Then
                Throw New InvalidOperationException("Use kg, g, L, ml, or pcs.")
            End If
            Return canonical
        End Function

        Public Function AreCompatible(left As String, right As String) As Boolean
            Dim leftUnit As String = ""
            Dim rightUnit As String = ""
            If Not TryNormalize(left, leftUnit) OrElse Not TryNormalize(right, rightUnit) Then Return False
            Return Family(leftUnit) = Family(rightUnit)
        End Function

        ''' <summary>
        ''' Units an admin may store on a recipe for this inventory item.
        ''' Known families offer each member. Anything else must be copied as stored.
        ''' </summary>
        Public Function UnitChoices(inventoryUnit As String) As IList(Of String)
            Dim canonical As String = ""
            If Not TryNormalize(inventoryUnit, canonical) Then
                Dim raw As String = If(inventoryUnit, "").Trim()
                If raw = "" Then Return Array.Empty(Of String)()
                Return New String() {raw}
            End If
            Select Case canonical
                Case "kg", "g"
                    Return New String() {"g", "kg"}
                Case "L", "ml"
                    Return New String() {"ml", "L"}
                Case "pcs"
                    Return New String() {"pc", "pcs"}
                Case Else
                    Return New String() {canonical}
            End Select
        End Function

        ''' <summary>
        ''' Returns the recipe unit to store. Blank means the inventory item's own unit.
        ''' </summary>
        Public Function MatchUnit(inventoryUnit As String, requestedUnit As String) As String
            Dim choices As IList(Of String) = UnitChoices(inventoryUnit)
            If choices.Count = 0 Then
                Throw New InvalidOperationException("That inventory item has no unit.")
            End If

            Dim requested As String = If(requestedUnit, "").Trim()
            If requested = "" Then requested = If(inventoryUnit, "").Trim()

            For Each choice As String In choices
                If choice.Equals(requested, StringComparison.OrdinalIgnoreCase) Then Return choice
            Next

            Dim requestedCanonical As String = ""
            If TryNormalize(requested, requestedCanonical) Then
                For Each choice As String In choices
                    Dim choiceCanonical As String = ""
                    If TryNormalize(choice, choiceCanonical) AndAlso choiceCanonical = requestedCanonical Then
                        Return choice
                    End If
                Next
            End If

            Throw New InvalidOperationException(
                "Use " & String.Join(" or ", choices) & " for this inventory item.")
        End Function

        Public Function ConvertQuantity(quantity As Decimal, fromUnit As String, toUnit As String) As Decimal
            Dim source As String = Normalize(fromUnit)
            Dim target As String = Normalize(toUnit)
            If source = target Then Return quantity
            If Family(source) <> Family(target) Then
                Throw New InvalidOperationException(
                    source & " cannot be converted to " & target & ". Use a matching unit.")
            End If
            Return FromBase(ToBase(quantity, source), target)
        End Function

        Public Function StockLevel(quantity As Decimal, reorderPoint As Decimal) As String
            If quantity <= 0D Then Return "Out of Stock"
            If quantity <= reorderPoint Then Return "Low"
            Return "Normal"
        End Function

        Public Function RoundQuantity(value As Decimal) As Decimal
            Return Decimal.Round(value, 3, MidpointRounding.AwayFromZero)
        End Function

        Public Function RoundMoney(value As Decimal) As Decimal
            Return Decimal.Round(value, 2, MidpointRounding.AwayFromZero)
        End Function

        Public Function RoundCost(value As Decimal) As Decimal
            Return Decimal.Round(value, 4, MidpointRounding.AwayFromZero)
        End Function

        Public Function WeightedAverageCost(onHand As Decimal, onHandCost As Decimal, receivedQty As Decimal, receivedSubtotal As Decimal) As Decimal
            If onHand < 0D OrElse receivedQty <= 0D Then
                Throw New InvalidOperationException("Inventory quantity cannot be negative.")
            End If
            Dim totalQty As Decimal = onHand + receivedQty
            If totalQty <= 0D Then
                Throw New InvalidOperationException("Inventory quantity cannot be negative.")
            End If
            Return RoundCost((onHand * onHandCost + receivedSubtotal) / totalQty)
        End Function

        Private Function Family(unit As String) As String
            Select Case unit
                Case "kg", "g"
                    Return "weight"
                Case "L", "ml"
                    Return "volume"
                Case "pcs"
                    Return "count"
                Case Else
                    Return ""
            End Select
        End Function

        Private Function ToBase(quantity As Decimal, unit As String) As Decimal
            Select Case unit
                Case "kg", "L"
                    Return quantity * 1000D
                Case Else
                    Return quantity
            End Select
        End Function

        Private Function FromBase(baseQuantity As Decimal, unit As String) As Decimal
            Select Case unit
                Case "kg", "L"
                    Return baseQuantity / 1000D
                Case Else
                    Return baseQuantity
            End Select
        End Function

    End Module

End Namespace
