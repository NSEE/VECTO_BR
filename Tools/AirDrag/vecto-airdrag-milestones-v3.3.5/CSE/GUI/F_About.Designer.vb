<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class F_About
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(F_About))
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.PictureBoxJRC = New System.Windows.Forms.PictureBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label14 = New System.Windows.Forms.Label()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.LinkLabel1 = New System.Windows.Forms.LinkLabel()
        Me.LinkLicensed = New System.Windows.Forms.LinkLabel()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.LVersion = New System.Windows.Forms.Label()
        CType(Me.PictureBoxJRC, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Verdana", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(12, 145)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(77, 13)
        Me.Label2.TabIndex = 7
        Me.Label2.Text = "On behalf of"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Font = New System.Drawing.Font("Verdana", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label8.Location = New System.Drawing.Point(27, 167)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(135, 26)
        Me.Label8.TabIndex = 3
        Me.Label8.Text = "European Commission" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "DG CLIMA" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10)
        '
        'PictureBoxJRC
        '
        Me.PictureBoxJRC.Cursor = System.Windows.Forms.Cursors.Hand
        Me.PictureBoxJRC.Image = CType(resources.GetObject("PictureBoxJRC.Image"), System.Drawing.Image)
        Me.PictureBoxJRC.Location = New System.Drawing.Point(291, 145)
        Me.PictureBoxJRC.Name = "PictureBoxJRC"
        Me.PictureBoxJRC.Size = New System.Drawing.Size(161, 40)
        Me.PictureBoxJRC.TabIndex = 15
        Me.PictureBoxJRC.TabStop = False
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Font = New System.Drawing.Font("Verdana", 20.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label10.Location = New System.Drawing.Point(12, 9)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(128, 32)
        Me.Label10.TabIndex = 16
        Me.Label10.Text = "Air Drag"
        '
        'Label14
        '
        Me.Label14.AutoSize = True
        Me.Label14.Font = New System.Drawing.Font("Verdana", 20.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label14.Location = New System.Drawing.Point(10, 44)
        Me.Label14.Name = "Label14"
        Me.Label14.Size = New System.Drawing.Size(252, 32)
        Me.Label14.TabIndex = 17
        Me.Label14.Text = "VECTO-Air Drag"
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Font = New System.Drawing.Font("Verdana", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label12.Location = New System.Drawing.Point(12, 218)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(78, 18)
        Me.Label12.TabIndex = 18
        Me.Label12.Text = "Support"
        '
        'LinkLabel1
        '
        Me.LinkLabel1.AutoSize = True
        Me.LinkLabel1.Font = New System.Drawing.Font("Verdana", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LinkLabel1.Location = New System.Drawing.Point(12, 242)
        Me.LinkLabel1.Name = "LinkLabel1"
        Me.LinkLabel1.Size = New System.Drawing.Size(143, 13)
        Me.LinkLabel1.TabIndex = 19
        Me.LinkLabel1.TabStop = True
        Me.LinkLabel1.Text = "jrc-vecto@ec.europa.eu"
        '
        'LinkLicensed
        '
        Me.LinkLicensed.AutoSize = True
        Me.LinkLicensed.Font = New System.Drawing.Font("Verdana", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LinkLicensed.Location = New System.Drawing.Point(12, 87)
        Me.LinkLicensed.Name = "LinkLicensed"
        Me.LinkLicensed.Size = New System.Drawing.Size(179, 13)
        Me.LinkLicensed.TabIndex = 21
        Me.LinkLicensed.TabStop = True
        Me.LinkLicensed.Text = "VECTO is licensed under EUPL"
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Font = New System.Drawing.Font("Verdana", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label13.Location = New System.Drawing.Point(12, 105)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(194, 13)
        Me.Label13.TabIndex = 20
        Me.Label13.Text = "Copyright: 2014 European Union"
        '
        'LVersion
        '
        Me.LVersion.AutoSize = True
        Me.LVersion.Font = New System.Drawing.Font("Microsoft Sans Serif", 11.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LVersion.Location = New System.Drawing.Point(277, 56)
        Me.LVersion.Name = "LVersion"
        Me.LVersion.Size = New System.Drawing.Size(65, 18)
        Me.LVersion.TabIndex = 22
        Me.LVersion.Text = "Version"
        '
        'F_About
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(464, 271)
        Me.Controls.Add(Me.LVersion)
        Me.Controls.Add(Me.LinkLicensed)
        Me.Controls.Add(Me.Label13)
        Me.Controls.Add(Me.LinkLabel1)
        Me.Controls.Add(Me.Label12)
        Me.Controls.Add(Me.Label14)
        Me.Controls.Add(Me.Label10)
        Me.Controls.Add(Me.PictureBoxJRC)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label8)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximumSize = New System.Drawing.Size(480, 310)
        Me.MinimumSize = New System.Drawing.Size(480, 310)
        Me.Name = "F_About"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "About Air Drag"
        CType(Me.PictureBoxJRC, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents PictureBoxJRC As System.Windows.Forms.PictureBox
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents Label14 As System.Windows.Forms.Label
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents LinkLabel1 As System.Windows.Forms.LinkLabel
    Friend WithEvents LinkLicensed As System.Windows.Forms.LinkLabel
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents LVersion As System.Windows.Forms.Label
End Class
