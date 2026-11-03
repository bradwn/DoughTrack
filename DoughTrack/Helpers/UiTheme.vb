Namespace Helpers

    ''' <summary>
    ''' Colors and grid styling shared by the main window and every module.
    ''' </summary>
    Public Module UiTheme

        Public ReadOnly Sidebar As Color = Color.FromArgb(42, 36, 32)
        Public ReadOnly SidebarHover As Color = Color.FromArgb(62, 52, 44)
        Public ReadOnly SidebarSelected As Color = Color.FromArgb(74, 56, 40)
        Public ReadOnly Accent As Color = Color.FromArgb(214, 164, 92)
        Public ReadOnly Content As Color = Color.FromArgb(243, 244, 246)
        Public ReadOnly Card As Color = Color.White
        Public ReadOnly TextDark As Color = Color.FromArgb(31, 36, 48)
        Public ReadOnly TextMuted As Color = Color.FromArgb(107, 114, 128)
        Public ReadOnly Line As Color = Color.FromArgb(226, 228, 232)
        Public ReadOnly Danger As Color = Color.FromArgb(176, 74, 58)

        Public Function FormatMoney(value As Decimal) As String
            If value < 0D Then
                Return "−₱" & Math.Abs(value).ToString("#,##0.00")
            End If
            Return "₱" & value.ToString("#,##0.00")
        End Function

        Public Function MakeButton(caption As String) As Button
            Dim button As New Button With {
                .Text = caption,
                .FlatStyle = FlatStyle.Flat,
                .BackColor = Sidebar,
                .ForeColor = Color.White,
                .Height = 34,
                .AutoSize = True,
                .Padding = New Padding(12, 0, 12, 0),
                .Cursor = Cursors.Hand,
                .UseVisualStyleBackColor = False
            }
            button.FlatAppearance.BorderSize = 0
            Return button
        End Function

        Public Sub StyleGrid(grid As DataGridView)
            grid.ReadOnly = True
            grid.AllowUserToAddRows = False
            grid.AllowUserToDeleteRows = False
            grid.AllowUserToResizeRows = False
            grid.RowHeadersVisible = False
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            grid.MultiSelect = False
            grid.BorderStyle = BorderStyle.None
            grid.BackgroundColor = Card
            grid.GridColor = Line
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            grid.EnableHeadersVisualStyles = False
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            grid.RowTemplate.Height = 34
            grid.ColumnHeadersHeight = 38
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            grid.ColumnHeadersDefaultCellStyle.BackColor = Sidebar
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Sidebar
            grid.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
            grid.ColumnHeadersDefaultCellStyle.Padding = New Padding(8, 0, 8, 0)
            grid.DefaultCellStyle.BackColor = Card
            grid.DefaultCellStyle.ForeColor = TextDark
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 232, 208)
            grid.DefaultCellStyle.SelectionForeColor = TextDark
            grid.DefaultCellStyle.Padding = New Padding(8, 0, 8, 0)
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 248, 245)
            AddHandler grid.DataError, Sub(sender, e) e.ThrowException = False
        End Sub

        Public Sub ApplyColumnFormats(grid As DataGridView, moneyColumns As IEnumerable(Of String), quantityColumns As IEnumerable(Of String))
            Dim money As New HashSet(Of String)(If(moneyColumns, Array.Empty(Of String)()), StringComparer.OrdinalIgnoreCase)
            Dim quantities As New HashSet(Of String)(If(quantityColumns, Array.Empty(Of String)()), StringComparer.OrdinalIgnoreCase)

            For Each column As DataGridViewColumn In grid.Columns
                If money.Contains(column.Name) OrElse money.Contains(column.HeaderText) Then
                    column.DefaultCellStyle.Format = "₱#,##0.00;−₱#,##0.00"
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
                ElseIf quantities.Contains(column.Name) OrElse quantities.Contains(column.HeaderText) Then
                    column.DefaultCellStyle.Format = "#,##0.###"
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
                ElseIf column.ValueType Is GetType(DateTime) Then
                    If column.HeaderText = "Date" Then
                        column.DefaultCellStyle.Format = "MMM d, yyyy"
                    Else
                        column.DefaultCellStyle.Format = "MMM d, yyyy h:mm tt"
                    End If
                End If
            Next
        End Sub

    End Module

End Namespace
