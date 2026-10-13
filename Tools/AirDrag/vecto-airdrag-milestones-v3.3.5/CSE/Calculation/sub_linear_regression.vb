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

Module sub_linear_regression

    ' Main function for the calculate the regression
    Public Function fCalcReg(ByVal vehicle As cVehicle) As Boolean
        ' Declaration
        Dim i, j, numLS1, numLS2, numHS, numHSg, numT, PosHS(), PosHSg(), lauf, anzLS1, anzLS2, anzHS As Integer
        Dim numH1, numH2 As Integer
        Dim XLS1_Array(,), XLS2_Array(,), XHS_Array(,), XHSg_Array(,), XHS_S(1, 1), YLS1_Array(), YLS2_Array(), YHS_Array(), YHSg_Array(), YHS_S(1) As Double
        Dim XLR(,), YLR(), WFLR(,), F0, F2, F095, F295, R2, Rho_air_LS1, Rho_air_LS2 As Double
        Dim EnumStr As tCompErgReg

        ' Output on the GUI
        logme(7, False, "Calculate the linear regression...")

        ' Initialisation
        lauf = -1
        anzLS1 = 0
        anzLS2 = 0
        anzHS = 0
        numH1 = 0
        numH2 = 0
        ErgValuesReg = New Dictionary(Of tCompErgReg, List(Of Double))
        Job.CdxAß_H1 = 0
        Job.beta_H1 = 0
        Job.CdxAß_H2 = 0
        Job.beta_H2 = 0
        Job.CdxAß = 0
        Job.CdxA0meas = 0
        Job.CdxA0 = 0
        Job.delta_CdxA_beta = 0
        Job.delta_CdxA_height = 0
        Job.beta = 0
        Job.t_amb_LS1 = 0
        Job.v_avg_LS = 0
        Job.v_avg_HS = 0
        Job.valid_RRC = True

        ' Generate the result dictionary variables
        For Each EnumStr In System.Enum.GetValues(GetType(tCompErgReg))
            ErgValuesReg.Add(EnumStr, New List(Of Double))
        Next

        ' Count the valid sections
        For i = 0 To ErgValuesComp(tCompErg.SecID).Count - 1
            If ErgValuesComp(tCompErg.calcT)(i) = 0 Then
                ' initialisation
                numLS1 = 0
                numLS2 = 0
                numHS = 0
                numHSg = 0
                lauf += 1
                Rho_air_LS1 = 0
                Rho_air_LS2 = 0
                ReDim XLS1_Array(1, 0)
                ReDim XLS2_Array(1, 0)
                ReDim XHS_Array(1, 0)
                ReDim YLS1_Array(0)
                ReDim YLS2_Array(0)
                ReDim YHS_Array(0)
                ReDim PosHS(0)
                ReDim PosHSg(0)
                ReDim XHSg_Array(1, 0)
                ReDim YHSg_Array(0)

                ' Save the SecID, DirID and HeadID in result dictionary
                ErgValuesReg(tCompErgReg.SecID).Add(ErgValuesComp(tCompErg.SecID)(i))
                ErgValuesReg(tCompErgReg.DirID).Add(ErgValuesComp(tCompErg.DirID)(i))
                ErgValuesReg(tCompErgReg.HeadID).Add(ErgValuesComp(tCompErg.HeadID)(i))
                ErgValuesReg(tCompErgReg.beta_ave_singleMS).Add(0)
                ErgValuesReg(tCompErgReg.CdxAß_ave_singleMS).Add(0)

                ' Go through all measurements
                For j = i To ErgValuesComp(tCompErg.SecID).Count - 1
                    ' Find all with the same SecID and DirID 
                    If ErgValuesComp(tCompErg.SecID)(i) = ErgValuesComp(tCompErg.SecID)(j) And ErgValuesComp(tCompErg.DirID)(i) = ErgValuesComp(tCompErg.DirID)(j) And ErgValuesComp(tCompErg.HeadID)(i) = ErgValuesComp(tCompErg.HeadID)(j) Then
                        ' Set there value to true
                        ErgValuesComp(tCompErg.calcT)(j) = 1

                        ' If the measurement is true add it
                        If ErgValuesComp(tCompErg.used)(j) = 1 Then
                            Select Case ErgValuesComp(tCompErg.RunID)(j)
                                Case IDLS1
                                    ' Initialise
                                    ReDim Preserve XLS1_Array(1, UBound(XLS1_Array, 2) + 1)
                                    ReDim Preserve YLS1_Array(UBound(YLS1_Array) + 1)
                                    numLS1 += 1

                                    ' Get the values
                                    XLS1_Array(0, UBound(XLS1_Array, 2)) = 1
                                    XLS1_Array(1, UBound(XLS1_Array, 2)) = ErgValuesComp(tCompErg.v_air_sq)(j)
                                    YLS1_Array(UBound(YLS1_Array)) = ErgValuesComp(tCompErg.F_res_ref)(j)

                                    ' Add values for v, t and rho_air into the result dictionary
                                    Rho_air_LS1 += ErgValuesComp(tCompErg.rho_air)(j)
                                    Job.v_avg_LS += ErgValuesComp(tCompErg.v_veh)(j)
                                    Job.t_amb_LS1 += ErgValuesComp(tCompErg.t_amb_veh)(j)
                                    anzLS1 += 1
                                Case IDLS2
                                    ' Initialise
                                    ReDim Preserve XLS2_Array(1, UBound(XLS2_Array, 2) + 1)
                                    ReDim Preserve YLS2_Array(UBound(YLS2_Array) + 1)
                                    numLS2 += 1

                                    ' Get the values
                                    XLS2_Array(0, UBound(XLS2_Array, 2)) = 1
                                    XLS2_Array(1, UBound(XLS2_Array, 2)) = ErgValuesComp(tCompErg.v_air_sq)(j)
                                    YLS2_Array(UBound(YLS2_Array)) = ErgValuesComp(tCompErg.F_res_ref)(j)

                                    ' Add values for v, t and rho_air into the result dictionary
                                    Rho_air_LS2 += ErgValuesComp(tCompErg.rho_air)(j)
                                    Job.v_avg_LS += ErgValuesComp(tCompErg.v_veh)(j)
                                    anzLS2 += 1
                                Case IDHS
                                    ' Initialise
                                    ReDim Preserve XHS_Array(1, UBound(XHS_Array, 2) + 1)
                                    ReDim Preserve YHS_Array(UBound(YHS_Array) + 1)
                                    ReDim Preserve PosHS(UBound(PosHS) + 1)
                                    numHS += 1

                                    ' Get the values
                                    PosHS(UBound(PosHS)) = j
                                    XHS_Array(0, UBound(XHS_Array, 2)) = 1
                                    XHS_Array(1, UBound(XHS_Array, 2)) = ErgValuesComp(tCompErg.v_air_sq)(j)
                                    YHS_Array(UBound(YHS_Array)) = ErgValuesComp(tCompErg.F_res_ref)(j)

                                    ' Add values for v, t and beta_HS into the result dictionary
                                    ErgValuesReg(tCompErgReg.beta_ave_singleMS)(lauf) += ErgValuesComp(tCompErg.beta_abs)(j)
                                    Job.v_avg_HS += ErgValuesComp(tCompErg.v_veh)(j)
                                    anzHS += 1
                            End Select
                        End If

                        ' Extra calculation with not used values
                        If ErgValuesComp(tCompErg.RunID)(j) = IDHS Then
                            ' Initialise
                            ReDim Preserve XHSg_Array(1, UBound(XHSg_Array, 2) + 1)
                            ReDim Preserve YHSg_Array(UBound(YHSg_Array) + 1)
                            ReDim Preserve PosHSg(UBound(PosHSg) + 1)
                            numHSg += 1

                            ' Get the values
                            PosHSg(UBound(PosHSg)) = j
                            XHSg_Array(0, UBound(XHSg_Array, 2)) = 1
                            XHSg_Array(1, UBound(XHSg_Array, 2)) = ErgValuesComp(tCompErg.v_air_sq)(j)
                            YHSg_Array(UBound(YHSg_Array)) = ErgValuesComp(tCompErg.F_res_ref)(j)
                        End If
                    End If
                Next j

                ' Check if the section is measured in every test run (LS1/2 and HS)
                If numLS1 >= 1 And numLS2 >= 1 And numHS >= 2 Then
                    '***** Calculate the linear regression for LS1
                    ' Redeminisionate the arrays
                    numT = numLS1 + numHS
                    ReDim XLR(1, numT)
                    ReDim YLR(numT)
                    ReDim WFLR(numT, numT)

                    ' Fill the linear regression arrays
                    fFillArray(XLS1_Array, XLS2_Array, YLS1_Array, YLS2_Array, XHS_Array, YHS_Array, XLR, YLR, WFLR, , False)

                    ' Do the linear regression
                    linear_regression(XLR, YLR, WFLR, F0, F2, F095, F295, R2)

                    ' Save the values
                    ErgValuesReg(tCompErgReg.F0_singleMS_LS1).Add(F0)
                    ErgValuesReg(tCompErgReg.F2_singleMS_LS1).Add(F2)
                    ErgValuesReg(tCompErgReg.RRC_singleMS_LS1).Add((ErgValuesReg(tCompErgReg.F0_singleMS_LS1)(lauf) / (vehicle.testMass * 9.81)) * 1000)

                    '***** Calculate the linear regression for LS2
                    ' Redeminisionate the arrays
                    numT = numLS2 + numHS
                    ReDim XLR(1, numT)
                    ReDim YLR(numT)
                    ReDim WFLR(numT, numT)

                    ' Fill the linear regression arrays
                    fFillArray(XLS1_Array, XLS2_Array, YLS1_Array, YLS2_Array, XHS_Array, YHS_Array, XLR, YLR, WFLR, False)

                    ' Do the linear regression
                    linear_regression(XLR, YLR, WFLR, F0, F2, F095, F295, R2)

                    ' Save the values
                    ErgValuesReg(tCompErgReg.F0_singleMS_LS2).Add(F0)
                    ErgValuesReg(tCompErgReg.F2_singleMS_LS2).Add(F2)
                    ErgValuesReg(tCompErgReg.RRC_singleMS_LS2).Add((ErgValuesReg(tCompErgReg.F0_singleMS_LS2)(lauf) / (vehicle.testMass * 9.81)) * 1000)

                    If Math.Abs(ErgValuesReg(tCompErgReg.RRC_singleMS_LS1)(lauf) - ErgValuesReg(tCompErgReg.RRC_singleMS_LS2)(lauf)) > Crt.delta_rr_max Then
                        ErgValuesReg(tCompErgReg.valid_RRC).Add(0)
                    Else
                        ErgValuesReg(tCompErgReg.valid_RRC).Add(1)
                    End If

                    '***** Calculate the linear regression for the MS
                    ' Redeminisionate the arrays
                    numT = numLS1 + numLS2 + numHS
                    ReDim XLR(1, numT)
                    ReDim YLR(numT)
                    ReDim WFLR(numT, numT)

                    ' Fill the linear regression arrays
                    fFillArray(XLS1_Array, XLS2_Array, YLS1_Array, YLS2_Array, XHS_Array, YHS_Array, XLR, YLR, WFLR)

                    ' Do the linear regression
                    linear_regression(XLR, YLR, WFLR, F0, F2, F095, F295, R2)

                    ' Save the values
                    ErgValuesReg(tCompErgReg.F0_singleMS).Add(F0)
                    ErgValuesReg(tCompErgReg.F2_singleMS).Add(F2)
                    ErgValuesReg(tCompErgReg.F0_singleMS_95).Add(F095)
                    ErgValuesReg(tCompErgReg.F2_singleMS_95).Add(F295)
                    ErgValuesReg(tCompErgReg.R_sq_singleMS).Add(R2)

                    ' Calculate CdxAß_singleDS for each HS
                    For j = 1 To numHSg
                        ErgValuesComp(tCompErg.CdxAß_singleDS)(PosHSg(j)) = 2 * (YHSg_Array(j) - F0) / (XHSg_Array(1, j) * ErgValuesComp(tCompErg.rho_air)(PosHSg(j)))
                        ' Summarise only CdxAß_singleDS values that are used
                        If ErgValuesComp(tCompErg.used)(PosHSg(j)) = 1 Then
                            ErgValuesReg(tCompErgReg.CdxAß_ave_singleMS)(lauf) += ErgValuesComp(tCompErg.CdxAß_singleDS)(PosHSg(j))
                        End If
                    Next j
                    ErgValuesReg(tCompErgReg.CdxAß_ave_singleMS)(lauf) = ErgValuesReg(tCompErgReg.CdxAß_ave_singleMS)(lauf) / numHS

                    ' Calculate additional values
                    ErgValuesReg(tCompErgReg.beta_ave_singleMS)(lauf) = ErgValuesReg(tCompErgReg.beta_ave_singleMS)(lauf) / (numHS)
                    ErgValuesReg(tCompErgReg.RRC_singleMS).Add(ErgValuesReg(tCompErgReg.F0_singleMS)(lauf) / (vehicle.testMass * 9.81) * 1000)
                    ErgValuesReg(tCompErgReg.delta_CdxA_singleMS).Add(fCalcGenShp(ErgValuesReg(tCompErgReg.beta_ave_singleMS)(lauf), vehicle))
                    ErgValuesReg(tCompErgReg.CdxA0_singleMS).Add(ErgValuesReg(tCompErgReg.CdxAß_ave_singleMS)(lauf) - ErgValuesReg(tCompErgReg.delta_CdxA_singleMS)(lauf))
                    ErgValuesReg(tCompErgReg.NumUsed).Add(numT)

                    ' Wighted summerise for the endresults
                    Select Case (ErgValuesComp(tCompErg.HeadID)(i))
                        Case 1
                            Job.CdxAß_H1 += ErgValuesReg(tCompErgReg.CdxAß_ave_singleMS)(lauf) * ErgValuesReg(tCompErgReg.NumUsed)(lauf)
                            Job.beta_H1 += ErgValuesReg(tCompErgReg.beta_ave_singleMS)(lauf) * ErgValuesReg(tCompErgReg.NumUsed)(lauf)
                            numH1 += ErgValuesReg(tCompErgReg.NumUsed)(lauf)
                        Case 2
                            Job.CdxAß_H2 += ErgValuesReg(tCompErgReg.CdxAß_ave_singleMS)(lauf) * ErgValuesReg(tCompErgReg.NumUsed)(lauf)
                            Job.beta_H2 += ErgValuesReg(tCompErgReg.beta_ave_singleMS)(lauf) * ErgValuesReg(tCompErgReg.NumUsed)(lauf)
                            numH2 += ErgValuesReg(tCompErgReg.NumUsed)(lauf)
                    End Select
                Else
                    ' Clear the data in the result dictionary
                    If ErgValuesReg(tCompErgReg.SecID).Count > 0 Then ErgValuesReg(tCompErgReg.SecID).RemoveAt(lauf)
                    If ErgValuesReg(tCompErgReg.DirID).Count > 0 Then ErgValuesReg(tCompErgReg.DirID).RemoveAt(lauf)
                    If ErgValuesReg(tCompErgReg.HeadID).Count > 0 Then ErgValuesReg(tCompErgReg.HeadID).RemoveAt(lauf)
                    If ErgValuesReg(tCompErgReg.beta_ave_singleMS).Count > 0 Then ErgValuesReg(tCompErgReg.beta_ave_singleMS).RemoveAt(lauf)
                    lauf -= 1
                End If
            End If
        Next i

        ' Calculate the Endresults
        If lauf <> -1 Then
            Job.CdxAß_H1 = Job.CdxAß_H1 / numH1
            Job.beta_H1 = Job.beta_H1 / numH1
            Job.CdxAß_H2 = Job.CdxAß_H2 / numH2
            Job.beta_H2 = Job.beta_H2 / numH2
            Job.CdxAß = (Job.CdxAß_H1 + Job.CdxAß_H2) / 2
            Job.beta = (Job.beta_H1 + Job.beta_H2) / 2
            Job.delta_CdxA_beta = fCalcGenShp(Job.beta, vehicle) * (-1)
            Job.CdxA0meas = Job.CdxAß + Job.delta_CdxA_beta
            Job.delta_CdxA_height = (Job.CdxA0meas * GenShape.h_ref / vehicle.vehHeight) - Job.CdxA0meas
            Job.CdxA0 = Job.CdxA0meas + Job.delta_CdxA_height + Crt.delta_CdxA_anemo
            Job.t_amb_LS1 = Job.t_amb_LS1 / anzLS1
            Job.v_avg_LS = Job.v_avg_LS / (anzLS1 + anzLS2)
            Job.v_avg_HS = Job.v_avg_HS / (anzHS)
        Else
            Job.CdxAß_H1 = 0
            Job.beta_H1 = 0
            Job.CdxAß_H2 = 0
            Job.beta_H2 = 0
            Job.CdxAß = 0
            Job.beta = 0
            Job.delta_CdxA_beta = 0
            Job.CdxA0meas = 0
            Job.delta_CdxA_height = 0
            Job.CdxA0 = 0
            Job.t_amb_LS1 = 0
            Job.v_avg_LS = 0
            Job.v_avg_HS = 0
        End If

        Return True
    End Function

    ' Function to fill the Arrays fpr the linear regression
    Private Function fFillArray(ByVal XLS1In(,) As Double, ByVal XLS2In(,) As Double, ByVal YLS1In() As Double, ByVal YLS2In() As Double, ByVal XHSIn(,) As Double, ByVal YHSIn() As Double, ByRef XOut(,) As Double, ByRef YOut() As Double, ByRef WFOut(,) As Double, Optional ByVal LS1T As Boolean = True, Optional ByVal LS2T As Boolean = True) As Boolean
        ' Declaration
        Dim num, numT As Integer
        Dim perc As Double

        ' initialisation
        num = 1
        numT = 0
        If LS1T Then
            numT += UBound(XLS1In, 2)
        End If
        If LS2T Then
            numT += UBound(XLS2In, 2)
        End If
        numT += UBound(XHSIn, 2)
        If LS1T And LS2T Then
            perc = 0.25
        Else
            perc = 0.5
        End If

        ' Low speed run 1
        If LS1T Then
            For k = 1 To UBound(XLS1In, 2)
                XOut(0, num) = XLS1In(0, k)
                XOut(1, num) = XLS1In(1, k)
                YOut(num) = YLS1In(k)
                WFOut(num, num) = perc / (UBound(XLS1In, 2) / numT)
                num += 1
            Next k
        End If

        ' High speed run
        For k = 1 To UBound(XHSIn, 2)
            XOut(0, num) = XHSIn(0, k)
            XOut(1, num) = XHSIn(1, k)
            YOut(num) = YHSIn(k)
            WFOut(num, num) = 0.5 / (UBound(XHSIn, 2) / numT)
            num += 1
        Next k

        ' Low Speed run 2
        If LS2T Then
            For k = 1 To UBound(XLS2In, 2)
                XOut(0, num) = XLS2In(0, k)
                XOut(1, num) = XLS2In(1, k)
                YOut(num) = YLS2In(k)
                WFOut(num, num) = perc / (UBound(XLS2In, 2) / numT)
                num += 1
            Next k
        End If
        Return True
    End Function

    ' Function to calculate interpolate delta_CdxA out of the generic shape
    Private Function fCalcGenShp(ByVal beta As Double, ByVal vehicleX As cVehicle) As Double
        ' Declaration
        Dim i As Integer
        Dim ValueX As Double

        ' Interpolate the value
        For i = 0 To GenShape.x_val.Length - 2
            If beta > GenShape.x_val(i) And beta < GenShape.x_val(i + 1) Then
                ValueX = InterpLinear(GenShape.x_val(i), GenShape.x_val(i + 1), GenShape.y_val(i), GenShape.y_val(i + 1), beta)
                Exit For
            End If
            If i = GenShape.x_val.Length - 1 And beta > GenShape.x_val(i + 1) Then
                ValueX = 0
                logme(8, False, "The calculated yaw angle is higher than the greatest value in the generic curve. Delta_CdxA is set to 0!")
            ElseIf i = 0 And GenShape.x_val(i) > beta Then
                ValueX = 0
                logme(8, False, "The calculated yaw angle is lower than the lowest value in the generic curve. Delta_CdxA is set to 0!")
            End If
        Next i

        Return ValueX
    End Function

    ' Calculate the linear regression
    Private Function linear_regression(ByVal X(,) As Double, ByVal Y() As Double, ByVal WF(,) As Double, ByRef beta1 As Double, ByRef beta2 As Double, ByRef CI_beta0_perc As Double, ByRef CI_beta1_perc As Double, ByRef R_sq As Double) As Boolean
        'Multiple intermediate values, description see below
        Dim X_T(,) As Double = {{}, {}}
        Dim INTERM_1(,) As Double = {{}, {}}
        Dim INTERM_2(,) As Double = {{}, {}}
        Dim INTERM_3(,) As Double = {{}, {}}
        Dim INTERM_4(,) As Double = {{}, {}}
        Dim V(,) As Double = {{}, {}}
        Dim V_T(,) As Double = {{}, {}}
        Dim V_mult_V_T(,) As Double = {{}, {}}
        Dim beta() As Double = {0, 0}
        Dim f_t95 = {{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 60, 70, 80, 90, 100, 120, 140, 160, 180, 200, 250, 300, 400, 500, 750, 1000, 2000, 5000, 10000, 20000, 50000, 100000, 200000, 500000, 1000000}, {12.706205, 4.3026527, 3.1824463, 2.7764451, 2.5705818, 2.4469119, 2.3646243, 2.3060041, 2.2621572, 2.2281389, 2.2009852, 2.1788128, 2.1603687, 2.1447867, 2.1314495, 2.1199053, 2.1098156, 2.100922, 2.0930241, 2.0859634, 2.0796138, 2.0738731, 2.0686576, 2.0638986, 2.0595386, 2.0555294, 2.0518305, 2.0484071, 2.0452296, 2.0422725, 2.0395134, 2.0369333, 2.0345153, 2.0322445, 2.0301079, 2.028094, 2.0261925, 2.0243942, 2.0226909, 2.0210754, 2.019541, 2.0180817, 2.0166922, 2.0153676, 2.0141034, 2.0128956, 2.0117405, 2.0106348, 2.0095752, 2.0085591, 2.0002978, 1.9944371, 1.9900634, 1.9866745, 1.9839715, 1.9799304, 1.9770537, 1.9749016, 1.9732308, 1.9718962, 1.9694984, 1.967903, 1.9659123, 1.9647198, 1.963132, 1.9623391, 1.9611508, 1.9604386, 1.9602012, 1.9600826, 1.9600114, 1.9599877, 1.9599758, 1.9599687, 1.9599664}}
        Const t95_inf = 1.959964

        'MSE
        Dim UE As Double = 0
        't quantile
        Dim t_quant As Double = 0
        'Quantity of input data points
        Dim n As Integer = X.GetUpperBound(1) + 1
        'Sum to calculate the MSE
        Dim sum_UE As Double = 0
        'Sum to calculate the average value of Y
        Dim sum_Y As Double = 0
        'The average value of Y
        Dim Y_avrg As Double = 0
        'Sum to calculate the total variability
        Dim sum_Var_tot As Double = 0
        'Sum to calculate the variability by regression
        Dim sum_Var_regr As Double = 0

        'Matrix X transposed: X_T
        matrix_transp(X, X_T)
        'Intermediate matrix INTERM_1 = X_T * W
        matrix_multiplic(X_T, WF, INTERM_1)
        'Intermediate matrix INTERM_2 = (X_T * W) * X
        matrix_multiplic(INTERM_1, X, INTERM_2)
        'Intermediate matrix INTERM_3 = inv((X_T * W) * X)
        matrix_2x2_inv(INTERM_2, INTERM_3)
        'Intermediate matrix INTERM_4 = inv((X_T * W) * X) * X_T
        matrix_multiplic(INTERM_3, X_T, INTERM_4)
        'Matrix V = (inv((X_T * W) * X) * X_T) * W
        matrix_multiplic(INTERM_4, WF, V)
        'Vector beta = V * Y
        matrix_vector_multiplic(V, Y, beta)

        'The two regression coefficients
        beta1 = beta(0)
        beta2 = beta(1)

        'Calculate the sums for the average Y value and the MSE
        For r = 0 To n - 1

            sum_Y += Y(r)

            sum_UE += (beta1 + beta2 * X(1, r) - Y(r)) ^ 2

        Next

        'Calculate MSE and Y_avrg
        UE = sum_UE / (n - 2)
        Y_avrg = sum_Y / n

        'Determine the t95 quantile value
        If n - 2 <= 10 ^ 6 Then
            interpolate(f_t95, n - 2, t_quant)
        Else
            t_quant = t95_inf
        End If

        'Matrix V transposed: V_T
        matrix_transp(V, V_T)
        'Matrix product_ V * V_T
        matrix_multiplic(V, V_T, V_mult_V_T)

        'Calculate the 95 % confidence intervals related to the values beta0 and beta1
        CI_beta0_perc = 2 * t_quant * (UE * V_mult_V_T(0, 0)) ^ 0.5 / beta1 * 100
        CI_beta1_perc = 2 * t_quant * (UE * V_mult_V_T(1, 1)) ^ 0.5 / beta2 * 100

        For r = 0 To n - 1

            'Calculate the weighted sum for the total variability
            sum_Var_tot += (Y(r) - Y_avrg) ^ 2

            'Calculate the sum for the variability by regression
            sum_Var_regr += (beta1 + beta2 * X(1, r) - Y_avrg) ^ 2

        Next

        'Calculate the coefficient of determination R^2
        R_sq = sum_Var_regr / sum_Var_tot

        Return True
    End Function

    'Transpose a  c x r matrix
    Private Function matrix_transp(ByVal M(,) As Double, ByRef M_T(,) As Double) As Boolean
        ' Declaration
        Dim c, r As Integer
        Dim c_max As Integer = M.GetUpperBound(0)
        Dim r_max As Integer = M.GetUpperBound(1)

        ' Initialisation
        ReDim M_T(r_max, c_max)

        ' Transpose the matrix
        For c = 0 To c_max
            For r = 0 To r_max
                M_T(r, c) = M(c, r)
            Next
        Next

        Return True
    End Function

    'Multiplicate a  c x r matrix (M_l) with a  d x c matrix (M_r) ==> the result is a  d x r matrix (M_prod)
    Private Function matrix_multiplic(ByVal M_l(,) As Double, ByVal M_r(,) As Double, ByRef M_prod(,) As Double) As Boolean
        ' Declaration
        Dim c, r As Integer
        Dim c_max_l As Integer = M_l.GetUpperBound(0)
        Dim r_max_l As Integer = M_l.GetUpperBound(1)
        Dim c_max_r As Integer = M_r.GetUpperBound(0)
        Dim sum As Double = 0

        ' Initialisation
        ReDim M_prod(c_max_r, r_max_l)

        ' Multiplicate the matrix
        For c = 0 To c_max_r
            For r = 0 To r_max_l
                sum = 0
                For i = 0 To c_max_l
                    sum += M_r(c, i) * M_l(i, r)
                Next
                M_prod(c, r) = sum
            Next r
        Next c

        Return True
    End Function

    'Calculate the inverse matrix (M_inv) of a 2 x 2 matrix (M)
    Private Function matrix_2x2_inv(ByVal M(,) As Double, ByRef M_inv(,) As Double) As Boolean
        ' Declaration
        Dim a As Double = M(0, 0)
        Dim b As Double = M(1, 0)
        Dim c As Double = M(0, 1)
        Dim d As Double = M(1, 1)

        ' Initialisation
        ReDim M_inv(1, 1)

        ' Calculate the inversion
        M_inv(0, 0) = d / (a * d - b * c)
        M_inv(1, 0) = -b / (a * d - b * c)
        M_inv(0, 1) = -c / (a * d - b * c)
        M_inv(1, 1) = a / (a * d - b * c)

        Return True
    End Function

    'Multiplicate a  c x r matrix (M_l) with a  1 x c vector (VECT) ==> the result is a  1 x r vector (RESULT)
    Private Function matrix_vector_multiplic(ByVal M_l(,) As Double, ByVal VECT() As Double, ByRef RESULT() As Double) As Boolean
        ' Declaration
        Dim c, r As Integer
        Dim c_max As Integer = M_l.GetUpperBound(0)
        Dim sum As Double = 0

        ' Calculate the matrix multiplication
        For r = 0 To 1
            sum = 0
            For c = 0 To c_max
                sum += VECT(c) * M_l(c, r)
            Next c
            RESULT(r) = sum
        Next r

        Return True
    End Function
End Module