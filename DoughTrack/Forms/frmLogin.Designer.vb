<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmLogin
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        split = New TableLayoutPanel()
        pnlForm = New FlowLayoutPanel()
        lblTitle = New Label()
        lblUsername = New Label()
        pnlUser = New Panel()
        txtUsername = New TextBox()
        lblPassword = New Label()
        pnlPass = New Panel()
        txtPassword = New TextBox()
        lblMessage = New Label()
        btnLogin = New Button()
        pnlLinks = New Panel()
        lblForgot = New Label()
        btnExit = New Button()
        pnlBrand = New FlowLayoutPanel()
        picLogo = New PictureBox()
        lblLogoFallback = New Label()
        lblTagline = New Label()
        split.SuspendLayout()
        pnlForm.SuspendLayout()
        pnlUser.SuspendLayout()
        pnlPass.SuspendLayout()
        pnlLinks.SuspendLayout()
        pnlBrand.SuspendLayout()
        CType(picLogo, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' split
        ' 
        split.BackColor = Color.White
        split.ColumnCount = 2
        split.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        split.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        split.Controls.Add(pnlForm, 0, 0)
        split.Controls.Add(pnlBrand, 1, 0)
        split.Dock = DockStyle.Fill
        split.Location = New Point(0, 0)
        split.Name = "split"
        split.RowCount = 1
        split.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        split.Size = New Size(900, 600)
        split.TabIndex = 0
        ' 
        ' pnlForm
        ' 
        pnlForm.Anchor = AnchorStyles.None
        pnlForm.AutoSize = True
        pnlForm.AutoSizeMode = AutoSizeMode.GrowAndShrink
        pnlForm.BackColor = Color.White
        pnlForm.Controls.Add(lblTitle)
        pnlForm.Controls.Add(lblUsername)
        pnlForm.Controls.Add(pnlUser)
        pnlForm.Controls.Add(lblPassword)
        pnlForm.Controls.Add(pnlPass)
        pnlForm.Controls.Add(lblMessage)
        pnlForm.Controls.Add(btnLogin)
        pnlForm.Controls.Add(pnlLinks)
        pnlForm.FlowDirection = FlowDirection.TopDown
        pnlForm.Location = New Point(60, 133)
        pnlForm.Margin = New Padding(0)
        pnlForm.Name = "pnlForm"
        pnlForm.Size = New Size(330, 333)
        pnlForm.TabIndex = 0
        pnlForm.WrapContents = False
        ' 
        ' lblTitle
        ' 
        lblTitle.AutoSize = True
        lblTitle.Font = New Font("Segoe UI", 22F, FontStyle.Bold)
        lblTitle.ForeColor = Color.Black
        lblTitle.Location = New Point(0, 0)
        lblTitle.Margin = New Padding(0, 0, 0, 22)
        lblTitle.Name = "lblTitle"
        lblTitle.Size = New Size(188, 41)
        lblTitle.TabIndex = 0
        lblTitle.Text = "DoughTrack"
        ' 
        ' lblUsername
        ' 
        lblUsername.AutoSize = True
        lblUsername.Font = New Font("Segoe UI", 12F)
        lblUsername.ForeColor = Color.Black
        lblUsername.Location = New Point(0, 63)
        lblUsername.Margin = New Padding(0, 0, 0, 6)
        lblUsername.Name = "lblUsername"
        lblUsername.Size = New Size(84, 21)
        lblUsername.TabIndex = 1
        lblUsername.Text = "Username:"
        ' 
        ' pnlUser
        ' 
        pnlUser.BackColor = Color.FromArgb(CByte(217), CByte(217), CByte(217))
        pnlUser.Controls.Add(txtUsername)
        pnlUser.Location = New Point(0, 90)
        pnlUser.Margin = New Padding(0, 0, 0, 20)
        pnlUser.Name = "pnlUser"
        pnlUser.Padding = New Padding(10, 9, 10, 0)
        pnlUser.Size = New Size(330, 40)
        pnlUser.TabIndex = 2
        ' 
        ' txtUsername
        ' 
        txtUsername.BackColor = Color.FromArgb(CByte(217), CByte(217), CByte(217))
        txtUsername.BorderStyle = BorderStyle.None
        txtUsername.Dock = DockStyle.Fill
        txtUsername.Font = New Font("Segoe UI", 12F)
        txtUsername.Location = New Point(10, 9)
        txtUsername.MaxLength = 50
        txtUsername.Name = "txtUsername"
        txtUsername.Size = New Size(310, 22)
        txtUsername.TabIndex = 1
        ' 
        ' lblPassword
        ' 
        lblPassword.AutoSize = True
        lblPassword.Font = New Font("Segoe UI", 12F)
        lblPassword.ForeColor = Color.Black
        lblPassword.Location = New Point(0, 150)
        lblPassword.Margin = New Padding(0, 0, 0, 6)
        lblPassword.Name = "lblPassword"
        lblPassword.Size = New Size(79, 21)
        lblPassword.TabIndex = 3
        lblPassword.Text = "Password:"
        ' 
        ' pnlPass
        ' 
        pnlPass.BackColor = Color.FromArgb(CByte(217), CByte(217), CByte(217))
        pnlPass.Controls.Add(txtPassword)
        pnlPass.Location = New Point(0, 177)
        pnlPass.Margin = New Padding(0)
        pnlPass.Name = "pnlPass"
        pnlPass.Padding = New Padding(10, 9, 10, 0)
        pnlPass.Size = New Size(330, 40)
        pnlPass.TabIndex = 4
        ' 
        ' txtPassword
        ' 
        txtPassword.BackColor = Color.FromArgb(CByte(217), CByte(217), CByte(217))
        txtPassword.BorderStyle = BorderStyle.None
        txtPassword.Dock = DockStyle.Fill
        txtPassword.Font = New Font("Segoe UI", 12F)
        txtPassword.Location = New Point(10, 9)
        txtPassword.MaxLength = 100
        txtPassword.Name = "txtPassword"
        txtPassword.Size = New Size(310, 22)
        txtPassword.TabIndex = 2
        txtPassword.UseSystemPasswordChar = True
        ' 
        ' lblMessage
        ' 
        lblMessage.Font = New Font("Segoe UI", 9.5F)
        lblMessage.ForeColor = Color.FromArgb(CByte(176), CByte(74), CByte(58))
        lblMessage.Location = New Point(0, 225)
        lblMessage.Margin = New Padding(0, 8, 0, 4)
        lblMessage.Name = "lblMessage"
        lblMessage.Size = New Size(330, 24)
        lblMessage.TabIndex = 5
        lblMessage.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' btnLogin
        ' 
        btnLogin.BackColor = Color.FromArgb(CByte(217), CByte(217), CByte(217))
        btnLogin.Cursor = Cursors.Hand
        btnLogin.FlatAppearance.BorderSize = 0
        btnLogin.FlatAppearance.MouseDownBackColor = Color.FromArgb(CByte(160), CByte(160), CByte(160))
        btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(179), CByte(179), CByte(179))
        btnLogin.FlatStyle = FlatStyle.Flat
        btnLogin.Font = New Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        btnLogin.ForeColor = Color.Black
        btnLogin.Location = New Point(0, 257)
        btnLogin.Margin = New Padding(0, 4, 0, 12)
        btnLogin.Name = "btnLogin"
        btnLogin.Size = New Size(110, 38)
        btnLogin.TabIndex = 3
        btnLogin.Text = "Login"
        btnLogin.UseVisualStyleBackColor = False
        ' 
        ' pnlLinks
        ' 
        pnlLinks.Controls.Add(lblForgot)
        pnlLinks.Controls.Add(btnExit)
        pnlLinks.Location = New Point(0, 307)
        pnlLinks.Margin = New Padding(0)
        pnlLinks.Name = "pnlLinks"
        pnlLinks.Size = New Size(330, 26)
        pnlLinks.TabIndex = 6
        ' 
        ' lblForgot
        ' 
        lblForgot.AutoSize = True
        lblForgot.Cursor = Cursors.Hand
        lblForgot.Font = New Font("Segoe UI", 9F)
        lblForgot.ForeColor = Color.Black
        lblForgot.Location = New Point(0, 4)
        lblForgot.Name = "lblForgot"
        lblForgot.Size = New Size(100, 15)
        lblForgot.TabIndex = 0
        lblForgot.Text = "Forgot Password?"
        ' 
        ' btnExit
        ' 
        btnExit.BackColor = Color.White
        btnExit.Cursor = Cursors.Hand
        btnExit.Dock = DockStyle.Right
        btnExit.FlatAppearance.BorderSize = 0
        btnExit.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(243), CByte(243), CByte(243))
        btnExit.FlatStyle = FlatStyle.Flat
        btnExit.Font = New Font("Segoe UI", 9F)
        btnExit.ForeColor = Color.FromArgb(CByte(107), CByte(114), CByte(128))
        btnExit.Location = New Point(270, 0)
        btnExit.Name = "btnExit"
        btnExit.Size = New Size(60, 26)
        btnExit.TabIndex = 4
        btnExit.Text = "Exit"
        btnExit.UseVisualStyleBackColor = False
        ' 
        ' pnlBrand
        ' 
        pnlBrand.Anchor = AnchorStyles.None
        pnlBrand.AutoSize = True
        pnlBrand.AutoSizeMode = AutoSizeMode.GrowAndShrink
        pnlBrand.BackColor = Color.White
        pnlBrand.Controls.Add(picLogo)
        pnlBrand.Controls.Add(lblLogoFallback)
        pnlBrand.Controls.Add(lblTagline)
        pnlBrand.FlowDirection = FlowDirection.TopDown
        pnlBrand.Location = New Point(485, 18)
        pnlBrand.Margin = New Padding(0)
        pnlBrand.Name = "pnlBrand"
        pnlBrand.Size = New Size(380, 564)
        pnlBrand.TabIndex = 1
        pnlBrand.WrapContents = False
        ' 
        ' picLogo
        ' 
        picLogo.BackColor = Color.White
        picLogo.ImageLocation = ""
        picLogo.Location = New Point(0, 0)
        picLogo.Margin = New Padding(0, 0, 0, 14)
        picLogo.Name = "picLogo"
        picLogo.Size = New Size(380, 250)
        picLogo.SizeMode = PictureBoxSizeMode.Zoom
        picLogo.TabIndex = 0
        picLogo.TabStop = False
        ' 
        ' lblLogoFallback
        ' 
        lblLogoFallback.Font = New Font("Segoe UI", 30F, FontStyle.Bold)
        lblLogoFallback.Location = New Point(0, 264)
        lblLogoFallback.Margin = New Padding(0, 0, 0, 14)
        lblLogoFallback.Name = "lblLogoFallback"
        lblLogoFallback.Size = New Size(380, 250)
        lblLogoFallback.TabIndex = 1
        lblLogoFallback.Text = "doughtown" & vbCrLf & "ph"
        lblLogoFallback.TextAlign = ContentAlignment.MiddleCenter
        lblLogoFallback.Visible = False
        ' 
        ' lblTagline
        ' 
        lblTagline.Font = New Font("Segoe UI", 15F, FontStyle.Bold)
        lblTagline.ForeColor = Color.Black
        lblTagline.Location = New Point(0, 528)
        lblTagline.Margin = New Padding(0)
        lblTagline.Name = "lblTagline"
        lblTagline.Size = New Size(380, 36)
        lblTagline.TabIndex = 2
        lblTagline.Text = "coffee • snacks • dessert"
        lblTagline.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' frmLogin
        ' 
        AcceptButton = btnLogin
        AutoScaleMode = AutoScaleMode.None
        BackColor = Color.White
        ClientSize = New Size(900, 600)
        Controls.Add(split)
        MinimumSize = New Size(780, 520)
        Name = "frmLogin"
        StartPosition = FormStartPosition.CenterScreen
        Text = "DoughTrack - Login"
        split.ResumeLayout(False)
        split.PerformLayout()
        pnlForm.ResumeLayout(False)
        pnlForm.PerformLayout()
        pnlUser.ResumeLayout(False)
        pnlUser.PerformLayout()
        pnlPass.ResumeLayout(False)
        pnlPass.PerformLayout()
        pnlLinks.ResumeLayout(False)
        pnlLinks.PerformLayout()
        pnlBrand.ResumeLayout(False)
        CType(picLogo, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents split As TableLayoutPanel
    Friend WithEvents pnlForm As FlowLayoutPanel
    Friend WithEvents lblTitle As Label
    Friend WithEvents lblUsername As Label
    Friend WithEvents pnlUser As Panel
    Friend WithEvents txtUsername As TextBox
    Friend WithEvents lblPassword As Label
    Friend WithEvents pnlPass As Panel
    Friend WithEvents txtPassword As TextBox
    Friend WithEvents lblMessage As Label
    Friend WithEvents btnLogin As Button
    Friend WithEvents pnlLinks As Panel
    Friend WithEvents lblForgot As Label
    Friend WithEvents btnExit As Button
    Friend WithEvents pnlBrand As FlowLayoutPanel
    Friend WithEvents picLogo As PictureBox
    Friend WithEvents lblLogoFallback As Label
    Friend WithEvents lblTagline As Label
End Class
