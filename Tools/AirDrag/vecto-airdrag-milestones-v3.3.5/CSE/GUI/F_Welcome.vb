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
Imports System.Windows.Forms

''' <summary>
''' Welcome screen. Shows only on the first time application start
''' </summary>
''' <remarks></remarks>
Public Class F_Welcome

    'Close
    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    'Init
    Private Sub F_Welcome_Load(sender As Object, e As System.EventArgs) Handles Me.Load
        Me.Text = "VECTO " & AppVers
    End Sub

    'Open Release Notes
    Private Sub B_Release_Click(sender As System.Object, e As System.EventArgs) Handles B_Release.Click
        Dim release_fname As String = (From fi As IO.FileInfo In (New IO.DirectoryInfo(joinPaths(MyPath, "Docs")).GetFiles("*VECTO_AirDrag_ReleaseNotes*.pdf", IO.SearchOption.TopDirectoryOnly)) Order By fi.LastAccessTime Select fi)(0).FullName
        Try
            System.Diagnostics.Process.Start(release_fname)
        Catch ex As Exception
            logme(8, False, format("Failed opening release notes({0}) due to: {1}", release_fname, ex.Message), ex)
        End Try
    End Sub

    'Open Quick Start Guide
    Private Sub B_UserM_Click(sender As System.Object, e As System.EventArgs) Handles B_UserM.Click
        Dim manual_fname As String = (From fi As IO.FileInfo In (New IO.DirectoryInfo(joinPaths(MyPath, "Docs")).GetFiles("*VECTO_AirDrag_UserManual*.pdf", IO.SearchOption.TopDirectoryOnly)) Order By fi.LastAccessTime Select fi)(0).FullName
        Try
            System.Diagnostics.Process.Start(manual_fname)
        Catch ex As Exception
            logme(8, False, format("Failed opening User Manual({0}) due to: {1}", manual_fname, ex.Message), ex)
        End Try
    End Sub
End Class
