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

' Read the input data

Public Module input

    ' Read the measurement section config file
    Sub ReadInputMSC(ByRef MSCX As cMSC, ByVal MSCfile As String, Optional ByVal calibration As Boolean = True)
        ' Declarations
        Dim i, j As Integer
        Dim RefDID, RefID, CoordID As Integer
        Dim Counter As Long = 0
        Dim DemoDataF As Boolean = False
        Dim RefHead As Double
        Dim Line() As String
        Using FileInMSCSpez As New cFile_V3

            ' Read the filelist with the MSC spezifications
            ' Output on the GUI
            logme(5, False, "Read MS configuration file")

            ' Open the MSC spezification file
            If fPath(MSCfile) = joinPaths(MyPath, "DemoData") Then
                System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator = "."
                FileInMSCSpez.OpenReadWithEx(MSCfile)
                DemoDataF = True
            Else
                FileInMSCSpez.OpenReadWithEx(MSCfile, Prefs.listSep)
            End If

            ' Determine the trigger status 
            Line = FileInMSCSpez.ReadLine
            If Line(0).Length <> 1 Then
                Throw New Exception(format("The configuration file does not fit the file format. Control the separators under Tools/preferences and in the file. Read value can not be converted ({0})", Line(0)))
            Else
                MSCX.tUse = Line(0)
            End If

            ' Check the headers for the coordinate unit
            Line = FileInMSCSpez.ReadLine
            If IsNumeric(Line(0)) Then
                Throw New Exception(format("The header of the *.csms file is missing. Please include it or comment it in the file ({0}).", MSCfile))
            Else
                CoordID = 0
                If Line(4).ToUpper.Contains("(D)") Then
                    CoordID = 1
                End If
            End If

            ' Input loop
            Try
                Do While Not FileInMSCSpez.EndOfFile
                    ' Read the dataline
                    Line = FileInMSCSpez.ReadLine

                    MSCX.meID.Add(Line(0))
                    MSCX.dID.Add(Line(1))
                    MSCX.len.Add(Line(2))
                    MSCX.head.Add(Line(3))
                    ' Save the coordinates in minutes
                    Select Case CoordID
                        Case 0
                            ' [MM.MM]
                            MSCX.latS.Add(Line(4))
                            MSCX.longS.Add(Line(5))
                            MSCX.latE.Add(Line(6))
                            MSCX.longE.Add(Line(7))
                        Case 1
                            ' [DD.DD]
                            MSCX.latS.Add(Line(4) * 60)
                            MSCX.longS.Add(Line(5) * 60)
                            MSCX.latE.Add(Line(6) * 60)
                            MSCX.longE.Add(Line(7) * 60)
                    End Select
                    If Crt.gradient_correction And Not calibration Then MSCX.AltPath.Add(Line(8))
                    If Line(4).Substring(Line(4).IndexOf(Prefs.decSep) + 1).Length < AnzDigit(Convert.ToInt32(MSCX.tUse), CoordID) Then Counter += 1
                    If Line(5).Substring(Line(5).IndexOf(Prefs.decSep) + 1).Length < AnzDigit(Convert.ToInt32(MSCX.tUse), CoordID) Then Counter += 1
                    If Line(6).Substring(Line(6).IndexOf(Prefs.decSep) + 1).Length < AnzDigit(Convert.ToInt32(MSCX.tUse), CoordID) Then Counter += 1
                    If Line(7).Substring(Line(7).IndexOf(Prefs.decSep) + 1).Length < AnzDigit(Convert.ToInt32(MSCX.tUse), CoordID) Then Counter += 1
                Loop
            Catch ex As Exception
                ' Falls kein gültiger Wert eingegeben wurde
                Throw New Exception(format("Invalid value in the *.csms data file({0}) due to: {1})", fName(MSCfile, True), ex.Message, ex))
            End Try

        End Using

        ' Minimum datacheck
        If (MSCX.meID.Count - 1) < 2 Then
            Throw New Exception(format("Not enough measurement sections definied in file ({0}). Minimum number of definitions are two.", fName(MSCfile, True)))
        End If

        ' Length check
        For i = 1 To MSCX.meID.Count - 1
            If (MSCX.len(i) < Crt.length_MS_min Or MSCX.len(i) > Crt.length_MS_max) Then
                Throw New Exception(format("The defined measurement section length ({0}) is outside the valid boundaries ({1} - {2}).", MSCX.len(i), Crt.length_MS_min, Crt.length_MS_max))
            End If
        Next i

        ' Checks for same definition
        For i = 1 To MSCX.meID.Count - 1
            For j = i + 1 To MSCX.meID.Count - 1
                If MSCX.meID(i) = MSCX.meID(j) And MSCX.dID(i) = MSCX.dID(j) Then
                    Throw New Exception(format("Double definition of measurement section ({0}) in file ({1}).", MSCX.meID(i), fName(MSCfile, True)))
                End If
            Next j
        Next i

        ' Checks by test runs
        If Not calibration Then
            ' Check the headings
            For i = 1 To MSCX.meID.Count - 1
                If i = 1 Then
                    RefHead = MSCX.head(i)
                    MSCX.headID.Add(1)
                Else
                    If Math.Abs(MSCX.head(i) - RefHead) < Crt.delta_parallel_max Then
                        MSCX.headID.Add(1)
                        Continue For
                    ElseIf (Math.Abs(MSCX.head(i) - RefHead + 180) < Crt.delta_parallel_max Or Math.Abs(MSCX.head(i) - RefHead - 180) < Crt.delta_parallel_max) Then
                        MSCX.headID.Add(2)
                        Continue For
                    Else
                        Throw New Exception(format("Measurement section with invalid headings identified (test track parallelism critera not met) at line: {0}", i))
                    End If
                End If
            Next i

            ' Control the altitude path
            For i = 1 To MSCX.meID.Count - 1
                If Crt.gradient_correction Then
                    If MSCX.AltPath(i) = Nothing Then
                        Throw New Exception("Altitude correction = on, missing altitude file at line: " & i)
                    End If

                    If fPath(MSCX.AltPath(i)) = Nothing Then MSCX.AltPath(i) = joinPaths(fPath(MSCfile), MSCX.AltPath(i))
                    If Not FileIO.FileSystem.FileExists(MSCX.AltPath(i)) Then
                        Throw New Exception("Altitude correction = on, altitude file doesen´t exist: " & MSCX.AltPath(i))
                    End If
                End If
            Next i
        Else
            For i = 1 To MSCX.meID.Count - 1
                RefID = MSCX.meID(i)
                RefHead = MSCX.head(i)
                RefDID = MSCX.dID(i)
                For j = i + 1 To MSCX.meID.Count - 1
                    If RefID = MSCX.meID(j) And RefDID = MSCX.dID(j) And RefHead <> MSCX.head(j) Or RefID = MSCX.meID(j) And RefDID <> MSCX.dID(j) And RefHead = MSCX.head(j) Then
                        Throw New Exception("Two different directions for same heading given. Please correct your input in the File: " & MSCfile)
                    End If
                Next j
                MSCX.headID.Add(1)
            Next i
        End If

        ' Check coordinate digits after decimal seperator
        If Counter > 0 Then
            If Job.mode = 1 Then
                Throw New Exception(format("The csms coordinates from file ({0}) have not enough digits after the decimal seperator (minimum digits are ({1}). Number of fails: ({1})!", MSCfile, AnzDigit(Convert.ToInt32(MSCX.tUse), CoordID), Counter))
            Else
                logme(8, False, format("The csms coordinates from file ({0}) have not enough digits after the decimal seperator (minimum digits are ({1}). Number of fails: ({1})!", MSCfile, AnzDigit(Convert.ToInt32(MSCX.tUse), CoordID), Counter))
            End If
        End If

        ' Change the decimal seperator back
        If DemoDataF Then
            System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator = Prefs.decSep
            DemoDataF = False
        End If
    End Sub

    ' Read the altitude files
    Sub ReadAltitudeFiles(ByVal MSCOrg As cMSC, ByRef Altdata As List(Of cAlt))
        ' Declarations
        Dim i As Integer
        Dim CoordID As Integer
        Dim CounterLat, CounterLong, CounterAlt As Long
        Dim DemoDataF As Boolean = False
        Dim FirstIn As Boolean = True
        Dim Line() As String

        ' Initialisation (first element)
        Altdata.Add(New cAlt)

        ' Read the filelist with the altitude spezifications
        ' Output on the GUI
        logme(5, False, "Read MS altitude file")

        For i = 1 To MSCOrg.AltPath.Count - 1
            Using FileInAltSpez As New cFile_V3
                ' Open the altitude file
                If fPath(MSCOrg.AltPath(i)) = joinPaths(MyPath, "DemoData") Then
                    System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator = "."
                    FileInAltSpez.OpenReadWithEx(MSCOrg.AltPath(i))
                    DemoDataF = True
                Else
                    FileInAltSpez.OpenReadWithEx(MSCOrg.AltPath(i), Prefs.listSep)
                End If

                ' Check the file
                Line = FileInAltSpez.ReadLine
                If Line.Length <> 3 Then
                    Throw New Exception(format("The altitude file does not fit the file format. Control the separators under Tools/preferences and in the file. Read value can not be converted ({0})", Line(0)))
                End If

                ' Check the headers for the coordinate unit
                If IsNumeric(Line(0)) Then
                    Throw New Exception(format("The header of the *.csalt file is missing. Please include it or comment it in the file ({0}).", MSCOrg.AltPath(i)))
                Else
                    CoordID = 0
                    If Line(0).ToUpper.Contains("(D)") Then
                        CoordID = 1
                    End If
                End If

                ' Input loop
                CounterLat = 0
                CounterLong = 0
                CounterAlt = 0
                Altdata.Add(New cAlt)
                FirstIn = True
                Try
                    Do While Not FileInAltSpez.EndOfFile
                        ' Read the dataline
                        Line = FileInAltSpez.ReadLine

                        ' Get the Sec and Dir ID
                        If FirstIn Then
                            Altdata(i).meID = MSCOrg.meID(i)
                            Altdata(i).dID = MSCOrg.dID(i)
                        End If

                        ' Read the data (Save the coordinates in minutes)
                        Select Case CoordID
                            Case 0
                                ' [MM.MM]
                                Altdata(i).KoordLat.Add(Line(0))
                                Altdata(i).KoordLong.Add(Line(1))
                            Case 1
                                ' [DD.DD]
                                Altdata(i).KoordLat.Add(Line(0) * 60)
                                Altdata(i).KoordLong.Add(Line(1) * 60)
                        End Select
                        Altdata(i).Altitude.Add(Line(2))

                        If Line(0).Substring(Line(0).IndexOf(Prefs.decSep) + 1).Length < AnzDigit(0, CoordID) Then CounterLat += 1
                        If Line(1).Substring(Line(1).IndexOf(Prefs.decSep) + 1).Length < AnzDigit(0, CoordID) Then CounterLong += 1
                        If Line(2).Substring(Line(2).IndexOf(Prefs.decSep) + 1).Length < 2 Then CounterAlt += 1

                        ' Calculate the UTM coordinates for each point
                        Altdata(i).UTM.Add(UTM(Altdata(i).KoordLat(Altdata(i).KoordLat.Count - 1) / 60, Altdata(i).KoordLong(Altdata(i).KoordLong.Count - 1) / 60))

                        ' Calculate the distance from the begin of the altitude profile
                        If FirstIn Then
                            Altdata(i).dist.Add(0)
                            FirstIn = False
                        Else
                            Altdata(i).dist.Add(Math.Sqrt(Math.Pow(Altdata(i).UTM(0).Easting - Altdata(i).UTM(Altdata(i).UTM.Count - 1).Easting, 2) + Math.Pow(Altdata(i).UTM(0).Northing - Altdata(i).UTM(Altdata(i).UTM.Count - 1).Northing, 2)))
                        End If
                    Loop

                    ' Check coordinate digits after decimal seperator
                    If CounterLat > 0 Then
                        If Job.mode = 1 Then
                            Throw New Exception(format("The altitude latitude coordinates from file ({0}) have not enough digits after the decimal seperator (minimum digits are ({1}). Number of fails: ({2})!", MSCOrg.AltPath(i), AnzDigit(0, CoordID), CounterLat))
                        Else
                            logme(8, False, format("The altitude latitude coordinates from file ({0}) have not enough digits after the decimal seperator (minimum digits are ({1}). Number of fails: ({2})!", MSCOrg.AltPath(i), AnzDigit(0, CoordID), CounterLat))
                        End If
                    End If
                    If CounterLong > 0 Then
                        If Job.mode = 1 Then
                            Throw New Exception(format("The altitude longitude coordinates from file ({0}) have not enough digits after the decimal seperator (minimum digits are ({1}). Number of fails: ({2})!", MSCOrg.AltPath(i), AnzDigit(0, CoordID), CounterLong))
                        Else
                            logme(8, False, format("The altitude longitude coordinates from file ({0}) have not enough digits after the decimal seperator (minimum digits are ({1}). Number of fails: ({2})!", MSCOrg.AltPath(i), AnzDigit(0, CoordID), CounterLong))
                        End If
                    End If
                    If CounterAlt > 0 Then
                        If Job.mode = 1 Then
                            Throw New Exception(format("The altitude in the altitude file ({0}) have not enough digits after the decimal seperator (minimum digits are ({1}). Number of fails: ({2})!", MSCOrg.AltPath(i), 2, CounterAlt))
                        Else
                            logme(8, False, format("The altitude in the altitude file ({0}) have not enough digits after the decimal seperator (minimum digits are ({1}). Number of fails: ({2})!", MSCOrg.AltPath(i), 2, CounterAlt))
                        End If
                    End If
                Catch ex As Exception
                    ' Falls kein gültiger Wert eingegeben wurde
                    Throw New Exception(format("Invalid value in the *.csalt data file({0}) due to: {1})", fName(MSCOrg.AltPath(i), True), ex.Message, ex))
                End Try
            End Using
        Next i

        ' Change the decimal seperator back
        If DemoDataF Then
            System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator = Prefs.decSep
            DemoDataF = False
        End If
    End Sub

    ' Read the wather data
    Public Sub ReadWeather(ByVal Datafile As String)
        ' Declaration
        Using FileInWeather As New cFile_V3
            Dim Line() As String
            Dim i, tdim, nDay As Integer
            Dim tWeath(1) As Double
            Dim DayTimeSec = 24 * 60 * 60                       ' Time of the day in Seconds
            Dim DemoDataF As Boolean = False
            Dim Comp As tCompWeat
            Dim WeathCheck As New Dictionary(Of tCompWeat, Boolean)
            Dim sKVW As New KeyValuePair(Of tCompWeat, Boolean)
            Dim Spalten As New Dictionary(Of tCompWeat, Integer)
            Dim sKV As New KeyValuePair(Of tCompWeat, Integer)

            ' Initialise
            tdim = -1
            nDay = 0
            InputWeatherData = New Dictionary(Of tCompWeat, List(Of Double))

            'Open file
            If fPath(Datafile) = joinPaths(MyPath, "DemoData") Then
                System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator = "."
                FileInWeather.OpenReadWithEx(Datafile)
                DemoDataF = True
            Else
                FileInWeather.OpenReadWithEx(Datafile, Prefs.listSep)
            End If

            ' Build check key
            WeathCheck.Add(tCompWeat.t, False)
            WeathCheck.Add(tCompWeat.t_amb_stat, False)
            WeathCheck.Add(tCompWeat.p_amp_stat, False)
            WeathCheck.Add(tCompWeat.rh_stat, False)

            '*** Second row: Name/Identification of the Components
            Line = FileInWeather.ReadLine

            ' Datacheck
            If Line.Length = 1 Then
                Throw New Exception(format("The weather file does not fit the file format. Control the separators under Tools/preferences and in the file. Read value can not be converted ({0})", Line(0)))
            End If

            'Check Number of Columns/Components
            For i = 0 To UBound(Line)

                Comp = fCompWeather(Line(i))

                ' If used Meascomp = Undefined it will get as EXS-Comp or Emission for KF-Creation / Eng-Analysis
                If Comp = tCompWeat.Undefined Then
                    'TODO: Was bei unbekannten Wetterdaten
                Else
                    ' Check if component is already defined
                    If WeathCheck(Comp) Then
                        Throw New Exception(format("Column {0}: Component({1}) already defined!", i + 1, Line(i)))
                    End If

                    ' Set the defined component true and save the position
                    WeathCheck(Comp) = True
                    Spalten.Add(Comp, i)
                    InputWeatherData.Add(Comp, New List(Of Double))
                End If
            Next i

            ' Check if all required data is given
            For Each sKVW In WeathCheck
                If Not WeathCheck(sKVW.Key) Then
                    Throw New Exception("Missing signal for " & fCompName(sKVW.Key))
                End If
            Next

            ' Read the date from the file
            Try
                Do While Not FileInWeather.EndOfFile
                    tdim += 1
                    Line = FileInWeather.ReadLine

                    For Each sKV In Spalten
                        If sKV.Key = tCompWeat.t Then
                            tWeath(1) = CDbl(Line(sKV.Value))
                            If tdim >= 1 Then
                                If (tWeath(1) < tWeath(0)) Then nDay += 1
                            End If
                            InputWeatherData(sKV.Key).Add(CDbl(Line(sKV.Value) + nDay * DayTimeSec))
                            tWeath(0) = CDbl(Line(sKV.Value))
                        Else
                            InputWeatherData(sKV.Key).Add(CDbl(Line(sKV.Value)))
                        End If
                    Next sKV
                Loop
            Catch ex As Exception
                Throw New Exception(format("Exception while reading file({0}), line({1}) due to: {2}!: ", Datafile, tdim + 1, ex.Message), ex)
            End Try

            ' Change the decimal seperator back
            If DemoDataF Then
                System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator = Prefs.decSep
                DemoDataF = False
            End If

        End Using
    End Sub

    ' Read the data file
    Public Sub ReadDataFile(ByVal Datafile As String, ByVal MSCX As cMSC, ByVal vehicleX As cVehicle, Optional ByVal Calib As Boolean = False)
        ' Declarations
        Using FileInMeasure As New cFile_V3
            Dim Line(), txt As String
            Dim i, tDim, nDay, CoordID As Integer
            Dim CounterLat, CounterLong As Long
            Dim HzIn = 100                                      ' Hz frequency demanded for .csdat-file
            Dim DayTimeSec = 24 * 60 * 60                       ' Time of the day in Seconds
            Dim valid_set As Boolean = False
            Dim valid_set2 As Boolean = False
            Dim UTMcalc As Boolean = False
            Dim ZoneChange As Boolean = False
            Dim DemoDataF As Boolean = False
            Dim Comp As tComp
            Dim MeasCheck As New Dictionary(Of tComp, Boolean)
            Dim sKVM As New KeyValuePair(Of tComp, Boolean)
            Dim Spalten As New Dictionary(Of tComp, Integer)    ' Spalten -> Columns
            Dim sKV As New KeyValuePair(Of tComp, Integer)
            Dim SpaltenUndef As New Dictionary(Of String, Integer)
            Dim sKVUndef As New KeyValuePair(Of String, Integer)
            Dim EnumStr As tCompCali
            Dim UTMCoord As New cUTMCoord
            Dim LowSpeedTestMaxDuration As Double = 25 * 60     ' 25 minutes in seconds

            ' Initialise
            tDim = -1
            nDay = 0
            InputData = New Dictionary(Of tComp, List(Of Double))
            InputUndefData = New Dictionary(Of String, List(Of Double))
            CalcData = New Dictionary(Of tCompCali, List(Of Double))
            JumpPoint = New List(Of Integer)
            For i = 0 To UBound(OptPar)
                OptPar(i) = True
            Next i
            For i = 0 To UBound(KoordSys)
                KoordSys(i) = False
            Next i

            ' Generate the calculation dictionary variables
            For Each EnumStr In System.Enum.GetValues(GetType(tCompCali))
                CalcData.Add(EnumStr, New List(Of Double))
            Next

            'Open file
            If fPath(Datafile) = joinPaths(MyPath, "DemoData") Then
                System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator = "."
                FileInMeasure.OpenReadWithEx(Datafile)
                DemoDataF = True
            Else
                FileInMeasure.OpenReadWithEx(Datafile, Prefs.listSep)
            End If

            ' Build check key
            MeasCheck.Add(tComp.t, False)
            MeasCheck.Add(tComp.lati, False)
            MeasCheck.Add(tComp.longi, False)
            MeasCheck.Add(tComp.lati_D, False)
            MeasCheck.Add(tComp.longi_D, False)
            MeasCheck.Add(tComp.hdg, False)
            MeasCheck.Add(tComp.v_veh_GPS, False)
            MeasCheck.Add(tComp.v_veh_CAN, False)
            MeasCheck.Add(tComp.vair_ic, False)
            MeasCheck.Add(tComp.beta_ic, False)
            MeasCheck.Add(tComp.n_card, False)
            MeasCheck.Add(tComp.n_eng, False)
            MeasCheck.Add(tComp.n_wheel_ave, False)
            MeasCheck.Add(tComp.n_EM, False)
            MeasCheck.Add(tComp.tq_l, False)
            MeasCheck.Add(tComp.tq_r, False)
            MeasCheck.Add(tComp.t_ground, False)
            MeasCheck.Add(tComp.t_amb_veh, False)
            MeasCheck.Add(tComp.trigger, False)
            MeasCheck.Add(tComp.user_valid, False)
            MeasCheck.Add(tComp.s_brake, False)

            '*** Second row: Name/Identification of the Components
            Line = FileInMeasure.ReadLine

            ' Datacheck
            If Line.Length = 1 Then
                Throw New Exception(format("The data file does not fit the file format. Control the separators under Tools/preferences and in the file. Read value can not be converted ({0})", Line(0)))
            End If

            'Check Number of Columns/Components
            For i = 0 To UBound(Line)

                Comp = fComp(Line(i))

                ' If used Meascomp = Undefined it will get as EXS-Comp or Emission for KF-Creation / Eng-Analysis
                If Comp = tComp.Undefined Then
                    ' Get the given Name
                    txt = Trim(Line(i))

                    ' Check if the component is already defined
                    If InputUndefData.ContainsKey(txt) Then
                        Throw New Exception(format("Column {0}: Component({1}) already defined!", i + 1, Line(i)))
                    End If

                    ' Add the component to the dictionary
                    SpaltenUndef.Add(txt, i)
                    InputUndefData.Add(txt, New List(Of Double))
                Else
                    ' Check if component is already defined
                    If MeasCheck(Comp) Then
                        Throw New Exception(format("Column {0}: Component({1}) already defined!", i + 1, Line(i)))
                    End If

                    ' Set the defined component true and save the position
                    MeasCheck(Comp) = True
                    Spalten.Add(Comp, i)
                    InputData.Add(Comp, New List(Of Double))
                End If
            Next i

            ' Check if all required data is given
            For Each sKVM In MeasCheck
                If Not MeasCheck(sKVM.Key) Then
                    Select Case sKVM.Key
                        Case tComp.trigger
                            If MSCX.tUse Then
                                Throw New Exception("No trigger signal detected, but trigger_used in MS config activated!")
                            End If
                            OptPar(0) = False
                        Case tComp.user_valid
                            valid_set = True
                        Case tComp.t_ground
                            If Not Calib Then
                                Throw New Exception("Missing signal for " & fCompName(sKVM.Key))
                            Else
                                valid_set2 = True
                                OptPar(1) = False
                            End If
                        Case tComp.n_eng, tComp.n_card, tComp.n_wheel_ave, tComp.n_EM
                            Dim outText = ""
                            Dim MoreAs1 = 0
                            If MeasCheck(tComp.n_eng) Then
                                outText = fCompName(tComp.n_eng)
                                SpeedPar(0) = True
                                MoreAs1 += 1
                            End If
                            If MeasCheck(tComp.n_card) Then
                                If outText.Length > 0 Then
                                    outText += ", " & fCompName(tComp.n_card)
                                Else
                                    outText = fCompName(tComp.n_card)
                                End If
                                SpeedPar(1) = True
                                MoreAs1 += 1
                            End If
                            If MeasCheck(tComp.n_wheel_ave) Then
                                If outText.Length > 0 Then
                                    outText += ", " & fCompName(tComp.n_wheel_ave)
                                Else
                                    outText = fCompName(tComp.n_card)
                                End If
                                SpeedPar(2) = True
                                MoreAs1 += 1
                            End If
                            If MeasCheck(tComp.n_EM) Then
                                If outText.Length > 0 Then
                                    outText += ", " & fCompName(tComp.n_EM)
                                Else
                                    outText = fCompName(tComp.n_card)
                                End If
                                SpeedPar(3) = True
                                MoreAs1 += 1
                            End If

                            If (MoreAs1 > 1) Then
                                Throw New Exception("Given components: " & outText & "! Please check input data and only give one.")
                            End If
                            If (MeasCheck(tComp.n_eng) = False And MeasCheck(tComp.n_card) = False And MeasCheck(tComp.n_wheel_ave) = False And MeasCheck(tComp.n_EM) = False) Then
                                Throw New Exception("Missing signal for " & fCompName(tComp.n_eng) & ", " & fCompName(tComp.n_card) & ", " & fCompName(tComp.n_wheel_ave) & " or " & fCompName(tComp.n_EM))
                            End If
                        Case tComp.n_card
                            If (MeasCheck(tComp.n_card) = True And (MeasCheck(tComp.n_eng) = True Or MeasCheck(tComp.n_wheel_ave) = True Or MeasCheck(tComp.n_EM) = True)) Then
                                Dim outText = fCompName(tComp.n_card)
                                If MeasCheck(tComp.n_eng) Then outText += ", " & fCompName(tComp.n_eng)
                                If MeasCheck(tComp.n_wheel_ave) Then outText += ", " & fCompName(tComp.n_wheel_ave)
                                If MeasCheck(tComp.n_EM) Then outText += ", " & fCompName(tComp.n_EM)
                                Throw New Exception("Given components: " & outText & "! Please check input data and only give one.")
                            ElseIf (MeasCheck(tComp.n_card) = False And MeasCheck(tComp.n_wheel_ave) = False And MeasCheck(tComp.n_eng) = False And MeasCheck(tComp.n_EM) = False) Then
                                Throw New Exception("Missing signal for " & fCompName(tComp.n_eng) & ", " & fCompName(tComp.n_card) & ", " & fCompName(tComp.n_wheel_ave) & " or " & fCompName(tComp.n_EM))
                            End If
                        Case tComp.n_wheel_ave
                            If (MeasCheck(tComp.n_wheel_ave) = True And (MeasCheck(tComp.n_eng) = True Or MeasCheck(tComp.n_card) = True Or MeasCheck(tComp.n_EM) = True)) Then
                                Dim outText = fCompName(tComp.n_wheel_ave)
                                If MeasCheck(tComp.n_eng) Then outText += ", " & fCompName(tComp.n_eng)
                                If MeasCheck(tComp.n_card) Then outText += ", " & fCompName(tComp.n_card)
                                If MeasCheck(tComp.n_EM) Then outText += ", " & fCompName(tComp.n_EM)
                                Throw New Exception("Given components: " & outText & "! Please check input data and only give one.")
                            ElseIf (MeasCheck(tComp.n_card) = False And MeasCheck(tComp.n_wheel_ave) = False And MeasCheck(tComp.n_eng) = False And MeasCheck(tComp.n_EM) = False) Then
                                Throw New Exception("Missing signal for " & fCompName(tComp.n_eng) & ", " & fCompName(tComp.n_card) & ", " & fCompName(tComp.n_wheel_ave) & " or " & fCompName(tComp.n_EM))
                            End If
                        Case tComp.n_EM
                            If (MeasCheck(tComp.n_EM) = True And (MeasCheck(tComp.n_eng) = True Or MeasCheck(tComp.n_card) = True Or MeasCheck(tComp.n_wheel_ave) = True)) Then
                                Dim outText = fCompName(tComp.n_EM)
                                If MeasCheck(tComp.n_eng) Then outText += ", " & fCompName(tComp.n_eng)
                                If MeasCheck(tComp.n_card) Then outText += ", " & fCompName(tComp.n_card)
                                If MeasCheck(tComp.n_wheel_ave) Then outText += ", " & fCompName(tComp.n_wheel_ave)
                                Throw New Exception("Given components: " & outText & "! Please check input data and only give one.")
                            ElseIf (MeasCheck(tComp.n_card) = False And MeasCheck(tComp.n_wheel_ave) = False And MeasCheck(tComp.n_eng) = False And MeasCheck(tComp.n_EM) = False) Then
                                Throw New Exception("Missing signal for " & fCompName(tComp.n_eng) & ", " & fCompName(tComp.n_card) & ", " & fCompName(tComp.n_wheel_ave) & " or " & fCompName(tComp.n_EM))
                            End If
                        Case tComp.lati, tComp.lati_D, tComp.longi, tComp.longi_D, tComp.longi, tComp.longi_D
                            ' Latitude controlling
                            If Not MeasCheck(tComp.lati) And Not MeasCheck(tComp.lati_D) Then
                                Throw New Exception("Missing coordinate signal for " & fCompName(sKVM.Key))
                            End If
                            ' Longitude controlling
                            If Not MeasCheck(tComp.longi) And Not MeasCheck(tComp.longi_D) Then
                                Throw New Exception("Missing coordinate signal for " & fCompName(sKVM.Key))
                            End If
                            ' Combination controlling
                            If MeasCheck(tComp.lati) And MeasCheck(tComp.longi) Then KoordSys(0) = True
                            If MeasCheck(tComp.lati_D) And MeasCheck(tComp.longi_D) Then KoordSys(1) = True
                            If Not KoordSys(0) And Not KoordSys(1) Then
                                Throw New Exception("Only same coordinate system allowed for calculation. Please give the coordinates in one system [DD.DD] or [MM.MM].")
                            End If
                        Case tComp.s_brake
                            If (Not TransitionalService.Use2ndAmendment) Then
                                If (MeasCheck(tComp.s_brake) = False Or Not MeasCheck(tComp.s_brake)) Then
                                    Throw New Exception("Missing signal for " & fCompName(tComp.s_brake))
                                End If
                            End If
                        Case Else
                            Throw New Exception("Missing signal for " & fCompName(sKVM.Key))
                    End Select
                End If
            Next

            ' Read the date from the file
            If KoordSys(0) Then CoordID = 0
            If KoordSys(1) Then CoordID = 1
            Try
                Do While Not FileInMeasure.EndOfFile
                    tDim += 1
                    Line = FileInMeasure.ReadLine


                    For Each sKV In Spalten
                        InputData(sKV.Key).Add(CDbl(Line(sKV.Value)))
                        If sKV.Key = tComp.t Then
                            If tDim >= 1 Then
                                If Math.Abs((InputData(sKV.Key)(tDim) - InputData(sKV.Key)(tDim - 1)) / (1 / HzIn) - 1) * 100 > Crt.delta_Hz_max Then
                                    JumpPoint.Add(tDim)
                                End If
                            End If
                            CalcData(tCompCali.t).Add(CDbl(Line(sKV.Value) + nDay * DayTimeSec))
                        ElseIf sKV.Key = tComp.s_brake Then
                            If (Not TransitionalService.Use2ndAmendment) Then
                                Dim wasServiceBrakeUsed = CBool(Line(sKV.Value))
                                If wasServiceBrakeUsed Then
                                    logme(9, False, "Service brake was activated during test. Usage of service brake is not permitted.")
                                    Throw New Exception("Service brake was activated during test. Usage of service brake is not permitted.")
                                End If
                            End If
                        ElseIf sKV.Key = tComp.lati Or sKV.Key = tComp.lati_D Then
                            If Line(sKV.Value).Substring(Line(sKV.Value).IndexOf(Prefs.decSep) + 1).Length < AnzDigit(Convert.ToInt32(MSCX.tUse), CoordID) Then CounterLat += 1
                            If UTMcalc Then
                                If MeasCheck(tComp.lati) And sKV.Key = tComp.lati Then UTMCoord = UTM(InputData(sKV.Key)(tDim) / 60, InputData(tComp.longi)(tDim) / 60)
                                If Not MeasCheck(tComp.lati) And MeasCheck(tComp.lati_D) And sKV.Key = tComp.lati_D Then UTMCoord = UTM(InputData(sKV.Key)(tDim), InputData(tComp.longi_D)(tDim))

                                If Not ZoneChange Then
                                    If tDim > 0 Then
                                        If CalcData(tCompCali.zone_UTM).Last <> UTMCoord.Zone Then
                                            logme(8, False, "The coordinates lie in different UTM Zones. A zone adjustment will be done!")
                                            ZoneChange = True
                                        End If
                                    End If
                                End If
                                CalcData(tCompCali.zone_UTM).Add(UTMCoord.Zone)
                                CalcData(tCompCali.lati_UTM).Add(UTMCoord.Northing)
                                CalcData(tCompCali.longi_UTM).Add(UTMCoord.Easting)
                                UTMcalc = False
                            Else
                                UTMcalc = True
                            End If
                        ElseIf sKV.Key = tComp.longi Or sKV.Key = tComp.longi_D Then
                            If Line(sKV.Value).Substring(Line(sKV.Value).IndexOf(Prefs.decSep) + 1).Length < AnzDigit(Convert.ToInt32(MSCX.tUse), CoordID) Then CounterLong += 1
                            If UTMcalc Then
                                If MeasCheck(tComp.longi) And sKV.Key = tComp.longi Then UTMCoord = UTM(InputData(tComp.lati)(tDim) / 60, InputData(sKV.Key)(tDim) / 60)
                                If Not MeasCheck(tComp.longi) And MeasCheck(tComp.longi_D) And sKV.Key = tComp.longi_D Then UTMCoord = UTM(InputData(tComp.lati_D)(tDim), InputData(sKV.Key)(tDim))

                                If Not ZoneChange Then
                                    If tDim > 0 Then
                                        If CalcData(tCompCali.zone_UTM).Last <> UTMCoord.Zone Then
                                            logme(8, False, "The coordinates lie in different UTM Zones. A zone adjustment will be done!")
                                            ZoneChange = True
                                        End If
                                    End If
                                End If
                                CalcData(tCompCali.zone_UTM).Add(UTMCoord.Zone)
                                CalcData(tCompCali.lati_UTM).Add(UTMCoord.Northing)
                                CalcData(tCompCali.longi_UTM).Add(UTMCoord.Easting)
                                UTMcalc = False
                            Else
                                UTMcalc = True
                            End If
                        ElseIf sKV.Key = tComp.trigger Then
                            CalcData(tCompCali.trigger_c).Add(CDbl(Line(sKV.Value)))
                        ElseIf sKV.Key = tComp.beta_ic Then
                            If InputData(sKV.Key)(tDim) > 360 Or InputData(sKV.Key)(tDim) < -360 Then
                                Throw New Exception("The beta_ic angle is higher then +-360°! This is not a valid angle. Please correct.")
                            End If
                        End If
                    Next sKV

                    If (vehicleX.OldVehicle) Then
                        CalcData(tCompCali.tq_l_c).Add(InputData(tComp.tq_l)(tDim))
                        CalcData(tCompCali.tq_r_c).Add(InputData(tComp.tq_r)(tDim))
                    Else
                        CalcData(tCompCali.tq_l_c).Add(InputData(tComp.tq_l)(tDim) - vehicleX.tqDriftLeft * (InputData(tComp.t)(tDim) - vehicleX.tDriftZero) / (vehicleX.tDriftCheck - vehicleX.tDriftZero))
                        CalcData(tCompCali.tq_r_c).Add(InputData(tComp.tq_r)(tDim) - vehicleX.tqDriftRight * (InputData(tComp.t)(tDim) - vehicleX.tDriftZero) / (vehicleX.tDriftCheck - vehicleX.tDriftZero))
                    End If

                    If valid_set Then
                        If tDim = 0 Then
                            InputData.Add(tComp.user_valid, New List(Of Double))
                            InputData(tComp.user_valid).Add(CDbl(1))
                        Else
                            InputData(tComp.user_valid).Add(CDbl(1))
                        End If
                    End If
                    If valid_set2 Then
                        If tDim = 0 Then
                            InputData.Add(tComp.t_ground, New List(Of Double))
                            InputData(tComp.t_ground).Add(CDbl(20))
                        Else
                            InputData(tComp.t_ground).Add(CDbl(20))
                        End If
                    End If

                    ' Add the additional data to the undefined values
                    For Each sKVUndef In SpaltenUndef
                        InputUndefData(sKVUndef.Key).Add(CDbl(Line(sKVUndef.Value)))
                    Next
                Loop

                If Datafile Is Job.low1_fpath Or Datafile Is Job.low2_fpath Then
                    Dim initTimestamp = CalcData(tComp.t).First()
                    Dim lastTimestamp = CalcData(tComp.t).Last()
                    If (lastTimestamp - initTimestamp) > LowSpeedTestMaxDuration Then
                        logme(9, False, "The maximum time for the low speed test shall not exceed 25 minutes.")
                        Throw New Exception("Low speed test shall not exceed 25 minutes.")
                    End If
                End If
            Catch ex As Exception
                Throw New Exception(format("Exception while reading file({0}), line({1}) due to: {2}!: ", Datafile, tDim + 1, ex.Message), ex)
            End Try

            ' Check coordinate digits
            If CounterLat > 0 Then
                If Job.mode = 1 Then
                    Throw New Exception(format("The latitude coordinates from file ({0}) have not enough digits after the decimal separator (minimum digits are ({1})). Number of fails: ({2})!", Datafile, AnzDigit(Convert.ToInt32(MSCX.tUse), CoordID), CounterLat))
                Else
                    logme(8, False, format("The latitude coordinates from file ({0}) have not enough digits after the decimal separator (minimum digits are ({1})). Number of fails: ({2})!", Datafile, AnzDigit(Convert.ToInt32(MSCX.tUse), CoordID), CounterLat))
                End If
            End If
            If CounterLong > 0 Then
                If Job.mode = 1 Then
                    Throw New Exception(format("The longitude coordinates from file ({0}) have not enough digits after the decimal separator (minimum digits are ({1})). Number of fails: ({2})!", Datafile, AnzDigit(Convert.ToInt32(MSCX.tUse), CoordID), CounterLong))
                Else
                    logme(8, False, format("The longitude coordinates from file ({0}) have not enough digits after the decimal separator (minimum digits are ({1})). Number of fails: ({2})!", Datafile, AnzDigit(Convert.ToInt32(MSCX.tUse), CoordID), CounterLong))
                End If
            End If

            ' Make the zone adjustment for the UTM coords
            Do While ZoneChange
                Zone1CentralMeridian = Zone1CentralMeridian + 5
                For i = 0 To CalcData(tCompCali.lati_UTM).Count - 1
                    If MeasCheck(tComp.lati) Then UTMCoord = UTM(InputData(tComp.lati)(i) / 60, InputData(tComp.longi)(i) / 60)
                    If Not MeasCheck(tComp.lati) And MeasCheck(tComp.lati_D) Then UTMCoord = UTM(InputData(tComp.lati_D)(i), InputData(tComp.longi_D)(i))
                    If i > 0 Then
                        If CalcData(tCompCali.zone_UTM)(i - 1) <> UTMCoord.Zone Then
                            Exit For
                        End If
                    End If
                    If i = CalcData(tCompCali.lati_UTM).Count - 1 Then ZoneChange = False
                    CalcData(tCompCali.zone_UTM)(i) = UTMCoord.Zone
                    CalcData(tCompCali.lati_UTM)(i) = UTMCoord.Northing
                    CalcData(tCompCali.longi_UTM)(i) = UTMCoord.Easting
                Next i
                If Zone1CentralMeridian > 180 Then
                    Throw New Exception("The UTM zone adjustment is not possible because the data lie to far away from each other to fit into one UTM stripe")
                End If
            Loop

            ' Change the decimal separator back
            If DemoDataF Then
                System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator = Prefs.decSep
                DemoDataF = False
            End If

            'Developer export of input data converted from MM.MM to UTM
            'fOuttest(Datafile)
        End Using
    End Sub
End Module
