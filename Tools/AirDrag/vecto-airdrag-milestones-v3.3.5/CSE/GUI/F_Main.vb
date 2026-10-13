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

Imports System.IO
Imports Newtonsoft.Json.Linq

Public Class F_Main
    ' Declarations
    Private ToolstripSave As Boolean = False
    Private firstrun As Boolean = False
    Private Formname As String = "Job configurations"
    Private SelMode As Integer = 0
    Public Use2ndAmdm As Boolean = False

    ' Load the GUI
    Private Sub FormLoadHandler(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Me.Load
        ' Declarations
        Dim configL As Boolean = True
        Dim NoLegFile As Boolean = False

        ' Initialisation
        Crt.hz_out = 1

        PBInfoIconCrt.Visible = False
        TBInfoCrt.Visible = False
        TBInfoCrt.BackColor = System.Drawing.Color.LightYellow
        setupInfoBox()

        ' Connect the Backgroundworker with the GUI
        BWorker = Me.BackgroundWorkerVECTO

        ' Name of the GUI
        Me.Text = AppName & " " & AppVers

        ' Write the beginning in the Log
        fWriteLog(1)
        AppFormStarted = True

        ' Load the config file
        Try
            Prefs = New cPreferences(PrefsPath)
        Catch ex As Exception
            logme(9, False, format( _
                    "Failed loading Preferences({0}) due to: {1}\n\iThis is normal the first time you launch the application.", _
                    PrefsPath, ex.Message), ex)
            configL = False
            firstrun = True
        End Try

        ' Create working dir if not exists.
        If Not IO.Directory.Exists(Prefs.workingDir) Then
            IO.Directory.CreateDirectory(Prefs.workingDir)
        End If

        ' Write a defult config file if failed to read one.
        If Not configL Then
            Try
                Prefs.Store(PrefsPath)
                logme(7, False, format("Stored new Preferences({0}).", PrefsPath))
            Catch ex As Exception
                logme(9, False, format("Failed storing default Preferences({0}) due to: {1}", PrefsPath, ex.Message), ex)
            End Try
        End If

        ' Load the default settings into criteria tab
        UI_PopulateFromCriteria()

        ' Set the mode to standard value
        SelMode = Job.mode
        ModeSwitch(True)

        ' Write the settings in the Infobox
        logme(7, False, format("Actual file settings:\n List separator ({0})\n Decimal separator ({1})\n You can change it under Tools/preferences.", Prefs.listSep, Prefs.decSep))
    End Sub

    ' Show the GUI and start direct if neccessary
    Private Sub F_Main_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        ' Declaration
        Dim fwelcome As F_Welcome

        ' If the start is done with command line then load jobfile and start calculation
        If fGetArgs() Then
            Dim reload As Boolean = True
            doLoadJob(reload)

            Try
                ' Start the calibration
                CalibrationHandler(sender, e)

                ' Wait till Backgroundworker is no longer busy
                Do While BWorker.IsBusy
                    Application.DoEvents()
                Loop

                ' Start the evaluation
                EvaluationHandler(sender, e)

                ' Wait till Backgroundworker is no longer busy
                Do While BWorker.IsBusy
                    Application.DoEvents()
                Loop

                ' Write the VECTO output files
                If SelMode = 1 Then
                    Me.ButtonGenVECTO.Enabled = True
                    F_VECTOInput.ShowDialog()
                End If
            Finally
                ' Close the form after calculation
                Me.Close()
            End Try
        Else
            ' Start the welcome only if not a direct start
            If firstrun Then
                fwelcome = New F_Welcome
                fwelcome.ShowDialog()
            End If
        End If
    End Sub

    Private Sub ClickExitHandler(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItemExit.Click
        Me.Close()
    End Sub

    Private Sub AppExitedHandler(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        ' Write the end into the Log
        fWriteLog(3)
    End Sub

    Private Sub ClearLogsHandler(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItemClearLog.Click
        ListBoxMSG.Items.Clear()
        TabPageMSG.Text = "Messages(0)"
        ListBoxWar.Items.Clear()
        TabPageWar.Text = "Warnings(0)"
        ListBoxErr.Items.Clear()
        TabPageErr.Text = "Errors(0)"
    End Sub


#Region "AsynJob"

    Private Enum OpType
        Calibration
        Evaluation
    End Enum

    ''' <summary>The Datum exchanged in the 3 AsyncWorked methods</summary>
    Private Class cAsyncJob

        Public ReadOnly Operation As OpType

        Sub New(ByVal type As OpType)
            Me.Operation = type
        End Sub

        Public ReadOnly Property IsCalibration As Boolean
            Get
                Return Me.Operation = OpType.Calibration
            End Get
        End Property

        Public ReadOnly Property IsEvaluation As Boolean
            Get
                Return Me.Operation = OpType.Evaluation
            End Get
        End Property
    End Class

    '*********Backgroundworker*********

    ' Backgroundworker for the calculation in the background
    Private Sub BackgroundWorkerVECTO_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) _
        Handles BackgroundWorkerVECTO.DoWork

        Dim asyncJob As cAsyncJob = e.Argument
        ' Pass the async-job datum down the road.
        e.Result = asyncJob

        System.Threading.Thread.CurrentThread.CurrentCulture = New System.Globalization.CultureInfo("en-US")
        System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator = Prefs.decSep

        '##### START THE CALCULATION #####
        '#################################
        calculation(asyncJob.IsCalibration)
        '#################################

        ' Cancel if cancel requested...
        If Me.BackgroundWorkerVECTO.CancellationPending Then e.Cancel = True

    End Sub

    ' Output from messages with the Backgroundworker
    Private Sub BackgroundWorkerVECTO_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) _
        Handles BackgroundWorkerVECTO.ProgressChanged

        Dim workerMsg As cLogMsg = e.UserState
        If workerMsg IsNot Nothing Then
            workerMsg.forwardLog()
        End If
        If uRB Then
            updateResultBoxes()
            uRB = False
        End If
    End Sub

    ' Identify the ending from the backgroundworker
    Private Sub BackgroundWorkerVECTO_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) _
        Handles BackgroundWorkerVECTO.RunWorkerCompleted
        ' If an Error is detected
        If e.Error IsNot Nothing Then
            logme(8, False, format("Background operation ended with exception: {0}", e.Error.Message), e.Error)
        ElseIf e.Cancelled Then
            logme(7, False, "Background operation aborted by user.")
        Else
            If ListBoxErr.Items.Count <> 0 Then
                logme(7, False, "Background operation ended with ERROR.")
            Else
                logme(7, False, "Background operation ended OK.")
                If RELEASE_CANDIDATE Then
                    logme(7, False, "ATTENTION: RELEASE CANDIDATE, Not FOR CERTIFICATION!")
                End If
                If BETA_VERSION Then
                    logme(7, False, "ATTENTION: BETA VERSION, Not FOR CERTIFICATION!")
                End If
            End If

                Dim asyncJob As cAsyncJob = e.Result
            If asyncJob.IsCalibration Then Me.ButtonEval.Enabled = True
            If asyncJob.IsEvaluation And SelMode = 1 Then Me.ButtonGenVECTO.Enabled = True
            doSaveJob(False)
        End If

        FileBlock = False

        '' Re-enable all "Exec" buttons
        CalibrationState = False
        EvaluationState = False

        Me.TextBoxRBetaMis.Text = Math.Round(Job.beta_ame, 2, MidpointRounding.AwayFromZero)
    End Sub


    '####################################################################
    '#### Only HERE manage "Exec" button's state (Text, Image, etc). ####
    '####################################################################
    Private _CalibrationState As Boolean = False
    Private ReadOnly _CalibrationTxts = {"Calibrate", "Cancel"}
    Private ReadOnly _CalibrationImgs = {My.Resources.Resources.Play, My.Resources.Resources.Stop_calc}
    Private Property CalibrationState As Boolean
        Get
            Return _CalibrationState
        End Get
        Set(ByVal value As Boolean)
            If _CalibrationState Xor value Then
                Dim indx = -CInt(value)
                Me.ButtonCalC.Text = _CalibrationTxts(indx)
                Me.ButtonCalC.Image = _CalibrationImgs(indx)
                Me.UseWaitCursor = value
            End If
            _CalibrationState = value
        End Set
    End Property

    ' Calculate button calibration test
    Private Sub CalibrationHandler(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonCalC.Click
        ' Generate cancel butten if the backgroundworker is busy
        If BWorker.IsBusy Then
            BWorker.CancelAsync()
            logme(8, False, "Cancel requested for background-operation...")
            Return
        End If

        ' Read the data from the GUI
        UI_PopulatePath()
        UI_PopulateToJob(True)
        UI_PopulateToCriteria()

        Me.TextBoxRVeh.Text = 0
        Me.TextBoxRAirPos.Text = 0
        Me.TextBoxRBetaMis.Text = 0

        Dim ok = False
        Try
            ' Change the button "Exec" --> "Cancel" 
            Me.CalibrationState = True

            ' Reset Values
            Job.ResetValue(True)

            ' Save the Jobfiles
            If Not doSaveJob(False) Then
                logme(9, False, format("No Jobfile name given!"))
                Me.CalibrationState = False
                Exit Sub
            End If

            ' Check if outfolder exist. If not then generate the folder
            If Not System.IO.Directory.Exists(OutFolder) Then
                If OutFolder <> Nothing Then
                    ' Generate the folder if it is desired
                    logme(7, False, format("Output-folder({0}) doesn´t exist and is created!", OutFolder))
                    IO.Directory.CreateDirectory(OutFolder)
                Else
                    logme(9, False, "No outputfolder is given!")
                    Me.CalibrationState = False
                    Exit Sub
                End If
            End If

            ' Clear the MSG on the GUI
            Me.ListBoxMSG.Items.Clear()
            fClear_VECTO_Form(False, False)

            logme(7, False, format("Starting MISALIGNMENT calculation: \n\i* Job: {0}\n* Out: {1}", JobFile, OutFolder))

            ' Start the calculation in the backgroundworker
            Dim jobType = OpType.Calibration
            Me.BackgroundWorkerVECTO.RunWorkerAsync(New cAsyncJob(jobType))

            ok = True
        Finally
            '' Re-enable "Exec" button on failures.
            If Not ok Then CalibrationState = False
        End Try
    End Sub

    '####################################################################
    '#### Only HERE manage "Exec" button's state (Text, Image, etc). ####
    '####################################################################
    Private _EvaluationState As Boolean = False
    Private ReadOnly _EvaluationTxts = {"Evaluate", "Cancel"}
    Private ReadOnly _EvaluationImgs = {My.Resources.Resources.Play, My.Resources.Resources.Stop_calc}
    Private Property EvaluationState As Boolean
        Get
            Return _EvaluationState
        End Get
        Set(ByVal value As Boolean)
            If _EvaluationState Xor value Then
                Dim indx = -CInt(value)
                Me.ButtonEval.Text = _EvaluationTxts(indx)
                Me.ButtonEval.Image = _EvaluationImgs(indx)
                Me.UseWaitCursor = value
            End If
            _EvaluationState = value
        End Set
    End Property

    ' Evaluate button test run
    Private Sub EvaluationHandler(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonEval.Click
        ' Generate cancel butten if the backgroundworker is busy
        If BWorker.IsBusy Then
            BWorker.CancelAsync()
            logme(8, False, "Cancel requested for background-operation...")
            Return
        End If

        ' Read the data from the GUI
        UI_PopulateToJob(True)
        UI_PopulateToCriteria()

        Me.TextBoxRVeh.Text = 0
        Me.TextBoxRAirPos.Text = 0

        Dim ok = False
        Try
            ' Change the button "Exec" --> "Cancel" 
            Me.EvaluationState = True

            fWriteLog(2, 4, "----- Speed runs ")

            ' Reset Values
            Job.ResetValue()

            ' Save the Jobfiles
            If Not doSaveJob(False) Then
                logme(9, False, format("No Jobfile name given!"))
                Me.EvaluationState = False
                Exit Sub
            End If

            ' Check if outfolder exist. If not then generate the folder
            If Not System.IO.Directory.Exists(OutFolder) Then
                If OutFolder <> Nothing Then
                    ' Generate the folder if it is desired
                    logme(7, False, format("Output-folder({0}) doesn´t exist and is created!", OutFolder))
                    IO.Directory.CreateDirectory(OutFolder)
                Else
                    logme(9, False, "No outputfolder is given!")
                    Me.EvaluationState = False
                    Exit Sub
                End If
            End If

            ' Clear the MSG on the GUI
            fClear_VECTO_Form(False, False)

            ' Write the Calculation status in the Messageoutput and in the Log
            logme(7, False, format("Starting EVALUATION: \n\i* Job: {0}\n* Out: {1}", JobFile, OutFolder))

            ' Start the calculation in the backgroundworker
            Dim jobType = OpType.Evaluation
            Me.BackgroundWorkerVECTO.RunWorkerAsync(New cAsyncJob(jobType))

            ok = True
        Finally
            '' Re-enable "Exec" button on failures.
            If Not ok Then EvaluationState = False
        End Try
    End Sub

#End Region ' AsynJob


#Region "Main tab"

#Region "Job IO"
    ' Menu New
    Private Sub NewJobHandler(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItemNewJob.Click
        fClear_VECTO_Form(True)
    End Sub

    Private Sub ReloadJobHandler(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItemReloadJob.Click
        Dim reload = True
        doLoadJob(reload)
    End Sub
    Private Sub LoadJobHandler(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItemLoadJob.Click
        Dim reload = False
        doLoadJob(reload)
    End Sub
    Private Sub doLoadJob(ByVal isReload As Boolean)
        Dim jobFileToLoad As String = Nothing

        If isReload Then
            jobFileToLoad = JobFile
        Else
            ' Open the filebrowser with the *.csjob parameter
            If fbVECTO.OpenDialog(Prefs.workingDir, False) Then

                jobFileToLoad = fbVECTO.Files(0)
                If (jobFileToLoad <> Nothing) Then
                    ' Clear the GUI
                    fClear_VECTO_Form(False)
                    OutFolder = joinPaths(fPath(jobFileToLoad), "Results\")
                End If
            End If
        End If

        If jobFileToLoad Is Nothing Then Return

        '' Read Jobfile according to its version and 
        ''  populate GUI.
        ''
        Try
            Dim newJob As cJob
            If jobFileToLoad.EndsWith(".csjob.json") Then
                newJob = New cJob(jobFileToLoad)
            Else
                newJob = New cJob(True)
                logme(9, False, format("The jobfile suffix is not known ({0}). Please use the correct format", JobFile))
            End If
            newJob.Validate()

            JobFile = jobFileToLoad
            installJob(newJob)
            UI_PopulateFromJob()
            UI_PopulateFromCriteria()
            ModeSwitch(True)
        Catch ex As Exception
            logme(9, False, format("Failed reading Job-file({0}) due to: {1}", JobFile, ex.Message), ex)
        End Try
    End Sub

    Private Sub SaveJobHandler(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItemSaveJob.Click
        Dim saveAs = False
        doSaveJob(saveAs)
    End Sub
    Private Sub SaveJobAsHandler(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MenuItemSaveAsJob.Click
        Dim saveAs = True
        doSaveJob(saveAs)
    End Sub

    Private Function doSaveJob(ByVal isSaveAs As Boolean) As Boolean
        ' Control the texboxes
        UI_PopulatePath()

        ' Identify if the file should save under a new name
        If JobFile = Nothing Or isSaveAs Then
            ' Open the filebrowser to select the folder and name of the Jobfile
            If fbVECTO.SaveDialog(JobFile) Then
                JobFile = fbVECTO.Files(0)
                OutFolder = joinPaths(fPath(JobFile), "Results\")
                Me.Text = Formname & " " & JobFile
            End If
            If (JobFile = Nothing) Then
                Return False
            End If
        End If

        ' Get all data from the GUI
        UI_PopulateToJob()
        UI_PopulateToCriteria()

        ' Write the file
        If Not JobFile.EndsWith(".csjob.json", StringComparison.OrdinalIgnoreCase) Then
            JobFile = joinPaths(fPath(JobFile), fName(JobFile, False) & ".csjob.json")
        End If
        Job.Store(JobFile)

        Return True
    End Function


#Region "UI populate"

    ' Function to get all parameter from the GUI
    Function UI_PopulateToJob(Optional ByVal validate As Boolean = False) As Boolean
        ' Get the mode
        Job.mode = SelMode
        Job.second_amdm_params = cbUse2ndAmdm.Checked

        ' Read the data from the textboxes (General)
        Job.vehicle_fpath = TextBoxVeh1.Text
        Job.ambient_fpath = TextBoxWeather.Text

        ' Appropriate the inputfiles from calibration run
        Job.calib_run_fpath = TextBoxDataC.Text
        Job.calib_track_fpath = TextBoxMSCC.Text

        ' Appropriate the inputfiles from test run
        Job.low1_fpath = TextBoxDataLS1.Text
        Job.high_fpath = TextBoxDataHS.Text
        Job.low2_fpath = TextBoxDataLS2.Text
        Job.coast_track_fpath = TextBoxMSCT.Text

        If validate Then
            Job.Validate()
        End If

        Return True
    End Function

    ' Get the parameters from option tab
    Sub UI_PopulateToCriteria()
        ' Evaluation box
        Crt.accel_correction = CB_accel_correction.Checked
        Crt.gradient_correction = CB_gradient_correction.Checked

        ' Output box
        If RB1Hz.Checked Then Crt.hz_out = 1
        If RB100Hz.Checked Then Crt.hz_out = 100

        ' Anemometer correction
        Crt.delta_CdxA_anemo = TB_delta_CdxA_anemo.Text

        ' Altitude
        Crt.dist_gridpoints_max = TB_dist_gridpoints_max.Text
        Crt.dist_grid_ms_max = TB_dist_grid_ms_max.Text
        Crt.slope_max = TB_slope_max.Text

        'Parameter boxes
        ' General valid criteria
        Crt.delta_rr_max = TB_delta_rr_max.Text
        Crt.t_ground_max = TB_t_ground_max.Text
        Crt.t_amb_max = TB_t_amb_max.Text
        Crt.t_amb_min = TB_t_amb_min.Text
        ' General
        Crt.delta_Hz_max = TB_delta_Hz_max.Text
        Crt.acc_corr_avg = TB_acc_corr_avg.Text
        Crt.rr_corr_factor = TB_rr_corr_factor.Text
        Crt.delta_parallel_max = TB_delta_parallel_max.Text
        ' Identification of measurement section
        Crt.trigger_delta_x_max = TB_trigger_delta_x_max.Text
        Crt.trigger_delta_y_max = TB_trigger_delta_y_max.Text
        Crt.delta_head_max = TB_delta_head_max.Text
        Crt.length_MS_max = TB_length_MS_max.Text
        Crt.length_MS_min = TB_length_MS_min.Text
        ' Requirements on number of valid datasets
        Crt.segruns_min_CAL = TB_segruns_min_CAL.Text
        Crt.segruns_min_LS = TB_segruns_min_LS.Text
        Crt.segruns_min_HS = TB_segruns_min_HS.Text
        Crt.segruns_min_head_HS = TB_segruns_min_head_HS.Text
        ' DataSet validity criteria
        Crt.dist_float = TB_dist_float.Text
        ' Calibration
        Crt.v_wind_avg_max_CAL = TB_v_wind_avg_max_CAL.Text
        Crt.v_wind_1s_max_CAL = TB_v_wind_1s_max_CAL.Text
        Crt.beta_avg_max_CAL = TB_beta_avg_max_CAL.Text
        Crt.v_veh_1s_delta_CAL = TB_v_veh_1s_delta_CAL.Text
        Crt.v_veh_ave_delta_CAL = TB_v_veh_ave_delta_CAL.Text
        ' Low and high speed test
        Crt.leng_crit = TB_leng_crit.Text
        ' Low speed test
        Crt.v_veh_avg_max_LS = TB_v_veh_avg_max_LS.Text
        Crt.v_veh_avg_min_LS = TB_v_veh_avg_min_LS.Text
        Crt.v_veh_float_delta_LS = TB_v_veh_float_delta_LS.Text
        Crt.tq_sum_float_delta_LS = TB_tq_sum_float_delta_LS.Text
        Crt.delta_n_ec_LS = TB_delta_n_ec_LS.Text
        ' High speed test
        Crt.v_wind_avg_max_HS = TB_v_wind_avg_max_HS.Text
        Crt.v_wind_1s_max_HS = TB_v_wind_1s_max_HS.Text
        Crt.v_veh_avg_min_HS = TB_v_veh_avg_min_HS.Text
        Crt.v_veh_avg_max_HS = TB_v_veh_avg_max_HS.Text
        Crt.v_veh_1s_delta_HS = TB_v_veh_1s_delta_HS.Text
        Crt.tq_sum_1s_delta_HS = TB_tq_sum_1s_delta_HS.Text
        Crt.delta_n_ec_HS = TB_delta_n_ec_HS.Text
        Crt.beta_avg_max_HS = TB_beta_avg_max_HS.Text
        Crt.delta_v_avg_min_HS = TB_delta_v_avg_min_HS.Text
    End Sub

    Sub UI_PopulateFromJob()
        ' Transfer the data to the GUI
        SelMode = Job.mode
        cbUse2ndAmdm.Checked = Job.second_amdm_params
        ' General
        TextBoxVeh1.Text = Job.vehicle_fpath
        TextBoxWeather.Text = Job.ambient_fpath
        ' Calibration
        TextBoxMSCC.Text = Job.calib_track_fpath
        TextBoxDataC.Text = Job.calib_run_fpath
        ' Test
        TextBoxMSCT.Text = Job.coast_track_fpath
        TextBoxDataLS1.Text = Job.low1_fpath
        TextBoxDataHS.Text = Job.high_fpath
        TextBoxDataLS2.Text = Job.low2_fpath

        logme(1, False, $"Use 2nd amdm: {TransitionalService.Use2ndAmendment}")

    End Sub

    ' Function to get the parameters from saved values
    Sub UI_PopulateFromCriteria()
        ' Write the Standard values in the textboxes
        ' General valid criteria
        TB_delta_rr_max.Text = Crt.delta_rr_max
        TB_t_ground_max.Text = Crt.t_ground_max
        TB_t_amb_max.Text = Crt.t_amb_max
        TB_t_amb_min.Text = Crt.t_amb_min
        ' General
        TB_delta_Hz_max.Text = Crt.delta_Hz_max
        TB_acc_corr_avg.Text = Crt.acc_corr_avg
        TB_rr_corr_factor.Text = Crt.rr_corr_factor
        TB_delta_parallel_max.Text = Crt.delta_parallel_max
        ' Identification of measurement section
        TB_trigger_delta_x_max.Text = Crt.trigger_delta_x_max
        TB_trigger_delta_y_max.Text = Crt.trigger_delta_y_max
        TB_delta_head_max.Text = Crt.delta_head_max
        TB_length_MS_max.Text = Crt.length_MS_max
        TB_length_MS_min.Text = Crt.length_MS_min
        ' Requirements on number of valid datasets
        TB_segruns_min_CAL.Text = Crt.segruns_min_CAL
        TB_segruns_min_LS.Text = Crt.segruns_min_LS
        TB_segruns_min_HS.Text = Crt.segruns_min_HS
        TB_segruns_min_head_HS.Text = Crt.segruns_min_head_HS
        ' DataSet validity criteria
        TB_dist_float.Text = Crt.dist_float
        ' Calibration
        TB_v_wind_avg_max_CAL.Text = Crt.v_wind_avg_max_CAL
        TB_v_wind_1s_max_CAL.Text = Crt.v_wind_1s_max_CAL
        TB_beta_avg_max_CAL.Text = Crt.beta_avg_max_CAL
        TB_v_veh_1s_delta_CAL.Text = Crt.v_veh_1s_delta_CAL
        TB_v_veh_ave_delta_CAL.Text = Crt.v_veh_ave_delta_CAL
        ' Low and high speed test
        TB_leng_crit.Text = Crt.leng_crit
        ' Low speed test
        TB_v_veh_avg_min_LS.Text = Crt.v_veh_avg_min_LS
        TB_v_veh_avg_max_LS.Text = Crt.v_veh_avg_max_LS
        TB_v_veh_float_delta_LS.Text = Crt.v_veh_float_delta_LS
        TB_tq_sum_float_delta_LS.Text = Crt.tq_sum_float_delta_LS
        TB_delta_n_ec_LS.Text = Crt.delta_n_ec_LS
        ' High speed test
        TB_v_wind_avg_max_HS.Text = Crt.v_wind_avg_max_HS
        TB_v_wind_1s_max_HS.Text = Crt.v_wind_1s_max_HS
        TB_v_veh_avg_min_HS.Text = Crt.v_veh_avg_min_HS
        TB_v_veh_avg_max_HS.Text = Crt.v_veh_avg_max_HS
        TB_v_veh_1s_delta_HS.Text = Crt.v_veh_1s_delta_HS
        TB_tq_sum_1s_delta_HS.Text = Crt.tq_sum_1s_delta_HS
        TB_delta_n_ec_HS.Text = Crt.delta_n_ec_HS
        TB_beta_avg_max_HS.Text = Crt.beta_avg_max_HS
        TB_delta_v_avg_min_HS.Text = Crt.delta_v_avg_min_HS
        ' Evaluation box
        CB_accel_correction.Checked = Crt.accel_correction
        CB_gradient_correction.Checked = Crt.gradient_correction
        ' Altitude
        TB_dist_gridpoints_max.Text = Crt.dist_gridpoints_max
        TB_dist_grid_ms_max.Text = Crt.dist_grid_ms_max
        TB_slope_max.Text = Crt.slope_max
        ' Anemometer correction
        TB_delta_CdxA_anemo.Text = Crt.delta_CdxA_anemo

        ' Output
        If Crt.hz_out = 1 Then
            RB1Hz.Checked = True
        ElseIf Crt.hz_out = 100 Then
            RB100Hz.Checked = True
        End If
    End Sub

    ' Function to set Path to default by calibration if nothing is used by testrun
    Sub UI_PopulatePath()
        ' Control if all testrun pathes have a value
        If TextBoxMSCT.Text = "" Then TextBoxMSCT.Text = MyPath
        If TextBoxDataLS1.Text = "" Then TextBoxDataLS1.Text = MyPath
        If TextBoxDataHS.Text = "" Then TextBoxDataHS.Text = MyPath
        If TextBoxDataLS2.Text = "" Then TextBoxDataLS2.Text = MyPath
    End Sub

    ' Clear the GUI
    Public Function fClear_VECTO_Form(ByVal Komplet As Boolean, Optional ByVal Fields As Boolean = True) As Boolean
        If Komplet Then
            ' Clear the Jobfile and the output folder
            JobFile = Nothing
            OutFolder = Nothing
        End If

        If Fields Then
            ' Clear the Textboxes or set them to default
            TextBoxVeh1.Clear()
            TextBoxWeather.Clear()
            CB_accel_correction.Checked = True
            CB_gradient_correction.Checked = True

            ' Calibration fields
            TextBoxDataC.Clear()
            TextBoxMSCC.Clear()

            ' Test run fields
            TextBoxMSCT.Clear()
            TextBoxDataLS1.Clear()
            TextBoxDataHS.Clear()
            TextBoxDataLS2.Clear()

            ButtonEval.Enabled = False
            ButtonGenVECTO.Enabled = False
            EvaluationState = False

            ' Option parameters to standard
            installJob(New cJob)
            UI_PopulateFromJob()
            UI_PopulateFromCriteria()
            ModeSwitch(True)
        End If

        ' Clear the text in the WarnigBox and ErrorBox and activate the MessageBox
        Me.ListBoxWar.Items.Clear()
        Me.ListBoxErr.Items.Clear()
        Me.TabControlOutMsg.SelectTab(0)
        Me.TabPageErr.Text = "Errors (0)"
        Me.TabPageWar.Text = "Warnings (0)"

        Return True
    End Function

    ' Disable/Enable all criteria
    Sub UI_SetCriteriaStatus(Optional ByVal SetState As Boolean = True)
        ' General valid criteria
        TB_delta_rr_max.Enabled = SetState
        TB_t_ground_max.Enabled = SetState
        TB_t_amb_max.Enabled = SetState
        TB_t_amb_min.Enabled = SetState
        ' General
        TB_delta_Hz_max.Enabled = SetState
        TB_acc_corr_avg.Enabled = SetState
        TB_rr_corr_factor.Enabled = SetState
        TB_delta_parallel_max.Enabled = SetState
        ' Identification of measurement section
        TB_trigger_delta_x_max.Enabled = SetState
        TB_trigger_delta_y_max.Enabled = SetState
        TB_delta_head_max.Enabled = SetState
        TB_length_MS_max.Enabled = SetState
        TB_length_MS_min.Enabled = SetState
        ' Requirements on number of valid datasets
        TB_segruns_min_CAL.Enabled = SetState
        TB_segruns_min_LS.Enabled = SetState
        TB_segruns_min_HS.Enabled = SetState
        TB_segruns_min_head_HS.Enabled = SetState
        ' DataSet validity criteria
        TB_dist_float.Enabled = SetState
        ' Calibration
        TB_v_wind_avg_max_CAL.Enabled = SetState
        TB_v_wind_1s_max_CAL.Enabled = SetState
        TB_beta_avg_max_CAL.Enabled = SetState
        TB_v_veh_1s_delta_CAL.Enabled = SetState
        TB_v_veh_ave_delta_CAL.Enabled = SetState
        ' Low and high speed test
        TB_leng_crit.Enabled = SetState
        ' Low speed test
        TB_v_veh_avg_min_LS.Enabled = SetState
        TB_v_veh_avg_max_LS.Enabled = SetState
        TB_v_veh_float_delta_LS.Enabled = SetState
        TB_tq_sum_float_delta_LS.Enabled = SetState
        TB_delta_n_ec_LS.Enabled = SetState
        ' High speed test
        TB_v_wind_avg_max_HS.Enabled = SetState
        TB_v_wind_1s_max_HS.Enabled = SetState
        TB_v_veh_avg_min_HS.Enabled = SetState
        TB_v_veh_avg_max_HS.Enabled = SetState
        TB_v_veh_1s_delta_HS.Enabled = SetState
        TB_tq_sum_1s_delta_HS.Enabled = SetState
        TB_delta_n_ec_HS.Enabled = SetState
        TB_beta_avg_max_HS.Enabled = SetState
        TB_delta_v_avg_min_HS.Enabled = SetState
        ' Evaluation box
        CB_accel_correction.Enabled = SetState
        ' Altitude
        TB_dist_gridpoints_max.Enabled = SetState
        TB_dist_grid_ms_max.Enabled = SetState
        TB_slope_max.Enabled = SetState
        ' Anemometer correction
        TB_delta_CdxA_anemo.Enabled = SetState

        ' Import Button
        ButtonCrtImport.Enabled = SetState
    End Sub
#End Region ' UI populate

#End Region ' Job IO


#Region "Vehicle"

    ' Open the filebrowser for the selection of the vehiclefile
    Private Sub ButtonSelectVeh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonSelectVeh.Click
        ' Open the filebrowser with the *.csveh parameter
        If fbVEH.OpenDialog(Prefs.workingDir, False) Then
            If (fbVEH.Files(0) <> Nothing) Then
                Me.TextBoxVeh1.Text = fbVEH.Files(0)
            End If
        End If
    End Sub

    ' Open the vehiclefile in the Notepad
    Private Sub ButtonVeh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonVeh.Click
        If IO.File.Exists(Me.TextBoxVeh1.Text) Then
            System.Diagnostics.Process.Start(Prefs.editor, Me.TextBoxVeh1.Text)
        Else
            logme(9, True, "No such Inputfile: " & Me.TextBoxVeh1.Text)
        End If
    End Sub

    ' Open the filebrowser for the selection of the weatherfile
    Private Sub ButtonSelectWeather_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonSelectWeather.Click
        ' Open the filebrowser with the *.cswea parameter
        If fbAMB.OpenDialog(Prefs.workingDir, False) Then
            If (fbAMB.Files(0) <> Nothing) Then
                Me.TextBoxWeather.Text = fbAMB.Files(0)
            End If
        End If
    End Sub

    ' Open the weatherfile in the Notepad
    Private Sub ButtonWeather_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonWeather.Click
        If IO.File.Exists(Me.TextBoxWeather.Text) Then
            System.Diagnostics.Process.Start(Prefs.editor, Me.TextBoxWeather.Text)
        Else
            logme(9, True, "No such Inputfile: " & Me.TextBoxWeather.Text)
        End If
    End Sub

#End Region ' Vehicle


#Region "Calibration"

    ' Open the filebrowser for the selection of the datafile from the calibration run
    Private Sub ButtonSelectDataC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonSelectDataC.Click
        ' Open the filebrowser with the *.csdat parameter
        If fbVEL.OpenDialog(Prefs.workingDir, False) Then
            If (fbVEL.Files(0) <> Nothing) Then
                Me.TextBoxDataC.Text = fbVEL.Files(0)
            End If
        End If
    End Sub

    ' Open the datafile from the calibration test in Excel
    Private Sub ButtonDataC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonDataC.Click
        ' Declarations
        Dim PSI As New ProcessStartInfo

        ' Exit the sub if no value is selected
        If Not Me.TextBoxDataC.Text.Count > 0 Then Exit Sub

        ' Open the File in Excel
        PSI.FileName = "excel"
        PSI.Arguments = ChrW(34) & Me.TextBoxDataC.Text & ChrW(34)
        Process.Start(PSI)
    End Sub

    ' Open the filebrowser for the selection of the measure section config file (MSC)
    Private Sub ButtonSelectMSCC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonSelectMSCC.Click
        ' Open the filebrowser with the *.csmsc parameter
        If fbMSC.OpenDialog(Prefs.workingDir, False) Then
            If (fbMSC.Files(0) <> Nothing) Then
                Me.TextBoxMSCC.Text = fbMSC.Files(0)
            End If
        End If
    End Sub

    ' Open the measure section config file (MSC) in Excel
    Private Sub ButtonMSCC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonMSCC.Click
        ' Declarations
        Dim PSI As New ProcessStartInfo

        ' Exit the sub if no value is selected
        If Not Me.TextBoxMSCC.Text.Count > 0 Then Exit Sub

        ' Open the File in Excel
        PSI.FileName = "excel"
        PSI.Arguments = ChrW(34) & Me.TextBoxMSCC.Text & ChrW(34)
        Process.Start(PSI)
    End Sub

#End Region ' Calibration


#Region "Evaluation"

    ' Open the filebrowser for the selection of the measure section file from the test run
    Private Sub ButtonSelectMSCT_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonSelectMSCT.Click
        ' Open the filebrowser with the *.csmsc parameter
        If fbMSC.OpenDialog(Prefs.workingDir, False) Then
            If (fbMSC.Files(0) <> Nothing) Then
                Me.TextBoxMSCT.Text = fbMSC.Files(0)
            End If
        End If
    End Sub

    ' Open the measure section config file (MSC) in Excel
    Private Sub ButtonMSCT_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonMSCT.Click
        ' Declarations
        Dim PSI As New ProcessStartInfo

        ' Exit the sub if no value is selected
        If Not Me.TextBoxMSCT.Text.Count > 0 Then Exit Sub

        ' Open the File in Excel
        PSI.FileName = "excel"
        PSI.Arguments = ChrW(34) & Me.TextBoxMSCT.Text & ChrW(34)
        Process.Start(PSI)
    End Sub

    ' Open the filebrowser for the selection of the first low speed data file from the test run
    Private Sub ButtonSelectDataLS1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonSelectDataLS1.Click
        ' Open the filebrowser with the *.csdat parameter
        If fbVEL.OpenDialog(Prefs.workingDir, False) Then
            If (fbVEL.Files(0) <> Nothing) Then
                Me.TextBoxDataLS1.Text = fbVEL.Files(0)
            End If
        End If
    End Sub

    ' Open the first low speed data file in Excel
    Private Sub ButtonDataLS1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonDataLS1.Click
        ' Declarations
        Dim PSI As New ProcessStartInfo

        ' Exit the sub if no value is selected
        If Not Me.TextBoxDataLS1.Text.Count > 0 Then Exit Sub

        ' Open the File in Excel
        PSI.FileName = "excel"
        PSI.Arguments = ChrW(34) & Me.TextBoxDataLS1.Text & ChrW(34)
        Process.Start(PSI)
    End Sub

    ' Open the filebrowser for the selection of the high speed data file from the test run
    Private Sub ButtonSelectDataHS_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonSelectDataHS.Click
        ' Open the filebrowser with the *.csdat parameter
        If fbVEL.OpenDialog(Prefs.workingDir, False) Then
            If (fbVEL.Files(0) <> Nothing) Then
                Me.TextBoxDataHS.Text = fbVEL.Files(0)
            End If
        End If
    End Sub

    ' Open the high speed data file in Excel
    Private Sub ButtonDataHS_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonDataHS.Click
        ' Declarations
        Dim PSI As New ProcessStartInfo

        ' Exit the sub if no value is selected
        If Not Me.TextBoxDataHS.Text.Count > 0 Then Exit Sub

        ' Open the File in Excel
        PSI.FileName = "excel"
        PSI.Arguments = ChrW(34) & Me.TextBoxDataHS.Text & ChrW(34)
        Process.Start(PSI)
    End Sub

    ' Open the filebrowser for the selection of the second low speed data file from the test run
    Private Sub ButtonSelectDataLS2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonSelectDataLS2.Click
        ' Open the filebrowser with the *.csdat parameter
        If fbVEL.OpenDialog(Prefs.workingDir, False) Then
            If (fbVEL.Files(0) <> Nothing) Then
                Me.TextBoxDataLS2.Text = fbVEL.Files(0)
            End If
        End If
    End Sub

    ' Open the second low speed data file in Excel
    Private Sub ButtonDataLS2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonDataLS2.Click
        ' Declarations
        Dim PSI As New ProcessStartInfo

        ' Exit the sub if no value is selected
        If Not Me.TextBoxDataLS2.Text.Count > 0 Then Exit Sub

        ' Open the File in Excel
        PSI.FileName = "excel"
        PSI.Arguments = ChrW(34) & Me.TextBoxDataLS2.Text & ChrW(34)
        Process.Start(PSI)
    End Sub

#End Region ' Evaluation


#Region "VECTO Input"
    ' Button to generate VECTO input 
    Private Sub ButtonGenVECTO_Click(sender As Object, e As EventArgs) Handles ButtonGenVECTO.Click
        ' Show the VECTOinput window
        F_VECTOInput.Show()
    End Sub
#End Region ' VECTOinput


#Region "Tools menu"
    ' Menu open the Log
    Private Sub ToolStripMenuItemLog_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuItemLog.Click
        System.Diagnostics.Process.Start(Prefs.editor, joinPaths(MyPath, "log.txt"))
    End Sub

    ' Menu open the config file
    Private Sub ToolStripMenuItemOption_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuItemOption.Click
        ' Show the confic GUI
        F_Preferences.Show()
    End Sub
#End Region ' Tools menu


#Region "Infos menu"
    ' Menu open the Infobox
    Private Sub ToolStripMenuItemAbout_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuItemAbout.Click
        ' Show the info GUI
        F_About.Show()
    End Sub

    ' Menu open the user manual
    Private Sub ToolStripMenuItemManu_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuItemManu.Click
        Dim manual_fname As String = (From fi As IO.FileInfo In (New IO.DirectoryInfo(joinPaths(MyPath, "Docs")).GetFiles("*VECTO_AirDrag_UserManual*.pdf", IO.SearchOption.TopDirectoryOnly)) Order By fi.LastAccessTime Select fi)(0).FullName

        Try
            System.Diagnostics.Process.Start(manual_fname)
        Catch ex As Exception
            logme(8, False, format("Failed opening User Manual({0}) due to: {1}", manual_fname, ex.Message), ex)
        End Try
    End Sub

    ' Menu open the release notes
    Private Sub ReleaseNotesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReleaseNotesToolStripMenuItem.Click
        Dim release_fname As String = (From fi As IO.FileInfo In (New IO.DirectoryInfo(joinPaths(MyPath, "Docs")).GetFiles("*VECTO_AirDrag_ReleaseNotes*.pdf", IO.SearchOption.TopDirectoryOnly)) Order By fi.LastAccessTime Select fi)(0).FullName

        Try
            System.Diagnostics.Process.Start(release_fname)
        Catch ex As Exception
            logme(8, False, format("Failed opening release notes({0}) due to: {1}", release_fname, ex.Message), ex)
        End Try
    End Sub

    ' Menue report a bug
    Private Sub ReportBugToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReportBugToolStripMenuItem.Click
        F_JIRA.Show()
    End Sub
#End Region  ' Infos menu

#End Region ' Main tab


#Region "Options tab"

    ' Check if the input is a number
    Private Sub TextBox_TextChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles TB_length_MS_max.KeyPress, TB_delta_rr_max.KeyPress, _
        TB_t_ground_max.KeyPress, TB_t_amb_max.KeyPress, TB_t_amb_min.KeyPress, TB_delta_Hz_max.KeyPress, TB_acc_corr_avg.KeyPress, TB_delta_parallel_max.KeyPress, TB_trigger_delta_x_max.KeyPress, TB_trigger_delta_y_max.KeyPress, TB_v_veh_1s_delta_CAL.KeyPress, TB_v_veh_ave_delta_CAL.KeyPress, _
        TB_delta_head_max.KeyPress, TB_segruns_min_CAL.KeyPress, TB_segruns_min_LS.KeyPress, TB_segruns_min_HS.KeyPress, TB_segruns_min_head_HS.KeyPress, TB_tq_sum_1s_delta_HS.KeyPress, TB_v_veh_1s_delta_HS.KeyPress, TB_beta_avg_max_HS.KeyPress, TB_v_veh_avg_min_HS.KeyPress, _
        TB_v_wind_1s_max_HS.KeyPress, TB_v_wind_avg_max_HS.KeyPress, TB_delta_n_ec_HS.KeyPress, TB_tq_sum_float_delta_LS.KeyPress, TB_v_veh_float_delta_LS.KeyPress, TB_v_veh_avg_max_LS.KeyPress, TB_v_veh_avg_min_LS.KeyPress, TB_slope_max.KeyPress, TB_length_MS_min.KeyPress, TB_delta_n_ec_LS.KeyPress, _
        TB_leng_crit.KeyPress, TB_beta_avg_max_CAL.KeyPress, TB_v_wind_1s_max_CAL.KeyPress, TB_v_wind_avg_max_CAL.KeyPress, TB_dist_float.KeyPress, TB_dist_gridpoints_max.KeyPress, TB_dist_grid_ms_max.KeyPress, TB_v_veh_avg_max_HS.KeyPress, TB_delta_v_avg_min_HS.KeyPress, TB_delta_CdxA_anemo.KeyPress
        Select Case Asc(e.KeyChar)
            Case 8, 45 To 46, 48 To 57 ' Backspace, Minus, Punkt und Zahlen zulassen (ASCII)
            Case Else ' Alles andere Unterdrücken
                e.Handled = True
        End Select
    End Sub

    ' Mode change buttons
    Private Sub ButtonMode_Click(sender As Object, e As EventArgs) Handles ButtonModeEng.Click, ButtonModeDec.Click, LB_SelMode.Click
        If SelMode = 0 And sender.text = "Engineering" Then Exit Sub
        If SelMode = 1 And sender.text = "Declaration" Then Exit Sub
        ' Switch after selected mode
        ModeSwitch()
    End Sub

    ' Switch after selected mode
    Private Function ModeSwitch(Optional ByVal setVal As Boolean = False) As Boolean
        ' Turn the mode to set the right one (turn is afterwards turned again)
        If setVal Then
            Select Case SelMode
                Case 0
                    SelMode = 1
                Case 1
                    SelMode = 0
            End Select
        End If

        ' Set the right mode on criteria tab
        Select Case SelMode
            Case 0 ' Set the programm to declaration mode
                SelMode = 1
                ' Set names and colors
                LB_SelMode.Text = "Declaration mode"
                LB_SelMode.ForeColor = Color.Blue
                ButtonModeEng.Font = New Font("Microsoft Sans Serif", 8.25, FontStyle.Regular)
                ButtonModeEng.ForeColor = Color.Black
                ButtonModeDec.Font = New Font("Microsoft Sans Serif", 8.25, FontStyle.Bold)
                ButtonModeDec.ForeColor = Color.Blue
                LInfoDM.Visible = True
                LStar.Visible = True
                LStar2.Visible = True
                ' Set all values to default
                Crt = New cCriteria
                UI_PopulateFromCriteria()
                ' Disable all criteria values
#If DEBUG Then
                UI_SetCriteriaStatus(True)
#Else
                UI_SetCriteriaStatus(False)
#End If
            Case 1 ' Set the programm to engeneering mode
                SelMode = 0
                ' Set names and colors
                LB_SelMode.Text = "Engineering mode"
                LB_SelMode.ForeColor = Color.Red
                ButtonModeEng.Font = New Font("Microsoft Sans Serif", 8.25, FontStyle.Bold)
                ButtonModeEng.ForeColor = Color.Red
                ButtonModeDec.Font = New Font("Microsoft Sans Serif", 8.25, FontStyle.Regular)
                ButtonModeDec.ForeColor = Color.Black
                LInfoDM.Visible = False
                LStar.Visible = False
                LStar2.Visible = False
                ' Enable all criteria values
                UI_SetCriteriaStatus()
        End Select

        Return True
    End Function

    ' Export all textboxes to file
    Private Sub doExportCriteria(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonCrtExport.Click
        UI_PopulateToJob(False)
        UI_PopulateToCriteria()
        If fbCRT.SaveDialog(Prefs.workingDir) Then
            Dim fname = fbCRT.Files(0)
            If fname Is Nothing Then Return
            Crt.Store(fname)
        End If
    End Sub

    ' Import all criteria textboxes
    Private Sub doImportCriteria(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonCrtImport.Click
        If fbCRT.OpenDialog(Prefs.workingDir) Then
            Dim fname = fbCRT.Files(0)
            If fname Is Nothing Then
                Return
            End If
            Dim newCrt = New cCriteria(fname)
            Job.Criteria = newCrt
            Crt = newCrt
        End If
        If Not IsNothing(JobFile) Then
            logme(8, False, format("The Job-file({0}) has changed because of criteria update", JobFile))
        End If
        UI_PopulateFromCriteria()
    End Sub

    ' CheckBox for the acceleration calibration
    Private Sub CheckBoxAcc_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CB_accel_correction.CheckedChanged
        Crt.accel_correction = CB_accel_correction.Checked
    End Sub

    ' Checkbox for the gradient correction
    Private Sub CheckBoxGrd_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CB_gradient_correction.CheckedChanged
        Crt.gradient_correction = CB_gradient_correction.Checked
    End Sub

    ' Change in the 1Hz radio button
    Private Sub RB1Hz_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RB1Hz.CheckedChanged
        If RB1Hz.Checked Then
            Crt.hz_out = 1
        Else
            Crt.hz_out = 100
        End If
    End Sub

    ' Change in the 100Hz radio button
    Private Sub RB100Hz_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RB100Hz.CheckedChanged
        If RB100Hz.Checked Then
            Crt.hz_out = 100
        Else
            Crt.hz_out = 1
        End If
    End Sub
#End Region 'Options tab


#Region "Infobox"
    ''' <summary>NOTE that the name of the controls below after the 3rd char is equal to the schema-property</summary>
    Private Sub setupInfoBox()
        Dim isStrict = False

        Dim schema As JObject
        Dim controls As Control()

        controls = New Control() {
                Me.CB_accel_correction, Nothing, _
                Me.CB_gradient_correction, Nothing, _
                Me.TB_rr_corr_factor, Me.LArr_corr_factor, _
                Me.GB_hz_out, Nothing, _
                Me.TB_acc_corr_avg, Me.LAccCorrAve, _
                Me.TB_delta_CdxA_anemo, Me.LB_delta_CdxA_anemo, _
                Me.TB_dist_float, Me.LDistFloat, _
                Me.TB_dist_gridpoints_max, Me.LB_dist_gridpoints_max, _
                Me.TB_dist_grid_ms_max, Me.LB_dist_grid_ms_max, _
                Me.TB_slope_max, LB_slope_max, _
                Me.TB_length_MS_max, LB_length_MS_max, _
                Me.TB_length_MS_min, LB_length_MS_min _
        }
        schema = New cCriteria(True).BodySchema.SelectToken("properties.Processing")
        armControlsWithInfoBox(schema, controls, AddressOf showInfoBox_crt, AddressOf hideInfoBox_crt)


        controls = New Control() {
            TB_trigger_delta_x_max, LDeltaXMax, _
            TB_trigger_delta_y_max, LDeltaYMax, _
            TB_delta_head_max, LContAng, _
            TB_segruns_min_CAL, LDsMinCAL, _
            TB_segruns_min_LS, LDsMinLS, _
            TB_segruns_min_HS, LDsMinHS, _
            TB_segruns_min_head_HS, LDsMinHeadHS, _
            TB_delta_Hz_max, LDeltaHzMax, _
            TB_delta_parallel_max, LDeltaParaMax, _
            TB_v_wind_avg_max_CAL, LvWindAveCALMax, _
            TB_v_wind_1s_max_CAL, LvWind1sCALMax, _
            TB_beta_avg_max_CAL, LBetaAveCALMax, _
            TB_v_veh_1s_delta_CAL, Lv_veh_1s_delta_CAL, _
            TB_v_veh_ave_delta_CAL, Lv_veh_ave_delta_CAL, _
            TB_leng_crit, LLengCrit, _
            TB_v_veh_avg_min_LS, LB_v_veh_avg_min_LS, _
            TB_v_veh_avg_max_LS, LB_v_veh_avg_max_LS, _
            TB_v_veh_float_delta_LS, LB_v_veh_float_delta_LS, _
            TB_tq_sum_float_delta_LS, LB_tq_sum_float_delta_LS, _
            TB_delta_n_ec_LS, LB_delta_n_ec_LS, _
            TB_v_wind_avg_max_HS, LB_v_wind_avg_max_HS, _
            TB_v_wind_1s_max_HS, LB_v_wind_1s_max_HS, _
            TB_beta_avg_max_HS, LB_beta_avg_max_HS, _
            TB_v_veh_avg_min_HS, LB_v_veh_avg_min_HS, _
            TB_v_veh_avg_max_HS, LB_v_veh_avg_max_HS, _
            TB_v_veh_1s_delta_HS, LB_v_veh_1s_delta_HS, _
            TB_tq_sum_1s_delta_HS, LB_tq_sum_1s_delta_HS, _
            TB_delta_n_ec_HS, LB_delta_n_ec_HS, _
            TB_delta_v_avg_min_HS, LB_delta_v_avg_min_HS, _
            TB_delta_rr_max, LB_delta_rr_max, _
            TB_t_amb_min, LB_t_amb_min, _
            TB_t_amb_max, LB_t_amb_max, _
            TB_t_ground_max, LB_t_amb_tarmac _
        }
        schema = New cCriteria(True).BodySchema.SelectToken("properties.Validation")
        armControlsWithInfoBox(schema, controls, AddressOf showInfoBox_crt, AddressOf hideInfoBox_crt)
    End Sub

    Private Sub showInfoBox_crt(ByVal sender As Object, ByVal e As System.EventArgs)
        TBInfoCrt.Text = sender.Tag
        TBInfoCrt.Visible = True
        PBInfoIconCrt.Visible = True
    End Sub

    Private Sub hideInfoBox_crt(ByVal sender As Object, ByVal e As System.EventArgs)
        TBInfoCrt.Visible = False
        PBInfoIconCrt.Visible = False
    End Sub

    Private Sub cbUse2ndAmdm_CheckedChanged(sender As Object, e As EventArgs) Handles cbUse2ndAmdm.CheckedChanged
        TransitionalService.Use2ndAmendment = cbUse2ndAmdm.Checked
        Crt = New cCriteria
        UI_PopulateFromCriteria()
    End Sub
#End Region
End Class
