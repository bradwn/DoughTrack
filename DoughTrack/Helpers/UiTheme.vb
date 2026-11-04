Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Runtime.InteropServices

Namespace Helpers

    ''' <summary>
    ''' Shared colors and control styling taken from the Figma screens.
    ''' </summary>
    Public Module UiTheme

        ' Sampled from Design/references (grayscale Doughtown screens).
        Public ReadOnly Sidebar As Color = Color.FromArgb(44, 44, 44)
        Public ReadOnly SidebarHover As Color = Color.FromArgb(63, 63, 63)
        Public ReadOnly SidebarSelected As Color = Color.FromArgb(90, 90, 90)
        Public ReadOnly Header As Color = Color.FromArgb(128, 128, 128)
        Public ReadOnly Accent As Color = Color.FromArgb(210, 210, 210)
        Public ReadOnly Content As Color = Color.FromArgb(232, 232, 232)
        Public ReadOnly Card As Color = Color.FromArgb(245, 245, 245)
        Public ReadOnly CardHeader As Color = Color.FromArgb(112, 112, 112)
        Public ReadOnly InputFill As Color = Color.FromArgb(217, 217, 217)
        Public ReadOnly TableHeader As Color = Color.FromArgb(70, 70, 70)
        Public ReadOnly TableRow As Color = Color.FromArgb(230, 230, 230)
        Public ReadOnly TableAlt As Color = Color.FromArgb(240, 240, 240)
        Public ReadOnly PrimaryButton As Color = Color.FromArgb(30, 30, 30)
        Public ReadOnly SecondaryButton As Color = Color.FromArgb(196, 196, 196)
        Public ReadOnly TextDark As Color = Color.FromArgb(20, 20, 20)
        Public ReadOnly TextMuted As Color = Color.FromArgb(120, 120, 120)
        Public ReadOnly Line As Color = Color.FromArgb(200, 200, 200)
        Public ReadOnly Success As Color = Color.FromArgb(40, 160, 70)
        Public ReadOnly Warning As Color = Color.FromArgb(184, 120, 32)
        Public ReadOnly Danger As Color = Color.FromArgb(200, 40, 40)
        Public ReadOnly SuccessWash As Color = Color.FromArgb(226, 244, 233)
        Public ReadOnly DangerWash As Color = Color.FromArgb(252, 228, 224)

        Private ReadOnly UiFont As Font = New Font("Segoe UI", 10.0!)
        Private _logo As Image

        Public Function FormatMoney(value As Decimal) As String
            If value < 0D Then
                Return "−₱" & Math.Abs(value).ToString("#,##0.00")
            End If
            Return "₱" & value.ToString("#,##0.00")
        End Function

        Public Function LogoFile() As String
            Dim besideApp As String = Path.Combine(AppContext.BaseDirectory, "Resources", "doughtown_logo.png")
            If File.Exists(besideApp) Then Return besideApp
            Return besideApp
        End Function

        Public Function LogoImage() As Image
            If _logo IsNot Nothing Then Return _logo
            Dim path As String = LogoFile()
            If Not File.Exists(path) Then Return Nothing
            Try
                Using stream As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                    Using source As Image = Image.FromStream(stream)
                        _logo = New Bitmap(source)
                    End Using
                End Using
            Catch
                _logo = Nothing
            End Try
            Return _logo
        End Function

        Public Function MakeButton(caption As String) As Button
            If caption = "Cancel" OrElse caption = "Close" OrElse caption = "Clear" OrElse
               caption = "Clear Filters" OrElse caption = "Clear Order" Then
                Return MakeSecondaryButton(caption)
            End If
            If caption = "Remove" OrElse caption = "Delete" Then
                Return MakeDangerButton(caption)
            End If
            Return MakePrimaryButton(caption)
        End Function

        Public Function MakePrimaryButton(caption As String) As Button
            Dim button As New Button With {
                .Text = caption,
                .FlatStyle = FlatStyle.Flat,
                .BackColor = PrimaryButton,
                .ForeColor = Color.White,
                .Height = 36,
                .AutoSize = True,
                .Padding = New Padding(14, 0, 14, 0),
                .Margin = New Padding(6, 2, 0, 2),
                .Cursor = Cursors.Hand,
                .Font = UiFont,
                .UseVisualStyleBackColor = False
            }
            button.FlatAppearance.BorderSize = 0
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 55, 55)
            button.FlatAppearance.MouseDownBackColor = Color.Black
            Return button
        End Function

        Public Function MakeSecondaryButton(caption As String) As Button
            Dim button As New Button With {
                .Text = caption,
                .FlatStyle = FlatStyle.Flat,
                .BackColor = SecondaryButton,
                .ForeColor = TextDark,
                .Height = 36,
                .AutoSize = True,
                .Padding = New Padding(14, 0, 14, 0),
                .Margin = New Padding(6, 2, 0, 2),
                .Cursor = Cursors.Hand,
                .Font = UiFont,
                .UseVisualStyleBackColor = False
            }
            button.FlatAppearance.BorderSize = 0
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(176, 176, 176)
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(160, 160, 160)
            Return button
        End Function

        Public Function MakeDangerButton(caption As String) As Button
            Dim button As Button = MakePrimaryButton(caption)
            button.BackColor = Danger
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(168, 32, 32)
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(140, 24, 24)
            Return button
        End Function

        Public Function MakeSectionHeader(title As String) As Panel
            Dim header As New Panel With {
                .Dock = DockStyle.Top,
                .Height = 36,
                .BackColor = CardHeader
            }
            Dim label As New Label With {
                .Text = title,
                .Dock = DockStyle.Fill,
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI", 10.0!, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleLeft,
                .Padding = New Padding(12, 0, 8, 0)
            }
            header.Controls.Add(label)
            Return header
        End Function

        Public Sub StyleTextBox(box As TextBox)
            box.BorderStyle = BorderStyle.FixedSingle
            box.BackColor = InputFill
            box.ForeColor = TextDark
            box.Font = UiFont
        End Sub

        Public Sub StyleCombo(combo As ComboBox)
            combo.FlatStyle = FlatStyle.Flat
            combo.BackColor = InputFill
            combo.ForeColor = TextDark
            combo.Font = UiFont
        End Sub

        Public Sub StyleNumber(number As NumericUpDown)
            number.BackColor = InputFill
            number.ForeColor = TextDark
            number.Font = UiFont
            number.BorderStyle = BorderStyle.FixedSingle
        End Sub

        Public Sub StyleDate(picker As DateTimePicker)
            picker.Font = UiFont
            picker.CalendarForeColor = TextDark
            picker.CalendarMonthBackground = Card
        End Sub

        Public Sub StyleInputs(root As Control)
            If root Is Nothing Then Return
            For Each child As Control In root.Controls
                If TypeOf child Is TextBox Then
                    StyleTextBox(DirectCast(child, TextBox))
                ElseIf TypeOf child Is ComboBox Then
                    StyleCombo(DirectCast(child, ComboBox))
                ElseIf TypeOf child Is NumericUpDown Then
                    StyleNumber(DirectCast(child, NumericUpDown))
                ElseIf TypeOf child Is DateTimePicker Then
                    StyleDate(DirectCast(child, DateTimePicker))
                End If
                If child.HasChildren Then StyleInputs(child)
            Next
        End Sub

        Public Sub SetCue(box As TextBox, cue As String)
            If box.IsHandleCreated Then
                SendMessage(box.Handle, &H1501, New IntPtr(1), cue)
            Else
                Dim text As String = cue
                AddHandler box.HandleCreated, Sub() SendMessage(box.Handle, &H1501, New IntPtr(1), text)
            End If
        End Sub

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
            grid.RowTemplate.Height = 36
            grid.ColumnHeadersHeight = 38
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            grid.ColumnHeadersDefaultCellStyle.BackColor = TableHeader
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = TableHeader
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White
            grid.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold)
            grid.ColumnHeadersDefaultCellStyle.Padding = New Padding(8, 0, 8, 0)
            grid.DefaultCellStyle.BackColor = TableRow
            grid.DefaultCellStyle.ForeColor = TextDark
            grid.DefaultCellStyle.Font = New Font("Segoe UI", 9.5!)
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 210, 210)
            grid.DefaultCellStyle.SelectionForeColor = TextDark
            grid.DefaultCellStyle.Padding = New Padding(8, 0, 8, 0)
            grid.AlternatingRowsDefaultCellStyle.BackColor = TableAlt
            grid.AlternatingRowsDefaultCellStyle.ForeColor = TextDark
            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 210, 210)
            grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = TextDark
            AddHandler grid.CellPainting, AddressOf PaintGridButton
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

        Public Function Confirm(owner As IWin32Window, message As String) As Boolean
            Using dialog As New frmNotice(message, True)
                Return dialog.ShowDialog(owner) = DialogResult.Yes
            End Using
        End Function

        Public Sub Notify(owner As IWin32Window, message As String, Optional title As String = "DoughTrack")
            Using dialog As New frmNotice(message, False, title)
                dialog.ShowDialog(owner)
            End Using
        End Sub

        Private Sub PaintGridButton(sender As Object, e As DataGridViewCellPaintingEventArgs)
            If e.RowIndex < 0 Then Return
            Dim grid As DataGridView = DirectCast(sender, DataGridView)
            If Not TypeOf grid.Columns(e.ColumnIndex) Is DataGridViewButtonColumn Then Return
            e.Paint(e.ClipBounds, DataGridViewPaintParts.Background Or DataGridViewPaintParts.Border)
            Dim rect As Rectangle = Rectangle.Inflate(e.CellBounds, -6, -7)
            If rect.Width < 8 OrElse rect.Height < 8 Then
                e.Handled = True
                Return
            End If
            Dim caption As String = Convert.ToString(e.FormattedValue)
            Dim fill As Color = PrimaryButton
            If String.Equals(caption, "Remove", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(caption, "Delete", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(caption, "Deactivate", StringComparison.OrdinalIgnoreCase) Then
                fill = Danger
            End If
            Using brush As New SolidBrush(fill)
                e.Graphics.FillRectangle(brush, rect)
            End Using
            TextRenderer.DrawText(e.Graphics, caption, New Font("Segoe UI", 8.5!), rect, Color.White,
                                  TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis)
            e.Handled = True
        End Sub

        <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
        Private Function SendMessage(hWnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As String) As IntPtr
        End Function

    End Module

    ''' <summary>Circular white badge that draws the cafe logo without stretching it.</summary>
    Public Class LogoBadge
        Inherits Control

        Public Sub New()
            SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer, True)
            Size = New Size(88, 88)
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.InterpolationMode = InterpolationMode.HighQualityBicubic
            g.PixelOffsetMode = PixelOffsetMode.HighQuality
            g.Clear(If(Parent Is Nothing, UiTheme.Sidebar, Parent.BackColor))
            Dim bounds As New Rectangle(1, 1, Width - 3, Height - 3)
            Using brush As New SolidBrush(Color.White)
                g.FillEllipse(brush, bounds)
            End Using
            Dim logo As Image = UiTheme.LogoImage()
            If logo Is Nothing Then Return
            Using clip As New GraphicsPath()
                clip.AddEllipse(5, 5, Width - 10, Height - 10)
                g.SetClip(clip)
            End Using
            Dim inner As New Rectangle(10, 8, Width - 20, Height - 16)
            Dim scale As Single = Math.Min(inner.Width / CSng(logo.Width), inner.Height / CSng(logo.Height))
            Dim drawWidth As Integer = Math.Max(1, CInt(logo.Width * scale))
            Dim drawHeight As Integer = Math.Max(1, CInt(logo.Height * scale))
            Dim dest As New Rectangle(inner.X + (inner.Width - drawWidth) \ 2,
                                      inner.Y + (inner.Height - drawHeight) \ 2,
                                      drawWidth, drawHeight)
            g.DrawImage(logo, dest)
            g.ResetClip()
        End Sub

    End Class

    ''' <summary>Header bell drawn to match the Figma shell. It has no notification feed.</summary>
    Public Class BellIcon
        Inherits Control

        Public Sub New()
            SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer, True)
            Size = New Size(32, 32)
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.Clear(If(Parent Is Nothing, UiTheme.Header, Parent.BackColor))
            Using pen As New Pen(Color.White, 1.6F)
                g.DrawArc(pen, 9, 7, 14, 12, 200, 140)
                g.DrawLine(pen, 9, 14, 7, 21)
                g.DrawLine(pen, 23, 14, 25, 21)
                g.DrawLine(pen, 7, 21, 25, 21)
                g.DrawEllipse(pen, 14, 22, 4, 4)
                g.DrawLine(pen, 16, 5, 16, 8)
            End Using
        End Sub

    End Class

End Namespace
