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

Module output
    ' Function for the output of the calculated datas
    Function fOutDataCalc1Hz(ByVal Datafile As String, ByVal calibration As Boolean) As Boolean
        ' Declaration
        Dim i As Integer
        Dim NameOutFile, key As String
        Using FileOut As New cFile_V3
            Dim first As Boolean
            Dim s As New System.Text.StringBuilder

            ' Initialise
            first = True
            ErgEntriesI = New Dictionary(Of tComp, CResult)
            ErgEntryListI = New List(Of tComp)
            ErgEntriesIU = New Dictionary(Of String, CResult)
            ErgEntryListIU = New List(Of String)
            ErgEntriesC = New Dictionary(Of tCompCali, CResult)
            ErgEntryListC = New List(Of tCompCali)
            GenErgOutData(calibration)
            If Crt.hz_out = 1 Then
                If InputUndefData.Count > 0 Then ConvTo1Hz(CalcData(tCompCali.t), InputUndefData)
                ConvTo1Hz(CalcData(tCompCali.t), InputData)
                ConvTo1Hz(CalcData)
            End If

            ' Exit function if error is detected
            If BWorker.CancellationPending And FileBlock Then
                Return False
            End If

            ' Write on GUI
            logme(5, False, "Writing output-file (*.csv)")

            ' Generate the file name
            NameOutFile = ""
            Select Case Crt.hz_out
                Case 1
                    NameOutFile = OutFolder & fName(Datafile, False) & "_1Hz.csv"
                Case 100
                    NameOutFile = OutFolder & fName(Datafile, False) & "_100Hz.csv"
            End Select

            ' Anlegen der Datei
            FileOut.OpenWrite(NameOutFile, , False)

            ' Filekopf
            FileOut.WriteLine("# Resultfile Programm " & AppName & " " & AppVers & " Comp " & AppDate)
            FileOut.WriteLine("# Datafile: " & Prefs.listSep & Datafile)
            FileOut.WriteLine("#")

            ' Write the head and units
            If ErgEntryListI.Count > 0 Then s.Append(ErgHead("InputData"))
            If ErgEntryListIU.Count > 0 Then s.Append(Prefs.listSep & ErgHead("InputUndefData"))
            If ErgEntryListC.Count > 0 Then s.Append(Prefs.listSep & ErgHead("CalcData"))
            FileOut.WriteLine(s.ToString)
            s.Clear()

            ' Write the data
            For i = 0 To InputData.Item(tComp.t).Count - 1
                For Each key In ErgEntryListI
                    If Not first Then s.Append(Prefs.listSep)
                    s.Append(InputData(key)(i))
                    first = False
                Next
                For Each key In ErgEntryListIU
                    If Not first Then s.Append(Prefs.listSep)
                    s.Append(InputUndefData(key)(i))
                    first = False
                Next
                For Each key In ErgEntryListC
                    If Not first Then s.Append(Prefs.listSep)
                    s.Append(CalcData(key)(i))
                    first = False
                Next
                FileOut.WriteLine(s.ToString)
                s.Clear()
                first = True
            Next i

        End Using

        ' Ausgabe bei blockierter Datei
        If BWorker.CancellationPending And FileBlock Then
            logme(9, False, "Can´t write in file " & NameOutFile & ". File is blocked by another process!")
        End If

        Return True
    End Function

    ' Function for the output of the result data
    Function fOutCalcRes(ByVal calibration As Boolean) As Boolean
        Dim coasting_fpaths() = Job.coasting_fpaths
        Dim NameOutFile, key As String
        Using FileOut As New cFile_V3
            Dim first As Boolean
            Dim s As New System.Text.StringBuilder

            ' Initialise
            first = True
            ErgEntriesR = New Dictionary(Of tCompErg, CResult)
            ErgEntryListR = New List(Of tCompErg)
            ErgEntriesRU = New Dictionary(Of String, CResult)
            ErgEntryListRU = New List(Of String)
            GenErgOutRes(calibration)

            ' Exit function if error is detected
            If BWorker.CancellationPending And FileBlock Then
                Return False
            End If

            ' Write on GUI
            logme(5, False, "Writing result-file (*.csv)")

            ' Generate the file name
            NameOutFile = OutFolder & fName(JobFile, False) & "_MS_CAL.csv"
            If Not calibration Then
                NameOutFile = OutFolder & fName(JobFile, False) & "_MS_MEAS.csv"
            End If

            ' Anlegen der Datei
            FileOut.OpenWrite(NameOutFile, , False)

            ' Filekopf
            FileOut.WriteLine("# Resultfile Programm " & AppName & " " & AppVers & " Comp " & AppDate)
            If calibration Then
                FileOut.WriteLine("# Datafile: " & Prefs.listSep & Job.calib_run_fpath)
            Else
                FileOut.WriteLine("# Datafile HS: " & Prefs.listSep & coasting_fpaths(0))
                FileOut.WriteLine("# Datafile LS1: " & Prefs.listSep & coasting_fpaths(1))
                FileOut.WriteLine("# Datafile LS2: " & Prefs.listSep & coasting_fpaths(2))
            End If
            FileOut.WriteLine("#")
            FileOut.WriteLine("# Results")
            FileOut.WriteLine("# fv_veh:" & Prefs.listSep & Job.fv_veh)
            FileOut.WriteLine("# fv_pe:" & Prefs.listSep & Job.fv_pe)
            FileOut.WriteLine("# beta_ame:" & Prefs.listSep & Job.beta_ame)
            FileOut.WriteLine("#")

            ' Write the head and units
            FileOut.WriteLine(ErgHead("ErgValues") & Prefs.listSep & ErgHead("ErgValuesUndef"))

            ' Write the data
            If calibration Then
                For i = 0 To ErgValues.Item(tCompErg.SecID).Count - 1
                    For Each key In ErgEntryListR
                        If Not first Then s.Append(Prefs.listSep)
                        s.Append(ErgValues(key)(i))
                        first = False
                    Next
                    For Each key In ErgEntryListRU
                        If Not first Then s.Append(Prefs.listSep)
                        s.Append(ErgValuesUndef(key)(i))
                        first = False
                    Next
                    FileOut.WriteLine(s.ToString)
                    s.Clear()
                    first = True
                Next i
            Else
                For i = 0 To ErgValuesComp.Item(tCompErg.SecID).Count - 1
                    For Each key In ErgEntryListR
                        If Not first Then s.Append(Prefs.listSep)
                        s.Append(ErgValuesComp(key)(i))
                        first = False
                    Next
                    For Each key In ErgEntryListRU
                        If Not first Then s.Append(Prefs.listSep)
                        s.Append(ErgValuesUndefComp(key)(i))
                        first = False
                    Next
                    FileOut.WriteLine(s.ToString)
                    s.Clear()
                    first = True
                Next i
            End If

        End Using


        ' Ausgabe bei blockierter Datei
        If BWorker.CancellationPending And FileBlock Then
            logme(9, False, "Can´t write in file " & NameOutFile & ". File is blocked by another process!")
        End If

        Return True
    End Function

    ' Function for the output of the result data of the regression
    Function fOutCalcResReg() As Boolean
        Dim coasting_fpaths() = Job.coasting_fpaths
        Dim NameOutFile, key As String
        Using FileOut As New cFile_V3
            Dim first As Boolean
            Dim s As New System.Text.StringBuilder

            ' Initialise
            first = True
            ErgEntriesReg = New Dictionary(Of tCompErgReg, CResult)
            ErgEntryListReg = New List(Of tCompErgReg)
            GenErgOutResReg()

            ' Exit function if error is detected
            If BWorker.CancellationPending And FileBlock Then
                Return False
            End If

            ' Write on GUI
            logme(5, False, "Writing result-file (*.csv)")

            ' Generate the file name
            NameOutFile = OutFolder & fName(JobFile, False) & "_AirDrag.csv"

            ' Anlegen der Datei
            FileOut.OpenWrite(NameOutFile, , False)

            ' Filekopf
            FileOut.WriteLine("# Resultfile Programm " & AppName & " " & AppVers & " Comp " & AppDate)
            FileOut.WriteLine("# Datafile LS1: " & Prefs.listSep & coasting_fpaths(1))
            FileOut.WriteLine("# Datafile HS: " & Prefs.listSep & coasting_fpaths(0))
            FileOut.WriteLine("# Datafile LS2: " & Prefs.listSep & coasting_fpaths(2))
            FileOut.WriteLine("#")
            If Job.mode = 1 Then
                FileOut.WriteLine("RESULTS CALCULATED IN DECLARATION MODE")
                FileOut.WriteLine("#")
            End If
            FileOut.WriteLine("# Results")
            FileOut.WriteLine("# fv_veh:" & Prefs.listSep & Job.fv_veh & Prefs.listSep & "[-] calibration factor for vehicle speed")
            FileOut.WriteLine("# fv_pe:" & Prefs.listSep & Job.fv_pe & Prefs.listSep & "[-] calibration factor for air speed (position error)")
            FileOut.WriteLine("# fa_pe:" & Prefs.listSep & Job.fa_pe & Prefs.listSep & "[-] position error correction factor for measured air inflow angle (beta)")
            FileOut.WriteLine("# beta_ame:" & Prefs.listSep & Job.beta_ame & Prefs.listSep & "[°] calibration factor for beta (misalignment)")
            FileOut.WriteLine("# t_amb_LS1:" & Prefs.listSep & Job.t_amb_LS1 & Prefs.listSep & "[°] average ambient temperature during first low speed test")
            FileOut.WriteLine("# v_avg_LS:" & Prefs.listSep & Job.v_avg_LS & Prefs.listSep & "[km/h] average vehicle speed used datasets low speed tests")
            FileOut.WriteLine("# v_avg_HS:" & Prefs.listSep & Job.v_avg_HS & Prefs.listSep & "[km/h] average vehicle speed used datasets high speed test")
            FileOut.WriteLine("# CdxA(ß)_H1:" & Prefs.listSep & Job.CdxAß_H1 & Prefs.listSep & "[m²] average CdxA before yaw angle correction for heading 1")
            FileOut.WriteLine("# beta_H1:" & Prefs.listSep & Job.beta_H1 & Prefs.listSep & "[°] average absolute yaw angle from high speed tests for heading 1")
            FileOut.WriteLine("# CdxA(ß)_H2:" & Prefs.listSep & Job.CdxAß_H2 & Prefs.listSep & "[m²] average CdxA before yaw angle correction for heading 2")
            FileOut.WriteLine("# beta_H2:" & Prefs.listSep & Job.beta_H2 & Prefs.listSep & "[°] average absolute yaw angle from high speed tests for heading 2")
            FileOut.WriteLine("# CdxA(ß):" & Prefs.listSep & Job.CdxAß & Prefs.listSep & "[m²] average CdxA before yaw angle correction")
            FileOut.WriteLine("# beta:" & Prefs.listSep & Job.beta & Prefs.listSep & "[°] average absolute yaw angle from high speed tests")
            FileOut.WriteLine("# delta_CdxA_beta:" & Prefs.listSep & Job.delta_CdxA_beta & Prefs.listSep & "[m²] correction of CdxA for yaw angle")
            FileOut.WriteLine("# CdxA(0)meas:" & Prefs.listSep & Job.CdxA0meas & Prefs.listSep & "[m²] average measured CdxA for zero yaw angle")
            FileOut.WriteLine("# delta_CdxA_height:" & Prefs.listSep & Job.delta_CdxA_height & Prefs.listSep & "[m²] correction of CdxA to reference vehicle height")
            FileOut.WriteLine("# delta_CdxA_anemo:" & Prefs.listSep & Crt.delta_CdxA_anemo & Prefs.listSep & "[m²] CdxA influence from anemometer")
            FileOut.WriteLine("# CdxA(0):" & Prefs.listSep & Math.Round(Job.CdxA0, 2, MidpointRounding.AwayFromZero) & Prefs.listSep & "[m²] average CdxA for zero yaw angle")
            FileOut.WriteLine("#")
            FileOut.WriteLine("# Validity criteria:")
            If Job.valid_RRC Then
                FileOut.WriteLine("# RRC:" & Prefs.listSep & "Ok")
            Else
                FileOut.WriteLine("# RRC:" & Prefs.listSep & "Invalid test - maximum deviation of RRCs between low speed tests exceeded")
            End If

            FileOut.WriteLine("#")

            ' Write the head and units
            FileOut.WriteLine(ErgHead("ErgValuesReg"))

            ' Write the data
            For i = 0 To ErgValuesReg.Item(tCompErgReg.SecID).Count - 1
                For Each key In ErgEntryListReg
                    If Not first Then s.Append(Prefs.listSep)
                    s.Append(ErgValuesReg(key)(i))
                    first = False
                Next
                FileOut.WriteLine(s.ToString)
                s.Clear()
                first = True
            Next i
        End Using

        ' Ausgabe bei blockierter Datei
        If BWorker.CancellationPending And FileBlock Then
            logme(9, False, "Can´t write in file " & NameOutFile & ". File is blocked by another process!")
        End If

        Return True
    End Function

    ' Generate the output dictionary (for calculate)
    Private Sub GenErgOutData(Optional ByVal calibration As Boolean = True)
        ' Declaration
        Dim sKV As New KeyValuePair(Of String, List(Of Double))

        ' Input data
        AddToErg(tComp.t, fCompName(tComp.t), fCompUnit(tComp.t), "InputData")
        If KoordSys(0) Then AddToErg(tComp.lati, fCompName(tComp.lati), fCompUnit(tComp.lati), "InputData")
        If KoordSys(0) Then AddToErg(tComp.longi, fCompName(tComp.longi), fCompUnit(tComp.longi), "InputData")
        If KoordSys(1) Then AddToErg(tComp.lati_D, fCompName(tComp.lati_D), fCompUnit(tComp.lati_D), "InputData")
        If KoordSys(1) Then AddToErg(tComp.longi_D, fCompName(tComp.longi_D), fCompUnit(tComp.longi_D), "InputData")
        AddToErg(tComp.hdg, fCompName(tComp.hdg), fCompUnit(tComp.hdg), "InputData")
        AddToErg(tComp.v_veh_GPS, fCompName(tComp.v_veh_GPS), fCompUnit(tComp.v_veh_GPS), "InputData")
        AddToErg(tComp.v_veh_CAN, fCompName(tComp.v_veh_CAN), fCompUnit(tComp.v_veh_CAN), "InputData")
        AddToErg(tComp.vair_ic, fCompName(tComp.vair_ic), fCompUnit(tComp.vair_ic), "InputData")
        AddToErg(tComp.beta_ic, fCompName(tComp.beta_ic), fCompUnit(tComp.beta_ic), "InputData")
        If InputData.ContainsKey(tComp.n_eng) Then AddToErg(tComp.n_eng, fCompName(tComp.n_eng), fCompUnit(tComp.n_eng), "InputData")
        If InputData.ContainsKey(tComp.n_card) Then AddToErg(tComp.n_card, fCompName(tComp.n_card), fCompUnit(tComp.n_card), "InputData")
        If InputData.ContainsKey(tComp.n_wheel_ave) Then AddToErg(tComp.n_wheel_ave, fCompName(tComp.n_wheel_ave), fCompUnit(tComp.n_wheel_ave), "InputData")
        If InputData.ContainsKey(tComp.n_EM) Then AddToErg(tComp.n_EM, fCompName(tComp.n_EM), fCompUnit(tComp.n_EM), "InputData")
        AddToErg(tComp.tq_l, fCompName(tComp.tq_l), fCompUnit(tComp.tq_l), "InputData")
        AddToErg(tComp.tq_r, fCompName(tComp.tq_r), fCompUnit(tComp.tq_r), "InputData")
        If OptPar(1) Then AddToErg(tComp.t_ground, fCompName(tComp.t_ground), fCompUnit(tComp.t_ground), "InputData")
        AddToErg(tComp.t_amb_veh, fCompName(tComp.t_amb_veh), fCompUnit(tComp.t_amb_veh), "InputData")
        ' Write optional parameters
        If OptPar(0) Then AddToErg(tComp.trigger, fCompName(tComp.trigger), fCompUnit(tComp.trigger), "InputData")

        ' Undefined input data
        For Each sKV In InputUndefData
            AddToErg(sKV.Key, sKV.Key, "", "InputUndefData")
        Next

        ' Calculated data
        AddToErg(tCompCali.zone_UTM, fCompName(tCompCali.zone_UTM), fCompUnit(tCompCali.zone_UTM), "CalcData")
        AddToErg(tCompCali.lati_UTM, fCompName(tCompCali.lati_UTM), fCompUnit(tCompCali.lati_UTM), "CalcData")
        AddToErg(tCompCali.longi_UTM, fCompName(tCompCali.longi_UTM), fCompUnit(tCompCali.longi_UTM), "CalcData")
        AddToErg(tCompCali.SecID, fCompName(tCompCali.SecID), fCompUnit(tCompCali.SecID), "CalcData")
        AddToErg(tCompCali.DirID, fCompName(tCompCali.DirID), fCompUnit(tCompCali.DirID), "CalcData")
        AddToErg(tCompCali.lati_root, fCompName(tCompCali.lati_root), fCompUnit(tCompCali.lati_root), "CalcData")
        AddToErg(tCompCali.longi_root, fCompName(tCompCali.longi_root), fCompUnit(tCompCali.longi_root), "CalcData")
        AddToErg(tCompCali.dist_root, fCompName(tCompCali.dist_root), fCompUnit(tCompCali.dist_root), "CalcData")
        AddToErg(tCompCali.slope_deg, fCompName(tCompCali.slope_deg), fCompUnit(tCompCali.slope_deg), "CalcData")
        AddToErg(tCompCali.alt, fCompName(tCompCali.alt), fCompUnit(tCompCali.alt), "CalcData")
        AddToErg(tCompCali.v_veh_c, fCompName(tCompCali.v_veh_c), fCompUnit(tCompCali.v_veh_c), "CalcData")
        AddToErg(tCompCali.dist, fCompName(tCompCali.dist), fCompUnit(tCompCali.dist), "CalcData")
        AddToErg(tCompCali.vair_uf, fCompName(tCompCali.vair_uf), fCompUnit(tCompCali.vair_uf), "CalcData")
        AddToErg(tCompCali.vair_c, fCompName(tCompCali.vair_c), fCompUnit(tCompCali.vair_c), "CalcData")
        AddToErg(tCompCali.beta_uf, fCompName(tCompCali.beta_uf), fCompUnit(tCompCali.beta_uf), "CalcData")
        AddToErg(tCompCali.beta_c, fCompName(tCompCali.beta_c), fCompUnit(tCompCali.beta_c), "CalcData")
        AddToErg(tCompCali.vwind_ha, fCompName(tCompCali.vwind_ha), fCompUnit(tCompCali.vwind_ha), "CalcData")
        AddToErg(tCompCali.vwind_c, fCompName(tCompCali.vwind_c), fCompUnit(tCompCali.vwind_c), "CalcData")
        AddToErg(tCompCali.vwind_1s, fCompName(tCompCali.vwind_1s), fCompUnit(tCompCali.vwind_1s), "CalcData")

        If Not calibration Then
            AddToErg(tCompCali.omega_wh, fCompName(tCompCali.omega_wh), fCompUnit(tCompCali.omega_wh), "CalcData")
            AddToErg(tCompCali.tq_l_c, fCompName(tCompCali.tq_l_c), fCompUnit(tCompCali.tq_l_c), "CalcData")
            AddToErg(tCompCali.tq_r_c, fCompName(tCompCali.tq_r_c), fCompUnit(tCompCali.tq_r_c), "CalcData")
            AddToErg(tCompCali.tq_sum, fCompName(tCompCali.tq_sum), fCompUnit(tCompCali.tq_sum), "CalcData")
            AddToErg(tCompCali.tq_sum_1s, fCompName(tCompCali.tq_sum_1s), fCompUnit(tCompCali.tq_sum_1s), "CalcData")
            AddToErg(tCompCali.tq_sum_float, fCompName(tCompCali.tq_sum_float), fCompUnit(tCompCali.tq_sum_float), "CalcData")
            AddToErg(tCompCali.t_float, fCompName(tCompCali.t_float), fCompUnit(tCompCali.t_float), "CalcData")
            AddToErg(tCompCali.F_trac, fCompName(tCompCali.F_trac), fCompUnit(tCompCali.F_trac), "CalcData")
            AddToErg(tCompCali.F_acc, fCompName(tCompCali.F_acc), fCompUnit(tCompCali.F_acc), "CalcData")
            AddToErg(tCompCali.F_grd, fCompName(tCompCali.F_grd), fCompUnit(tCompCali.F_grd), "CalcData")
            AddToErg(tCompCali.F_res, fCompName(tCompCali.F_res), fCompUnit(tCompCali.F_res), "CalcData")
            AddToErg(tCompCali.v_veh_1s, fCompName(tCompCali.v_veh_1s), fCompUnit(tCompCali.v_veh_1s), "CalcData")
            AddToErg(tCompCali.v_veh_acc, fCompName(tCompCali.v_veh_acc), fCompUnit(tCompCali.v_veh_acc), "CalcData")
            AddToErg(tCompCali.a_veh_avg, fCompName(tCompCali.a_veh_avg), fCompUnit(tCompCali.a_veh_avg), "CalcData")
            AddToErg(tCompCali.v_veh_float, fCompName(tCompCali.v_veh_float), fCompUnit(tCompCali.v_veh_float), "CalcData")
            AddToErg(tCompCali.t_amp_stat, fCompName(tCompCali.t_amp_stat), fCompUnit(tCompCali.t_amp_stat), "CalcData")
            AddToErg(tCompCali.p_amp_stat, fCompName(tCompCali.p_amp_stat), fCompUnit(tCompCali.p_amp_stat), "CalcData")
            AddToErg(tCompCali.rh_stat, fCompName(tCompCali.rh_stat), fCompUnit(tCompCali.rh_stat), "CalcData")
            AddToErg(tCompCali.vair_c_sq, fCompName(tCompCali.vair_c_sq), fCompUnit(tCompCali.vair_c_sq), "CalcData")
        End If
    End Sub

    ' Generate the output dictionary (for results)
    Private Sub GenErgOutRes(Optional ByVal calibration As Boolean = True)
        ' Result data
        AddToErg(tCompErg.SecID, fCompName(tCompErg.SecID), fCompUnit(tCompErg.SecID), "ErgValues")
        AddToErg(tCompErg.DirID, fCompName(tCompErg.DirID), fCompUnit(tCompErg.DirID), "ErgValues")
        If Not calibration Then
            AddToErg(tCompErg.RunID, fCompName(tCompErg.RunID), fCompUnit(tCompErg.RunID), "ErgValues")
            AddToErg(tCompErg.HeadID, fCompName(tCompErg.HeadID), fCompUnit(tCompErg.HeadID), "ErgValues")
        End If
        AddToErg(tCompErg.delta_t, fCompName(tCompErg.delta_t), fCompUnit(tCompErg.delta_t), "ErgValues")
        AddToErg(tCompErg.s_MSC, fCompName(tCompErg.s_MSC), fCompUnit(tCompErg.s_MSC), "ErgValues")
        AddToErg(tCompErg.dist, fCompName(tCompErg.dist), fCompUnit(tCompErg.dist), "ErgValues")
        AddToErg(tCompErg.v_MSC, fCompName(tCompErg.v_MSC), fCompUnit(tCompErg.v_MSC), "ErgValues")
        AddToErg(tCompErg.v_MSC_GPS, fCompName(tCompErg.v_MSC_GPS), fCompUnit(tCompErg.v_MSC_GPS), "ErgValues")
        AddToErg(tCompErg.v_veh_CAN, fCompName(tCompErg.v_veh_CAN), fCompUnit(tCompErg.v_veh_CAN), "ErgValues")
        AddToErg(tCompErg.v_veh, fCompName(tCompErg.v_veh), fCompUnit(tCompErg.v_veh), "ErgValues")
        AddToErg(tCompErg.vair_ic, fCompName(tCompErg.vair_ic), fCompUnit(tCompErg.vair_ic), "ErgValues")
        AddToErg(tCompErg.vair_uf, fCompName(tCompErg.vair_uf), fCompUnit(tCompErg.vair_uf), "ErgValues")
        AddToErg(tCompErg.beta_ic, fCompName(tCompErg.beta_ic), fCompUnit(tCompErg.beta_ic), "ErgValues")
        AddToErg(tCompErg.beta_uf, fCompName(tCompErg.beta_uf), fCompUnit(tCompErg.beta_uf), "ErgValues")
        AddToErg(tCompErg.valid, fCompName(tCompErg.valid), fCompUnit(tCompErg.valid), "ErgValues")
        AddToErg(tCompErg.used, fCompName(tCompErg.used), fCompUnit(tCompErg.used), "ErgValues")

        If calibration Then
            AddToErg(tCompErg.val_User, fCompName(tCompErg.val_User), fCompUnit(tCompErg.val_User), "ErgValues")
            AddToErg(tCompErg.val_vWind, fCompName(tCompErg.val_vWind), fCompUnit(tCompErg.val_vWind), "ErgValues")
            AddToErg(tCompErg.val_vWind_1s, fCompName(tCompErg.val_vWind_1s), fCompUnit(tCompErg.val_vWind_1s), "ErgValues")
            AddToErg(tCompErg.val_beta, fCompName(tCompErg.val_beta), fCompUnit(tCompErg.val_beta), "ErgValues")
            AddToErg(tCompErg.val_vVeh_avg, fCompName(tCompErg.val_vVeh_avg), fCompUnit(tCompErg.val_vVeh_avg), "ErgValues")
            AddToErg(tCompErg.val_vVeh_1s, fCompName(tCompErg.val_vVeh_1s), fCompUnit(tCompErg.val_vVeh_1s), "ErgValues")
        End If

        If Not calibration Then
            AddToErg(tCompErg.val_User, fCompName(tCompErg.val_User), fCompUnit(tCompErg.val_User), "ErgValues")
            AddToErg(tCompErg.val_vVeh_avg, fCompName(tCompErg.val_vVeh_avg), fCompUnit(tCompErg.val_vVeh_avg), "ErgValues")
            AddToErg(tCompErg.val_vVeh_f, fCompName(tCompErg.val_vVeh_f), fCompUnit(tCompErg.val_vVeh_f), "ErgValues")
            AddToErg(tCompErg.val_vVeh_1s, fCompName(tCompErg.val_vVeh_1s), fCompUnit(tCompErg.val_vVeh_1s), "ErgValues")
            AddToErg(tCompErg.val_vWind, fCompName(tCompErg.val_vWind), fCompUnit(tCompErg.val_vWind), "ErgValues")
            AddToErg(tCompErg.val_vWind_1s, fCompName(tCompErg.val_vWind_1s), fCompUnit(tCompErg.val_vWind_1s), "ErgValues")
            AddToErg(tCompErg.val_tq_f, fCompName(tCompErg.val_tq_f), fCompUnit(tCompErg.val_tq_f), "ErgValues")
            AddToErg(tCompErg.val_tq_1s, fCompName(tCompErg.val_tq_1s), fCompUnit(tCompErg.val_tq_1s), "ErgValues")
            AddToErg(tCompErg.val_beta, fCompName(tCompErg.val_beta), fCompUnit(tCompErg.val_beta), "ErgValues")
            AddToErg(tCompErg.val_n_eng, fCompName(tCompErg.val_n_eng), fCompUnit(tCompErg.val_n_eng), "ErgValues")
            AddToErg(tCompErg.val_dist, fCompName(tCompErg.val_dist), fCompUnit(tCompErg.val_dist), "ErgValues")
            AddToErg(tCompErg.val_t_amb, fCompName(tCompErg.val_t_amb), fCompUnit(tCompErg.val_t_amb), "ErgValues")
            AddToErg(tCompErg.val_t_ground, fCompName(tCompErg.val_t_ground), fCompUnit(tCompErg.val_t_ground), "ErgValues")
        End If

        AddToErg(tCompErg.vair, fCompName(tCompErg.vair), fCompUnit(tCompErg.vair), "ErgValues")
        AddToErg(tCompErg.v_wind_avg, fCompName(tCompErg.v_wind_avg), fCompUnit(tCompErg.v_wind_avg), "ErgValues")
        AddToErg(tCompErg.v_wind_1s, fCompName(tCompErg.v_wind_1s), fCompUnit(tCompErg.v_wind_1s), "ErgValues")
        AddToErg(tCompErg.v_wind_1s_max, fCompName(tCompErg.v_wind_1s_max), fCompUnit(tCompErg.v_wind_1s_max), "ErgValues")
        AddToErg(tCompErg.beta_avg, fCompName(tCompErg.beta_avg), fCompUnit(tCompErg.beta_avg), "ErgValues")

        If Not calibration Then
            AddToErg(tCompErg.beta_abs, fCompName(tCompErg.beta_abs), fCompUnit(tCompErg.beta_abs), "ErgValues")
            AddToErg(tCompErg.v_air_sq, fCompName(tCompErg.v_air_sq), fCompUnit(tCompErg.v_air_sq), "ErgValues")
            AddToErg(tCompErg.n_ec, fCompName(tCompErg.n_ec), fCompUnit(tCompErg.n_ec), "ErgValues")
            AddToErg(tCompErg.n_ec_1s_max, fCompName(tCompErg.n_ec_1s_max), fCompUnit(tCompErg.n_ec_1s_max), "ErgValues")
            AddToErg(tCompErg.n_ec_1s_min, fCompName(tCompErg.n_ec_1s_min), fCompUnit(tCompErg.n_ec_1s_min), "ErgValues")
            AddToErg(tCompErg.n_ec_float_max, fCompName(tCompErg.n_ec_float_max), fCompUnit(tCompErg.n_ec_float_max), "ErgValues")
            AddToErg(tCompErg.n_ec_float_min, fCompName(tCompErg.n_ec_float_min), fCompUnit(tCompErg.n_ec_float_min), "ErgValues")
            AddToErg(tCompErg.r_dyn, fCompName(tCompErg.r_dyn), fCompUnit(tCompErg.r_dyn), "ErgValues")
            AddToErg(tCompErg.tq_sum, fCompName(tCompErg.tq_sum), fCompUnit(tCompErg.tq_sum), "ErgValues")
            AddToErg(tCompErg.tq_sum_1s, fCompName(tCompErg.tq_sum_1s), fCompUnit(tCompErg.tq_sum_1s), "ErgValues")
            AddToErg(tCompErg.tq_sum_1s_max, fCompName(tCompErg.tq_sum_1s_max), fCompUnit(tCompErg.tq_sum_1s_max), "ErgValues")
            AddToErg(tCompErg.tq_sum_1s_min, fCompName(tCompErg.tq_sum_1s_min), fCompUnit(tCompErg.tq_sum_1s_min), "ErgValues")
            AddToErg(tCompErg.tq_sum_float, fCompName(tCompErg.tq_sum_float), fCompUnit(tCompErg.tq_sum_float), "ErgValues")
            AddToErg(tCompErg.tq_sum_float_max, fCompName(tCompErg.tq_sum_float_max), fCompUnit(tCompErg.tq_sum_float_max), "ErgValues")
            AddToErg(tCompErg.tq_sum_float_min, fCompName(tCompErg.tq_sum_float_min), fCompUnit(tCompErg.tq_sum_float_min), "ErgValues")
            AddToErg(tCompErg.tq_grd, fCompName(tCompErg.tq_grd), fCompUnit(tCompErg.tq_grd), "ErgValues")
            AddToErg(tCompErg.t_float, fCompName(tCompErg.t_float), fCompUnit(tCompErg.t_float), "ErgValues")
            AddToErg(tCompErg.F_trac, fCompName(tCompErg.F_trac), fCompUnit(tCompErg.F_trac), "ErgValues")
            AddToErg(tCompErg.F_acc, fCompName(tCompErg.F_acc), fCompUnit(tCompErg.F_acc), "ErgValues")
            AddToErg(tCompErg.F_grd, fCompName(tCompErg.F_grd), fCompUnit(tCompErg.F_grd), "ErgValues")
            AddToErg(tCompErg.F_res, fCompName(tCompErg.F_res), fCompUnit(tCompErg.F_res), "ErgValues")
            AddToErg(tCompErg.F_res_ref, fCompName(tCompErg.F_res_ref), fCompUnit(tCompErg.F_res_ref), "ErgValues")
            AddToErg(tCompErg.v_veh_1s, fCompName(tCompErg.v_veh_1s), fCompUnit(tCompErg.v_veh_1s), "ErgValues")
            AddToErg(tCompErg.v_veh_1s_max, fCompName(tCompErg.v_veh_1s_max), fCompUnit(tCompErg.v_veh_1s_max), "ErgValues")
            AddToErg(tCompErg.v_veh_1s_min, fCompName(tCompErg.v_veh_1s_min), fCompUnit(tCompErg.v_veh_1s_min), "ErgValues")
            AddToErg(tCompErg.v_veh_acc, fCompName(tCompErg.v_veh_acc), fCompUnit(tCompErg.v_veh_acc), "ErgValues")
            AddToErg(tCompErg.a_veh_avg, fCompName(tCompErg.a_veh_avg), fCompUnit(tCompErg.a_veh_avg), "ErgValues")
            AddToErg(tCompErg.v_veh_float, fCompName(tCompErg.v_veh_float), fCompUnit(tCompErg.v_veh_float), "ErgValues")
            AddToErg(tCompErg.v_veh_float_max, fCompName(tCompErg.v_veh_float_max), fCompUnit(tCompErg.v_veh_float_max), "ErgValues")
            AddToErg(tCompErg.v_veh_float_min, fCompName(tCompErg.v_veh_float_min), fCompUnit(tCompErg.v_veh_float_min), "ErgValues")
            AddToErg(tCompErg.t_ground, fCompName(tCompErg.t_ground), fCompUnit(tCompErg.t_ground), "ErgValues")
            AddToErg(tCompErg.t_amb_veh, fCompName(tCompErg.t_amb_veh), fCompUnit(tCompErg.t_amb_veh), "ErgValues")
            AddToErg(tCompErg.t_amb_stat, fCompName(tCompErg.t_amb_stat), fCompUnit(tCompErg.t_amb_stat), "ErgValues")
            AddToErg(tCompErg.p_amb_stat, fCompName(tCompErg.p_amb_stat), fCompUnit(tCompErg.p_amb_stat), "ErgValues")
            AddToErg(tCompErg.rh_stat, fCompName(tCompErg.rh_stat), fCompUnit(tCompErg.rh_stat), "ErgValues")
            AddToErg(tCompErg.vp_H2O, fCompName(tCompErg.vp_H2O), fCompUnit(tCompErg.vp_H2O), "ErgValues")
            AddToErg(tCompErg.rho_air, fCompName(tCompErg.rho_air), fCompUnit(tCompErg.rho_air), "ErgValues")
            AddToErg(tCompErg.CdxAß_singleDS, fCompName(tCompErg.CdxAß_singleDS), fCompUnit(tCompErg.CdxAß_singleDS), "ErgValues")
        End If

        ' Undefined input data
        If calibration Then
            For Each sKV In InputUndefData
                AddToErg(sKV.Key, sKV.Key, "", "ErgValuesUndef")
            Next
        Else
            For Each sKV In ErgValuesUndefComp
                AddToErg(sKV.Key, sKV.Key, "", "ErgValuesUndef")
            Next
        End If
    End Sub

    ' Generate the output dictionary (for regression results)
    Private Sub GenErgOutResReg()
        ' Result data
        AddToErg(tCompErgReg.SecID, fCompName(tCompErgReg.SecID), fCompUnit(tCompErgReg.SecID), "ErgValuesReg")
        AddToErg(tCompErgReg.DirID, fCompName(tCompErgReg.DirID), fCompUnit(tCompErgReg.DirID), "ErgValuesReg")
        AddToErg(tCompErgReg.HeadID, fCompName(tCompErgReg.HeadID), fCompUnit(tCompErgReg.HeadID), "ErgValuesReg")
        AddToErg(tCompErgReg.NumUsed, fCompName(tCompErgReg.NumUsed), fCompUnit(tCompErgReg.NumUsed), "ErgValuesReg")
        AddToErg(tCompErgReg.F0_singleMS, fCompName(tCompErgReg.F0_singleMS), fCompUnit(tCompErgReg.F0_singleMS), "ErgValuesReg")
        AddToErg(tCompErgReg.F0_singleMS_LS1, fCompName(tCompErgReg.F0_singleMS_LS1), fCompUnit(tCompErgReg.F0_singleMS_LS1), "ErgValuesReg")
        AddToErg(tCompErgReg.F0_singleMS_LS2, fCompName(tCompErgReg.F0_singleMS_LS2), fCompUnit(tCompErgReg.F0_singleMS_LS2), "ErgValuesReg")
        AddToErg(tCompErgReg.CdxAß_ave_singleMS, fCompName(tCompErgReg.CdxAß_ave_singleMS), fCompUnit(tCompErgReg.CdxAß_ave_singleMS), "ErgValuesReg")
        AddToErg(tCompErgReg.CdxA0_singleMS, fCompName(tCompErgReg.CdxA0_singleMS), fCompUnit(tCompErgReg.CdxA0_singleMS), "ErgValuesReg")
        AddToErg(tCompErgReg.delta_CdxA_singleMS, fCompName(tCompErgReg.delta_CdxA_singleMS), fCompUnit(tCompErgReg.delta_CdxA_singleMS), "ErgValuesReg")
        AddToErg(tCompErgReg.beta_ave_singleMS, fCompName(tCompErgReg.beta_ave_singleMS), fCompUnit(tCompErgReg.beta_ave_singleMS), "ErgValuesReg")
        AddToErg(tCompErgReg.RRC_singleMS, fCompName(tCompErgReg.RRC_singleMS), fCompUnit(tCompErgReg.RRC_singleMS), "ErgValuesReg")
        AddToErg(tCompErgReg.RRC_singleMS_LS1, fCompName(tCompErgReg.RRC_singleMS_LS1), fCompUnit(tCompErgReg.RRC_singleMS_LS1), "ErgValuesReg")
        AddToErg(tCompErgReg.RRC_singleMS_LS2, fCompName(tCompErgReg.RRC_singleMS_LS2), fCompUnit(tCompErgReg.RRC_singleMS_LS2), "ErgValuesReg")
        AddToErg(tCompErgReg.valid_RRC, fCompName(tCompErgReg.valid_RRC), fCompUnit(tCompErgReg.valid_RRC), "ErgValuesReg")
        AddToErg(tCompErgReg.F2_singleMS, fCompName(tCompErgReg.F2_singleMS), fCompUnit(tCompErgReg.F2_singleMS), "ErgValuesReg")
        AddToErg(tCompErgReg.F2_singleMS_LS1, fCompName(tCompErgReg.F2_singleMS_LS1), fCompUnit(tCompErgReg.F2_singleMS_LS1), "ErgValuesReg")
        AddToErg(tCompErgReg.F2_singleMS_LS2, fCompName(tCompErgReg.F2_singleMS_LS2), fCompUnit(tCompErgReg.F2_singleMS_LS2), "ErgValuesReg")
    End Sub

    ' Generate the output sequence for input data
    Public Sub AddToErg(ByVal EnumID As tComp, ByVal Head As String, ByVal Unit As String, ByVal Dic As String)
        If Not ErgEntriesI.ContainsKey(EnumID) Then
            ErgEntriesI.Add(EnumID, New CResult(Head, Unit, Dic))
            ErgEntryListI.Add(EnumID)
        End If
    End Sub

    ' Generate the output sequence for undefined input data
    Public Sub AddToErg(ByVal EnumID As String, ByVal Head As String, ByVal Unit As String, ByVal Dic As String)
        If Dic = "InputUndefData" Then
            If Not ErgEntriesIU.ContainsKey(EnumID) Then
                ErgEntriesIU.Add(EnumID, New CResult(Head, Unit, Dic))
                ErgEntryListIU.Add(EnumID)
            End If
        ElseIf Dic = "ErgValuesUndef" Then
            If Not ErgEntriesRU.ContainsKey(EnumID) Then
                ErgEntriesRU.Add(EnumID, New CResult(Head, Unit, Dic))
                ErgEntryListRU.Add(EnumID)
            End If
        End If
    End Sub

    ' Generate the output sequence for calculated data
    Public Sub AddToErg(ByVal EnumID As tCompCali, ByVal Head As String, ByVal Unit As String, ByVal Dic As String)
        If Not ErgEntriesC.ContainsKey(EnumID) Then
            ErgEntriesC.Add(EnumID, New CResult(Head, Unit, Dic))
            ErgEntryListC.Add(EnumID)
        End If
    End Sub

    ' Generate the output sequence for calculated data
    Public Sub AddToErg(ByVal EnumID As tCompErg, ByVal Head As String, ByVal Unit As String, ByVal Dic As String)
        If Not ErgEntriesR.ContainsKey(EnumID) Then
            ErgEntriesR.Add(EnumID, New CResult(Head, Unit, Dic))
            ErgEntryListR.Add(EnumID)
        End If
    End Sub

    ' Generate the output sequence for regression calculated data
    Public Sub AddToErg(ByVal EnumID As tCompErgReg, ByVal Head As String, ByVal Unit As String, ByVal Dic As String)
        If Not ErgEntriesReg.ContainsKey(EnumID) Then
            ErgEntriesReg.Add(EnumID, New CResult(Head, Unit, Dic))
            ErgEntryListReg.Add(EnumID)
        End If
    End Sub

    ' Generate the head output string with units
    Public Function ErgHead(ByVal Dic As String) As String
        Dim s As New System.Text.StringBuilder
        Dim key As String
        Dim First As Boolean

        First = True
        If Dic = "InputData" Then
            For Each key In ErgEntryListI
                If Not First Then s.Append(Prefs.listSep)
                s.Append(ErgEntriesI(key).Head + " " + ErgEntriesI(key).Unit)
                First = False
            Next
        ElseIf Dic = "InputUndefData" Then
            For Each key In ErgEntryListIU
                If Not First Then s.Append(Prefs.listSep)
                s.Append(ErgEntriesIU(key).Head + " " + ErgEntriesIU(key).Unit)
                First = False
            Next
        ElseIf Dic = "CalcData" Then
            For Each key In ErgEntryListC
                If Not First Then s.Append(Prefs.listSep)
                s.Append(ErgEntriesC(key).Head + " " + ErgEntriesC(key).Unit)
                First = False
            Next
        ElseIf Dic = "ErgValues" Then
            For Each key In ErgEntryListR
                If Not First Then s.Append(Prefs.listSep)
                s.Append(ErgEntriesR(key).Head + " " + ErgEntriesR(key).Unit)
                First = False
            Next
        ElseIf Dic = "ErgValuesUndef" Then
            For Each key In ErgEntryListRU
                If Not First Then s.Append(Prefs.listSep)
                s.Append(ErgEntriesRU(key).Head + " " + ErgEntriesRU(key).Unit)
                First = False
            Next
        ElseIf Dic = "ErgValuesReg" Then
            For Each key In ErgEntryListReg
                If Not First Then s.Append(Prefs.listSep)
                s.Append(ErgEntriesReg(key).Head + " " + ErgEntriesReg(key).Unit)
                First = False
            Next
        End If

        Return s.ToString
    End Function

    ' Convert the data to 1Hz
    Public Function ConvTo1Hz(ByRef ValuesX As Dictionary(Of tCompCali, List(Of Double))) As Boolean
        ' Declaration
        Dim tMin As Double
        Dim tMax As Double
        Dim tMid As Integer
        Dim Anz As Integer
        Dim z, t0, tI, lauf, laufE, zEnd, pos As Integer
        Dim Time As Double
        Dim tEnd, tIns As Double
        Dim Finish, Sprung As Boolean
        Dim NewValues As Dictionary(Of tCompCali, List(Of Double))
        Dim KV As KeyValuePair(Of tCompCali, List(Of Double))
        Dim KVd As KeyValuePair(Of tCompCali, Double)
        Dim fTime As List(Of Double)
        Dim Summe As Dictionary(Of tCompCali, Double)

        ' Initialise
        Sprung = False
        tI = 0
        fTime = ValuesX(tCompCali.t)

        'Check whether Time is not reversed
        For z = 1 To ValuesX.Item(tCompCali.t).Count - 1
            If fTime(z) < fTime(z - 1) Then
                If Sprung Then
                    logme(9, False, "Time step invalid! t(" & z - 1 & ") = " & fTime(z - 1) & "[s], t(" & z & ") = " & fTime(z) & "[s]")
                    Return False
                Else
                    Sprung = True
                    tI = z
                End If
            End If
        Next z

        'Define Time-range
        t0 = CInt(Math.Round(fTime(0), 0, MidpointRounding.AwayFromZero))
        If Sprung Then tIns = fTime(tI - 1)
        tEnd = fTime(ValuesX.Item(tCompCali.t).Count - 1)

        'Create Output, Total and Num-of-Dictionaries
        NewValues = New Dictionary(Of tCompCali, List(Of Double))
        Summe = New Dictionary(Of tCompCali, Double)

        ' Generate the dictionary folder
        For Each KV In ValuesX
            NewValues.Add(KV.Key, New List(Of Double))
            If KV.Key <> tCompCali.t Then Summe.Add(KV.Key, 0)
        Next

        'Start-values
        tMin = fTime(0)
        tMid = CInt(tMin)
        tMax = tMid + 0.5

        If fTime(0) >= tMax Then
            tMid = tMid + 1
            tMin = tMid - 0.5
            tMax = tMid + 0.5
            t0 = tMid
        End If

        ' Initialise
        Anz = 0
        Finish = False
        If Sprung Then
            laufE = 2
        Else
            laufE = 1
        End If

        For lauf = 1 To laufE
            ' Set the time range (If a jump is detected to the calculation till the jump)
            If Sprung And lauf = 1 Then
                tEnd = tIns
                zEnd = tI - 1
                pos = 0
            Else
                pos = tI
                zEnd = ValuesX.Item(tCompCali.t).Count - 1
                tEnd = fTime(ValuesX.Item(tCompCali.t).Count - 1)

                If Sprung Then
                    ' Initialise
                    Anz = 0
                    Finish = False

                    'New Sum /Num no start
                    For Each KV In ValuesX
                        If KV.Key <> tComp.t Then Summe(KV.Key) = 0
                    Next

                    'Start-values
                    tMin = fTime(pos)
                    tMid = CInt(tMin)
                    tMax = tMid + 0.5

                    If fTime(pos) >= tMax Then
                        tMid = tMid + 1
                        tMin = tMid - 0.5
                        tMax = tMid + 0.5
                        t0 = tMid
                    End If
                End If
            End If

            For z = pos To zEnd
                'Next Time-step
                Time = fTime(z)

                'If Time-step > tMax:
                Do While (Time >= tMax Or z = zEnd)
                    'Conclude Second
                    NewValues(tCompCali.t).Add(tMid)

                    'If no values ​​in Sum: Interpolate
                    If Anz = 0 Then
                        For Each KVd In Summe
                            NewValues(KVd.Key).Add((tMid - fTime(z - 1)) * (ValuesX(KVd.Key)(z) - ValuesX(KVd.Key)(z - 1)) / (fTime(z) - fTime(z - 1)) + ValuesX(KVd.Key)(z - 1))
                        Next
                    Else
                        If Time = tMax Then

                            For Each KVd In Summe
                                NewValues(KVd.Key).Add((Summe(KVd.Key) + ValuesX(KVd.Key)(z)) / (Anz + 1))
                            Next
                        Else
                            'If only one Value: Inter- /Extrapolate
                            If Anz = 1 Then

                                If z < 2 OrElse fTime(z - 1) < tMid Then

                                    For Each KVd In Summe
                                        NewValues(KVd.Key).Add((tMid - fTime(z - 1)) * (ValuesX(KVd.Key)(z) - ValuesX(KVd.Key)(z - 1)) / (fTime(z) - fTime(z - 1)) + ValuesX(KVd.Key)(z - 1))
                                    Next
                                Else

                                    For Each KVd In Summe
                                        NewValues(KVd.Key).Add((tMid - fTime(z - 2)) * (ValuesX(KVd.Key)(z - 1) - ValuesX(KVd.Key)(z - 2)) / (fTime(z - 1) - fTime(z - 2)) + ValuesX(KVd.Key)(z - 2))
                                    Next
                                End If

                            Else

                                For Each KVd In Summe
                                    NewValues(KVd.Key).Add(Summe(KVd.Key) / Anz)
                                Next
                            End If
                        End If
                    End If

                    If Not Finish Then

                        'Set New Area(Bereich)
                        tMid = tMid + 1
                        tMin = tMid - 0.5
                        tMax = tMid + 0.5

                        'Check whether last second
                        If tMax > tEnd Then
                            tMax = tEnd
                            Finish = True
                        End If

                        'New Sum /Num no start
                        For Each KV In ValuesX
                            If KV.Key <> tCompCali.t Then Summe(KV.Key) = 0
                        Next
                        Anz = 0
                    End If

                    ' Exit while after the last calculation
                    If Finish And z = zEnd Then
                        Exit Do
                    End If
                Loop

                For Each KV In ValuesX
                    If KV.Key <> tCompCali.t Then Summe(KV.Key) += ValuesX(KV.Key)(z)
                Next

                Anz = Anz + 1
            Next z
        Next lauf

        'Accept New fields
        ValuesX = NewValues

        Return True
    End Function

    ' Convert the data to 1Hz
    Public Function ConvTo1Hz(ByVal dTime As List(Of Double), ByRef ValuesX As Dictionary(Of tComp, List(Of Double))) As Boolean
        ' Declaration
        Dim tMin As Double
        Dim tMax As Double
        Dim tMid As Integer
        Dim Anz As Integer
        Dim z, t0, tI, lauf, laufE, zEnd, pos As Integer
        Dim Time As Double
        Dim tEnd, tIns As Double
        Dim Finish, Sprung As Boolean
        Dim NewValues As Dictionary(Of tComp, List(Of Double))
        Dim KV As KeyValuePair(Of tComp, List(Of Double))
        Dim KVd As KeyValuePair(Of tComp, Double)
        Dim fTime As List(Of Double)
        Dim Summe As Dictionary(Of tComp, Double)

        ' Initialise
        Sprung = False
        tI = 0
        fTime = dTime 'ValuesX(tComp.t)

        'Check whether Time is not reversed
        For z = 1 To ValuesX.Item(tComp.t).Count - 1
            If fTime(z) < fTime(z - 1) Then
                If Sprung Then
                    logme(9, False, "Time step invalid! t(" & z - 1 & ") = " & fTime(z - 1) & "[s], t(" & z & ") = " & fTime(z) & "[s]")
                    Return False
                Else
                    Sprung = True
                    tI = z
                End If
            End If
        Next z

        'Define Time-range
        t0 = CInt(Math.Round(fTime(0), 0, MidpointRounding.AwayFromZero))
        If Sprung Then tIns = fTime(tI - 1)
        tEnd = fTime(ValuesX.Item(tComp.t).Count - 1)

        'Create Output, Total and Num-of-Dictionaries
        NewValues = New Dictionary(Of tComp, List(Of Double))
        Summe = New Dictionary(Of tComp, Double)

        ' Generate the dictionary folder
        For Each KV In ValuesX
            NewValues.Add(KV.Key, New List(Of Double))
            If KV.Key <> tComp.t Then Summe.Add(KV.Key, 0)
        Next

        'Start-values
        tMin = fTime(0)
        tMid = CInt(tMin)
        tMax = tMid + 0.5

        If fTime(0) >= tMax Then
            tMid = tMid + 1
            tMin = tMid - 0.5
            tMax = tMid + 0.5
            t0 = tMid
        End If

        ' Initialise
        Anz = 0
        Finish = False
        If Sprung Then
            laufE = 2
        Else
            laufE = 1
        End If

        For lauf = 1 To laufE
            ' Set the time range (If a jump is detected to the calculation till the jump)
            If Sprung And lauf = 1 Then
                tEnd = tIns
                zEnd = tI - 1
                pos = 0
            Else
                pos = tI
                zEnd = ValuesX.Item(tComp.t).Count - 1
                tEnd = fTime(ValuesX.Item(tComp.t).Count - 1)

                If Sprung Then
                    ' Initialise
                    Anz = 0
                    Finish = False

                    'New Sum /Num no start
                    For Each KV In ValuesX
                        If KV.Key <> tComp.t Then Summe(KV.Key) = 0
                    Next

                    'Start-values
                    tMin = fTime(pos)
                    tMid = CInt(tMin)
                    tMax = tMid + 0.5

                    If fTime(pos) >= tMax Then
                        tMid = tMid + 1
                        tMin = tMid - 0.5
                        tMax = tMid + 0.5
                        t0 = tMid
                    End If
                End If
            End If

            For z = pos To zEnd
                'Next Time-step
                Time = fTime(z)

                'If Time-step > tMax:
                Do While (Time >= tMax Or z = zEnd)
                    'Conclude Second
                    NewValues(tComp.t).Add(tMid)

                    'If no values ​​in Sum: Interpolate
                    If Anz = 0 Then
                        For Each KVd In Summe
                            NewValues(KVd.Key).Add((tMid - fTime(z - 1)) * (ValuesX(KVd.Key)(z) - ValuesX(KVd.Key)(z - 1)) / (fTime(z) - fTime(z - 1)) + ValuesX(KVd.Key)(z - 1))
                        Next
                    Else
                        If Time = tMax Then

                            For Each KVd In Summe
                                NewValues(KVd.Key).Add((Summe(KVd.Key) + ValuesX(KVd.Key)(z)) / (Anz + 1))
                            Next
                        Else
                            'If only one Value: Inter- /Extrapolate
                            If Anz = 1 Then

                                If z < 2 OrElse fTime(z - 1) < tMid Then

                                    For Each KVd In Summe
                                        NewValues(KVd.Key).Add((tMid - fTime(z - 1)) * (ValuesX(KVd.Key)(z) - ValuesX(KVd.Key)(z - 1)) / (fTime(z) - fTime(z - 1)) + ValuesX(KVd.Key)(z - 1))
                                    Next
                                Else

                                    For Each KVd In Summe
                                        NewValues(KVd.Key).Add((tMid - fTime(z - 2)) * (ValuesX(KVd.Key)(z - 1) - ValuesX(KVd.Key)(z - 2)) / (fTime(z - 1) - fTime(z - 2)) + ValuesX(KVd.Key)(z - 2))
                                    Next
                                End If

                            Else

                                For Each KVd In Summe
                                    NewValues(KVd.Key).Add(Summe(KVd.Key) / Anz)
                                Next
                            End If
                        End If
                    End If

                    If Not Finish Then

                        'Set New Area(Bereich)
                        tMid = tMid + 1
                        tMin = tMid - 0.5
                        tMax = tMid + 0.5

                        'Check whether last second
                        If tMax > tEnd Then
                            tMax = tEnd
                            Finish = True
                        End If

                        'New Sum /Num no start
                        For Each KV In ValuesX
                            If KV.Key <> tComp.t Then Summe(KV.Key) = 0
                        Next
                        Anz = 0
                    End If

                    ' Exit while after the last calculation
                    If Finish And z = zEnd Then
                        Exit Do
                    End If
                Loop

                For Each KV In ValuesX
                    If KV.Key <> tComp.t Then Summe(KV.Key) += ValuesX(KV.Key)(z)
                Next

                Anz = Anz + 1
            Next z
        Next lauf

        'Accept New fields
        ValuesX = NewValues

        Return True
    End Function

    ' Convert the data to 1Hz
    Public Function ConvTo1Hz(ByVal TimesX As List(Of Double), ByRef ValuesX As Dictionary(Of String, List(Of Double))) As Boolean
        ' Declaration
        Dim tMin As Double
        Dim tMax As Double
        Dim tMid As Integer
        Dim Anz As Integer
        Dim z, t0, tI, lauf, laufE, zEnd, pos As Integer
        Dim Time As Double
        Dim tEnd, tIns As Double
        Dim Finish, Sprung As Boolean
        Dim NewValues As Dictionary(Of String, List(Of Double))
        Dim KV As KeyValuePair(Of String, List(Of Double))
        Dim KVd As KeyValuePair(Of String, Double)
        Dim fTime As List(Of Double)
        Dim Summe As Dictionary(Of String, Double)

        ' Initialise
        Sprung = False
        tI = 0
        fTime = TimesX

        'Check whether Time is not reversed
        For z = 1 To ValuesX.Item(ValuesX.First.Key).Count - 1
            If fTime(z) < fTime(z - 1) Then
                If Sprung Then
                    logme(9, False, "Time step invalid! t(" & z - 1 & ") = " & fTime(z - 1) & "[s], t(" & z & ") = " & fTime(z) & "[s]")
                    Return False
                Else
                    Sprung = True
                    tI = z
                End If
            End If
        Next z

        'Define Time-range
        t0 = CInt(Math.Round(fTime(0), 0, MidpointRounding.AwayFromZero))
        If Sprung Then tIns = fTime(tI - 1)
        tEnd = fTime(ValuesX.Item(ValuesX.First.Key).Count - 1)

        'Create Output, Total and Num-of-Dictionaries
        NewValues = New Dictionary(Of String, List(Of Double))
        Summe = New Dictionary(Of String, Double)

        ' Generate the dictionary folder
        For Each KV In ValuesX
            NewValues.Add(KV.Key, New List(Of Double))
            Summe.Add(KV.Key, 0)
        Next

        'Start-values
        tMin = fTime(0)
        tMid = CInt(tMin)
        tMax = tMid + 0.5

        If fTime(0) >= tMax Then
            tMid = tMid + 1
            tMin = tMid - 0.5
            tMax = tMid + 0.5
            t0 = tMid
        End If

        ' Initialise
        Anz = 0
        Finish = False
        If Sprung Then
            laufE = 2
        Else
            laufE = 1
        End If

        For lauf = 1 To laufE
            ' Set the time range (If a jump is detected to the calculation till the jump)
            If Sprung And lauf = 1 Then
                tEnd = tIns
                zEnd = tI - 1
                pos = 0
            Else
                pos = tI
                zEnd = ValuesX.Item(ValuesX.First.Key).Count - 1
                tEnd = fTime(ValuesX.Item(ValuesX.First.Key).Count - 1)

                If Sprung Then
                    ' Initialise
                    Anz = 0
                    Finish = False

                    'Start-values
                    tMin = fTime(pos)
                    tMid = CInt(tMin)
                    tMax = tMid + 0.5

                    If fTime(pos) >= tMax Then
                        tMid = tMid + 1
                        tMin = tMid - 0.5
                        tMax = tMid + 0.5
                        t0 = tMid
                    End If

                    'New Sum /Num no start
                    For Each KV In ValuesX
                        Summe(KV.Key) = 0
                    Next
                End If
            End If

            For z = pos To zEnd
                'Next Time-step
                Time = fTime(z)

                'If Time-step > tMax:
                Do While (Time >= tMax Or z = zEnd)
                    'If no values ​​in Sum: Interpolate
                    If Anz = 0 Then
                        For Each KVd In Summe
                            NewValues(KVd.Key).Add((tMid - fTime(z - 1)) * (ValuesX(KVd.Key)(z) - ValuesX(KVd.Key)(z - 1)) / (fTime(z) - fTime(z - 1)) + ValuesX(KVd.Key)(z - 1))
                        Next
                    Else
                        If Time = tMax Then

                            For Each KVd In Summe
                                NewValues(KVd.Key).Add((Summe(KVd.Key) + ValuesX(KVd.Key)(z)) / (Anz + 1))
                            Next
                        Else
                            'If only one Value: Inter- /Extrapolate
                            If Anz = 1 Then

                                If z < 2 OrElse fTime(z - 1) < tMid Then

                                    For Each KVd In Summe
                                        NewValues(KVd.Key).Add((tMid - fTime(z - 1)) * (ValuesX(KVd.Key)(z) - ValuesX(KVd.Key)(z - 1)) / (fTime(z) - fTime(z - 1)) + ValuesX(KVd.Key)(z - 1))
                                    Next
                                Else

                                    For Each KVd In Summe
                                        NewValues(KVd.Key).Add((tMid - fTime(z - 2)) * (ValuesX(KVd.Key)(z - 1) - ValuesX(KVd.Key)(z - 2)) / (fTime(z - 1) - fTime(z - 2)) + ValuesX(KVd.Key)(z - 2))
                                    Next
                                End If

                            Else

                                For Each KVd In Summe
                                    NewValues(KVd.Key).Add(Summe(KVd.Key) / Anz)
                                Next
                            End If
                        End If
                    End If

                    If Not Finish Then

                        'Set New Area(Bereich)
                        tMid = tMid + 1
                        tMin = tMid - 0.5
                        tMax = tMid + 0.5

                        'Check whether last second
                        If tMax > tEnd Then
                            tMax = tEnd
                            Finish = True
                        End If

                        'New Sum /Num no start
                        For Each KV In ValuesX
                            Summe(KV.Key) = 0
                        Next
                        Anz = 0
                    End If

                    ' Exit while after the last calculation
                    If Finish And z = zEnd Then
                        Exit Do
                    End If
                Loop

                For Each KV In ValuesX
                    Summe(KV.Key) += ValuesX(KV.Key)(z)
                Next

                Anz = Anz + 1
            Next z
        Next lauf

        'Accept New fields
        ValuesX = NewValues

        Return True
    End Function
End Module
