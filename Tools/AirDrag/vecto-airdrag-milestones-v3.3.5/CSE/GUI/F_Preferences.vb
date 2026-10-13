' Copyright 2014 European Union.
' Licensed under the EUPL (the 'Licence');
'
' * You may not use this work except in compliance with the Licence.
' * You may obtain a copy of the Licence at: http://ec.europa.eu/idabc/eupl
' * Unless required by applicable law or agreed to in writing,
'   software distributed under the Licence is distributed on an "AS IS" basis,
'   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'
' See the LICENSE.txt for the specific language governing permissions and limitations.

Imports Newtonsoft.Json.Linq

Public Class F_Preferences

    Private Sub FormLoadHandler(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim controlPairs As IList(Of Control()) = New List(Of Control())
        ''                CONTROL        LABEL
        controlPairs.Add({Me.workingDir, Me.GroupBoxWorDir})
        controlPairs.Add({Me.editor, Me.GroupBoxNotepad})
        controlPairs.Add({Me.writeLog, Nothing})
        controlPairs.Add({Me.logLevel, Label1})
        controlPairs.Add({Me.logSize, Label16})
        controlPairs.Add({Me.listSep, Label2})
        controlPairs.Add({Me.decSep, Label4})
        controlPairs.Add({Me.includeSchemas, Nothing})
        controlPairs.Add({Me.strictBodies, Nothing})
        controlPairs.Add({Me.hideUsername, Nothing})

        '' Add help-tooltips from Json-Schema and 
        '' dirty-check them.
        ''
        Dim schema = JObject.Parse(cPreferences.JSchemaStr)
        For Each row In controlPairs
            Dim ctrl = row(0)
            Dim Label = row(1)
            updateControlsFromSchema(schema, ctrl, Label)
            If TypeOf ctrl Is CheckBox Then
                AddHandler DirectCast(ctrl, CheckBox).CheckedChanged, AddressOf DirtyHandler
            Else
                AddHandler ctrl.TextChanged, AddressOf DirtyHandler
            End If
        Next

        UI_PopulateFrom(Prefs)
    End Sub

    Private Sub FormClosingHandler(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Me.Dirty Then
            Dim res = MsgBox(format("Preferences have changed.\nSave changes?"), MsgBoxStyle.YesNoCancel, "Save Preferences?")
            Select Case res
                Case MsgBoxResult.No
                Case MsgBoxResult.Yes
                    Try
                        StorePrefs()
                    Catch ex As Exception
                        e.Cancel = True
                        logme(9, True, format("Failed storing Preferences({0}) due to: {1} \n  Preferences left unmodified!", _
                                                    PrefsPath, ex.Message), ex)
                    End Try
                Case Else
                    e.Cancel = True
            End Select
        End If
    End Sub

    Private _Dirty
    Private Property Dirty As Boolean
        Get
            Return _Dirty
        End Get
        Set(ByVal value As Boolean)
            If _Dirty Xor value Then
                Me.Text = "Preferences" & IIf(value, "*", "")
            End If
            _Dirty = value
        End Set
    End Property


    Private Sub DirtyHandler(ByVal sender As Object, ByVal e As System.EventArgs)
        Dirty = True
    End Sub

    Private Sub UI_PopulateFrom(ByVal value As cPreferences)
        ' Allocate the data from the confic file (Only by the start)
        Me.workingDir.Text = value.workingDir
        Me.editor.Text = value.editor
        Me.writeLog.Checked = value.writeLog
        Me.logLevel.Text = value.logLevel
        Me.logSize.Text = value.logSize
        Me.listSep.Text = value.listSep
        Me.decSep.Text = value.decSep
        Me.includeSchemas.Checked = value.includeSchemas
        Me.strictBodies.Checked = value.strictBodies
        Me.hideUsername.Checked = value.hideUsername

        Me.Dirty = False
    End Sub

    Private Sub UI_PopulateTo(ByVal value As cPreferences)
        value.workingDir = Me.workingDir.Text
        value.editor = Me.editor.Text
        value.writeLog = Me.writeLog.Checked
        value.logLevel = Me.logLevel.Text
        value.logSize = Me.logSize.Text
        value.listSep = Me.listSep.Text
        value.decSep = Me.decSep.Text
        value.includeSchemas = Me.includeSchemas.Checked
        value.strictBodies = Me.strictBodies.Checked
        value.hideUsername = Me.hideUsername.Checked
    End Sub

    ' Open the filebrowser for selecting the working dir
    Private Sub ButtonWorDir_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonSelectWorDir.Click
        If fbWorkDir.OpenDialog(Me.workingDir.Text) Then
            Me.workingDir.Text = fbWorkDir.Files(0)
        End If
    End Sub

    Private Sub SaveHandler(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonOK.Click, ButtonSave.Click
        Try
            ' Look if List and Dec seperator are the same
            If Me.decSep.Text = Me.listSep.Text Then
                logme(8, True, format("List and decimal separator are the same ({0})! \n  Both are set to default", Me.decSep.Text))
                Me.decSep.Text = Prefs.PropDefault("decSep")
                Me.listSep.Text = Prefs.PropDefault("listSep")
            End If

            '' OK-btn save when dirty, always closes-form.
            '' Save-btn: always saves, burt not closes-form.
            If sender IsNot ButtonOK OrElse Me.Dirty Then
                StorePrefs()
            End If

            If sender Is ButtonOK Then Me.Close()
        Catch ex As Exception
            logme(9, True, format("Failed storing Preferences({0}) due to: {1} \n  Preferences left unmodified!", _
                                        PrefsPath, ex.Message), ex)
        End Try

    End Sub
    Private Sub StorePrefs()
        Dim newPrefs As cPreferences = Prefs.Clone()
        UI_PopulateTo(newPrefs)

        ' Write the config file
        newPrefs.Store(PrefsPath, newPrefs)
        Prefs = newPrefs           ' Replace active prefs if successful.
        Me.Dirty = False

        ' Message for the restart of VECTO
        logme(7, False, format("Stored Preferences({0}).", PrefsPath))
    End Sub

    ' Ok button
    Private Sub ReloadPrefs(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonReload.Click
        Try
            Prefs = New cPreferences(PrefsPath)
            UI_PopulateFrom(Prefs)
        Catch ex As Exception
            logme(9, True, format("Failed loading Preferences({0}) due to: {1}", PrefsPath, ex.Message), ex)
        End Try
    End Sub

    ' Close button
    Private Sub ButtonCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonCancel.Click
        Me.Close()
    End Sub

    ' Select the Notepad path
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonSelectNotepad.Click
        If fbExe.OpenDialog(Me.workingDir.Text) Then
            Me.editor.Text = fbExe.Files(0)
        End If
    End Sub

    ' Interception from kyepress events in the MSG box
    Private Sub TextBoxMSG_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles logLevel.KeyPress
        Select Case Asc(e.KeyChar)
            Case 48 To 56, 8 ' Numbers from 1 till 8 allowed (ASCII)
            Case Else ' Eliminate all other inputs
                e.Handled = True
        End Select
    End Sub

    ' Set the MSG box to default if it is leave without an input
    Private Sub TextBoxMSG_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles logLevel.Leave
        If Me.logLevel.Text = Nothing Then Me.logLevel.Text = Prefs.PropDefault("logLevel")
    End Sub

    ' Changes in the MSG --> Change the lable
    Private Sub TextBoxMSG_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles logLevel.TextChanged
        Select Case Me.logLevel.Text
            Case 0 To 2 ' Show all
                Me.LabelInfo.Text = "All"
            Case 3 ' No infos with priority 5 (*)
                Me.LabelInfo.Text = "No Infos with priority 5 (+)"
            Case 4 ' No infos with priority 4 (*)
                Me.LabelInfo.Text = "No Infos with priority 4 (~)"
            Case 5 ' No infos with priority 3 (*)
                Me.LabelInfo.Text = "No Infos with priority 3 (*)"
            Case 6 ' No infos with priority 2 (-)
                Me.LabelInfo.Text = "No Infos with priority 2 (-)"
            Case 7 ' No infos
                Me.LabelInfo.Text = "No Infos"
            Case Else ' No warnings / infos
                Me.LabelInfo.Text = "No Warnings / Infos"
        End Select
    End Sub

    ' Changes in the LogSizeBox
    Private Sub TextBoxLogSize_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles logSize.KeyPress, TextBox1.KeyPress
        Select Case Asc(e.KeyChar)
            Case 48 To 57, 8 ' Numbers allowed (ASCII)
            Case Else ' Eliminate all other input data
                e.Handled = True
        End Select
    End Sub

    ' Changes in the lisSepBox
    Private Sub TextBoxlistSep_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles listSep.KeyPress
        Select Case Asc(e.KeyChar)
            Case 44, 46, 58, 59, 8 ' Numbers allowed (ASCII)
            Case Else ' Eliminate all other input data
                e.Handled = True
        End Select
    End Sub

    ' Changes in the lisSepBox
    Private Sub TextBoxdecSep_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles decSep.KeyPress
        Select Case Asc(e.KeyChar)
            Case 44, 46, 58, 59, 8 ' Numbers allowed (ASCII)
            Case Else ' Eliminate all other input data
                e.Handled = True
        End Select
    End Sub

    ' Set the LogSize to default if it is leave without an input
    Private Sub TextBoxLogSize_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles logSize.Leave, TextBox1.Leave
        If Me.logSize.Text = Nothing Then Me.logSize.Text = Prefs.PropDefault("logSize")
    End Sub

    ' Set the listsep to default if it is leave without an input
    Private Sub TextBoxlistSep_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles listSep.Leave
        If Me.listSep.Text = Nothing Then Me.listSep.Text = Prefs.PropDefault("listSep")
    End Sub

    ' Set the decsep to default if it is leave without an input
    Private Sub TextBoxdecSep_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles decSep.Leave
        If Me.decSep.Text = Nothing Then Me.decSep.Text = Prefs.PropDefault("decSep")
    End Sub
End Class

