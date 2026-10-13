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

Module declaration_public

    ' Description of the form
    Public Const AppName As String = "Air Drag"            ' Name of the programm
    Public AppVers As String = "3.3.5"                     ' Version of the Programm
    Public AppDate As String                                    ' Date of the compilation of the programm
    Public RELEASE_CANDIDATE As Boolean = AppVers.Contains("RC") 'False
    Public BETA_VERSION As Boolean = False

    ' Control variables
    Public Const komment = "#"                                  ' Symbol for a comment in the input files
    Public AnzeigeMessage() As String = ({"", "", "", "         + ", "      ~ ", "   * ", " - ", "", "", ""})

    Public AppFormStarted = False
    Public PrefsPath As String
    Public Prefs As cPreferences
    Public VECTOconfigPath As String
    Public VECTOconf As cVECTOconfig
    Public Job As cJob                                          ' The values for the 'Main' tab (and Criteria)
    Public Crt As cCriteria                                     ' The values for the 'Options' tab
    Public Sub installJob(ByVal newJob As cJob)
        Job = newJob
        Crt = newJob.Criteria
    End Sub

    ' General Path variables
    Public MyPath As String                                         ' Path of the *.exe
    Public RestartN As Boolean = False                              ' Restart of the *.exe

    ' Jobfile and output folder
    Public JobFile As String                                        ' Jobfile
    Public OutFolder As String                                      ' Output folder

    ' Constant Values
    Public Const LengKX = 1852                                      ' Length in meter for one minute in X (Latitude) direction
    Public Const AveSec = 1                                         ' Seconds over that the moving average should be calculated
    Public Const IDLS1 = 1                                          ' Run ID for the first low test run
    Public Const IDLS2 = 2                                          ' Run ID for the second low test run
    Public Const IDHS = 0                                           ' Run ID for the high test run
    Public Const f_rollHS = 1                                       ' Constant value for HS rolling resistance
    Public Zone1CentralMeridian = -177                              ' Central UTM zone meridian (Will be changed by zone adjustment)
    Public Const AmeAng = 180                                       ' Installation angle of the anemomenter

    ' Constances for the array declaration
    Public JumpPoint As List(Of Integer)                            ' Point at that a jump in the time-resolved data is detected
    Public OptPar() As Boolean = ({True, True})                     ' Array to identify if optional parameters are given
    Public SpeedPar() As Boolean = ({False, False, False, False})   ' Array to identify which speed parameters are given
    Public APT As Boolean = False                                   ' Calculation of an automatic transmission (with n_card)
    Public SMT_AMT_DCT As Boolean = False                           ' Calculation of an manual transmission (always when n_eng available)
    Public KoordSys() As Boolean = {False, False}                   ' Used Koordinate system 0. (MM.MM); 1.(DD.DD)
    Public AnzDigit(,) As Integer = {{5, 7}, {3, 5}}                ' Minimum ammount of digits position (x) 0. DGPS, 1. GPS --> x.0 (MM.MM); x.1 (DD.DD)

    ' Boolean for the programm control
    Public FileBlock As Boolean = False                             ' Variable if a file is blocked by an other process
    Public uRB As Boolean = False                                   ' Update the result boxes on the GUI
    Public DecErrorFlag As Boolean = False                          ' Error Flag for declaration mode

    'File browser
    Public FB_Drives() As String
    Public FB_Init As Boolean = False
    Public FB_FilHisDir As String
    Public FB_FolderHistory(19) As String

    Public fbTXT As cFileBrowser
    Public fbVECTO As cFileBrowser
    Public fbCRT As cFileBrowser
    Public fbCSV As cFileBrowser
    Public fbDir As cFileBrowser
    Public fbWorkDir As cFileBrowser
    Public fbExe As cFileBrowser
    Public fbVEH As cFileBrowser
    Public fbAMB As cFileBrowser
    Public fbALT As cFileBrowser
    Public fbVEL As cFileBrowser
    Public fbMSC As cFileBrowser

    ' Output file from the Logfile
    Public FileOutLog As New cFile_V3

    ' Dictionaries for the input data
    Public InputData As Dictionary(Of tComp, List(Of Double))                               ' Dictionary for the input data
    Public InputUndefData As Dictionary(Of String, List(Of Double))                         ' Dictionary for the undefined input data
    Public CalcData As Dictionary(Of tCompCali, List(Of Double))                            ' Dictionary for the calculation data
    Public ErgValues As Dictionary(Of tCompErg, List(Of Double))                            ' Dictionary for the result data
    Public ErgValuesUndef As Dictionary(Of String, List(Of Double))                         ' Dictionary for the undefined result data (from the undefined input data)
    Public InputWeatherData As Dictionary(Of tCompWeat, List(Of Double))                    ' Dictionary for the weather data
    Public sKey As csKey                                                                    ' Key array for the input data (Definition of the column identifier)
    Public ErgValuesComp As Dictionary(Of tCompErg, List(Of Double))                        ' Dictionary for the result data (complete)
    Public ErgValuesUndefComp As Dictionary(Of String, List(Of Double))                     ' Dictionary for the undefined result data (from the undefined input data, complete)
    Public ErgValuesReg As Dictionary(Of tCompErgReg, List(Of Double))                      ' Dictionary for the result data from the linear regression

    ' Result dictionaries
    Public ErgEntriesI As New Dictionary(Of tComp, CResult)                                 ' Dictionary of the result from the input data (100Hz)
    Public ErgEntryListI As New List(Of tComp)                                              ' Array with the output sequenz of the result from the input data
    Public ErgEntriesIU As New Dictionary(Of String, CResult)                               ' Dictionary of the result from the undefined input data (100Hz)
    Public ErgEntryListIU As New List(Of String)                                            ' Array with the output sequenz of the result from the undefined input data
    Public ErgEntriesC As New Dictionary(Of tCompCali, CResult)                             ' Dictionary of the result from the calculated data (100Hz)
    Public ErgEntryListC As New List(Of tCompCali)                                          ' Array with the output sequenz of the result from the calculated data
    Public ErgEntriesR As New Dictionary(Of tCompErg, CResult)                              ' Dictionary of the result from the calculated result data (1Hz)
    Public ErgEntryListR As New List(Of tCompErg)                                           ' Array with the output sequenz of the result from the calculated result data
    Public ErgEntriesRU As New Dictionary(Of String, CResult)                               ' Dictionary of the result from the undefined input data (1Hz)
    Public ErgEntryListRU As New List(Of String)                                            ' Array with the output sequenz of the result from the undefined input data
    Public ErgEntriesReg As New Dictionary(Of tCompErgReg, CResult)                         ' Dictionary of the result from the regression calculation
    Public ErgEntryListReg As New List(Of tCompErgReg)                                      ' Array with the output sequenz of the result from the regression calculation

    ' Result values
    Public GenShape As New cGenShp

    ' *****************************************************************
    ' Backgroundworker
    Public BWorker As System.ComponentModel.BackgroundWorker
End Module
