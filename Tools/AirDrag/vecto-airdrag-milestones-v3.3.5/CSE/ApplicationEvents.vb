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

Namespace My
    Partial Friend Class MyApplication

        Private Sub MyApplication_Shutdown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shutdown
            ' Declarations
            Dim PSI As New ProcessStartInfo

            ' Close the open Filebrowser (Save the History)
            fbTXT.Close()
            fbExe.Close()
            fbVECTO.Close()
            fbCSV.Close()
            fbDir.Close()
            fbWorkDir.Close()
            fbVEH.Close()
            fbAMB.Close()
            fbALT.Close()
            fbVEL.Close()
            fbMSC.Close()

            ' Restart VECTO Air Drag (Only by changes on the Confic)
            If RestartN Then
                ' Start the *.exe
                PSI.FileName = My.Application.Info.AssemblyName & ".exe"
                Process.Start(PSI)
            End If
        End Sub

        Private Sub MyApplication_Startup(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.StartupEventArgs) Handles Me.Startup
            Try
                ' Declaration
                Dim fiAss As New IO.FileInfo(joinPaths(Application.Info.DirectoryPath, Application.Info.AssemblyName & ".exe"))

                Prefs = New cPreferences(True) ' !!!Skip schema-validation here, or else app hangs as zombie! (do it instead when creating new for Dialog)
                Job = New cJob(True)  ' !!!Skip schema
                VECTOconf = New cVECTOconfig(True)
                Crt = Job.Criteria

                ' Path to the *.exe 
                MyPath = My.Application.Info.DirectoryPath & "\"
                PrefsPath = joinPaths(MyPath, "config", "preferences.json")
                VECTOconfigPath = joinPaths(MyPath, "config", "VECTOconfig.json")

                'Separator!
                If System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator <> "." Or Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName <> "en" Then
                    Try
                        System.Threading.Thread.CurrentThread.CurrentCulture = New System.Globalization.CultureInfo("en-US")
                        System.Threading.Thread.CurrentThread.CurrentUICulture = New System.Globalization.CultureInfo("en-US")
                    Catch ex As Exception
                        logme(9, False, "Failed to set Application Regional Settings to 'en-US'! Check system decimal- and group- separators!")
                    End Try
                End If

                ' Generateion of folder for the file history if not exists
                FB_FilHisDir = joinPaths(MyPath, "config", "fileHistory\")
                If Not IO.Directory.Exists(FB_FilHisDir) Then IO.Directory.CreateDirectory(FB_FilHisDir)

                ' compile date
                AppDate = fiAss.LastWriteTime.Date

                ' Declaration from the filebrowser optionen
                fbVECTO = New cFileBrowser("AirDrag")
                fbVECTO.Extensions = New String() {"csjob.json", "csjob"}

                fbCRT = New cFileBrowser("CRT.json")
                fbCRT.Extensions = New String() {"cscrt.json"}

                fbTXT = New cFileBrowser("TXT")
                fbTXT.Extensions = New String() {"txt"}

                fbExe = New cFileBrowser("EXE")
                fbExe.Extensions = New String() {"exe"}

                fbCSV = New cFileBrowser("CSV")
                fbCSV.Extensions = New String() {"csv", "txt"}

                fbDir = New cFileBrowser("DIR", True)

                fbWorkDir = New cFileBrowser("DIR", True)

                fbVEH = New cFileBrowser("VEH.json")
                fbVEH.Extensions = New String() {"csveh.json"}

                fbAMB = New cFileBrowser("AMB")
                fbAMB.Extensions = New String() {"csamb"}

                fbALT = New cFileBrowser("ALT")
                fbALT.Extensions = New String() {"csalt"}

                fbVEL = New cFileBrowser("VEL")
                fbVEL.Extensions = New String() {"csdat"}

                fbMSC = New cFileBrowser("MSC")
                fbMSC.Extensions = New String() {"csms"}

                ' Initialise the key array
                sKey = New csKey

                'Dim currentDomain As AppDomain = AppDomain.CurrentDomain
                'AddHandler currentDomain.UnhandledException, AddressOf Me.MyApplicationDomain_UnhandledException
            Catch ex As Exception
                MsgBox(format("{0} failed on init due to: \n\i{1}", AppName, ex), MsgBoxStyle.Critical, format("{0} failed to Start!", AppName))
            End Try

        End Sub

        Private Sub MyApplication_UnhandledException(ByVal sender As Object, ByVal ev As Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs) Handles Me.UnhandledException
            Dim ex As Exception = ev.Exception
            If AppFormStarted Then
                logme(9, False, format("Unhandled exception in {0}: \i{1}", ex.Source, ex.Message), ex)
                ev.ExitApplication = False
            Else
                MsgBox(format("{0} failed after init due to: \n\i{1}", AppName, ex), MsgBoxStyle.Critical, format("{0} failed to Start!", AppName))
            End If
        End Sub
    End Class

End Namespace

