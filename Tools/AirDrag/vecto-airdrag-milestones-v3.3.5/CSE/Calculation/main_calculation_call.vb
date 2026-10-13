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

Public Module main_calculation_call

    ' Main calculation
    Sub calculation(ByVal isCalibrate As Boolean)
        ' Declaration
        Dim i As Integer
        Dim Starttime As Double() = New Double(2) {0.0, 0.0, 0.0}
        Dim Endtime As Double() = New Double(2) {0.0, 0.0, 0.0}

        ' Initialisation
        InputData = Nothing
        InputUndefData = Nothing
        CalcData = Nothing
        ErgValues = Nothing
        ErgValuesUndef = Nothing

        If isCalibrate Then
            ' Declarations
            Dim MSC As New cMSC
            Dim vMSC As New cVirtMSC

            ' Read the input data
            logme(7, False, "Reading Input Files...")
            Dim vehicle As New cVehicle(Job.vehicle_fpath)
            If Not fCheckVeh(3, vehicle) Then Throw New Exception("Vehicle file for calculation incorrect!")
            ReadInputMSC(MSC, Job.calib_track_fpath, isCalibrate)
            ReadDataFile(Job.calib_run_fpath, MSC, vehicle, isCalibrate)

            ' Exit function if error is detected
            If BWorker.CancellationPending Then Return

            ' Output on the GUI
            logme(7, False, "Calculating the misalignment run...")

            ' Identify the signal measurement sections
            fIdentifyMS(MSC, vMSC)

            ' Exit function if error is detected
            If BWorker.CancellationPending Then Return

            ' Output on the GUI
            logme(6, False, "Calculating the misalignment run parameter")

            Try
                ' Calculate the results from the misalignment test
                fCalcCalib(vehicle)

            Finally
                ' Output on the GUI
                logme(7, False, "Writing the output files...")

                ' Output
                fOutDataCalc1Hz(Job.calib_run_fpath, isCalibrate)
                fOutCalcRes(isCalibrate)
            End Try
        Else
            ' Declaration
            Dim MSC As New cMSC
            Dim vMSC As New cVirtMSC
            Dim r_dyn_ref As Double
            Dim Altdata As New List(Of cAlt)

            ' Output on the GUI
            logme(7, False, "Calculating the speed runs...")

            ' Read the input files
            logme(7, False, "Reading Input Files...")
            Dim vehicle As New cVehicle(Job.vehicle_fpath)
            If Not fCheckVeh(3, vehicle) And Job.mode = 1 Then Throw New Exception("Accuracy of vehicle parameters not ensured!")
            ReadInputMSC(MSC, Job.coast_track_fpath, isCalibrate)
            If Crt.gradient_correction Then ReadAltitudeFiles(MSC, Altdata)
            ReadWeather(Job.ambient_fpath)

            ' Check altitude files
            If Crt.gradient_correction Then fcheckAlt(MSC, Altdata)

            ' Calculation of the virtual MSC points
            fIdentifyMS(MSC, vMSC, , False)

            ' Exit function if error is detected
            If BWorker.CancellationPending Then Return

            ' Output which test are calculated
            For i = 0 To UBound(Job.coasting_fpaths)
                If i = 1 Or i = 2 Then
                    ' Output on the GUI
                    If i = 1 Then
                        logme(7, False, "Calculating the first low speed run...")
                    Else
                        logme(7, False, "Calculating the second low speed run...")
                    End If
                Else
                    ' Output on the GUI
                    logme(7, False, "Calculating the high speed run...")
                End If

                ' Output on the GUI
                logme(6, False, "Reading the data file...")
                ReadDataFile(Job.coasting_fpaths(i), MSC, vehicle)
                Starttime(i) = CalcData(tCompCali.t).First
                Endtime(i) = CalcData(tCompCali.t).Last
                If (i = 1 And Starttime(i) < vehicle.tDriftZero) Then
                    logme(9, False, format("The Starttime of LS1 is shorter then tDriftZero in the vehicle file (Starttime: {0} < tDriftZero {1})!", Starttime(i), vehicle.tDriftZero))
                End If
                If (i = 2 And Starttime(i) > vehicle.tDriftCheck) Then
                    logme(9, False, format("The Starttime of LS2 is greater then tDriftCheck in the vehicle file (Starttime: {0} > tDriftCheck {1})!", Starttime(i), vehicle.tDriftCheck))
                End If

                ' Exit function if error is detected
                If BWorker.CancellationPending Then Return

                ' Identify the signal measurement sections
                fIdentifyMS(MSC, vMSC, False)

                ' Exit function if error is detected
                If BWorker.CancellationPending Then Return

                ' Output on the GUI
                If i = 0 Then logme(6, False, "Calibration of vehicle speed and anemometer position correction")

                ' Calculate the run
                fCalcRun(MSC, Altdata, vehicle, i, r_dyn_ref)

                ' Exit function if error is detected
                If BWorker.CancellationPending Then Return

                ' Output on the GUI
                logme(6, False, "Writing the output files...")

                ' Output
                fOutDataCalc1Hz(Job.coasting_fpaths(i), isCalibrate)

                ' Save the Result dictionaries
                fSaveDic(i)

                ' Exit function if error is detected
                If BWorker.CancellationPending Then Return

                ' Clear the dictionaries
                InputData = Nothing
                InputUndefData = Nothing
                CalcData = Nothing
                ErgValues = Nothing
                ErgValuesUndef = Nothing
            Next i

            Try
                ' Control the measurement sequence
                If Not Endtime(1) < Starttime(0) Or Not Endtime(0) < Starttime(2) Then
                    logme(9, False, format("The given measurement times are not in the right sequence! Ending LS1:({0}), Start HS:({1}), Ending HS:({2}), Start LS2:({3})", Endtime(1), Starttime(0), Endtime(0), Starttime(2)))
                End If

                ' Check if the LS/HS test run is valid
                fCheckLSHS()

                ' Calculate the regressions
                fCalcReg(vehicle)
            Finally
                ' Write the summerised output file
                logme(7, False, "Writing the summarised output file...")
                fOutCalcRes(isCalibrate)
            End Try

            ' Check if all is valid
            For i = 0 To ErgValuesReg(tCompErgReg.SecID).Count - 1
                If ErgValuesReg(tCompErgReg.valid_RRC)(i) = 0 Then
                    Job.valid_RRC = False
                    If Job.mode = 1 Then
                        Throw New Exception("Invalid test - maximum deviation of RRCs between low speed tests exceeded")
                    Else
                        logme(8, False, format("Invalid test - maximum deviation of RRCs between low speed tests exceeded"))
                    End If
                End If
            Next i


            ' Output of the final data
            fOutCalcResReg()

            ' Write the results on the GUI
            logme(7, False, "Results from the calculation")
            logme(6, False, "average absolute beta HS test: " & Math.Round(Job.beta, 4, MidpointRounding.AwayFromZero))
            logme(6, False, "delta CdxA correction: " & Math.Round(Job.delta_CdxA_beta, 4, MidpointRounding.AwayFromZero))
            logme(6, False, "CdxA(0): " & Math.Round(Job.CdxA0, 2, MidpointRounding.AwayFromZero))

            ' Clear the dictionaries
            ErgValuesComp = Nothing
            ErgValuesUndefComp = Nothing
            ErgValuesReg = Nothing
            InputWeatherData = Nothing
        End If
    End Sub

    ' Calculate the calibration test parameter
    Sub fCalcCalib(ByVal vehicleX As cVehicle)
        ' Declaration
        Dim run As Integer
        Dim Change As Boolean

        ' Initialisation
        Job.fv_veh = 0
        Job.fv_pe = 0
        Job.beta_ame = 0
        Change = False
        run = 0

        ' Check if the calibration run has the same and enough sections measured
        fCheckCalib(run, vehicleX, Change)

        Do While Change
            ' Initialise Parameter
            Job.fv_veh = 0
            Job.fv_pe = 0
            Job.beta_ame = 0
            run += 1

            ' Calculate fv_veh
            ffv_veh()

            ' Calculate the corrected vehicle speed
            fCalcCorVveh()

            ' Calculate fv_pe and beta_amn
            ffvpeBeta()

            ' Calculate the values for v_wind, beta and v_air
            fWindBetaAir(vehicleX)

            ' Calculate the moving average from v_wind
            fMoveAve(CalcData(tCompCali.t), CalcData(tCompCali.vwind_c), CalcData(tCompCali.vwind_1s))

            ' Calculate the average values for v_wind, beta and v_wind_1s_max
            fWindBetaAirErg()

            ' Calculate the other speed run relevant values
            fCalcSpeedValCalib()

            ' Check if the calibration run is valid
            fCheckCalib(run, vehicleX, Change)

            ' Error
            If run > 10 Then
                Throw New Exception("The calibration is not possible because iteration for valid datasets does not converge (n>10)!")
            End If
        Loop
    End Sub

    ' Calculate the speed run parameter
    Function fCalcRun(ByVal MSCX As cMSC, ByVal Altdata As List(Of cAlt), ByVal vehicleX As cVehicle, ByVal coastingSeq As Integer, ByRef r_dyn_ref As Double) As Boolean
        ' Declaration
        Dim run As Integer
        Dim Change As Boolean = True

        ' Calculate fv_veh + fv_pe by HS otherwise use the values
        If coastingSeq = 0 Then
            ' Initialisation
            Job.fv_veh = 0
            Job.fv_pe = 0
            run = 0
        End If

        Do While Change
            ' Initialise Parameter
            r_dyn_ref = 0
            If coastingSeq = 0 Then
                Job.fv_veh = 0
                Job.fv_pe = 0
                run += 1
            End If

            ' Calculate fv_veh
            If coastingSeq = 0 Then ffv_veh()

            ' Calculate the corrected vehicle speed
            fCalcCorVveh()

            ' Calculate fv_pe
            If coastingSeq = 0 Then ffvpeBeta(False)

            ' Calculate the values for v_wind, beta and v_air
            fWindBetaAir(vehicleX)

            ' Calculate the moving average from v_vind
            fMoveAve(CalcData(tCompCali.t), CalcData(tCompCali.vwind_c), CalcData(tCompCali.vwind_1s))

            ' Calculate the average values for v_wind, beta and v_wind_1s_max
            fWindBetaAirErg()

            ' Calculate the other speed run relevant values
            fCalcSpeedVal(Altdata, vehicleX, coastingSeq, r_dyn_ref)

            ' Evaluate the valid sections
            fCalcValidSec(MSCX, vehicleX, coastingSeq, run, r_dyn_ref, Change)

            ' Error
            If run > 10 Then
                Throw New Exception("The evaluation is not possible because iteration for valid datasets does not converge (n>10)!")
            End If
        Loop

        ' Show the results on GUI
        If coastingSeq = 0 Then uRB = True

        Return True
    End Function

    ' Function to calibrate fv_veh
    Sub ffv_veh()
        ' Declaration
        Dim i, num As Integer

        ' Initialise
        Job.fv_veh = 0
        num = 0

        ' Calculate velocity correction
        For i = 0 To ErgValues(tCompErg.SecID).Count - 1
            If ErgValues(tCompErg.used)(i) = 1 Then
                ' Error if the vehicle velocity of the CAN is 0
                If ErgValues(tCompErg.v_veh_CAN)(i) = 0 Then
                    Throw New Exception("The measured vehicle velocity (v_veh_CAN) is 0 in section: " & ErgValues(tCompErg.SecID)(i))
                End If
                Job.fv_veh += ErgValues(tCompErg.v_MSC)(i) / ErgValues(tCompErg.v_veh_CAN)(i)
                num += 1
            End If
        Next i

        ' Calculate the average over all factors
        Job.fv_veh = Job.fv_veh / num
        If num = 0 Then Job.fv_veh = 0
    End Sub

    ' Function to calculate fv_pe & beta_amn
    Function ffvpeBeta(Optional ByVal CalibRun As Boolean = True) As Boolean
        ' Declaration
        Dim i, anz_DirID(1) As Integer
        Dim vair_ic(1), beta_ic(1), sum_v_veh As Double

        ' Initialise
        For i = 0 To 1
            vair_ic(i) = 0
            If CalibRun Then beta_ic(i) = 0
            anz_DirID(i) = 0
        Next i
        sum_v_veh = 0

        ' Calculate velocity and beta correction
        For i = 0 To ErgValues(tCompErg.SecID).Count - 1
            If ErgValues(tCompErg.used)(i) = 1 Then
                sum_v_veh += ErgValues(tCompErg.v_veh)(i) / 3.6
                If CalibRun Then
                    If ErgValues(tCompErg.DirID)(i) = ErgValues(tCompErg.DirID)(0) Then
                        vair_ic(0) += ErgValues(tCompErg.vair_ic)(i)
                        beta_ic(0) += ErgValues(tCompErg.beta_ic)(i)
                        anz_DirID(0) += 1
                    Else
                        vair_ic(1) += ErgValues(tCompErg.vair_ic)(i)
                        beta_ic(1) += ErgValues(tCompErg.beta_ic)(i)
                        anz_DirID(1) += 1
                    End If
                Else
                    If ErgValues(tCompErg.HeadID)(i) = ErgValues(tCompErg.HeadID)(0) Then
                        vair_ic(0) += ErgValues(tCompErg.vair_ic)(i)
                        anz_DirID(0) += 1
                    Else
                        vair_ic(1) += ErgValues(tCompErg.vair_ic)(i)
                        anz_DirID(1) += 1
                    End If
                End If
            End If
        Next i

        Job.fv_pe = (sum_v_veh / (anz_DirID(0) + anz_DirID(1))) / (0.5 * (vair_ic(0) / anz_DirID(0) + vair_ic(1) / anz_DirID(1)))
        If CalibRun Then 'Job.beta_ame = (0.5 * (beta_ic(0) / anz_DirID(0) + beta_ic(1) / anz_DirID(1))) - AmeAng
            If ((beta_ic(0) / anz_DirID(0)) <= 90 And (beta_ic(1) / anz_DirID(1)) >= 270) Or ((beta_ic(1) / anz_DirID(1)) <= 90 And (beta_ic(0) / anz_DirID(0)) >= 270) Then
                If Job.beta_ame = (0.5 * (beta_ic(0) / anz_DirID(0) + beta_ic(1) / anz_DirID(1))) > 180 Then
                    Job.beta_ame = (0.5 * (beta_ic(0) / anz_DirID(0) + beta_ic(1) / anz_DirID(1))) - 360
                Else
                    Job.beta_ame = (0.5 * (beta_ic(0) / anz_DirID(0) + beta_ic(1) / anz_DirID(1)))
                End If
            Else
                Job.beta_ame = (0.5 * (beta_ic(0) / anz_DirID(0) + beta_ic(1) / anz_DirID(1))) - AmeAng
            End If
        End If
        Return True
    End Function

    ' Function for the result calculation
    Function fWindBetaAir(ByVal vehicleX As cVehicle) As Boolean
        ' Declaration
        Dim i, h, Pstep As Integer
        Dim vwind_x, vwind_y, vwind_x_ha, vwind_y_ha As Double
        Dim vairX, vwindX, betaX As Double

        ' Constant
        Pstep = 10

        ' Calculate the values
        For i = 0 To CalcData(tCompCali.lati_UTM).Count - 1
            CalcData(tCompCali.vair_uf)(i) = (InputData(tComp.vair_ic)(i) * Job.fv_pe)
            CalcData(tCompCali.beta_uf)(i) = ((InputData(tComp.beta_ic)(i) - Job.beta_ame) * Job.fa_pe)
            vwind_x_ha = CalcData(tCompCali.vair_uf)(i) * Math.Cos((CalcData(tCompCali.beta_uf)(i) - AmeAng) * Math.PI / 180) - CalcData(tCompCali.v_veh_c)(i) / 3.6
            vwind_y_ha = CalcData(tCompCali.vair_uf)(i) * Math.Sin((CalcData(tCompCali.beta_uf)(i) - AmeAng) * Math.PI / 180)
            CalcData(tCompCali.vwind_ha)(i) = (Math.Sqrt(vwind_x_ha ^ 2 + vwind_y_ha ^ 2))

            ' Calculate the steps
            For h = 5 To 95 Step Pstep
                vwind_x = vwind_x_ha * Math.Pow((((h / 100) * vehicleX.vehHeight) / vehicleX.anemometerHeight), 0.2)
                vwind_y = vwind_y_ha * Math.Pow((((h / 100) * vehicleX.vehHeight) / vehicleX.anemometerHeight), 0.2)
                vairX = vairX + (Math.Sqrt((vwind_x + CalcData(tCompCali.v_veh_c)(i) / 3.6) ^ 2 + vwind_y ^ 2)) * vehicleX.vehHeight * Pstep / 100
                vwindX = vwindX + (Math.Sqrt(vwind_x ^ 2 + vwind_y ^ 2)) * vehicleX.vehHeight * Pstep / 100
                betaX = betaX + (Math.Atan(vwind_y / (vwind_x + CalcData(tCompCali.v_veh_c)(i) / 3.6)) * 180 / Math.PI) * vehicleX.vehHeight * Pstep / 100
            Next h

            ' Add the calculated values to the calculate arrays
            CalcData(tCompCali.vair_c)(i) = (vairX / vehicleX.vehHeight)
            CalcData(tCompCali.vwind_c)(i) = (vwindX / vehicleX.vehHeight)
            CalcData(tCompCali.beta_c)(i) = (betaX / vehicleX.vehHeight)
            vairX = 0
            vwindX = 0
            betaX = 0
        Next i

        Return True
    End Function

    ' Function to check if the calibration run is valid
    Sub fCheckCalib(ByVal Run As Integer, ByVal vehicleX As cVehicle, ByRef Change As Boolean)
        ' Declaration
        Dim i, j, k, anz As Integer
        Dim control As Boolean
        Dim SecCount As New cValidSec
        Dim OldValid(ErgValues(tCompErg.SecID).Count - 1), OldUse(ErgValues(tCompErg.SecID).Count - 1) As Boolean
        Dim lim_v_veh_avg_max_CAL, lim_v_veh_avg_min_CAL As Single

        ' Initialisation
        Change = False

        ' Save the old values
        For i = 0 To ErgValues(tCompErg.SecID).Count - 1
            OldValid(i) = ErgValues(tCompErg.valid)(i)
            OldUse(i) = ErgValues(tCompErg.used)(i)
        Next i

        ' Reset the ErgValues for the criterias
        ResetErgVal(True)

        ' Get the speed limit (!!use same criterias like for the HS test)
        fgetSpeedLim(vehicleX, lim_v_veh_avg_max_CAL, lim_v_veh_avg_min_CAL)

        ' Set the values
        For i = 0 To ErgValues(tCompErg.SecID).Count - 1
            If ErgValues(tCompErg.v_wind_avg)(i) < Crt.v_wind_avg_max_CAL Then ErgValues(tCompErg.val_vWind)(i) = 1
            If Math.Abs(ErgValues(tCompErg.beta_avg)(i)) < Crt.beta_avg_max_CAL Then ErgValues(tCompErg.val_beta)(i) = 1
            If ErgValues(tCompErg.v_wind_1s_max)(i) < Crt.v_wind_1s_max_CAL Then ErgValues(tCompErg.val_vWind_1s)(i) = 1
            If ErgValues(tCompErg.user_valid)(i) = 1 Then ErgValues(tCompErg.val_User)(i) = 1
            If Run = 0 Then
                ' Only by initialisation. Otherwise its always false
                ErgValues(tCompErg.val_vVeh_avg)(i) = 1
            Else
                ' Check by calibration
                If ErgValues(tCompErg.v_veh)(i) < lim_v_veh_avg_max_CAL And _
                   ErgValues(tCompErg.v_veh)(i) > lim_v_veh_avg_min_CAL Then ErgValues(tCompErg.val_vVeh_avg)(i) = 1
            End If
            If ErgValues(tCompErg.v_veh_1s_max)(i) < (ErgValues(tCompErg.v_veh)(i) + Crt.v_veh_1s_delta_CAL) And _
               ErgValues(tCompErg.v_veh_1s_min)(i) > (ErgValues(tCompErg.v_veh)(i) - Crt.v_veh_1s_delta_CAL) Then ErgValues(tCompErg.val_vVeh_1s)(i) = 1

            ' Check if all criterias are valid
            If ErgValues(tCompErg.val_vWind)(i) = 1 And ErgValues(tCompErg.val_beta)(i) = 1 And
               ErgValues(tCompErg.val_vWind_1s)(i) = 1 And ErgValues(tCompErg.val_User)(i) = 1 And
               ErgValues(tCompErg.val_vVeh_avg)(i) = 1 And ErgValues(tCompErg.val_vVeh_1s)(i) = 1 And
               ErgValues(tCompErg.system_valid)(i) = 1 Then
                ErgValues(tCompErg.valid)(i) = 1
                ErgValues(tCompErg.used)(i) = 1
            Else
                ErgValues(tCompErg.valid)(i) = 0
                ErgValues(tCompErg.used)(i) = 0
            End If
        Next i

        ' Count the valid sections in both rounds and calculate the average vehicle speed
        For i = 0 To ErgValues(tCompErg.SecID).Count - 1
            ' Initialisation
            control = False

            ' First in
            If i = 0 Then
                SecCount.NameSec.Add(ErgValues(tCompErg.SecID)(i) & " (" & ErgValues(tCompErg.DirID)(i) & ")")
                SecCount.ValidSec.Add(False)
                If ErgValues(tCompErg.valid)(i) = 1 Then
                    SecCount.AnzSec.Add(1)
                    SecCount.vVeh.Add(ErgValues(tCompErg.v_veh)(i))
                Else
                    SecCount.AnzSec.Add(0)
                    SecCount.vVeh.Add(0)
                End If
            End If

            ' Look if section is checked
            For k = 0 To SecCount.NameSec.Count - 1
                If i > 0 And SecCount.NameSec(k) = ErgValues(tCompErg.SecID)(i) & " (" & ErgValues(tCompErg.DirID)(i) & ")" Then
                    control = True
                End If
            Next k

            ' Count the valid section and calculate the average vehicle speed
            If control = False Then
                If i = ErgValues(tCompErg.SecID).Count - 1 Then
                    SecCount.NameSec.Add(ErgValues(tCompErg.SecID)(i) & " (" & ErgValues(tCompErg.DirID)(i) & ")")
                    SecCount.ValidSec.Add(False)
                    If ErgValues(tCompErg.valid)(i) = 1 Then
                        SecCount.AnzSec.Add(1)
                        SecCount.vVeh.Add(ErgValues(tCompErg.v_veh)(i))
                    Else
                        SecCount.AnzSec.Add(0)
                        SecCount.vVeh.Add(0)
                    End If
                Else
                    For j = i + 1 To ErgValues(tCompErg.SecID).Count - 1
                        ' Add a new count point
                        If i > 0 And j = i + 1 Then
                            SecCount.NameSec.Add(ErgValues(tCompErg.SecID)(i) & " (" & ErgValues(tCompErg.DirID)(i) & ")")
                            SecCount.ValidSec.Add(False)
                            If ErgValues(tCompErg.valid)(i) = 1 Then
                                SecCount.AnzSec.Add(1)
                                SecCount.vVeh.Add(ErgValues(tCompErg.v_veh)(i))
                            Else
                                SecCount.AnzSec.Add(0)
                                SecCount.vVeh.Add(0)
                            End If
                        End If

                        ' Count the valid sections
                        If ErgValues(tCompErg.SecID)(i) = ErgValues(tCompErg.SecID)(j) And ErgValues(tCompErg.DirID)(i) = ErgValues(tCompErg.DirID)(j) Then
                            If ErgValues(tCompErg.valid)(j) = 1 Then
                                SecCount.AnzSec(SecCount.AnzSec.Count - 1) += 1
                                SecCount.vVeh(SecCount.vVeh.Count - 1) += ErgValues(tCompErg.v_veh)(j)
                            End If
                        End If
                    Next j
                End If
            End If
        Next i
        'Calculate average vehicle speed
        SecCount.calcAveSpeed()

        ' Ceck if enough sections are detected
        If SecCount.AnzSec.Count - 1 < 1 Then
            Throw New Exception(format("Insufficient numbers of valid measurement sections({0}) available!", SecCount.AnzSec.Count))
        End If

        ' Check if enough valid sections in both directions and average vehicle speeds differ not more then v_veh_ave_delta_CAL
        For i = 0 To SecCount.NameSec.Count - 1
            For j = i + 1 To SecCount.NameSec.Count - 1
                If Trim(Mid(SecCount.NameSec(i), 1, InStr(SecCount.NameSec(i), "(") - 2)) = Trim(Mid(SecCount.NameSec(j), 1, InStr(SecCount.NameSec(j), "(") - 2)) Then
                    ' If enough sections in both directions are detected
                    If SecCount.AnzSec(i) >= Crt.segruns_min_CAL And SecCount.AnzSec(j) >= Crt.segruns_min_CAL And _
                       Math.Abs(SecCount.vVeh(i) - SecCount.vVeh(j)) < Crt.v_veh_ave_delta_CAL Then
                        ' Set the whole sections on valid
                        SecCount.ValidSec(i) = True
                        SecCount.ValidSec(j) = True
                    Else
                        If Math.Abs(SecCount.vVeh(i) - SecCount.vVeh(j)) > Crt.v_veh_ave_delta_CAL Then
                            logme(8, False, format("Deviation of average speeds per heading is to high ({0} km/h) in SecID: {1}! Section is set to invalid", Math.Round(Math.Abs(SecCount.vVeh(i) - SecCount.vVeh(j)), 3, MidpointRounding.AwayFromZero), ErgValues(tCompErg.SecID)(i)))
                        End If

                        SecCount.ValidSec(i) = False
                        SecCount.ValidSec(j) = False
                        For k = 0 To ErgValues(tCompErg.SecID).Count - 1
                            If (Trim(Mid(SecCount.NameSec(i), 1, InStr(SecCount.NameSec(i), "(") - 2)) = ErgValues(tCompErg.SecID)(k)) And (Trim(Mid(SecCount.NameSec(i), InStr(SecCount.NameSec(i), "(") + 1, InStr(SecCount.NameSec(i), ")") - (InStr(SecCount.NameSec(i), "(") + 1))) = ErgValues(tCompErg.DirID)(k)) Then ErgValues(tCompErg.used)(k) = 0
                            If (Trim(Mid(SecCount.NameSec(j), 1, InStr(SecCount.NameSec(j), "(") - 2)) = ErgValues(tCompErg.SecID)(k)) And (Trim(Mid(SecCount.NameSec(j), InStr(SecCount.NameSec(j), "(") + 1, InStr(SecCount.NameSec(j), ")") - (InStr(SecCount.NameSec(j), "(") + 1))) = ErgValues(tCompErg.DirID)(k)) Then ErgValues(tCompErg.used)(k) = 0
                        Next k
                    End If
                End If
            Next j
        Next i

        ' Check if enough sections are valid
        anz = 0
        For i = 0 To SecCount.ValidSec.Count - 1
            If SecCount.ValidSec(i) Then
                anz += 1
            End If
        Next i
        If anz < 2 Then
            Throw New Exception(format("Insufficient numbers of valid measurement sections({0}) available!", anz))
        End If

        ' Look if something have changed
        If Run <> 0 Then
            For i = 0 To ErgValues(tCompErg.SecID).Count - 1
                If Not Int(OldValid(i)) = ErgValues(tCompErg.valid)(i) And Not Int(OldUse(i)) = ErgValues(tCompErg.used)(i) Then
                    Change = True
                End If
            Next i
        Else
            Change = True
        End If
    End Sub

    ' Function to check if the evaluation run is valid
    Sub fCheckLSHS()
        ' Declaration
        Dim i, j, k, anz, anzHS1, anzHS2 As Integer
        Dim control, FirstIn As Boolean
        Dim SecCount As New cValidSec

        '****** Low Speed Test ******
        ' Count the valid sections in both rounds
        For i = 0 To ErgValuesComp(tCompErg.SecID).Count - 1.0
            If ErgValuesComp(tCompErg.RunID)(i) = IDLS1 Or ErgValuesComp(tCompErg.RunID)(i) = IDLS2 Then
                ' Initialisation
                control = False

                ' First in
                If i = 0 Then
                    SecCount.NameSec.Add(ErgValuesComp(tCompErg.SecID)(i) & " (" & ErgValuesComp(tCompErg.DirID)(i) & "," & ErgValuesComp(tCompErg.RunID)(i) & ")")
                    SecCount.ValidSec.Add(False)
                    If ErgValuesComp(tCompErg.valid)(i) = 1 Then
                        SecCount.AnzSec.Add(1)
                    Else
                        SecCount.AnzSec.Add(0)
                    End If
                End If

                ' Look if section is checked
                For k = 0 To SecCount.NameSec.Count - 1
                    If i > 0 And SecCount.NameSec(k) = ErgValuesComp(tCompErg.SecID)(i) & " (" & ErgValuesComp(tCompErg.DirID)(i) & "," & ErgValuesComp(tCompErg.RunID)(i) & ")" Then
                        control = True
                    End If
                Next k

                ' Count the valid section
                If control = False Then
                    If i = ErgValuesComp(tCompErg.SecID).Count - 1.0 Then
                        SecCount.NameSec.Add(ErgValuesComp(tCompErg.SecID)(i) & " (" & ErgValuesComp(tCompErg.DirID)(i) & "," & ErgValuesComp(tCompErg.RunID)(i) & ")")
                        SecCount.ValidSec.Add(False)
                        If ErgValuesComp(tCompErg.valid)(i) = 1 Then
                            SecCount.AnzSec.Add(1)
                        Else
                            SecCount.AnzSec.Add(0)
                        End If
                    Else
                        For j = i + 1 To ErgValuesComp(tCompErg.SecID).Count - 1
                            ' Add a new count point
                            If i > 0 And j = i + 1 Then
                                SecCount.NameSec.Add(ErgValuesComp(tCompErg.SecID)(i) & " (" & ErgValuesComp(tCompErg.DirID)(i) & "," & ErgValuesComp(tCompErg.RunID)(i) & ")")
                                SecCount.ValidSec.Add(False)
                                If ErgValuesComp(tCompErg.valid)(i) = 1 Then
                                    SecCount.AnzSec.Add(1)
                                Else
                                    SecCount.AnzSec.Add(0)
                                End If
                            End If

                            ' Count the valid sections
                            If ErgValuesComp(tCompErg.SecID)(i) = ErgValuesComp(tCompErg.SecID)(j) And ErgValuesComp(tCompErg.DirID)(i) = ErgValuesComp(tCompErg.DirID)(j) And ErgValuesComp(tCompErg.RunID)(i) = ErgValuesComp(tCompErg.RunID)(j) Then
                                If ErgValuesComp(tCompErg.valid)(j) = 1 Then
                                    SecCount.AnzSec(SecCount.AnzSec.Count - 1) += 1
                                End If
                            End If
                        Next j
                    End If
                End If
            End If
        Next i

        ' Check if enough sections are detected
        If SecCount.AnzSec.Count - 1 < 1 Then
            Throw New Exception(format("Insufficient numbers of valid measurement sections({0}) in the low speed test available!", SecCount.AnzSec.Count))
        End If

        ' Check if enough valid sections in both directionsection
        For i = 0 To SecCount.NameSec.Count - 1
            For j = i + 1 To SecCount.NameSec.Count - 1
                If Trim(Mid(SecCount.NameSec(i), 1, InStr(SecCount.NameSec(i), "(") - 2)) = Trim(Mid(SecCount.NameSec(j), 1, InStr(SecCount.NameSec(j), "(") - 2)) And _
                   Trim(Mid(SecCount.NameSec(i), InStr(SecCount.NameSec(i), "(") + 1, InStr(SecCount.NameSec(i), ",") - (InStr(SecCount.NameSec(i), "(") + 1))) = Trim(Mid(SecCount.NameSec(j), InStr(SecCount.NameSec(j), "(") + 1, InStr(SecCount.NameSec(j), ",") - (InStr(SecCount.NameSec(j), "(") + 1))) Then
                    ' If enough sections in both directions are detected
                    If SecCount.AnzSec(i) >= Crt.segruns_min_LS And SecCount.AnzSec(j) >= Crt.segruns_min_LS Then

                        ' If not both the same number
                        If Not SecCount.AnzSec(i) = SecCount.AnzSec(j) Then
                            ' First section greater then second
                            If SecCount.AnzSec(i) > SecCount.AnzSec(j) Then
                                anz = SecCount.AnzSec(i) - SecCount.AnzSec(j)
                                For k = 0 To ErgValuesComp(tCompErg.SecID).Count - 1
                                    If (Trim(Mid(SecCount.NameSec(i), 1, InStr(SecCount.NameSec(i), "(") - 2)) = ErgValuesComp(tCompErg.SecID)(k)) And (Trim(Mid(SecCount.NameSec(i), InStr(SecCount.NameSec(i), "(") + 1, InStr(SecCount.NameSec(i), ",") - (InStr(SecCount.NameSec(i), "(") + 1))) = ErgValuesComp(tCompErg.DirID)(k)) And (Trim(Mid(SecCount.NameSec(i), InStr(SecCount.NameSec(i), ",") + 1, InStr(SecCount.NameSec(i), ")") - (InStr(SecCount.NameSec(i), ",") + 1))) = ErgValuesComp(tCompErg.RunID)(k)) Then
                                        If ErgValuesComp(tCompErg.used)(k) = 1 Then
                                            anz -= 1
                                            If anz >= 0 Then
                                                ErgValuesComp(tCompErg.used)(k) = 0
                                            End If
                                        End If
                                    End If
                                Next k
                            Else
                                ' Second section greater then first
                                anz = SecCount.AnzSec(j) - SecCount.AnzSec(i)
                                For k = 0 To ErgValuesComp(tCompErg.SecID).Count - 1
                                    If (Trim(Mid(SecCount.NameSec(j), 1, InStr(SecCount.NameSec(j), "(") - 2)) = ErgValuesComp(tCompErg.SecID)(k)) And (Trim(Mid(SecCount.NameSec(j), InStr(SecCount.NameSec(j), "(") + 1, InStr(SecCount.NameSec(j), ",") - (InStr(SecCount.NameSec(j), "(") + 1))) = ErgValuesComp(tCompErg.DirID)(k)) And (Trim(Mid(SecCount.NameSec(j), InStr(SecCount.NameSec(j), ",") + 1, InStr(SecCount.NameSec(j), ")") - (InStr(SecCount.NameSec(j), ",") + 1))) = ErgValuesComp(tCompErg.RunID)(k)) Then
                                        If ErgValuesComp(tCompErg.used)(k) = 1 Then
                                            anz -= 1
                                            If anz >= 0 Then
                                                ErgValuesComp(tCompErg.used)(k) = 0
                                            End If
                                        End If
                                    End If
                                Next k
                            End If
                        End If
                    Else
                        logme(9, False, "Not enough valid data for low speed tests available in section " & Trim(Mid(SecCount.NameSec(i), 1, InStr(SecCount.NameSec(i), "(") - 2)))
                        ' TODO: is this an error?
                    End If
                End If
            Next j
        Next i

        '****** High Speed Test ******
        ' Initialisation
        SecCount = New cValidSec
        FirstIn = True
        anzHS1 = 0
        anzHS2 = 0

        ' Count the valid sections in both rounds
        For i = 0 To ErgValuesComp(tCompErg.SecID).Count - 1.0
            If ErgValuesComp(tCompErg.RunID)(i) = IDHS Then
                ' Initialisation
                control = False

                ' First in
                If FirstIn Then
                    SecCount.NameSec.Add(ErgValuesComp(tCompErg.SecID)(i) & " (" & ErgValuesComp(tCompErg.DirID)(i) & "," & ErgValuesComp(tCompErg.HeadID)(i) & ")")
                    SecCount.ValidSec.Add(False)
                    If ErgValuesComp(tCompErg.valid)(i) = 1 Then
                        SecCount.AnzSec.Add(1)
                    Else
                        SecCount.AnzSec.Add(0)
                    End If
                End If

                ' Look if section is checked
                For k = 0 To SecCount.NameSec.Count - 1
                    If Not FirstIn And i > 0 And SecCount.NameSec(k) = ErgValuesComp(tCompErg.SecID)(i) & " (" & ErgValuesComp(tCompErg.DirID)(i) & "," & ErgValuesComp(tCompErg.HeadID)(i) & ")" Then
                        control = True
                    End If
                Next k

                ' Count the valid section
                If control = False Then
                    If i = ErgValuesComp(tCompErg.SecID).Count - 1.0 Then
                        SecCount.NameSec.Add(ErgValuesComp(tCompErg.SecID)(i) & " (" & ErgValuesComp(tCompErg.DirID)(i) & "," & ErgValuesComp(tCompErg.HeadID)(i) & ")")
                        SecCount.ValidSec.Add(False)
                        If ErgValuesComp(tCompErg.valid)(i) = 1 Then
                            SecCount.AnzSec.Add(1)
                        Else
                            SecCount.AnzSec.Add(0)
                        End If
                    Else
                        For j = i + 1 To ErgValuesComp(tCompErg.SecID).Count - 1
                            ' Add a new count point
                            If i > 0 And j = i + 1 And Not FirstIn Then
                                SecCount.NameSec.Add(ErgValuesComp(tCompErg.SecID)(i) & " (" & ErgValuesComp(tCompErg.DirID)(i) & "," & ErgValuesComp(tCompErg.HeadID)(i) & ")")
                                SecCount.ValidSec.Add(False)
                                If ErgValuesComp(tCompErg.valid)(i) = 1 Then
                                    SecCount.AnzSec.Add(1)
                                Else
                                    SecCount.AnzSec.Add(0)
                                End If
                            End If

                            ' Count the valid sections
                            If ErgValuesComp(tCompErg.SecID)(i) = ErgValuesComp(tCompErg.SecID)(j) And ErgValuesComp(tCompErg.DirID)(i) = ErgValuesComp(tCompErg.DirID)(j) And ErgValuesComp(tCompErg.HeadID)(i) = ErgValuesComp(tCompErg.HeadID)(j) And ErgValuesComp(tCompErg.RunID)(j) = IDHS Then
                                If ErgValuesComp(tCompErg.valid)(j) = 1 Then
                                    SecCount.AnzSec(SecCount.AnzSec.Count - 1) += 1
                                End If
                            End If

                            If FirstIn Then FirstIn = False
                        Next j
                    End If
                End If
            End If
        Next i

        ' Check if enough sections are detected
        If SecCount.AnzSec.Count - 1 < 1 Then
            Throw New Exception(format("Insufficient numbers of valid measurement sections({0}) in the high speed test available!", SecCount.AnzSec.Count))
        End If

        ' Check if enough valid sections in both directionsection
        For i = 0 To SecCount.NameSec.Count - 1
            ' If enough runs in the direction are detected
            If SecCount.AnzSec(i) >= Crt.segruns_min_HS Then
                ' Count the valid tests per HeadID
                Dim headId = Trim(Mid(SecCount.NameSec(i), InStr(SecCount.NameSec(i), ",") + 1, InStr(SecCount.NameSec(i), ")") - (InStr(SecCount.NameSec(i), ",") + 1)))
                Select Case headId
                    Case 1
                        anzHS1 += SecCount.AnzSec(i)
                    Case 2
                        anzHS2 += SecCount.AnzSec(i)
                    Case Else
                        Throw New Exception(format("Unknown headID({0})!", headId))
                End Select
            Else
                Throw New Exception(format("Not enough valid data({0}) for high speed tests available in section({1})!", SecCount.AnzSec(i), Trim(Mid(SecCount.NameSec(i), 1, InStr(SecCount.NameSec(i), "(") - 2))))
            End If
        Next i

        ' Check if enough sections are detected
        If anzHS1 < Crt.segruns_min_head_HS Or anzHS2 < Crt.segruns_min_head_HS Then
            Throw New Exception(format("Number of valid high speed datasets too low! HeadDir1: {0}; HeadDir2: {1}.", anzHS1, anzHS2))
        End If
    End Sub

    ' Evaluate the Valid sections
    Sub fCalcValidSec(ByVal MSCX As cMSC, ByVal vehicleX As cVehicle, ByVal coastingSeq As Integer, ByVal Run As Integer, ByVal r_dyn_ref As Double, ByRef Change As Boolean)
        ' Declaration
        Dim i As Integer
        Dim lim_v_veh_avg_max_HS, lim_v_veh_avg_min_HS As Single
        Dim OldValid(ErgValues(tCompErg.SecID).Count - 1), OldUse(ErgValues(tCompErg.SecID).Count - 1) As Boolean
        Dim igear As Double
        Dim allFalse As Boolean

        ' Initialization
        Change = False
        allFalse = True

        ' Evaluation
        Select Case coastingSeq
            Case 1, 2 ' Low speed test
                If SMT_AMT_DCT Then
                    igear = vehicleX.gearRatio_low
                Else
                    igear = 1
                End If
                ' Reset the ErgValues for the criterias
                ResetErgVal()
                For i = 0 To ErgValues(tCompErg.SecID).Count - 1
                    ' Identify which criteria is not valid
                    If ErgValues(tCompErg.user_valid)(i) = 1 Then ErgValues(tCompErg.val_User)(i) = 1
                    If ErgValues(tCompErg.v_veh)(i) < Crt.v_veh_avg_max_LS And
                       ErgValues(tCompErg.v_veh)(i) > Crt.v_veh_avg_min_LS Then ErgValues(tCompErg.val_vVeh_avg)(i) = 1
                    If ErgValues(tCompErg.v_veh_float_max)(i) < (ErgValues(tCompErg.v_veh)(i) + Crt.v_veh_float_delta_LS) And
                       ErgValues(tCompErg.v_veh_float_min)(i) > (ErgValues(tCompErg.v_veh)(i) - Crt.v_veh_float_delta_LS) Then ErgValues(tCompErg.val_vVeh_f)(i) = 1
                    If (ErgValues(tCompErg.tq_sum_float_max)(i) - ErgValues(tCompErg.tq_grd)(i)) < ((ErgValues(tCompErg.tq_sum)(i) - ErgValues(tCompErg.tq_grd)(i)) * (1 + Crt.tq_sum_float_delta_LS)) And
                       (ErgValues(tCompErg.tq_sum_float_min)(i) - ErgValues(tCompErg.tq_grd)(i)) > ((ErgValues(tCompErg.tq_sum)(i) - ErgValues(tCompErg.tq_grd)(i)) * (1 - Crt.tq_sum_float_delta_LS)) Then ErgValues(tCompErg.val_tq_f)(i) = 1
                    If ErgValues(tCompErg.n_ec_float_max)(i) < ((30 * igear * vehicleX.axleRatio * (ErgValues(tCompErg.v_veh)(i) + Crt.v_veh_float_delta_LS) / 3.6) / (r_dyn_ref * Math.PI)) * (1 + Crt.delta_n_ec_LS) And
                       ErgValues(tCompErg.n_ec_float_min)(i) > ((30 * igear * vehicleX.axleRatio * (ErgValues(tCompErg.v_veh)(i) - Crt.v_veh_float_delta_LS) / 3.6) / (r_dyn_ref * Math.PI)) * (1 - Crt.delta_n_ec_LS) Then ErgValues(tCompErg.val_n_eng)(i) = 1
                    If ErgValues(tCompErg.dist)(i) < fSecLen(MSCX, ErgValues(tCompErg.SecID)(i), ErgValues(tCompErg.DirID)(i)) + Crt.leng_crit And
                       ErgValues(tCompErg.dist)(i) > fSecLen(MSCX, ErgValues(tCompErg.SecID)(i), ErgValues(tCompErg.DirID)(i)) - Crt.leng_crit Then ErgValues(tCompErg.val_dist)(i) = 1
                    If ErgValues(tCompErg.t_amb_veh)(i) > Crt.t_amb_min And
                       ErgValues(tCompErg.t_amb_veh)(i) < Crt.t_amb_max Then ErgValues(tCompErg.val_t_amb)(i) = 1
                    If ErgValues(tCompErg.t_ground)(i) < Crt.t_ground_max Then ErgValues(tCompErg.val_t_ground)(i) = 1

                    ' Check if all criterias are valid
                    If ErgValues(tCompErg.val_User)(i) = 1 And ErgValues(tCompErg.val_vVeh_avg)(i) = 1 And ErgValues(tCompErg.val_vVeh_f)(i) = 1 And
                        ErgValues(tCompErg.val_tq_f)(i) = 1 And ErgValues(tCompErg.val_n_eng)(i) = 1 And ErgValues(tCompErg.val_dist)(i) = 1 And
                        ErgValues(tCompErg.val_t_amb)(i) = 1 And ErgValues(tCompErg.val_t_ground)(i) = 1 And
                        ErgValues(tCompErg.system_valid)(i) = 1 Then
                        ErgValues(tCompErg.valid)(i) = 1
                        ErgValues(tCompErg.used)(i) = 1
                        allFalse = False
                    Else
                        ErgValues(tCompErg.valid)(i) = 0
                        ErgValues(tCompErg.used)(i) = 0
                    End If

                    ' Set the only used in HS test criterias on valid
                    ErgValues(tCompErg.val_vWind)(i) = 1
                    ErgValues(tCompErg.val_vWind_1s)(i) = 1
                    ErgValues(tCompErg.val_beta)(i) = 1
                    ErgValues(tCompErg.val_vVeh_1s)(i) = 1
                    ErgValues(tCompErg.val_tq_1s)(i) = 1
                Next i
            Case Else ' high speed test
                If SMT_AMT_DCT Then
                    igear = vehicleX.gearRatio_high
                Else
                    igear = 1
                End If
                ' Save the old values
                For i = 0 To ErgValues(tCompErg.SecID).Count - 1
                    OldValid(i) = ErgValues(tCompErg.valid)(i)
                    OldUse(i) = ErgValues(tCompErg.used)(i)
                Next i

                ' Reset the ErgValues for the criterias
                ResetErgVal()

                ' Control the criterias
                fgetSpeedLim(vehicleX, lim_v_veh_avg_max_HS, lim_v_veh_avg_min_HS)
                For i = 0 To ErgValues(tCompErg.SecID).Count - 1
                    ' Identify whitch criteria is not valid
                    If ErgValues(tCompErg.user_valid)(i) = 1 Then ErgValues(tCompErg.val_User)(i) = 1
                    If ErgValues(tCompErg.v_veh)(i) < lim_v_veh_avg_max_HS And _
                       ErgValues(tCompErg.v_veh)(i) > lim_v_veh_avg_min_HS Then ErgValues(tCompErg.val_vVeh_avg)(i) = 1
                    If ErgValues(tCompErg.v_wind_avg)(i) < Crt.v_wind_avg_max_HS Then ErgValues(tCompErg.val_vWind)(i) = 1
                    If ErgValues(tCompErg.v_wind_1s_max)(i) < Crt.v_wind_1s_max_HS Then ErgValues(tCompErg.val_vWind_1s)(i) = 1
                    If ErgValues(tCompErg.beta_abs)(i) < Crt.beta_avg_max_HS Then ErgValues(tCompErg.val_beta)(i) = 1
                    If ErgValues(tCompErg.v_veh_1s_max)(i) < (ErgValues(tCompErg.v_veh)(i) + Crt.v_veh_1s_delta_HS) And _
                       ErgValues(tCompErg.v_veh_1s_min)(i) > (ErgValues(tCompErg.v_veh)(i) - Crt.v_veh_1s_delta_HS) Then ErgValues(tCompErg.val_vVeh_1s)(i) = 1
                    If (ErgValues(tCompErg.tq_sum_1s_max)(i) - ErgValues(tCompErg.tq_grd)(i)) < ((ErgValues(tCompErg.tq_sum)(i) - ErgValues(tCompErg.tq_grd)(i)) * (1 + Crt.tq_sum_1s_delta_HS)) And _
                       (ErgValues(tCompErg.tq_sum_1s_min)(i) - ErgValues(tCompErg.tq_grd)(i)) > ((ErgValues(tCompErg.tq_sum)(i) - ErgValues(tCompErg.tq_grd)(i)) * (1 - Crt.tq_sum_1s_delta_HS)) Then ErgValues(tCompErg.val_tq_1s)(i) = 1
                    If ErgValues(tCompErg.n_ec_1s_max)(i) < ((30 * igear * vehicleX.axleRatio * (ErgValues(tCompErg.v_veh)(i) + Crt.v_veh_1s_delta_HS) / 3.6) / (r_dyn_ref * Math.PI)) * (1 + Crt.delta_n_ec_HS) And _
                       ErgValues(tCompErg.n_ec_1s_min)(i) > ((30 * igear * vehicleX.axleRatio * (ErgValues(tCompErg.v_veh)(i) - Crt.v_veh_1s_delta_HS) / 3.6) / (r_dyn_ref * Math.PI)) * (1 - Crt.delta_n_ec_HS) Then ErgValues(tCompErg.val_n_eng)(i) = 1
                    If ErgValues(tCompErg.dist)(i) < fSecLen(MSCX, ErgValues(tCompErg.SecID)(i), ErgValues(tCompErg.DirID)(i)) + Crt.leng_crit And _
                       ErgValues(tCompErg.dist)(i) > fSecLen(MSCX, ErgValues(tCompErg.SecID)(i), ErgValues(tCompErg.DirID)(i)) - Crt.leng_crit Then ErgValues(tCompErg.val_dist)(i) = 1
                    If ErgValues(tCompErg.t_amb_veh)(i) > Crt.t_amb_min And _
                       ErgValues(tCompErg.t_amb_veh)(i) < Crt.t_amb_max Then ErgValues(tCompErg.val_t_amb)(i) = 1
                    If ErgValues(tCompErg.t_ground)(i) < Crt.t_ground_max Then ErgValues(tCompErg.val_t_ground)(i) = 1

                    ' Check if all criterias are valid
                    If ErgValues(tCompErg.val_User)(i) = 1 And ErgValues(tCompErg.val_vVeh_avg)(i) = 1 And ErgValues(tCompErg.val_vWind)(i) = 1 And ErgValues(tCompErg.val_vWind_1s)(i) = 1 And ErgValues(tCompErg.val_beta)(i) = 1 And ErgValues(tCompErg.val_vVeh_1s)(i) = 1 And
                       ErgValues(tCompErg.val_tq_1s)(i) = 1 And ErgValues(tCompErg.val_n_eng)(i) = 1 And ErgValues(tCompErg.val_dist)(i) = 1 And ErgValues(tCompErg.val_t_amb)(i) = 1 And ErgValues(tCompErg.val_t_ground)(i) = 1 And
                       ErgValues(tCompErg.system_valid)(i) = 1 Then
                        ErgValues(tCompErg.valid)(i) = 1
                        ErgValues(tCompErg.used)(i) = 1
                        allFalse = False
                    Else
                        ErgValues(tCompErg.valid)(i) = 0
                        ErgValues(tCompErg.used)(i) = 0
                    End If

                    ' Set the only used in LS test criterias on valid
                    ErgValues(tCompErg.val_vVeh_f)(i) = 1
                    ErgValues(tCompErg.val_tq_f)(i) = 1
                Next i

                ' Look if something have changed
                If Run <> 0 Then
                    For i = 0 To ErgValues(tCompErg.SecID).Count - 1
                        If Not Int(OldValid(i)) = ErgValues(tCompErg.valid)(i) And Not Int(OldUse(i)) = ErgValues(tCompErg.used)(i) Then
                            Change = True
                        End If
                    Next i
                Else
                    Change = True
                End If

                ' Look if something is true
                If allFalse Then
                    logme(9, False, "No used/valid section is found for calculation of fv_veh and fv_pe in HS test!")
                    Change = False
                End If
        End Select
    End Sub

    ' Get the maximum allowed speed limit
    Private Function fgetSpeedLim(ByVal vehicle As cVehicle, ByRef lim_v_veh_avg_max_HS As Single, ByRef lim_v_veh_avg_min_HS As Single) As Boolean
        ' Get the limits dependent of maximum vehicle speed
        lim_v_veh_avg_max_HS = Crt.v_veh_avg_max_HS
        If vehicle.vVehMax < (Crt.v_veh_avg_min_HS + Crt.delta_v_avg_min_HS) Then
            lim_v_veh_avg_min_HS = vehicle.vVehMax - Crt.delta_v_avg_min_HS
        Else
            lim_v_veh_avg_min_HS = Crt.v_veh_avg_min_HS
        End If
        Return True
    End Function

    ' Check the altitude files
    Sub fcheckAlt(ByVal MSCOrg As cMSC, ByRef Altdata As List(Of cAlt))
        ' Declarations
        Dim i, j As Integer
        Dim distAlt As Double
        Dim DiffAltDist, Diffhp, StartPIn, EndPIn, SlopeOut As Boolean
        Dim UTMCoordA As New cUTMCoord
        Dim UTMCoordE As New cUTMCoord
        Dim KoordA As Array
        Dim KoordE As Array
        Dim KoordP As Array
        Dim ErgHHF As New cHHF

        ' Check the altitude files
        ' Output on the GUI
        logme(5, False, "Check altitude files")

        For i = 1 To Altdata.Count - 1
            ' Calculation of the MS UTM-Coordinates
            UTMCoordA = UTM(MSCOrg.latS(i) / 60, MSCOrg.longS(i) / 60)
            UTMCoordE = UTM(MSCOrg.latE(i) / 60, MSCOrg.longE(i) / 60)
            KoordA = ({UTMCoordA.Northing, UTMCoordA.Easting})
            KoordE = ({UTMCoordE.Northing, UTMCoordE.Easting})

            ' Check the altitude files
            DiffAltDist = False
            Diffhp = False
            StartPIn = False
            EndPIn = False
            SlopeOut = False
            For j = 0 To Altdata(i).KoordLat.Count - 1
                ' Generate the coordinate array
                KoordP = ({Altdata(i).UTM(j).Northing, Altdata(i).UTM(j).Easting})

                ' Calculate the Höhe-Höhenfüßpunkt values
                ErgHHF = HHF(KoordA, KoordE, KoordP)

                ' Check if Startpoint is outside the MS
                If j = 0 And ErgHHF.p > 0 And ErgHHF.q > 0 Then StartPIn = True

                ' Check if the allowed distance between MS center line and altitude grid points is <= dist_grid_ms_max
                If ErgHHF.hp > Crt.dist_grid_ms_max Then
                    logme(9, False, "Point " & (j + 1) & ": hp (" & ErgHHF.hp & "), allowed hp (" & Crt.dist_grid_ms_max & ")")
                    Diffhp = True
                End If

                ' Check if the difference between the altitude input points is <= criterium dist_gridpoints_max
                If j > 0 Then
                    distAlt = Math.Sqrt(Math.Pow(Altdata(i).UTM(j).Easting - Altdata(i).UTM(j - 1).Easting, 2) + Math.Pow(Altdata(i).UTM(j).Northing - Altdata(i).UTM(j - 1).Northing, 2))
                    If distAlt > Crt.dist_gridpoints_max Then
                        logme(9, False, "Point " & (j - 1) & " - " & j & ": dist (" & distAlt & "), allowed dist (" & Crt.dist_gridpoints_max & ")")
                        DiffAltDist = True
                    End If
                End If

                ' Check if the last Point is outside the MS
                If j = Altdata(i).KoordLat.Count - 1 And ErgHHF.p > 0 And ErgHHF.q > 0 Then EndPIn = True

                ' Check if the slope is < the criterium slope_max
                If j = Altdata(i).KoordLat.Count - 1 And Not StartPIn And Not EndPIn Then
                    If ((Math.Abs(fAltInterp(Altdata(i), UTMCoordA.Northing, UTMCoordA.Easting) - fAltInterp(Altdata(i), UTMCoordE.Northing, UTMCoordE.Easting)) / MSCOrg.len(i)) * 100 > Crt.slope_max) Then SlopeOut = True
                End If
            Next j
            If StartPIn Then Throw New Exception(format("Invalid altitude data file ({0}). First value lies inside the MS.", fName(MSCOrg.AltPath(i), True)))
            If EndPIn Then Throw New Exception(format("Invalid altitude data file ({0}). Last value lies inside the MS.", fName(MSCOrg.AltPath(i), True)))
            If DiffAltDist Then Throw New Exception(format("Invalid altitude data file ({0}). The difference between the altitude points is > {1}.", fName(MSCOrg.AltPath(i), True), Crt.dist_gridpoints_max))
            If Diffhp Then Throw New Exception(format("Invalid altitude data file ({0}). The altitude grid points differ more then {1}m from MS center line.", fName(MSCOrg.AltPath(i), True), Crt.dist_grid_ms_max))
            If SlopeOut Then Throw New Exception(format("Invalid altitude data file ({0}). The maximum allowed slope {1}% is exceeded.", fName(MSCOrg.AltPath(i), True), Crt.slope_max))
        Next i
    End Sub

    ' Check the vehicle file
    Private Function fCheckVeh(ByVal AnzDigit As Integer, ByVal vehicle As cVehicle) As Boolean
        ' Declaration
        Dim Flag As Boolean = True

        ' Check the vehicle class
        Select Case vehicle.classCode
            Case tVehClass.Class1, tVehClass.Class2, tVehClass.Class3, tVehClass.Class4, tVehClass.Class5, tVehClass.Class9, tVehClass.Class10, tVehClass.Class53, tVehClass.Class54, tVehClass.Class31b2, tVehClass.Class32a, tVehClass.Class32b, tVehClass.Class32c, tVehClass.Class32d, tVehClass.Class32e, tVehClass.Class32f, tVehClass.Class33b2, tVehClass.Class34a, tVehClass.Class34b, tVehClass.Class34c, tVehClass.Class34d, tVehClass.Class34e, tVehClass.Class34f, tVehClass.Class35b2, tVehClass.Class36a, tVehClass.Class36b, tVehClass.Class36c, tVehClass.Class36d, tVehClass.Class36e, tVehClass.Class36f, tVehClass.Class37b2, tVehClass.Class38a, tVehClass.Class38b, tVehClass.Class38c, tVehClass.Class38d, tVehClass.Class38e, tVehClass.Class38f, tVehClass.Class39b2, tVehClass.Class40a, tVehClass.Class40b, tVehClass.Class40c, tVehClass.Class40d, tVehClass.Class40e, tVehClass.Class40f
                ' Check the hight of the vehicle
                If Job.mode = 1 Then
                    If (vehicle.vehHeight > GenShape.h_max) Then
                        Flag = False
                        logme(9, False, format("Vehicle height grater then allowed vehicle height (vehicle: {0} > maximum: {1})!", vehicle.vehHeight, GenShape.h_max))
                    End If
                    If (vehicle.vehHeight < GenShape.h_min) Then
                        Flag = False
                        logme(9, False, format("Vehicle height smaller then allowed vehicle height (vehicle: {0} < minimum: {1})!", vehicle.vehHeight, GenShape.h_min))
                    End If
                End If
            Case Else
                Flag = False
                logme(9, False, format("Vehicle class not supported ({0})!", vehicle.classCode))
        End Select

        ' Check the configuration
        If Not GenShape.valid Then
            Flag = False
            logme(9, False, format("The configuration from the vehicle (class: {0}, configuration {1}) was not supported by the generic shape file. \n\iPlease change to supported values.", vehicle.classCode, vehicle.configuration))
        End If

        ' Check the geraRatio_high
        If Not fCheckDigits(Prefs.decSep, AnzDigit, vehicle.gearRatio_high) Then
            Flag = False
            If Job.mode = 1 Then
                logme(9, False, format("The gearRatio_high in the vehicle file ({0}) have not enough digits after the decimal seperator (minimum digits are ({1})!", Job.vehicle_fpath, AnzDigit))
            Else
                logme(8, False, format("The gearRatio_high in the vehicle file ({0}) have not enough digits after the decimal seperator (minimum digits are ({1})!", Job.vehicle_fpath, AnzDigit))
            End If
        End If
        ' Check the geraRatio_low
        If Not fCheckDigits(Prefs.decSep, AnzDigit, vehicle.gearRatio_low) Then
            Flag = False
            If Job.mode = 1 Then
                logme(9, False, format("The gearRatio_low in the vehicle file ({0}) have not enough digits after the decimal seperator (minimum digits are ({1})!", Job.vehicle_fpath, AnzDigit))
            Else
                logme(8, False, format("The gearRatio_low in the vehicle file ({0}) have not enough digits after the decimal seperator (minimum digits are ({1})!", Job.vehicle_fpath, AnzDigit))
            End If
        End If
        ' Check the axleRatio
        If Not fCheckDigits(Prefs.decSep, AnzDigit, vehicle.axleRatio) Then
            Flag = False
            If Job.mode = 1 Then
                logme(9, False, format("The axleRatio in the vehicle file ({0}) have not enough digits after the decimal seperator (minimum digits are ({1})!", Job.vehicle_fpath, AnzDigit))
            Else
                logme(8, False, format("The axleRatio in the vehicle file ({0}) have not enough digits after the decimal seperator (minimum digits are ({1})!", Job.vehicle_fpath, AnzDigit))
            End If
        End If

        'Check the left drift
        If Not vehicle.OldVehicle Then
            If Math.Abs(Convert.ToDouble(vehicle.tqDriftLeft)) > 25 Then
                Flag = False
                logme(9, False, format("The tqLeftDrift in the vehicle file ({0}) is greater then |25|!", Job.vehicle_fpath))
            End If
            'Check the right drift
            If Math.Abs(Convert.ToDouble(vehicle.tqDriftRight)) > 25 Then
                Flag = False
                logme(9, False, format("The tqRightDrift in the vehicle file ({0}) is greater then |25|!", Job.vehicle_fpath))
            End If
        Else
            logme(8, False, format("Old vehicle file is used. CALCULATION WITHOUT DRIFT CORRECTION! ({0})", Job.vehicle_fpath))
        End If

        'Check the criteria parameter dependent of the vehicle class and calculation mode
        If SetCritVeh(Job.mode, vehicle.classCode) Then
            logme(7, False, format("Criteria parameter tq_sum_float_delta_LS set to new value ({0})", Crt.tq_sum_float_delta_LS))
            logme(7, False, format("Criteria parameter v_veh_avg_max_HS set to new value ({0})", Crt.v_veh_avg_max_HS))
        End If

        Return Flag
    End Function

    ' Check the digits after the seperator from an value
    Private Function fCheckDigits(ByVal Sep As Char, ByVal AnzDigit As Integer, ByVal dvalue As String) As Boolean
        ' Declaration
        Dim counter As Long = 0

        If dvalue.Substring(dvalue.IndexOf(Sep) + 1).Length < AnzDigit Then
            Return False
        End If

        Return True
    End Function

    ' Set the criteria parameters dependent of the vehicle class and mode
    Private Function SetCritVeh(ByVal mode As Integer, ByVal vehicleclass As tVehClass) As Boolean
        If mode = 1 Then
            Select Case (vehicleclass)
                Case tVehClass.Class1, tVehClass.Class2, tVehClass.Class53, tVehClass.Class54
                    Crt.tq_sum_float_delta_LS = 0.5
                Case tVehClass.Class3, tVehClass.Class4, tVehClass.Class5, tVehClass.Class9, tVehClass.Class10
                    Crt.tq_sum_float_delta_LS = 0.3
                Case tVehClass.Class31b2,
                     tVehClass.Class32a,
                     tVehClass.Class32b,
                     tVehClass.Class32c,
                     tVehClass.Class32d,
                     tVehClass.Class32e,
                     tVehClass.Class32f,
                     tVehClass.Class33b2,
                     tVehClass.Class34a,
                     tVehClass.Class34b,
                     tVehClass.Class34c,
                     tVehClass.Class34d,
                     tVehClass.Class34e,
                     tVehClass.Class34f,
                     tVehClass.Class35b2,
                     tVehClass.Class36a,
                     tVehClass.Class36b,
                     tVehClass.Class36c,
                     tVehClass.Class36d,
                     tVehClass.Class36e,
                     tVehClass.Class36f,
                     tVehClass.Class37b2,
                     tVehClass.Class38a,
                     tVehClass.Class38b,
                     tVehClass.Class38c,
                     tVehClass.Class38d,
                     tVehClass.Class38e,
                     tVehClass.Class38f,
                     tVehClass.Class39b2,
                     tVehClass.Class40a,
                     tVehClass.Class40b,
                     tVehClass.Class40c,
                     tVehClass.Class40d,
                     tVehClass.Class40e,
                     tVehClass.Class40f
                    Crt.tq_sum_float_delta_LS = 0.3
                    Crt.v_veh_avg_max_HS = If(TransitionalService.Use2ndAmendment, 103, 102)
                    If (Not TransitionalService.Use2ndAmendment) Then
                        Crt.v_veh_avg_min_HS = 97
                    End If
            End Select
            Return True
        End If

        Return False
    End Function

    ' Save the Dictionaries
    Sub fSaveDic(ByVal coastingSeq As Integer)
        ' Declaration
        Dim sKV As New KeyValuePair(Of tCompErg, List(Of Double))
        Dim sKVUndef As New KeyValuePair(Of String, List(Of Double))

        ' Initialisation
        If coastingSeq = 0 Then
            ErgValuesComp = New Dictionary(Of tCompErg, List(Of Double))
            ErgValuesUndefComp = New Dictionary(Of String, List(Of Double))

            For Each sKV In ErgValues
                ErgValuesComp.Add(sKV.Key, New List(Of Double))
            Next

            For Each sKVUndef In ErgValuesUndef
                ErgValuesUndefComp.Add(sKVUndef.Key, New List(Of Double))
            Next

            ' Transfer the ResultValues in the complet result file
            ErgValuesComp = ErgValues
            ErgValuesUndefComp = ErgValuesUndef
        Else
            ' Add the ResultValues to the complet dictionary
            For Each sKV In ErgValues
                ErgValuesComp(sKV.Key).AddRange(ErgValues(sKV.Key))
            Next
            For Each sKVUndef In ErgValuesUndef
                ErgValuesUndefComp(sKVUndef.Key).AddRange(ErgValuesUndef(sKVUndef.Key))
            Next
        End If
    End Sub

    ' Reset the ErgValues
    Private Function ResetErgVal(Optional ByVal calib As Boolean = False) As Boolean
        ' Deklaration
        Dim i As Integer

        If calib Then
            For i = 0 To ErgValues(tCompErg.SecID).Count - 1
                ErgValues(tCompErg.val_vWind)(i) = 0
                ErgValues(tCompErg.val_beta)(i) = 0
                ErgValues(tCompErg.val_vWind_1s)(i) = 0
                ErgValues(tCompErg.val_vVeh_avg)(i) = 0
                ErgValues(tCompErg.val_vVeh_1s)(i) = 0
                ErgValues(tCompErg.val_User)(i) = 0
            Next i
        Else
            For i = 0 To ErgValues(tCompErg.SecID).Count - 1
                ErgValues(tCompErg.val_User)(i) = 0
                ErgValues(tCompErg.val_vVeh_avg)(i) = 0
                ErgValues(tCompErg.val_vVeh_f)(i) = 0
                ErgValues(tCompErg.val_tq_f)(i) = 0
                ErgValues(tCompErg.val_n_eng)(i) = 0
                ErgValues(tCompErg.val_dist)(i) = 0
                ErgValues(tCompErg.val_t_amb)(i) = 0
                ErgValues(tCompErg.val_t_ground)(i) = 0
                ErgValues(tCompErg.val_vWind)(i) = 0
                ErgValues(tCompErg.val_vWind_1s)(i) = 0
                ErgValues(tCompErg.val_beta)(i) = 0
                ErgValues(tCompErg.val_vVeh_1s)(i) = 0
                ErgValues(tCompErg.val_tq_1s)(i) = 0
            Next i
        End If
        Return True
    End Function
End Module
