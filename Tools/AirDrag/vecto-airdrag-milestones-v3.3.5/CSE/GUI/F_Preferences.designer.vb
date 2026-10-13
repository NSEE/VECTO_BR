<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class F_Preferences
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(F_Preferences))
        Me.workingDir = New System.Windows.Forms.TextBox()
        Me.ButtonSelectWorDir = New System.Windows.Forms.Button()
        Me.GroupBoxWorDir = New System.Windows.Forms.GroupBox()
        Me.ButtonOK = New System.Windows.Forms.Button()
        Me.ButtonCancel = New System.Windows.Forms.Button()
        Me.writeLog = New System.Windows.Forms.CheckBox()
        Me.GroupBoxInterface = New System.Windows.Forms.GroupBox()
        Me.LabelInfo = New System.Windows.Forms.Label()
        Me.logSize = New System.Windows.Forms.TextBox()
        Me.Label16 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.logLevel = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.listSep = New System.Windows.Forms.TextBox()
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.decSep = New System.Windows.Forms.TextBox()
        Me.GroupBoxNotepad = New System.Windows.Forms.GroupBox()
        Me.ButtonSelectNotepad = New System.Windows.Forms.Button()
        Me.editor = New System.Windows.Forms.TextBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.hideUsername = New System.Windows.Forms.CheckBox()
        Me.strictBodies = New System.Windows.Forms.CheckBox()
        Me.includeSchemas = New System.Windows.Forms.CheckBox()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.CheckBox1 = New System.Windows.Forms.CheckBox()
        Me.ButtonReload = New System.Windows.Forms.Button()
        Me.ButtonSave = New System.Windows.Forms.Button()
        Me.GroupBoxWorDir.SuspendLayout()
        Me.GroupBoxInterface.SuspendLayout()
        Me.TabControl1.SuspendLayout()
        Me.TabPage2.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBoxNotepad.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'workingDir
        '
        Me.workingDir.Location = New System.Drawing.Point(6, 19)
        Me.workingDir.Name = "workingDir"
        Me.workingDir.Size = New System.Drawing.Size(514, 20)
        Me.workingDir.TabIndex = 1
        '
        'ButtonSelectWorDir
        '
        Me.ButtonSelectWorDir.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ButtonSelectWorDir.Location = New System.Drawing.Point(526, 18)
        Me.ButtonSelectWorDir.Name = "ButtonSelectWorDir"
        Me.ButtonSelectWorDir.Size = New System.Drawing.Size(26, 20)
        Me.ButtonSelectWorDir.TabIndex = 2
        Me.ButtonSelectWorDir.Text = "..."
        Me.ButtonSelectWorDir.UseVisualStyleBackColor = True
        '
        'GroupBoxWorDir
        '
        Me.GroupBoxWorDir.Controls.Add(Me.ButtonSelectWorDir)
        Me.GroupBoxWorDir.Controls.Add(Me.workingDir)
        Me.GroupBoxWorDir.Location = New System.Drawing.Point(5, 6)
        Me.GroupBoxWorDir.Name = "GroupBoxWorDir"
        Me.GroupBoxWorDir.Size = New System.Drawing.Size(558, 51)
        Me.GroupBoxWorDir.TabIndex = 2
        Me.GroupBoxWorDir.TabStop = False
        Me.GroupBoxWorDir.Text = "Working Directory"
        '
        'ButtonOK
        '
        Me.ButtonOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.ButtonOK.Location = New System.Drawing.Point(502, 266)
        Me.ButtonOK.Name = "ButtonOK"
        Me.ButtonOK.Size = New System.Drawing.Size(75, 23)
        Me.ButtonOK.TabIndex = 0
        Me.ButtonOK.Text = "OK"
        Me.ButtonOK.UseVisualStyleBackColor = True
        '
        'ButtonCancel
        '
        Me.ButtonCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.ButtonCancel.Location = New System.Drawing.Point(421, 266)
        Me.ButtonCancel.Name = "ButtonCancel"
        Me.ButtonCancel.Size = New System.Drawing.Size(75, 23)
        Me.ButtonCancel.TabIndex = 1
        Me.ButtonCancel.Text = "Cancel"
        Me.ButtonCancel.UseVisualStyleBackColor = True
        '
        'writeLog
        '
        Me.writeLog.AutoSize = True
        Me.writeLog.Location = New System.Drawing.Point(13, 45)
        Me.writeLog.Name = "writeLog"
        Me.writeLog.Size = New System.Drawing.Size(108, 17)
        Me.writeLog.TabIndex = 2
        Me.writeLog.Text = "Log-file Enabled?"
        Me.writeLog.UseVisualStyleBackColor = True
        '
        'GroupBoxInterface
        '
        Me.GroupBoxInterface.Controls.Add(Me.LabelInfo)
        Me.GroupBoxInterface.Controls.Add(Me.logSize)
        Me.GroupBoxInterface.Controls.Add(Me.Label16)
        Me.GroupBoxInterface.Controls.Add(Me.Label1)
        Me.GroupBoxInterface.Controls.Add(Me.logLevel)
        Me.GroupBoxInterface.Controls.Add(Me.writeLog)
        Me.GroupBoxInterface.Location = New System.Drawing.Point(5, 120)
        Me.GroupBoxInterface.Name = "GroupBoxInterface"
        Me.GroupBoxInterface.Size = New System.Drawing.Size(285, 104)
        Me.GroupBoxInterface.TabIndex = 11
        Me.GroupBoxInterface.TabStop = False
        Me.GroupBoxInterface.Text = "Logging, Messages && Separator"
        '
        'LabelInfo
        '
        Me.LabelInfo.AutoSize = True
        Me.LabelInfo.Location = New System.Drawing.Point(154, 22)
        Me.LabelInfo.Name = "LabelInfo"
        Me.LabelInfo.Size = New System.Drawing.Size(39, 13)
        Me.LabelInfo.TabIndex = 12
        Me.LabelInfo.Text = "Lable2"
        '
        'logSize
        '
        Me.logSize.Location = New System.Drawing.Point(128, 71)
        Me.logSize.Name = "logSize"
        Me.logSize.Size = New System.Drawing.Size(36, 20)
        Me.logSize.TabIndex = 3
        '
        'Label16
        '
        Me.Label16.AutoSize = True
        Me.Label16.Location = New System.Drawing.Point(10, 74)
        Me.Label16.Name = "Label16"
        Me.Label16.Size = New System.Drawing.Size(112, 13)
        Me.Label16.TabIndex = 10
        Me.Label16.Text = "Log-file size limit [MiB]:"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(10, 22)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(96, 13)
        Me.Label1.TabIndex = 7
        Me.Label1.Text = "Log-window Level:"
        '
        'logLevel
        '
        Me.logLevel.Location = New System.Drawing.Point(112, 19)
        Me.logLevel.Name = "logLevel"
        Me.logLevel.Size = New System.Drawing.Size(36, 20)
        Me.logLevel.TabIndex = 1
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(6, 22)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(70, 13)
        Me.Label2.TabIndex = 18
        Me.Label2.Text = "List separator"
        '
        'listSep
        '
        Me.listSep.Location = New System.Drawing.Point(103, 19)
        Me.listSep.Name = "listSep"
        Me.listSep.Size = New System.Drawing.Size(36, 20)
        Me.listSep.TabIndex = 17
        '
        'TabControl1
        '
        Me.TabControl1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TabControl1.Controls.Add(Me.TabPage2)
        Me.TabControl1.Location = New System.Drawing.Point(3, 3)
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        Me.TabControl1.Size = New System.Drawing.Size(574, 257)
        Me.TabControl1.TabIndex = 12
        '
        'TabPage2
        '
        Me.TabPage2.Controls.Add(Me.GroupBox2)
        Me.TabPage2.Controls.Add(Me.GroupBoxNotepad)
        Me.TabPage2.Controls.Add(Me.GroupBoxWorDir)
        Me.TabPage2.Controls.Add(Me.GroupBox1)
        Me.TabPage2.Controls.Add(Me.GroupBoxInterface)
        Me.TabPage2.Location = New System.Drawing.Point(4, 22)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2.Size = New System.Drawing.Size(566, 231)
        Me.TabPage2.TabIndex = 0
        Me.TabPage2.Text = "General"
        Me.TabPage2.UseVisualStyleBackColor = True
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.Label2)
        Me.GroupBox2.Controls.Add(Me.Label4)
        Me.GroupBox2.Controls.Add(Me.listSep)
        Me.GroupBox2.Controls.Add(Me.decSep)
        Me.GroupBox2.Location = New System.Drawing.Point(296, 120)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(145, 71)
        Me.GroupBox2.TabIndex = 21
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "File settings"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(6, 49)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(92, 13)
        Me.Label4.TabIndex = 20
        Me.Label4.Text = "Decimal separator"
        '
        'decSep
        '
        Me.decSep.Location = New System.Drawing.Point(103, 45)
        Me.decSep.Name = "decSep"
        Me.decSep.Size = New System.Drawing.Size(36, 20)
        Me.decSep.TabIndex = 19
        '
        'GroupBoxNotepad
        '
        Me.GroupBoxNotepad.Controls.Add(Me.ButtonSelectNotepad)
        Me.GroupBoxNotepad.Controls.Add(Me.editor)
        Me.GroupBoxNotepad.Location = New System.Drawing.Point(5, 63)
        Me.GroupBoxNotepad.Name = "GroupBoxNotepad"
        Me.GroupBoxNotepad.Size = New System.Drawing.Size(558, 51)
        Me.GroupBoxNotepad.TabIndex = 3
        Me.GroupBoxNotepad.TabStop = False
        Me.GroupBoxNotepad.Text = "Editor"
        '
        'ButtonSelectNotepad
        '
        Me.ButtonSelectNotepad.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ButtonSelectNotepad.Location = New System.Drawing.Point(526, 18)
        Me.ButtonSelectNotepad.Name = "ButtonSelectNotepad"
        Me.ButtonSelectNotepad.Size = New System.Drawing.Size(26, 20)
        Me.ButtonSelectNotepad.TabIndex = 2
        Me.ButtonSelectNotepad.Text = "..."
        Me.ButtonSelectNotepad.UseVisualStyleBackColor = True
        '
        'editor
        '
        Me.editor.Location = New System.Drawing.Point(6, 19)
        Me.editor.Name = "editor"
        Me.editor.Size = New System.Drawing.Size(514, 20)
        Me.editor.TabIndex = 1
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.hideUsername)
        Me.GroupBox1.Controls.Add(Me.strictBodies)
        Me.GroupBox1.Controls.Add(Me.includeSchemas)
        Me.GroupBox1.Controls.Add(Me.TextBox1)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.CheckBox1)
        Me.GroupBox1.Location = New System.Drawing.Point(447, 120)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(116, 71)
        Me.GroupBox1.TabIndex = 11
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "JSON"
        '
        'hideUsername
        '
        Me.hideUsername.AutoSize = True
        Me.hideUsername.Location = New System.Drawing.Point(6, 48)
        Me.hideUsername.Name = "hideUsername"
        Me.hideUsername.Size = New System.Drawing.Size(94, 17)
        Me.hideUsername.TabIndex = 12
        Me.hideUsername.Text = "hideUsername"
        Me.hideUsername.UseVisualStyleBackColor = True
        '
        'strictBodies
        '
        Me.strictBodies.AutoSize = True
        Me.strictBodies.Location = New System.Drawing.Point(6, 31)
        Me.strictBodies.Name = "strictBodies"
        Me.strictBodies.Size = New System.Drawing.Size(91, 17)
        Me.strictBodies.TabIndex = 12
        Me.strictBodies.Text = "Strict Bodies?"
        Me.strictBodies.UseVisualStyleBackColor = True
        '
        'includeSchemas
        '
        Me.includeSchemas.AutoSize = True
        Me.includeSchemas.Location = New System.Drawing.Point(6, 15)
        Me.includeSchemas.Name = "includeSchemas"
        Me.includeSchemas.Size = New System.Drawing.Size(114, 17)
        Me.includeSchemas.TabIndex = 12
        Me.includeSchemas.Text = "Include Schemas?"
        Me.includeSchemas.UseVisualStyleBackColor = True
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(440, 38)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(36, 20)
        Me.TextBox1.TabIndex = 11
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(356, 41)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(78, 13)
        Me.Label3.TabIndex = 10
        Me.Label3.Text = "Size Limit [MiB]"
        '
        'CheckBox1
        '
        Me.CheckBox1.AutoSize = True
        Me.CheckBox1.Location = New System.Drawing.Point(353, 15)
        Me.CheckBox1.Name = "CheckBox1"
        Me.CheckBox1.Size = New System.Drawing.Size(121, 17)
        Me.CheckBox1.TabIndex = 5
        Me.CheckBox1.Text = "Write log file (log.txt)"
        Me.CheckBox1.UseVisualStyleBackColor = True
        '
        'ButtonReload
        '
        Me.ButtonReload.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonReload.Location = New System.Drawing.Point(3, 266)
        Me.ButtonReload.Name = "ButtonReload"
        Me.ButtonReload.Size = New System.Drawing.Size(75, 23)
        Me.ButtonReload.TabIndex = 0
        Me.ButtonReload.Text = "Reload"
        Me.ButtonReload.UseVisualStyleBackColor = True
        '
        'ButtonSave
        '
        Me.ButtonSave.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonSave.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.ButtonSave.Location = New System.Drawing.Point(84, 266)
        Me.ButtonSave.Name = "ButtonSave"
        Me.ButtonSave.Size = New System.Drawing.Size(75, 23)
        Me.ButtonSave.TabIndex = 13
        Me.ButtonSave.Text = "Save"
        Me.ButtonSave.UseVisualStyleBackColor = True
        '
        'F_Preferences
        '
        Me.AcceptButton = Me.ButtonOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.ButtonCancel
        Me.ClientSize = New System.Drawing.Size(581, 301)
        Me.Controls.Add(Me.ButtonSave)
        Me.Controls.Add(Me.TabControl1)
        Me.Controls.Add(Me.ButtonCancel)
        Me.Controls.Add(Me.ButtonReload)
        Me.Controls.Add(Me.ButtonOK)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(521, 294)
        Me.Name = "F_Preferences"
        Me.Text = "Preferences"
        Me.GroupBoxWorDir.ResumeLayout(False)
        Me.GroupBoxWorDir.PerformLayout()
        Me.GroupBoxInterface.ResumeLayout(False)
        Me.GroupBoxInterface.PerformLayout()
        Me.TabControl1.ResumeLayout(False)
        Me.TabPage2.ResumeLayout(False)
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBoxNotepad.ResumeLayout(False)
        Me.GroupBoxNotepad.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents workingDir As System.Windows.Forms.TextBox
    Friend WithEvents ButtonSelectWorDir As System.Windows.Forms.Button
    Friend WithEvents GroupBoxWorDir As System.Windows.Forms.GroupBox
    Friend WithEvents ButtonOK As System.Windows.Forms.Button
    Friend WithEvents ButtonCancel As System.Windows.Forms.Button
    Friend WithEvents writeLog As System.Windows.Forms.CheckBox
    Friend WithEvents GroupBoxInterface As System.Windows.Forms.GroupBox
    Friend WithEvents TabControl1 As System.Windows.Forms.TabControl
    Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents logLevel As System.Windows.Forms.TextBox
    Friend WithEvents logSize As System.Windows.Forms.TextBox
    Friend WithEvents Label16 As System.Windows.Forms.Label
    Friend WithEvents GroupBoxNotepad As System.Windows.Forms.GroupBox
    Friend WithEvents ButtonSelectNotepad As System.Windows.Forms.Button
    Friend WithEvents editor As System.Windows.Forms.TextBox
    Friend WithEvents LabelInfo As System.Windows.Forms.Label
    Friend WithEvents ButtonReload As System.Windows.Forms.Button
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents CheckBox1 As System.Windows.Forms.CheckBox
    Friend WithEvents strictBodies As System.Windows.Forms.CheckBox
    Friend WithEvents includeSchemas As System.Windows.Forms.CheckBox
    Friend WithEvents hideUsername As System.Windows.Forms.CheckBox
    Friend WithEvents ButtonSave As System.Windows.Forms.Button
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents listSep As System.Windows.Forms.TextBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents decSep As System.Windows.Forms.TextBox
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
End Class
