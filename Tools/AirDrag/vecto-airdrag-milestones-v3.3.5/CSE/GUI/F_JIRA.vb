Imports System.Windows.Forms

Public Class F_JIRA

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

	Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
		Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
		Me.Close()
	End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Dim Jira_fname As String = joinPaths(MyPath, "Docs", "JIRA Quick Start Guide.pdf")
        Try
            System.Diagnostics.Process.Start(Jira_fname)
        Catch ex As Exception
            logme(8, False, format("Failed opening JIRA Quick Start({0}) due to: {1}", Jira_fname, ex.Message), ex)
        End Try
    End Sub

	Private Sub LinkLabel3_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel3.LinkClicked
		Dim bodytext As String

		bodytext = "Please provide the following information:" & "%0A" & _
				   "- Email" & "%0A" & _
				   "- Name, Surname" & "%0A" & _
				   "- Country of workplace" & "%0A" & _
				   "- Position"

		System.Diagnostics.Process.Start("mailto:vecto@jrc.ec.europa.eu?subject=CITnet%20account&body=" & bodytext)

	End Sub

	Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
		System.Diagnostics.Process.Start("https://webgate.ec.europa.eu/CITnet/jira/browse/VECTO")
	End Sub

End Class
