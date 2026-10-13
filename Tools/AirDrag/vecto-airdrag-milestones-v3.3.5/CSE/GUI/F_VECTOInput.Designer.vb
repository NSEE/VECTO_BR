<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class F_VECTOInput
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
        Me.TBManufacturer = New System.Windows.Forms.TextBox()
        Me.TBModel = New System.Windows.Forms.TextBox()
        Me.TBCertNum = New System.Windows.Forms.TextBox()
        Me.TBCdxA = New System.Windows.Forms.TextBox()
        Me.TBTransR = New System.Windows.Forms.TextBox()
        Me.TBWorstCase = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.ButtonCancel = New System.Windows.Forms.Button()
        Me.ButtonOK = New System.Windows.Forms.Button()
        Me.GBCFD = New System.Windows.Forms.GroupBox()
        Me.CBCFD_Enabled = New System.Windows.Forms.CheckBox()
        Me.LBDeltaCFD_Unit = New System.Windows.Forms.Label()
        Me.LBLicenseCFDMethod = New System.Windows.Forms.Label()
        Me.LBCdACFD = New System.Windows.Forms.Label()
        Me.TBCdACFD = New System.Windows.Forms.TextBox()
        Me.TBLicenseCFDMethod = New System.Windows.Forms.TextBox()
        Me.GBCFD.SuspendLayout()
        Me.SuspendLayout()
        '
        'TBManufacturer
        '
        Me.TBManufacturer.Location = New System.Drawing.Point(256, 18)
        Me.TBManufacturer.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.TBManufacturer.Name = "TBManufacturer"
        Me.TBManufacturer.Size = New System.Drawing.Size(224, 26)
        Me.TBManufacturer.TabIndex = 0
        '
        'TBModel
        '
        Me.TBModel.Location = New System.Drawing.Point(256, 58)
        Me.TBModel.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.TBModel.Name = "TBModel"
        Me.TBModel.Size = New System.Drawing.Size(224, 26)
        Me.TBModel.TabIndex = 1
        '
        'TBCertNum
        '
        Me.TBCertNum.Location = New System.Drawing.Point(256, 98)
        Me.TBCertNum.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.TBCertNum.Name = "TBCertNum"
        Me.TBCertNum.Size = New System.Drawing.Size(224, 26)
        Me.TBCertNum.TabIndex = 2
        '
        'TBCdxA
        '
        Me.TBCdxA.Location = New System.Drawing.Point(256, 138)
        Me.TBCdxA.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.TBCdxA.Name = "TBCdxA"
        Me.TBCdxA.Size = New System.Drawing.Size(148, 26)
        Me.TBCdxA.TabIndex = 3
        '
        'TBTransR
        '
        Me.TBTransR.Location = New System.Drawing.Point(254, 362)
        Me.TBTransR.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.TBTransR.Name = "TBTransR"
        Me.TBTransR.Size = New System.Drawing.Size(148, 26)
        Me.TBTransR.TabIndex = 4
        '
        'TBWorstCase
        '
        Me.TBWorstCase.Location = New System.Drawing.Point(254, 402)
        Me.TBWorstCase.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.TBWorstCase.Name = "TBWorstCase"
        Me.TBWorstCase.Size = New System.Drawing.Size(148, 26)
        Me.TBWorstCase.TabIndex = 5
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(18, 23)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(104, 20)
        Me.Label1.TabIndex = 6
        Me.Label1.Text = "Manufacturer"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(18, 63)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(52, 20)
        Me.Label2.TabIndex = 7
        Me.Label2.Text = "Model"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(18, 103)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(153, 20)
        Me.Label3.TabIndex = 8
        Me.Label3.Text = "Certification Number"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(18, 143)
        Me.Label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(47, 20)
        Me.Label4.TabIndex = 9
        Me.Label4.Text = "CdxA"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(16, 367)
        Me.Label5.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(195, 20)
        Me.Label5.TabIndex = 10
        Me.Label5.Text = "Delta CdxA - transfer rules"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(16, 407)
        Me.Label6.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(228, 20)
        Me.Label6.TabIndex = 11
        Me.Label6.Text = "Delta CdxA - worst case parent"
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(416, 143)
        Me.Label7.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(27, 20)
        Me.Label7.TabIndex = 12
        Me.Label7.Text = "m²"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(414, 407)
        Me.Label8.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(27, 20)
        Me.Label8.TabIndex = 13
        Me.Label8.Text = "m²"
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(414, 367)
        Me.Label9.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(27, 20)
        Me.Label9.TabIndex = 14
        Me.Label9.Text = "m²"
        '
        'ButtonCancel
        '
        Me.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.ButtonCancel.Location = New System.Drawing.Point(368, 462)
        Me.ButtonCancel.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.ButtonCancel.Name = "ButtonCancel"
        Me.ButtonCancel.Size = New System.Drawing.Size(112, 35)
        Me.ButtonCancel.TabIndex = 15
        Me.ButtonCancel.Text = "Cancel"
        Me.ButtonCancel.UseVisualStyleBackColor = True
        '
        'ButtonOK
        '
        Me.ButtonOK.Location = New System.Drawing.Point(247, 462)
        Me.ButtonOK.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.ButtonOK.Name = "ButtonOK"
        Me.ButtonOK.Size = New System.Drawing.Size(112, 35)
        Me.ButtonOK.TabIndex = 16
        Me.ButtonOK.Text = "OK"
        Me.ButtonOK.UseVisualStyleBackColor = True
        '
        'GBCFD
        '
        Me.GBCFD.Controls.Add(Me.CBCFD_Enabled)
        Me.GBCFD.Controls.Add(Me.LBDeltaCFD_Unit)
        Me.GBCFD.Controls.Add(Me.LBLicenseCFDMethod)
        Me.GBCFD.Controls.Add(Me.LBCdACFD)
        Me.GBCFD.Controls.Add(Me.TBCdACFD)
        Me.GBCFD.Controls.Add(Me.TBLicenseCFDMethod)
        Me.GBCFD.Location = New System.Drawing.Point(22, 172)
        Me.GBCFD.Name = "GBCFD"
        Me.GBCFD.Size = New System.Drawing.Size(458, 182)
        Me.GBCFD.TabIndex = 17
        Me.GBCFD.TabStop = False
        Me.GBCFD.Text = "CFD"
        '
        'CBCFD_Enabled
        '
        Me.CBCFD_Enabled.AutoSize = True
        Me.CBCFD_Enabled.Location = New System.Drawing.Point(22, 29)
        Me.CBCFD_Enabled.Name = "CBCFD_Enabled"
        Me.CBCFD_Enabled.Size = New System.Drawing.Size(94, 24)
        Me.CBCFD_Enabled.TabIndex = 22
        Me.CBCFD_Enabled.Text = "Enabled"
        Me.CBCFD_Enabled.UseVisualStyleBackColor = True
        '
        'LBDeltaCFD_Unit
        '
        Me.LBDeltaCFD_Unit.AutoSize = True
        Me.LBDeltaCFD_Unit.Location = New System.Drawing.Point(413, 69)
        Me.LBDeltaCFD_Unit.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LBDeltaCFD_Unit.Name = "LBDeltaCFD_Unit"
        Me.LBDeltaCFD_Unit.Size = New System.Drawing.Size(27, 20)
        Me.LBDeltaCFD_Unit.TabIndex = 20
        Me.LBDeltaCFD_Unit.Text = "m²"
        '
        'LBLicenseCFDMethod
        '
        Me.LBLicenseCFDMethod.AutoSize = True
        Me.LBLicenseCFDMethod.Location = New System.Drawing.Point(18, 109)
        Me.LBLicenseCFDMethod.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LBLicenseCFDMethod.Name = "LBLicenseCFDMethod"
        Me.LBLicenseCFDMethod.Size = New System.Drawing.Size(219, 20)
        Me.LBLicenseCFDMethod.TabIndex = 21
        Me.LBLicenseCFDMethod.Text = "License Number CFD Method"
        '
        'LBCdACFD
        '
        Me.LBCdACFD.AutoSize = True
        Me.LBCdACFD.Location = New System.Drawing.Point(18, 69)
        Me.LBCdACFD.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.LBCdACFD.Name = "LBCdACFD"
        Me.LBCdACFD.Size = New System.Drawing.Size(135, 20)
        Me.LBCdACFD.TabIndex = 19
        Me.LBCdACFD.Text = "Delta CdxA - CFD"
        '
        'TBCdACFD
        '
        Me.TBCdACFD.Location = New System.Drawing.Point(253, 64)
        Me.TBCdACFD.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.TBCdACFD.Name = "TBCdACFD"
        Me.TBCdACFD.Size = New System.Drawing.Size(148, 26)
        Me.TBCdACFD.TabIndex = 18
        '
        'TBLicenseCFDMethod
        '
        Me.TBLicenseCFDMethod.Location = New System.Drawing.Point(19, 136)
        Me.TBLicenseCFDMethod.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.TBLicenseCFDMethod.Name = "TBLicenseCFDMethod"
        Me.TBLicenseCFDMethod.Size = New System.Drawing.Size(382, 26)
        Me.TBLicenseCFDMethod.TabIndex = 3
        '
        'F_VECTOInput
        '
        Me.AcceptButton = Me.ButtonOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(9.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.ButtonCancel
        Me.ClientSize = New System.Drawing.Size(501, 519)
        Me.Controls.Add(Me.GBCFD)
        Me.Controls.Add(Me.ButtonOK)
        Me.Controls.Add(Me.ButtonCancel)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.TBWorstCase)
        Me.Controls.Add(Me.TBTransR)
        Me.Controls.Add(Me.TBCdxA)
        Me.Controls.Add(Me.TBCertNum)
        Me.Controls.Add(Me.TBModel)
        Me.Controls.Add(Me.TBManufacturer)
        Me.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.Name = "F_VECTOInput"
        Me.Text = "F_VECTOInput"
        Me.GBCFD.ResumeLayout(False)
        Me.GBCFD.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents TBManufacturer As TextBox
    Friend WithEvents TBModel As TextBox
    Friend WithEvents TBCertNum As TextBox
    Friend WithEvents TBCdxA As TextBox
    Friend WithEvents TBTransR As TextBox
    Friend WithEvents TBWorstCase As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents Label9 As Label
    Friend WithEvents ButtonCancel As Button
    Friend WithEvents ButtonOK As Button
    Friend WithEvents GBCFD As GroupBox
    Friend WithEvents TBLicenseCFDMethod As TextBox
    Friend WithEvents LBDeltaCFD_Unit As Label
    Friend WithEvents LBCdACFD As Label
    Friend WithEvents TBCdACFD As TextBox
    Friend WithEvents LBLicenseCFDMethod As Label
    Friend WithEvents CBCFD_Enabled As CheckBox
End Class
