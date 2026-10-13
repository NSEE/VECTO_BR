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

Imports AirDrag.cCriteria
Module Signal_identification

    ' Divide the signal into there directions
    Public Sub fIdentifyMS(ByVal MSC As cMSC, ByRef vMSC As cVirtMSC, Optional ByVal virtMSC As Boolean = True, Optional ByVal SectionDev As Boolean = True)
        ' Declaration
        Dim i As Integer

        If virtMSC Then
            ' Calculation of the virtual MSC points
            fvirtMSC(MSC, vMSC)
        End If

        If SectionDev Then
            ' Output on the GUI
            logme(6, False, "Identifying the sections")

            ' Devide the measured data into there sections
            DevInSec(vMSC)

            ' Leap in time control
            If JumpPoint.Count > 0 Then
                For i = 0 To JumpPoint.Count - 1
                    If CalcData(tCompCali.SecID)(JumpPoint(i)) <> 0 Then
                        Throw New Exception(format("The detected leap in time({0}) is not allowed to be inside a measurement section!", JumpPoint(i)))
                    End If
                Next i
            End If

            ' Calculate the root points from the measuered data between the MSC points
            fCalcroot(MSC)

            ' Calculate the section overview
            fSecOverview(MSC)
        End If
    End Sub

    ' Calculation of the virtual trigger points
    Function fvirtMSC(ByVal MSCOrg As cMSC, ByRef MSCVirt As cVirtMSC) As Boolean
        ' Declaration
        Dim i As Integer
        Dim first As Boolean = True
        Dim AddSec As Boolean = False
        Dim LenDiff As Boolean = False
        Dim HeadDiff As Boolean = False
        Dim Aae As Double
        Dim len(MSCOrg.meID.Count - 1) As Double
        Dim Head(MSCOrg.meID.Count - 1) As Double
        Dim UTMCoordP As New cUTMCoord
        Dim UTMCoordV As New cUTMCoord

        ' Trigger status
        MSCVirt.tUse = MSCOrg.tUse

        ' Calculate the virtual points for every measured section id
        For i = 1 To MSCOrg.meID.Count - 1
            UTMCoordP = UTM(MSCOrg.latS(i) / 60, MSCOrg.longS(i) / 60)
            UTMCoordV = UTM(MSCOrg.latE(i) / 60, MSCOrg.longE(i) / 60)
            Aae = QuadReq(UTMCoordV.Easting - UTMCoordP.Easting, UTMCoordV.Northing - UTMCoordP.Northing)
            len(i) = Math.Sqrt(Math.Pow(UTMCoordV.Easting - UTMCoordP.Easting, 2) + Math.Pow(UTMCoordV.Northing - UTMCoordP.Northing, 2))
            If (Math.Sqrt(1 - (Math.Cos((MSCOrg.longS(i) - MSCOrg.longE(i)) / 60 * Math.PI / 180) * Math.Cos(MSCOrg.latS(i) / 60 * Math.PI / 180) * _
                Math.Cos(MSCOrg.latE(i) / 60 * Math.PI / 180) + Math.Sin(MSCOrg.latS(i) / 60 * Math.PI / 180) * Math.Sin(MSCOrg.latE(i) / 60 * Math.PI / 180)) ^ 2)) <> 0 Then
                Head(i) = Math.Acos((Math.Cos(MSCOrg.latS(i) / 60 * Math.PI / 180) * Math.Sin(MSCOrg.latE(i) / 60 * Math.PI / 180) - _
                           Math.Cos((MSCOrg.longE(i) - MSCOrg.longS(i)) / 60 * Math.PI / 180) * Math.Cos(MSCOrg.latE(i) / 60 * Math.PI / 180) * _
                           Math.Sin(MSCOrg.latS(i) / 60 * Math.PI / 180)) / (Math.Sqrt(1 - (Math.Cos((MSCOrg.longS(i) - MSCOrg.longE(i)) / 60 * Math.PI / 180) * _
                           Math.Cos(MSCOrg.latS(i) / 60 * Math.PI / 180) * Math.Cos(MSCOrg.latE(i) / 60 * Math.PI / 180) + _
                           Math.Sin(MSCOrg.latS(i) / 60 * Math.PI / 180) * Math.Sin(MSCOrg.latE(i) / 60 * Math.PI / 180)) ^ 2))) * 180 / Math.PI
                If (MSCOrg.longE(i) - MSCOrg.longS(i)) < 0 Then Head(i) = 360 - Head(i)
            Else
                Head(i) = 0
            End If
            MSCVirt.meID.Add(MSCOrg.meID(i))
            MSCVirt.dID.Add(MSCOrg.dID(i))
            MSCVirt.KoordA.Add(KleinPkt(UTMCoordP.Easting, UTMCoordP.Northing, Aae, 0, -Crt.trigger_delta_y_max))
            MSCVirt.KoordE.Add(KleinPkt(UTMCoordP.Easting, UTMCoordP.Northing, Aae, 0, Crt.trigger_delta_y_max))
            MSCVirt.NewSec.Add(False)
            MSCVirt.Head.Add(MSCOrg.head(i))

            If i < MSCOrg.meID.Count - 1 Then
                If Not Math.Abs(MSCOrg.latS(i + 1) - MSCOrg.latE(i)) <= 0.001 And Not Math.Abs(MSCOrg.longS(i + 1) - MSCOrg.longE(i)) <= 0.001 Or Not (MSCOrg.dID(i + 1) = MSCOrg.dID(i)) Then
                    MSCVirt.NewSec.Add(True)
                    MSCVirt.meID.Add(0)
                    MSCVirt.dID.Add(MSCOrg.dID(i))
                    MSCVirt.KoordA.Add(KleinPkt(UTMCoordP.Easting, UTMCoordP.Northing, Aae, len(i), -Crt.trigger_delta_y_max))
                    MSCVirt.KoordE.Add(KleinPkt(UTMCoordP.Easting, UTMCoordP.Northing, Aae, len(i), Crt.trigger_delta_y_max))
                    MSCVirt.Head.Add(MSCOrg.head(i))
                End If
            ElseIf i = MSCOrg.meID.Count - 1 Then
                MSCVirt.NewSec.Add(True)
                MSCVirt.meID.Add(0)
                MSCVirt.dID.Add(MSCOrg.dID(i))
                MSCVirt.KoordA.Add(KleinPkt(UTMCoordP.Easting, UTMCoordP.Northing, Aae, len(i), -Crt.trigger_delta_y_max))
                MSCVirt.KoordE.Add(KleinPkt(UTMCoordP.Easting, UTMCoordP.Northing, Aae, len(i), Crt.trigger_delta_y_max))
                MSCVirt.Head.Add(MSCOrg.head(i))
            End If
        Next i

        ' Controll the spezified csms length
        For i = 1 To MSCOrg.meID.Count - 1
            If Math.Abs(len(i) - MSCOrg.len(i)) > Crt.leng_crit Then
                If Not headDiff Then logme(9, False, "Length difference between given coordinates and spezified section length in *.csms file!")
                logme(9, False, "SecID(" & MSCOrg.meID(i) & "), DirID(" & MSCOrg.dID(i) & "), spez. Len(" & MSCOrg.len(i) & "), coord. Len(" & Math.Round(len(i), 2, MidpointRounding.AwayFromZero) & ")")
                LenDiff = True
            End If
            If Math.Abs(Head(i) - MSCOrg.head(i)) > Crt.delta_head_max Then
                If Not HeadDiff Then logme(9, False, "Heading difference between given coordinates and spezified section heading in *.csms file!")
                logme(9, False, "SecID(" & MSCOrg.meID(i) & "), DirID(" & MSCOrg.dID(i) & "), spez. heading(" & MSCOrg.head(i) & "), coord. Heading(" & Math.Round(Head(i), 2, MidpointRounding.AwayFromZero) & ")")
                HeadDiff = True
            End If
        Next i
        ' Exit the programm
        If LenDiff Then Throw New Exception(format("Length difference between given coordinates and spezified section length in *.csms file! Please correct the length or coordinates!"))
        If HeadDiff Then Throw New Exception(format("Heading difference between given coordinates and spezified section heading in *.csms file! Please correct the heading or coordinates!"))

        Return True
    End Function

    ' Function to test if a measurement point is in an measure section or if an signal was detected
    Function MSCTest(ByVal KoordA As Array, ByVal KoordE As Array, ByVal KoordP As Array, Optional ByVal ContSec As Boolean = False, Optional ByVal CCW As Boolean = False) As Boolean
        ' Declaration
        Dim ErgHHF As New cHHF

        ' Calculate Höhe-Höhenpußpunkt values
        ErgHHF = HHF(KoordA, KoordE, KoordP)

        ' Appropriate if the point is in the section or if it is in the detection area
        If ContSec Then
            ' Appropriate if the point is in the detection area
            If ErgHHF.hp <= Crt.trigger_delta_x_max And ErgHHF.p > 0 And ErgHHF.q > 0 Then
                Return True
            Else
                Return False
            End If
        Else
            ' Appropriate if the point is in the section
            If CCW Then
                If ErgHHF.KAap <= 180 And ErgHHF.p > 0 And ErgHHF.q > 0 Then
                    Return True
                Else
                    Return False
                End If
            Else
                If ErgHHF.KAap >= 180 And ErgHHF.p > 0 And ErgHHF.q > 0 Then
                    Return True
                Else
                    Return False
                End If
            End If
        End If
    End Function

    ' Divide the measured points into there sections
    Function DevInSec(ByVal vMSCX As cVirtMSC) As Boolean
        ' Declaration
        Dim i, j, k, SecID, SaveDir, SaveSecID, SaveDirID, DirID, PktArea As Integer
        Dim sKV As New KeyValuePair(Of tComp, Integer)
        Dim PKoord As Array
        Dim SecTest(vMSCX.meID.Count - 1), LastSecTest(vMSCX.meID.Count - 1), ChangeT, Signal, CCW As Boolean
        Dim Sigdet(vMSCX.meID.Count - 1), LastSigdet(vMSCX.meID.Count - 1), AreaTest As Boolean

        ' Initialise
        SaveDir = vMSCX.dID(1)
        SaveSecID = 0
        SaveDirID = 0
        PktArea = 0
        CCW = False
        ChangeT = False
        For j = 1 To UBound(SecTest)
            SecTest(j) = False
            LastSecTest(j) = False
            Sigdet(j) = False
            LastSigdet(j) = False
        Next j

        ' Devide the signal into the sections
        For i = 0 To CalcData(tCompCali.lati_UTM).Count - 1
            SecID = 0
            DirID = 0
            Signal = False
            AreaTest = vMSCX.tUse
            PKoord = ({CalcData(tCompCali.longi_UTM)(i), CalcData(tCompCali.lati_UTM)(i)})

            ' Control if the point is in an section
            For j = 1 To UBound(SecTest)
                If vMSCX.dID(j) = SaveDir Then
                    CCW = False
                ElseIf vMSCX.dID(j) <> 0 Then
                    CCW = True
                Else
                    ' Use the last CCW
                End If

                SecTest(j) = MSCTest(vMSCX.KoordA(j), vMSCX.KoordE(j), PKoord, AreaTest, CCW)

                ' Control if an signal is detected
                If AreaTest And i > 0 Then
                    If SecTest(j) And Math.Abs(CalcData(tCompCali.trigger_c)(i) - CalcData(tCompCali.trigger_c)(i - 1)) >= 1 Then
                        Sigdet(j) = True
                        Signal = True
                    Else
                        SecTest(j) = False
                    End If
                End If
            Next j

            ' Check if somthing has changed
            If AreaTest Then
                If Signal Then
                    ChangeT = True
                Else
                    ChangeT = False
                End If
            Else
                For j = 1 To UBound(SecTest)
                    If SecTest(j) <> LastSecTest(j) Then
                        ChangeT = True
                        Exit For
                    Else
                        ChangeT = False
                    End If
                Next j
            End If

            ' If something has changed
            If ChangeT Then
                ' Set the direction ID
                For j = 1 To UBound(SecTest)
                    If SecTest(j) Then
                        If Math.Abs(InputData(tComp.hdg)(i) - vMSCX.Head(j)) <= Crt.delta_head_max Then
                            DirID = vMSCX.dID(j)
                        End If
                    End If
                Next j

                If AreaTest Then
                    ' Correct the written sign and direction data
                    For j = 1 To UBound(Sigdet) - 1
                        If LastSigdet(j) Then
                            If Sigdet(j + 1) = False And DirID = vMSCX.dID(j) And PktArea > 1 Then
                                For k = (i - PktArea) To i - 1
                                    CalcData(tCompCali.SecID)(k) = CDbl(0)
                                    CalcData(tCompCali.DirID)(k) = CDbl(0)
                                Next k
                            End If
                        End If
                    Next j
                    PktArea = 1

                    ' Copy the Sigdet to save it
                    For j = 1 To UBound(Sigdet)
                        LastSigdet(j) = Sigdet(j)
                        Sigdet(j) = False
                    Next j
                End If

                ' Set the section ID
                For j = 1 To UBound(SecTest)
                    If SecTest(j) And DirID = vMSCX.dID(j) Then
                        If vMSCX.dID(j) = SaveDir Then
                            For k = j To vMSCX.meID.Count - 1
                                If vMSCX.NewSec(k) Then
                                    If SecTest(k) Then
                                        SecID = k
                                        Exit For
                                    Else
                                        Exit For
                                    End If
                                Else
                                    If SecTest(k) Then
                                        SecID = k
                                    End If
                                End If
                            Next k
                        Else
                            If AreaTest Then
                                SecID = j
                            Else
                                SecID = j - 1
                            End If
                            Exit For
                        End If
                    End If
                Next j

                ' Set the variables on default if a non section area is detected
                If vMSCX.meID(SecID) = 0 Then
                    DirID = 0
                    If AreaTest Then
                        For j = 1 To UBound(Sigdet)
                            LastSigdet(j) = False
                        Next j
                    End If
                End If

                ' Copy the SecTest to save array
                For j = 1 To UBound(SecTest)
                    LastSecTest(j) = SecTest(j)
                Next j
            Else
                ' Use the values from before
                SecID = SaveSecID
                DirID = SaveDirID
                If AreaTest Then PktArea += 1
            End If

            ' Add the calculated Point to the dictionary
            CalcData(tCompCali.SecID).Add(CDbl(vMSCX.meID(SecID)))
            CalcData(tCompCali.DirID).Add(CDbl(DirID))
            SaveSecID = SecID
            SaveDirID = DirID
        Next i

        Return True
    End Function

    ' Function for calculation of the root points
    Function fCalcroot(ByVal orgMSCX As cMSC) As Boolean
        ' Declaration
        Dim i, pos As Integer
        Dim DXae, DYae, DXap, DYap, DXep, DYep, Aae, q As Double
        Dim BegCoordX, BegCoordY As Double
        Dim FirstIn As Boolean = True
        Dim koord As Array
        Dim UTMCoordA As New cUTMCoord
        Dim UTMCoordE As New cUTMCoord

        ' Initialisation
        pos = 1

        ' Calculation
        For i = 0 To CalcData(tCompCali.SecID).Count - 1
            If CalcData(tCompCali.SecID)(i) <> 0 Then
                ' Get the position of the section
                pos = fMSPos(orgMSCX, CalcData(tCompCali.SecID)(i), CalcData(tCompCali.DirID)(i))

                ' Calculation of the parameters
                UTMCoordA = UTM(orgMSCX.latS(pos) / 60, orgMSCX.longS(pos) / 60)
                UTMCoordE = UTM(orgMSCX.latE(pos) / 60, orgMSCX.longE(pos) / 60)
                DYae = UTMCoordE.Northing - UTMCoordA.Northing
                DXae = UTMCoordE.Easting - UTMCoordA.Easting
                DYap = CalcData(tCompCali.lati_UTM)(i) - UTMCoordA.Northing
                DXap = CalcData(tCompCali.longi_UTM)(i) - UTMCoordA.Easting
                DYep = CalcData(tCompCali.lati_UTM)(i) - UTMCoordE.Northing
                DXep = CalcData(tCompCali.longi_UTM)(i) - UTMCoordE.Easting

                ' Calculate the angles
                Aae = QuadReq(DXae, DYae)

                ' Calculate the range till the root point from the point A
                q = ((DXae ^ 2 + DYae ^ 2) - (DXep ^ 2 + DYep ^ 2) + (DXap ^ 2 + DYap ^ 2)) / (2 * Math.Sqrt(DXae ^ 2 + DYae ^ 2))

                ' Calculate the coordinates from the root point
                koord = KleinPkt(UTMCoordA.Easting, UTMCoordA.Northing, Aae, q, 0)

                ' Write the calculated root points in the dictionary
                CalcData(tCompCali.longi_root).Add(koord(0))
                CalcData(tCompCali.lati_root).Add(koord(1))

                ' Calculate the root distance
                If i = 0 Then
                    logme(8, False, "The first data point started in a measurement section! This is not allowed and this section is set to invalid.")
                    BegCoordX = koord(0)
                    BegCoordY = koord(1)
                    CalcData(tCompCali.dist_root).Add(0)
                    FirstIn = False
                ElseIf FirstIn Or CalcData(tCompCali.SecID)(i - 1) <> CalcData(tCompCali.SecID)(i) Then
                    BegCoordX = koord(0)
                    BegCoordY = koord(1)
                    CalcData(tCompCali.dist_root).Add(0)
                    FirstIn = False
                Else
                    CalcData(tCompCali.dist_root).Add(Math.Sqrt((koord(0) - BegCoordX) ^ 2 + (koord(1) - BegCoordY) ^ 2))
                End If
            Else
                ' Write 0 for the root points in the dictionary
                CalcData(tCompCali.lati_root).Add(0)
                CalcData(tCompCali.longi_root).Add(0)
                CalcData(tCompCali.dist_root).Add(0)
                FirstIn = True
            End If
        Next i

        Return True
    End Function

    ' Function to generate the sectionoverview
    Function fSecOverview(ByVal MSCX As cMSC) As Boolean
        ' Declaration
        Dim i, j, run, anz As Integer
        Dim v_wind_1s(0), Time_1s(0), beta_1s(0) As Double
        Dim firstIn As Boolean = True
        Dim foundSec(MSCX.meID.Count - 1) As Boolean
        Dim sKVE As New KeyValuePair(Of String, List(Of Double))
        Dim sKVC As New KeyValuePair(Of tCompCali, List(Of Double))
        Dim sKV As New KeyValuePair(Of tCompErg, List(Of Double))
        Dim EnumStrErg As tCompErg

        ' Initialisation
        run = 0
        anz = 0
        ErgValues = New Dictionary(Of tCompErg, List(Of Double))
        ErgValuesUndef = New Dictionary(Of String, List(Of Double))
        For i = 1 To MSCX.meID.Count - 1
            foundSec(i) = False
        Next i

        ' Generate the undefined result dictionary variables
        For Each sKVE In InputUndefData
            ErgValuesUndef.Add(sKVE.Key, New List(Of Double))
        Next

        ' Generate the result dictionary variables
        For Each EnumStrErg In System.Enum.GetValues(GetType(tCompErg))
            ErgValues.Add(EnumStrErg, New List(Of Double))
        Next

        ' Calculate the section average values
        For i = 0 To CalcData(tCompCali.SecID).Count - 1
            For Each sKVC In CalcData
                If CalcData(sKVC.Key).Count <= i Then
                    CalcData(sKVC.Key).Add(0)
                End If
            Next

            If CalcData(tCompCali.SecID)(i) <> 0 Then
                If firstIn Then
                    ErgValues(tCompErg.SecID).Add(CalcData(tCompCali.SecID)(i))
                    ErgValues(tCompErg.DirID).Add(CalcData(tCompCali.DirID)(i))
                    For j = 1 To MSCX.headID.Count - 1
                        If CalcData(tCompCali.SecID)(i) = MSCX.meID(j) And CalcData(tCompCali.DirID)(i) = MSCX.dID(j) Then
                            ErgValues(tCompErg.HeadID).Add(MSCX.headID(j))
                            ErgValues(tCompErg.s_MSC).Add(MSCX.len(j))
                            foundSec(j) = True
                            Exit For
                        End If
                    Next
                    ErgValues(tCompErg.delta_t).Add(CalcData(tCompCali.t)(i))
                    ErgValues(tCompErg.v_veh_CAN).Add(InputData(tComp.v_veh_CAN)(i))
                    ErgValues(tCompErg.vair_ic).Add(InputData(tComp.vair_ic)(i))
                    ErgValues(tCompErg.beta_ic).Add(InputData(tComp.beta_ic)(i))
                    ErgValues(tCompErg.user_valid).Add(InputData(tComp.user_valid)(i))

                    If MSCX.tUse Then
                        ErgValues(tCompErg.v_MSC).Add(0)
                    Else
                        ErgValues(tCompErg.v_MSC).Add(0)
                        ErgValues(tCompErg.v_MSC_GPS).Add(InputData(tComp.v_veh_GPS)(i))
                    End If

                    If i = 0 Then
                        ' First data Point lies in a section. This is not allowed and set this section to invalid
                        ErgValues(tCompErg.valid).Add(0)
                        ErgValues(tCompErg.used).Add(0)
                        ErgValues(tCompErg.system_valid).Add(0)
                    ElseIf i = CalcData(tCompCali.SecID).Count - 1 Then
                        ' Last data Point lies in a section. This is not allowed and set this section to invalid
                        ErgValues(tCompErg.valid).Add(0)
                        ErgValues(tCompErg.used).Add(0)
                        ErgValues(tCompErg.system_valid).Add(0)
                        ErgValues(tCompErg.delta_t)(run) = ErgValues(tCompErg.delta_t)(run) - CalcData(tCompCali.t)(i - 1)
                        ErgValues(tCompErg.v_MSC)(run) = (ErgValues(tCompErg.s_MSC)(run) / ErgValues(tCompErg.delta_t)(run)) * 3.6
                        'ErgValues(tCompErg.user_valid)(run) = 0
                    Else
                        ErgValues(tCompErg.valid).Add(1)
                        ErgValues(tCompErg.used).Add(1)
                        ErgValues(tCompErg.system_valid).Add(1)
                    End If

                    For Each sKV In ErgValues
                        If ErgValues(sKV.Key).Count <= run Then
                            ErgValues(sKV.Key).Add(0)
                        End If
                    Next
                    For Each sKVE In InputUndefData
                        ErgValuesUndef(sKVE.Key).Add(InputUndefData(sKVE.Key)(i))
                    Next

                    firstIn = False
                    anz += 1
                Else
                    If (ErgValues(tCompErg.SecID).Last = CalcData(tCompCali.SecID)(i)) And (ErgValues(tCompErg.DirID).Last = CalcData(tCompCali.DirID)(i)) Then
                        ' Build the sum
                        ErgValues(tCompErg.v_veh_CAN)(run) += InputData(tComp.v_veh_CAN)(i)
                        ErgValues(tCompErg.vair_ic)(run) += InputData(tComp.vair_ic)(i)
                        ErgValues(tCompErg.beta_ic)(run) += InputData(tComp.beta_ic)(i)
                        ErgValues(tCompErg.user_valid)(run) += InputData(tComp.user_valid)(i)
                        If Not MSCX.tUse Then
                            ErgValues(tCompErg.v_MSC_GPS)(run) += InputData(tComp.v_veh_GPS)(i)
                        End If
                        For Each sKVE In InputUndefData
                            ErgValuesUndef(sKVE.Key)(run) += InputUndefData(sKVE.Key)(i)
                        Next

                        anz += 1

                        ' Last data Point lies in a section. This is not allowed and set this section to invalid
                        If i = CalcData(tCompCali.SecID).Count - 1 Then
                            ' Calculate the results from the last section
                            ErgValues(tCompErg.delta_t)(run) = CalcData(tCompCali.t)(i) - ErgValues(tCompErg.delta_t)(run)
                            ErgValues(tCompErg.v_veh_CAN)(run) = ErgValues(tCompErg.v_veh_CAN)(run) / anz
                            ErgValues(tCompErg.vair_ic)(run) = ErgValues(tCompErg.vair_ic)(run) / anz
                            ErgValues(tCompErg.beta_ic)(run) = ErgValues(tCompErg.beta_ic)(run) / anz
                            ErgValues(tCompErg.v_MSC)(run) = (ErgValues(tCompErg.s_MSC)(run) / ErgValues(tCompErg.delta_t)(run)) * 3.6
                            ErgValues(tCompErg.user_valid)(run) = ErgValues(tCompErg.user_valid)(run) / anz
                            If Not MSCX.tUse Then
                                ErgValues(tCompErg.v_MSC_GPS)(run) = ErgValues(tCompErg.v_MSC_GPS)(run) / anz
                            End If
                            For Each sKVE In InputUndefData
                                ErgValuesUndef(sKVE.Key)(run) = ErgValuesUndef(sKVE.Key)(run) / anz
                            Next

                            ' Set the section to invalid
                            ErgValues(tCompErg.valid)(run) = 0
                            ErgValues(tCompErg.used)(run) = 0
                            ErgValues(tCompErg.system_valid)(run) = 0
                        End If
                    Else
                        ' Calculate the results from the last section
                        ErgValues(tCompErg.delta_t)(run) = CalcData(tCompCali.t)(i - 1) - ErgValues(tCompErg.delta_t)(run)
                        ErgValues(tCompErg.v_veh_CAN)(run) = ErgValues(tCompErg.v_veh_CAN)(run) / anz
                        ErgValues(tCompErg.vair_ic)(run) = ErgValues(tCompErg.vair_ic)(run) / anz
                        ErgValues(tCompErg.beta_ic)(run) = ErgValues(tCompErg.beta_ic)(run) / anz
                        ErgValues(tCompErg.v_MSC)(run) = (ErgValues(tCompErg.s_MSC)(run) / ErgValues(tCompErg.delta_t)(run)) * 3.6
                        ErgValues(tCompErg.user_valid)(run) = ErgValues(tCompErg.user_valid)(run) / anz
                        If Not MSCX.tUse Then
                            ErgValues(tCompErg.v_MSC_GPS)(run) = ErgValues(tCompErg.v_MSC_GPS)(run) / anz
                        End If
                        For Each sKVE In InputUndefData
                            ErgValuesUndef(sKVE.Key)(run) = ErgValuesUndef(sKVE.Key)(run) / anz
                        Next

                        ' Add a new Section to the resultfile
                        ErgValues(tCompErg.SecID).Add(CalcData(tCompCali.SecID)(i))
                        ErgValues(tCompErg.DirID).Add(CalcData(tCompCali.DirID)(i))
                        For j = 1 To MSCX.headID.Count - 1
                            If CalcData(tCompCali.SecID)(i) = MSCX.meID(j) And CalcData(tCompCali.DirID)(i) = MSCX.dID(j) Then
                                ErgValues(tCompErg.HeadID).Add(MSCX.headID(j))
                                ErgValues(tCompErg.s_MSC).Add(MSCX.len(j))
                                foundSec(j) = True
                                Exit For
                            End If
                        Next
                        ErgValues(tCompErg.delta_t).Add(CalcData(tCompCali.t)(i))
                        ErgValues(tCompErg.v_veh_CAN).Add(InputData(tComp.v_veh_CAN)(i))
                        ErgValues(tCompErg.vair_ic).Add(InputData(tComp.vair_ic)(i))
                        ErgValues(tCompErg.beta_ic).Add(InputData(tComp.beta_ic)(i))
                        ErgValues(tCompErg.user_valid).Add(InputData(tComp.user_valid)(i))
                        ErgValues(tCompErg.valid).Add(1)
                        ErgValues(tCompErg.used).Add(1)
                        ErgValues(tCompErg.system_valid).Add(1)
                        If MSCX.tUse Then
                            ErgValues(tCompErg.v_MSC).Add(0)
                        Else
                            ErgValues(tCompErg.v_MSC).Add(0)
                            ErgValues(tCompErg.v_MSC_GPS).Add(InputData(tComp.v_veh_GPS)(i))
                        End If
                        For Each sKV In ErgValues
                            If ErgValues(sKV.Key).Count <= run + 1 Then
                                ErgValues(sKV.Key).Add(0)
                            End If
                        Next
                        For Each sKVE In InputUndefData
                            ErgValuesUndef(sKVE.Key).Add(InputUndefData(sKVE.Key)(i))
                        Next

                        anz = 1
                        run += 1

                        ' Last data Point lies in a section. This is not allowed and set this section to invalid
                        If i = CalcData(tCompCali.SecID).Count - 1 Then
                            ErgValues(tCompErg.valid)(run) = 0
                            ErgValues(tCompErg.used)(run) = 0
                            ErgValues(tCompErg.system_valid)(run) = 0
                            'ErgValues(tCompErg.user_valid)(run) = 0
                            ErgValues(tCompErg.delta_t)(run) = ErgValues(tCompErg.delta_t)(run) - CalcData(tCompCali.t)(i - 1)
                            ErgValues(tCompErg.v_MSC)(run) = (ErgValues(tCompErg.s_MSC)(run) / ErgValues(tCompErg.delta_t)(run)) * 3.6
                        End If
                    End If
                End If
            Else
                ' Finish calculation after a valid section
                If firstIn = False Then
                    ' Calculate the results from the last section
                    ErgValues(tCompErg.delta_t)(run) = CalcData(tCompCali.t)(i - 1) - ErgValues(tCompErg.delta_t)(run)
                    ErgValues(tCompErg.v_veh_CAN)(run) = ErgValues(tCompErg.v_veh_CAN)(run) / anz
                    ErgValues(tCompErg.vair_ic)(run) = ErgValues(tCompErg.vair_ic)(run) / anz
                    ErgValues(tCompErg.beta_ic)(run) = ErgValues(tCompErg.beta_ic)(run) / anz
                    ErgValues(tCompErg.v_MSC)(run) = (ErgValues(tCompErg.s_MSC)(run) / ErgValues(tCompErg.delta_t)(run)) * 3.6
                    ErgValues(tCompErg.user_valid)(run) = ErgValues(tCompErg.user_valid)(run) / anz
                    If Not MSCX.tUse Then
                        ErgValues(tCompErg.v_MSC_GPS)(run) = ErgValues(tCompErg.v_MSC_GPS)(run) / anz
                    End If
                    For Each sKVE In InputUndefData
                        ErgValuesUndef(sKVE.Key)(run) = ErgValuesUndef(sKVE.Key)(run) / anz
                    Next

                    anz = 0
                    run += 1
                    firstIn = True
                End If
            End If
        Next i

        ' Display the non founded sections
        firstIn = True
        For i = 1 To foundSec.Count - 1
            If Not foundSec(i) Then
                If firstIn Then logme(8, False, "Not all defined sections in the *.csms were found! Please check your section definition(s)!")
                logme(8, False, format("SecID ({0}), DID ({1})", MSCX.meID(i), MSCX.dID(i)))
                firstIn = False
            End If
        Next i

        Return True
    End Function

    ' Calculate the corrected vehicle speed
    Public Function fCalcCorVveh() As Boolean
        ' Declaration
        Dim i, run, anz As Integer
        Dim firstIn As Boolean = True
        Dim HzIn2 As Double

        ' Initialise
        run = 0
        anz = 0

        ' Calculate the corrected vehicle speed
        For i = 0 To CalcData(tCompCali.SecID).Count - 1
            CalcData(tCompCali.v_veh_c)(i) = (InputData(tComp.v_veh_CAN)(i) * Job.fv_veh)
            If i = 0 Then
                CalcData(tCompCali.dist)(i) = 0
            Else
                HzIn2 = 1 / (CalcData(tCompCali.t)(i) - CalcData(tCompCali.t)(i - 1))
                CalcData(tCompCali.dist)(i) = CalcData(tCompCali.dist)(i - 1) + (CalcData(tCompCali.v_veh_c)(i) / 3.6) * (1 / HzIn2)
            End If
            If CalcData(tCompCali.SecID)(i) <> 0 Then
                If firstIn Then
                    ErgValues(tCompErg.v_veh)(run) = (CalcData(tCompCali.v_veh_c)(i))
                    ErgValues(tCompErg.dist)(run) = (CalcData(tCompCali.dist)(i))
                    firstIn = False
                    anz += 1

                    ' Last data Point lies in a section. This is not allowed but done for correct calculation
                    If i = CalcData(tCompCali.SecID).Count - 1 Then
                        ErgValues(tCompErg.dist)(run) = (CalcData(tCompCali.v_veh_c)(i) / 3.6) * (1 / HzIn2)
                    End If
                Else
                    If (CalcData(tCompCali.SecID)(i) = CalcData(tCompCali.SecID)(i - 1)) And (CalcData(tCompCali.DirID)(i) = CalcData(tCompCali.DirID)(i - 1)) Then
                        ' Build the sum
                        ErgValues(tCompErg.v_veh)(run) += CalcData(tCompCali.v_veh_c)(i)
                        anz += 1

                        ' Last data Point lies in a section. This is not allowed but done for correct calculation
                        If i = CalcData(tCompCali.SecID).Count - 1 Then
                            ErgValues(tCompErg.v_veh)(run) = ErgValues(tCompErg.v_veh)(run) / anz
                            ErgValues(tCompErg.dist)(run) = CalcData(tCompCali.dist)(i) - ErgValues(tCompErg.dist)(run)
                        End If
                    Else
                        ' Calculate the results from the last section
                        ErgValues(tCompErg.v_veh)(run) = ErgValues(tCompErg.v_veh)(run) / anz
                        ErgValues(tCompErg.dist)(run) = CalcData(tCompCali.dist)(i - 1) - ErgValues(tCompErg.dist)(run)

                        ' Add a new Section to the resultfile
                        run += 1
                        anz = 1
                        ErgValues(tCompErg.v_veh)(run) = (CalcData(tCompCali.v_veh_c)(i))
                        ErgValues(tCompErg.dist)(run) = (CalcData(tCompCali.dist)(i))

                        ' Last data Point lies in a section. This is not allowed but done for correct calculation
                        If i = CalcData(tCompCali.SecID).Count - 1 Then
                            ErgValues(tCompErg.dist)(run) = (CalcData(tCompCali.v_veh_c)(run) / 3.6) * (1 / HzIn2)
                        End If
                    End If
                End If
            Else
                ' Finish calculation after a valid section
                If firstIn = False Then
                    ' Calculate the results from the last section
                    ErgValues(tCompErg.v_veh)(run) = ErgValues(tCompErg.v_veh)(run) / anz
                    ErgValues(tCompErg.dist)(run) = CalcData(tCompCali.dist)(i - 1) - ErgValues(tCompErg.dist)(run)
                    anz = 0
                    run += 1
                    firstIn = True
                End If
            End If
        Next i

        Return True
    End Function

    ' Calculate the values for v_wind
    Public Function fWindBetaAirErg() As Boolean
        ' Declaration
        Dim i, run, anz As Integer
        Dim firstIn As Boolean = True

        ' Initialise
        run = 0
        anz = 0

        ' Calculate the values for v_wind
        For i = 0 To CalcData(tCompCali.SecID).Count - 1
            If CalcData(tCompCali.SecID)(i) <> 0 Then
                If firstIn Then
                    ErgValues(tCompErg.v_wind_avg)(run) = (CalcData(tCompCali.vwind_c)(i))
                    ErgValues(tCompErg.v_wind_1s)(run) = (CalcData(tCompCali.vwind_1s)(i))
                    ErgValues(tCompErg.beta_avg)(run) = (CalcData(tCompCali.beta_c)(i))
                    ErgValues(tCompErg.vair)(run) = (CalcData(tCompCali.vair_c)(i))
                    ErgValues(tCompErg.beta_uf)(run) = (CalcData(tCompCali.beta_uf)(i))
                    ErgValues(tCompErg.vair_uf)(run) = (CalcData(tCompCali.vair_uf)(i))
                    ErgValues(tCompErg.v_wind_1s_max)(run) = (CalcData(tCompCali.vwind_1s)(i))
                    firstIn = False
                    anz += 1
                Else
                    If (CalcData(tCompCali.SecID)(i) = CalcData(tCompCali.SecID)(i - 1)) And (CalcData(tCompCali.DirID)(i) = CalcData(tCompCali.DirID)(i - 1)) Then
                        ' Build the sum
                        ErgValues(tCompErg.v_wind_avg)(run) += CalcData(tCompCali.vwind_c)(i)
                        ErgValues(tCompErg.v_wind_1s)(run) += CalcData(tCompCali.vwind_1s)(i)
                        ErgValues(tCompErg.beta_avg)(run) += CalcData(tCompCali.beta_c)(i)
                        ErgValues(tCompErg.vair)(run) += CalcData(tCompCali.vair_c)(i)
                        ErgValues(tCompErg.beta_uf)(run) += CalcData(tCompCali.beta_uf)(i)
                        ErgValues(tCompErg.vair_uf)(run) += CalcData(tCompCali.vair_uf)(i)
                        If ErgValues(tCompErg.v_wind_1s_max)(run) < CalcData(tCompCali.vwind_1s)(i) Then ErgValues(tCompErg.v_wind_1s_max)(run) = CalcData(tCompCali.vwind_1s)(i)
                        anz += 1

                        ' If last point lies inside of a section (Not allowed only for correct calculation)
                        If i = CalcData(tCompCali.SecID).Count - 1 Then
                            ErgValues(tCompErg.v_wind_avg)(run) = ErgValues(tCompErg.v_wind_avg)(run) / anz
                            ErgValues(tCompErg.v_wind_1s)(run) = ErgValues(tCompErg.v_wind_1s)(run) / anz
                            ErgValues(tCompErg.beta_avg)(run) = ErgValues(tCompErg.beta_avg)(run) / anz
                            ErgValues(tCompErg.vair)(run) = ErgValues(tCompErg.vair)(run) / anz
                            ErgValues(tCompErg.beta_uf)(run) = ErgValues(tCompErg.beta_uf)(run) / anz
                            ErgValues(tCompErg.vair_uf)(run) = ErgValues(tCompErg.vair_uf)(run) / anz
                        End If
                    Else
                        ' Calculate the results from the last section
                        ErgValues(tCompErg.v_wind_avg)(run) = ErgValues(tCompErg.v_wind_avg)(run) / anz
                        ErgValues(tCompErg.v_wind_1s)(run) = ErgValues(tCompErg.v_wind_1s)(run) / anz
                        ErgValues(tCompErg.beta_avg)(run) = ErgValues(tCompErg.beta_avg)(run) / anz
                        ErgValues(tCompErg.vair)(run) = ErgValues(tCompErg.vair)(run) / anz
                        ErgValues(tCompErg.beta_uf)(run) = ErgValues(tCompErg.beta_uf)(run) / anz
                        ErgValues(tCompErg.vair_uf)(run) = ErgValues(tCompErg.vair_uf)(run) / anz

                        ' Add a new Section to the resultfile
                        anz = 1
                        run += 1
                        ErgValues(tCompErg.v_wind_avg)(run) = (CalcData(tCompCali.vwind_c)(i))
                        ErgValues(tCompErg.v_wind_1s)(run) = (CalcData(tCompCali.vwind_1s)(i))
                        ErgValues(tCompErg.beta_avg)(run) = (CalcData(tCompCali.beta_c)(i))
                        ErgValues(tCompErg.vair)(run) = (CalcData(tCompCali.vair_c)(i))
                        ErgValues(tCompErg.beta_uf)(run) = (CalcData(tCompCali.beta_uf)(i))
                        ErgValues(tCompErg.vair_uf)(run) = (CalcData(tCompCali.vair_uf)(i))
                        ErgValues(tCompErg.v_wind_1s_max)(run) = (CalcData(tCompCali.vwind_1s)(i))
                    End If
                End If
            Else
                ' Finish calculation after a valid section
                If run > 0 And firstIn = False Then
                    ' Calculate the results from the last section
                    ErgValues(tCompErg.v_wind_avg)(run) = ErgValues(tCompErg.v_wind_avg)(run) / anz
                    ErgValues(tCompErg.v_wind_1s)(run) = ErgValues(tCompErg.v_wind_1s)(run) / anz
                    ErgValues(tCompErg.beta_avg)(run) = ErgValues(tCompErg.beta_avg)(run) / anz
                    ErgValues(tCompErg.vair)(run) = ErgValues(tCompErg.vair)(run) / anz
                    ErgValues(tCompErg.beta_uf)(run) = ErgValues(tCompErg.beta_uf)(run) / anz
                    ErgValues(tCompErg.vair_uf)(run) = ErgValues(tCompErg.vair_uf)(run) / anz
                    anz = 0
                    run += 1
                    firstIn = True
                End If
            End If
        Next i

        Return True
    End Function

    ' Calculate the corrected vehicle speed
    Public Function fCalcSpeedValCalib() As Boolean
        ' Declaration
        Dim i, run As Integer
        Dim firstIn As Boolean = True

        ' Initialise
        run = 0

        ' Calculate the moving average
        fMoveAve(CalcData(tCompCali.t), CalcData(tCompCali.v_veh_c), CalcData(tCompCali.v_veh_1s))

        ' Calculate the section averages
        For i = 0 To CalcData(tCompCali.SecID).Count - 1
            If CalcData(tCompCali.SecID)(i) <> 0 Then
                If firstIn Then
                    ErgValues(tCompErg.v_veh_1s_max)(run) = (CalcData(tCompCali.v_veh_1s)(i))
                    ErgValues(tCompErg.v_veh_1s_min)(run) = (CalcData(tCompCali.v_veh_1s)(i))

                    firstIn = False
                Else
                    If (CalcData(tCompCali.SecID)(i) = CalcData(tCompCali.SecID)(i - 1)) And (CalcData(tCompCali.DirID)(i) = CalcData(tCompCali.DirID)(i - 1)) Then
                        ' Find min/max
                        If ErgValues(tCompErg.v_veh_1s_max)(run) < CalcData(tCompCali.v_veh_1s)(i) Then ErgValues(tCompErg.v_veh_1s_max)(run) = CalcData(tCompCali.v_veh_1s)(i)
                        If ErgValues(tCompErg.v_veh_1s_min)(run) > CalcData(tCompCali.v_veh_1s)(i) Then ErgValues(tCompErg.v_veh_1s_min)(run) = CalcData(tCompCali.v_veh_1s)(i)
                    Else
                        ' Start with a new section
                        run += 1
                        ErgValues(tCompErg.v_veh_1s_max)(run) = (CalcData(tCompCali.v_veh_1s)(i))
                        ErgValues(tCompErg.v_veh_1s_min)(run) = (CalcData(tCompCali.v_veh_1s)(i))
                    End If
                End If
            End If
        Next i

        Return True
    End Function

    ' Calculate the corrected vehicle speed
    Public Function fCalcSpeedVal(ByVal Altdata As List(Of cAlt), ByVal vehicleX As cVehicle, ByVal coastingSeq As Integer, ByRef r_dyn_ref As Double) As Boolean
        ' Declaration
        Dim i, j, run, anz, RunIDx As Integer
        Dim firstIn As Boolean = True
        Dim igear As Double
        Dim HzIn2 As Double

        ' Initialise
        run = 0
        anz = 0
        If coastingSeq = 1 Or coastingSeq = 2 Then
            ' Set the gearRatio
            If SMT_AMT_DCT Then
                igear = vehicleX.gearRatio_low
            Else
                igear = 1
            End If
            If coastingSeq = 1 Then
                RunIDx = IDLS1
            Else
                RunIDx = IDLS2
            End If
        Else
            ' Set the gearRatio
            If SMT_AMT_DCT Then
                igear = vehicleX.gearRatio_high
            Else
                igear = 1
            End If
            RunIDx = IDHS
        End If

        ' Calculate n_eng/n_card/n_wheel_ave/n_EM of the speed run
        For i = 0 To CalcData(tCompCali.SecID).Count - 1
            If InputData.ContainsKey(tComp.n_eng) Then CalcData(tCompCali.n_ec)(i) = InputData(tComp.n_eng)(i)
            If InputData.ContainsKey(tComp.n_card) Then CalcData(tCompCali.n_ec)(i) = InputData(tComp.n_card)(i)
            If InputData.ContainsKey(tComp.n_wheel_ave) Then CalcData(tCompCali.n_ec)(i) = InputData(tComp.n_wheel_ave)(i)
            If InputData.ContainsKey(tComp.n_EM) Then CalcData(tCompCali.n_ec)(i) = InputData(tComp.n_EM)(i)
        Next i

        ' Calculate the other values of the speed run
        For i = 0 To CalcData(tCompCali.SecID).Count - 1
            ' Wheel rotation
            CalcData(tCompCali.omega_wh)(i) = (CalcData(tCompCali.n_ec)(i) * Math.PI / (30 * vehicleX.axleRatio * igear))

            ' Torque sum
            CalcData(tCompCali.tq_sum)(i) = CalcData(tCompCali.tq_l_c)(i) + CalcData(tCompCali.tq_r_c)(i)

            ' Time
            If CalcData(tCompCali.v_veh_c)(i) < (2.5 * 3.6) Then
                CalcData(tCompCali.t_float)(i) = Crt.dist_float / 2.5
            Else
                CalcData(tCompCali.t_float)(i) = Crt.dist_float / (CalcData(tCompCali.v_veh_c)(i) / 3.6)
            End If

            ' F trac raw
            CalcData(tCompCali.F_trac)(i) = (CalcData(tCompCali.tq_l_c)(i) + CalcData(tCompCali.tq_r_c)(i)) * CalcData(tCompCali.omega_wh)(i) / (CalcData(tCompCali.v_veh_c)(i) / 3.6)

            If Crt.gradient_correction Then
                If CalcData(tCompCali.SecID)(i) <> 0 Then
                    ' Altitude
                    CalcData(tCompCali.alt)(i) = fAltInterp(Altdata(fSecPos(Altdata, CalcData(tCompCali.SecID)(i), CalcData(tCompCali.DirID)(i))), CalcData(tCompCali.lati_root)(i), CalcData(tCompCali.longi_root)(i))
                End If
            End If

            ' vair_sq
            CalcData(tCompCali.vair_c_sq)(i) = CalcData(tCompCali.vair_c)(i) ^ 2

            ' Find the position in the weather data
            j = fFindPos(CalcData(tCompCali.t)(i), InputWeatherData(tCompWeat.t))

            ' Temprature, Pressure, Humidity
            If j = -1 Then
                Throw New Exception(format("The test time({0}) is outside the range of the data from the stationary weather station. Please check weather file and if data time is correct (day crossings correct included?)", InputData(tComp.t)(i)))
            Else
                CalcData(tCompCali.t_amp_stat)(i) = InterpLinear(InputWeatherData(tCompWeat.t)(j - 1), InputWeatherData(tCompWeat.t)(j), InputWeatherData(tCompWeat.t_amb_stat)(j - 1), InputWeatherData(tCompWeat.t_amb_stat)(j), CalcData(tCompCali.t)(i))
                CalcData(tCompCali.p_amp_stat)(i) = InterpLinear(InputWeatherData(tCompWeat.t)(j - 1), InputWeatherData(tCompWeat.t)(j), InputWeatherData(tCompWeat.p_amp_stat)(j - 1), InputWeatherData(tCompWeat.p_amp_stat)(j), CalcData(tCompCali.t)(i))
                CalcData(tCompCali.rh_stat)(i) = InterpLinear(InputWeatherData(tCompWeat.t)(j - 1), InputWeatherData(tCompWeat.t)(j), InputWeatherData(tCompWeat.rh_stat)(j - 1), InputWeatherData(tCompWeat.rh_stat)(j), CalcData(tCompCali.t)(i))
            End If
        Next i

        ' Calculate the moving averages
        fMoveAve(CalcData(tCompCali.t), CalcData(tCompCali.tq_sum), CalcData(tCompCali.tq_sum_float), CalcData(tCompCali.t_float))
        fMoveAve(CalcData(tCompCali.t), CalcData(tCompCali.tq_sum), CalcData(tCompCali.tq_sum_1s))
        fMoveAve(CalcData(tCompCali.t), CalcData(tCompCali.v_veh_c), CalcData(tCompCali.v_veh_1s))
        fMoveAve(CalcData(tCompCali.t), CalcData(tCompCali.v_veh_c), CalcData(tCompCali.v_veh_acc), Crt.acc_corr_avg)
        fMoveAve(CalcData(tCompCali.t), CalcData(tCompCali.v_veh_c), CalcData(tCompCali.v_veh_float), CalcData(tCompCali.t_float))
        fMoveAve(CalcData(tCompCali.t), CalcData(tCompCali.n_ec), CalcData(tCompCali.n_ec_1s))
        fMoveAve(CalcData(tCompCali.t), CalcData(tCompCali.n_ec), CalcData(tCompCali.n_ec_float), CalcData(tCompCali.t_float))

        ' Calculate the remaining values
        For i = 0 To CalcData(tCompCali.SecID).Count - 1
            ' Acceleration and omega_p
            If i = 0 Or i = CalcData(tCompCali.SecID).Count - 1 Then
                CalcData(tCompCali.a_veh_avg)(i) = 0
            Else
                HzIn2 = 1 / ((CalcData(tCompCali.t)(i + 1) - CalcData(tCompCali.t)(i - 1)) / 2)
                CalcData(tCompCali.a_veh_avg)(i) = (CalcData(tCompCali.v_veh_acc)(i + 1) - CalcData(tCompCali.v_veh_acc)(i - 1)) / (3.6 * 2) * HzIn2
            End If

            If Crt.gradient_correction Then
                If CalcData(tCompCali.SecID)(i) <> 0 Then
                    ' Slope Deg
                    If i > 0 And i < CalcData(tCompCali.SecID).Count - 1 Then
                        If CalcData(tCompCali.SecID)(i - 1) <> CalcData(tCompCali.SecID)(i) Then
                            ' First value
                            If (CalcData(tCompCali.dist_root)(i + 1) - CalcData(tCompCali.dist_root)(i)) = 0 Then
                                CalcData(tCompCali.slope_deg)(i) = 0
                                logme(8, False, "Standstill or loss of vehicle speed signal inside MS (at line " & i & ")! Gradient set to 0")
                            Else
                                CalcData(tCompCali.slope_deg)(i) = (Math.Atan((CalcData(tCompCali.alt)(i + 1) - CalcData(tCompCali.alt)(i)) / (CalcData(tCompCali.dist_root)(i + 1) - CalcData(tCompCali.dist_root)(i)))) * 180 / Math.PI
                            End If
                        ElseIf CalcData(tCompCali.SecID)(i + 1) <> CalcData(tCompCali.SecID)(i) Then
                            ' Last value
                            If (CalcData(tCompCali.dist_root)(i) - CalcData(tCompCali.dist_root)(i - 1)) = 0 Then
                                CalcData(tCompCali.slope_deg)(i) = 0
                                logme(8, False, "Standstill or loss of vehicle speed signal inside MS (at line " & i & ")! Gradient set to 0")
                            Else
                                CalcData(tCompCali.slope_deg)(i) = (Math.Atan((CalcData(tCompCali.alt)(i) - CalcData(tCompCali.alt)(i - 1)) / (CalcData(tCompCali.dist_root)(i) - CalcData(tCompCali.dist_root)(i - 1)))) * 180 / Math.PI
                            End If
                        ElseIf CalcData(tCompCali.SecID)(i - 1) = CalcData(tCompCali.SecID)(i) And CalcData(tCompCali.SecID)(i + 1) = CalcData(tCompCali.SecID)(i) Then
                            ' All other values inside MS
                            If (CalcData(tCompCali.dist_root)(i + 1) - CalcData(tCompCali.dist_root)(i - 1)) = 0 Then
                                CalcData(tCompCali.slope_deg)(i) = 0
                                logme(8, False, "Standstill or loss of vehicle speed signal inside MS (at line " & i & ")! Gradient set to 0")
                            Else
                                CalcData(tCompCali.slope_deg)(i) = (Math.Atan((CalcData(tCompCali.alt)(i + 1) - CalcData(tCompCali.alt)(i - 1)) / (CalcData(tCompCali.dist_root)(i + 1) - CalcData(tCompCali.dist_root)(i - 1)))) * 180 / Math.PI
                            End If
                        End If
                    End If
                End If

                ' F gradient
                CalcData(tCompCali.F_grd)(i) = vehicleX.testMass * 9.81 * Math.Sin(CalcData(tCompCali.slope_deg)(i) * Math.PI / 180)
            End If

            ' Force acceleration
            CalcData(tCompCali.F_acc)(i) = vehicleX.testMass * CalcData(tCompCali.a_veh_avg)(i) * 1.03

            ' Force trajectory
            CalcData(tCompCali.F_res)(i) = CalcData(tCompCali.F_trac)(i)
            If Crt.accel_correction Then CalcData(tCompCali.F_res)(i) -= CalcData(tCompCali.F_acc)(i)
            If Crt.gradient_correction Then CalcData(tCompCali.F_res)(i) -= CalcData(tCompCali.F_grd)(i)
        Next i


        ' Calculate the section averages
        For i = 0 To CalcData(tCompCali.SecID).Count - 1
            If CalcData(tCompCali.SecID)(i) <> 0 Then
                If firstIn Then
                    ErgValues(tCompErg.RunID)(run) = RunIDx
                    ErgValues(tCompErg.n_ec)(run) = (CalcData(tCompCali.n_ec)(i))
                    ErgValues(tCompErg.tq_sum)(run) = (CalcData(tCompCali.tq_sum)(i))
                    ErgValues(tCompErg.tq_sum_1s)(run) = (CalcData(tCompCali.tq_sum_1s)(i))
                    ErgValues(tCompErg.tq_sum_float)(run) = (CalcData(tCompCali.tq_sum_float)(i))
                    ErgValues(tCompErg.t_float)(run) = (CalcData(tCompCali.t_float)(i))
                    ErgValues(tCompErg.F_trac)(run) = (CalcData(tCompCali.F_trac)(i))
                    ErgValues(tCompErg.v_veh_acc)(run) = (CalcData(tCompCali.v_veh_acc)(i))
                    ErgValues(tCompErg.a_veh_avg)(run) = (CalcData(tCompCali.a_veh_avg)(i))
                    ErgValues(tCompErg.F_acc)(run) = (CalcData(tCompCali.F_acc)(i))
                    ErgValues(tCompErg.F_grd)(run) = (CalcData(tCompCali.F_grd)(i))
                    ErgValues(tCompErg.F_res)(run) = (CalcData(tCompCali.F_res)(i))
                    ErgValues(tCompErg.v_veh_1s)(run) = (CalcData(tCompCali.v_veh_1s)(i))
                    ErgValues(tCompErg.v_veh_float)(run) = (CalcData(tCompCali.v_veh_float)(i))
                    ErgValues(tCompErg.t_amb_veh)(run) = (InputData(tComp.t_amb_veh)(i))
                    ErgValues(tCompErg.t_amb_stat)(run) = (CalcData(tCompCali.t_amp_stat)(i))
                    ErgValues(tCompErg.p_amb_stat)(run) = (CalcData(tCompCali.p_amp_stat)(i))
                    ErgValues(tCompErg.rh_stat)(run) = (CalcData(tCompCali.rh_stat)(i))
                    ErgValues(tCompErg.v_air_sq)(run) = (CalcData(tCompCali.vair_c_sq)(i))
                    ErgValues(tCompErg.v_veh_1s_max)(run) = (CalcData(tCompCali.v_veh_1s)(i))
                    ErgValues(tCompErg.v_veh_1s_min)(run) = (CalcData(tCompCali.v_veh_1s)(i))
                    ErgValues(tCompErg.v_veh_float_max)(run) = (CalcData(tCompCali.v_veh_float)(i))
                    ErgValues(tCompErg.v_veh_float_min)(run) = (CalcData(tCompCali.v_veh_float)(i))
                    ErgValues(tCompErg.tq_sum_1s_max)(run) = (CalcData(tCompCali.tq_sum_1s)(i))
                    ErgValues(tCompErg.tq_sum_1s_min)(run) = (CalcData(tCompCali.tq_sum_1s)(i))
                    ErgValues(tCompErg.tq_sum_float_max)(run) = (CalcData(tCompCali.tq_sum_float)(i))
                    ErgValues(tCompErg.tq_sum_float_min)(run) = (CalcData(tCompCali.tq_sum_float)(i))
                    ErgValues(tCompErg.n_ec_1s_max)(run) = (CalcData(tCompCali.n_ec_1s)(i))
                    ErgValues(tCompErg.n_ec_1s_min)(run) = (CalcData(tCompCali.n_ec_1s)(i))
                    ErgValues(tCompErg.n_ec_float_max)(run) = (CalcData(tCompCali.n_ec_float)(i))
                    ErgValues(tCompErg.n_ec_float_min)(run) = (CalcData(tCompCali.n_ec_float)(i))
                    ErgValues(tCompErg.t_ground)(run) = (InputData(tComp.t_ground)(i))

                    ' Last point lies inside of a section (Not allowed only for correct calculation)
                    If i = CalcData(tCompCali.SecID).Count - 1 Then
                        ErgValues(tCompErg.beta_abs)(run) = Math.Abs(ErgValues(tCompErg.beta_avg)(run))
                        ErgValues(tCompErg.vp_H2O)(run) = ((ErgValues(tCompErg.rh_stat)(run) / 100) * 611 * 10 ^ ((7.5 * ErgValues(tCompErg.t_amb_stat)(run)) / (237 + ErgValues(tCompErg.t_amb_stat)(run))))
                        ErgValues(tCompErg.rho_air)(run) = (ErgValues(tCompErg.p_amb_stat)(run) * 100 - ErgValues(tCompErg.vp_H2O)(run)) / (287.05 * (ErgValues(tCompErg.t_amb_veh)(run) + 273.15)) + ErgValues(tCompErg.vp_H2O)(run) / (461.9 * (ErgValues(tCompErg.t_amb_veh)(run) + 273.15))
                        If ErgValues(tCompErg.RunID)(run) = IDHS Then
                            ErgValues(tCompErg.F_res_ref)(run) = ErgValues(tCompErg.F_res)(run) * f_rollHS
                        Else
                            ErgValues(tCompErg.F_res_ref)(run) = ErgValues(tCompErg.F_res)(run) * Crt.rr_corr_factor
                        End If
                        ErgValues(tCompErg.r_dyn)(run) = (30 * igear * vehicleX.axleRatio * ErgValues(tCompErg.v_veh)(run) / 3.6) / (ErgValues(tCompErg.n_ec)(run) * Math.PI)
                        ErgValues(tCompErg.tq_grd)(run) = ErgValues(tCompErg.F_grd)(run) * ErgValues(tCompErg.r_dyn)(run)
                    End If

                    firstIn = False
                    anz += 1
                Else
                    If (CalcData(tCompCali.SecID)(i) = CalcData(tCompCali.SecID)(i - 1)) And (CalcData(tCompCali.DirID)(i) = CalcData(tCompCali.DirID)(i - 1)) Then
                        ' Build the sum
                        ErgValues(tCompErg.n_ec)(run) += (CalcData(tCompCali.n_ec)(i))
                        ErgValues(tCompErg.tq_sum)(run) += (CalcData(tCompCali.tq_sum)(i))
                        ErgValues(tCompErg.tq_sum_1s)(run) += (CalcData(tCompCali.tq_sum_1s)(i))
                        ErgValues(tCompErg.tq_sum_float)(run) += (CalcData(tCompCali.tq_sum_float)(i))
                        ErgValues(tCompErg.t_float)(run) += (CalcData(tCompCali.t_float)(i))
                        ErgValues(tCompErg.F_trac)(run) += (CalcData(tCompCali.F_trac)(i))
                        ErgValues(tCompErg.v_veh_acc)(run) += (CalcData(tCompCali.v_veh_acc)(i))
                        ErgValues(tCompErg.a_veh_avg)(run) += (CalcData(tCompCali.a_veh_avg)(i))
                        ErgValues(tCompErg.F_acc)(run) += (CalcData(tCompCali.F_acc)(i))
                        ErgValues(tCompErg.F_grd)(run) += (CalcData(tCompCali.F_grd)(i))
                        ErgValues(tCompErg.F_res)(run) += (CalcData(tCompCali.F_res)(i))
                        ErgValues(tCompErg.v_veh_1s)(run) += (CalcData(tCompCali.v_veh_1s)(i))
                        ErgValues(tCompErg.v_veh_float)(run) += (CalcData(tCompCali.v_veh_float)(i))
                        ErgValues(tCompErg.t_amb_veh)(run) += (InputData(tComp.t_amb_veh)(i))
                        ErgValues(tCompErg.t_amb_stat)(run) += (CalcData(tCompCali.t_amp_stat)(i))
                        ErgValues(tCompErg.p_amb_stat)(run) += (CalcData(tCompCali.p_amp_stat)(i))
                        ErgValues(tCompErg.rh_stat)(run) += (CalcData(tCompCali.rh_stat)(i))
                        ErgValues(tCompErg.v_air_sq)(run) += (CalcData(tCompCali.vair_c_sq)(i))
                        If ErgValues(tCompErg.v_veh_1s_max)(run) < CalcData(tCompCali.v_veh_1s)(i) Then ErgValues(tCompErg.v_veh_1s_max)(run) = CalcData(tCompCali.v_veh_1s)(i)
                        If ErgValues(tCompErg.v_veh_1s_min)(run) > CalcData(tCompCali.v_veh_1s)(i) Then ErgValues(tCompErg.v_veh_1s_min)(run) = CalcData(tCompCali.v_veh_1s)(i)
                        If ErgValues(tCompErg.v_veh_float_max)(run) < CalcData(tCompCali.v_veh_float)(i) Then ErgValues(tCompErg.v_veh_float_max)(run) = CalcData(tCompCali.v_veh_float)(i)
                        If ErgValues(tCompErg.v_veh_float_min)(run) > CalcData(tCompCali.v_veh_float)(i) Then ErgValues(tCompErg.v_veh_float_min)(run) = CalcData(tCompCali.v_veh_float)(i)
                        If ErgValues(tCompErg.tq_sum_1s_max)(run) < CalcData(tCompCali.tq_sum_1s)(i) Then ErgValues(tCompErg.tq_sum_1s_max)(run) = CalcData(tCompCali.tq_sum_1s)(i)
                        If ErgValues(tCompErg.tq_sum_1s_min)(run) > CalcData(tCompCali.tq_sum_1s)(i) Then ErgValues(tCompErg.tq_sum_1s_min)(run) = CalcData(tCompCali.tq_sum_1s)(i)
                        If ErgValues(tCompErg.tq_sum_float_max)(run) < CalcData(tCompCali.tq_sum_float)(i) Then ErgValues(tCompErg.tq_sum_float_max)(run) = CalcData(tCompCali.tq_sum_float)(i)
                        If ErgValues(tCompErg.tq_sum_float_min)(run) > CalcData(tCompCali.tq_sum_float)(i) Then ErgValues(tCompErg.tq_sum_float_min)(run) = CalcData(tCompCali.tq_sum_float)(i)
                        If ErgValues(tCompErg.n_ec_1s_max)(run) < CalcData(tCompCali.n_ec_1s)(i) Then ErgValues(tCompErg.n_ec_1s_max)(run) = CalcData(tCompCali.n_ec_1s)(i)
                        If ErgValues(tCompErg.n_ec_1s_min)(run) > CalcData(tCompCali.n_ec_1s)(i) Then ErgValues(tCompErg.n_ec_1s_min)(run) = CalcData(tCompCali.n_ec_1s)(i)
                        If ErgValues(tCompErg.n_ec_float_max)(run) < CalcData(tCompCali.n_ec_float)(i) Then ErgValues(tCompErg.n_ec_float_max)(run) = CalcData(tCompCali.n_ec_float)(i)
                        If ErgValues(tCompErg.n_ec_float_min)(run) > CalcData(tCompCali.n_ec_float)(i) Then ErgValues(tCompErg.n_ec_float_min)(run) = CalcData(tCompCali.n_ec_float)(i)
                        ErgValues(tCompErg.t_ground)(run) += (InputData(tComp.t_ground)(i))
                        anz += 1

                        ' Last point lies inside of a section (Not allowed only for correct calculation)
                        If i = CalcData(tCompCali.SecID).Count - 1 Then
                            ErgValues(tCompErg.n_ec)(run) = ErgValues(tCompErg.n_ec)(run) / anz
                            ErgValues(tCompErg.tq_sum)(run) = ErgValues(tCompErg.tq_sum)(run) / anz
                            ErgValues(tCompErg.tq_sum_1s)(run) = ErgValues(tCompErg.tq_sum_1s)(run) / anz
                            ErgValues(tCompErg.tq_sum_float)(run) = ErgValues(tCompErg.tq_sum_float)(run) / anz
                            ErgValues(tCompErg.t_float)(run) = ErgValues(tCompErg.t_float)(run) / anz
                            ErgValues(tCompErg.F_trac)(run) = ErgValues(tCompErg.F_trac)(run) / anz
                            ErgValues(tCompErg.v_veh_acc)(run) = ErgValues(tCompErg.v_veh_acc)(run) / anz
                            ErgValues(tCompErg.a_veh_avg)(run) = ErgValues(tCompErg.a_veh_avg)(run) / anz
                            ErgValues(tCompErg.F_acc)(run) = ErgValues(tCompErg.F_acc)(run) / anz
                            ErgValues(tCompErg.F_grd)(run) = ErgValues(tCompErg.F_grd)(run) / anz
                            ErgValues(tCompErg.F_res)(run) = ErgValues(tCompErg.F_res)(run) / anz
                            ErgValues(tCompErg.v_veh_1s)(run) = ErgValues(tCompErg.v_veh_1s)(run) / anz
                            ErgValues(tCompErg.v_veh_float)(run) = ErgValues(tCompErg.v_veh_float)(run) / anz
                            ErgValues(tCompErg.t_amb_veh)(run) = ErgValues(tCompErg.t_amb_veh)(run) / anz
                            ErgValues(tCompErg.t_amb_stat)(run) = ErgValues(tCompErg.t_amb_stat)(run) / anz
                            ErgValues(tCompErg.p_amb_stat)(run) = ErgValues(tCompErg.p_amb_stat)(run) / anz
                            ErgValues(tCompErg.rh_stat)(run) = ErgValues(tCompErg.rh_stat)(run) / anz
                            ErgValues(tCompErg.v_air_sq)(run) = ErgValues(tCompErg.v_air_sq)(run) / anz
                            ErgValues(tCompErg.beta_abs)(run) = Math.Abs(ErgValues(tCompErg.beta_avg)(run))
                            ErgValues(tCompErg.vp_H2O)(run) = ((ErgValues(tCompErg.rh_stat)(run) / 100) * 611 * 10 ^ ((7.5 * ErgValues(tCompErg.t_amb_stat)(run)) / (237 + ErgValues(tCompErg.t_amb_stat)(run))))
                            ErgValues(tCompErg.rho_air)(run) = (ErgValues(tCompErg.p_amb_stat)(run) * 100 - ErgValues(tCompErg.vp_H2O)(run)) / (287.05 * (ErgValues(tCompErg.t_amb_veh)(run) + 273.15)) + ErgValues(tCompErg.vp_H2O)(run) / (461.9 * (ErgValues(tCompErg.t_amb_veh)(run) + 273.15))
                            If ErgValues(tCompErg.RunID)(run) = IDHS Then
                                ErgValues(tCompErg.F_res_ref)(run) = ErgValues(tCompErg.F_res)(run) * f_rollHS
                            Else
                                ErgValues(tCompErg.F_res_ref)(run) = ErgValues(tCompErg.F_res)(run) * Crt.rr_corr_factor
                            End If
                            ErgValues(tCompErg.t_ground)(run) = ErgValues(tCompErg.t_ground)(run) / anz
                            ErgValues(tCompErg.r_dyn)(run) = (30 * igear * vehicleX.axleRatio * ErgValues(tCompErg.v_veh)(run) / 3.6) / (ErgValues(tCompErg.n_ec)(run) * Math.PI)
                            ErgValues(tCompErg.tq_grd)(run) = ErgValues(tCompErg.F_grd)(run) * ErgValues(tCompErg.r_dyn)(run)
                        End If
                    Else
                        ' Calculate the results from the last section
                        ErgValues(tCompErg.n_ec)(run) = ErgValues(tCompErg.n_ec)(run) / anz
                        ErgValues(tCompErg.tq_sum)(run) = ErgValues(tCompErg.tq_sum)(run) / anz
                        ErgValues(tCompErg.tq_sum_1s)(run) = ErgValues(tCompErg.tq_sum_1s)(run) / anz
                        ErgValues(tCompErg.tq_sum_float)(run) = ErgValues(tCompErg.tq_sum_float)(run) / anz
                        ErgValues(tCompErg.t_float)(run) = ErgValues(tCompErg.t_float)(run) / anz
                        ErgValues(tCompErg.F_trac)(run) = ErgValues(tCompErg.F_trac)(run) / anz
                        ErgValues(tCompErg.v_veh_acc)(run) = ErgValues(tCompErg.v_veh_acc)(run) / anz
                        ErgValues(tCompErg.a_veh_avg)(run) = ErgValues(tCompErg.a_veh_avg)(run) / anz
                        ErgValues(tCompErg.F_acc)(run) = ErgValues(tCompErg.F_acc)(run) / anz
                        ErgValues(tCompErg.F_grd)(run) = ErgValues(tCompErg.F_grd)(run) / anz
                        ErgValues(tCompErg.F_res)(run) = ErgValues(tCompErg.F_res)(run) / anz
                        ErgValues(tCompErg.v_veh_1s)(run) = ErgValues(tCompErg.v_veh_1s)(run) / anz
                        ErgValues(tCompErg.v_veh_float)(run) = ErgValues(tCompErg.v_veh_float)(run) / anz
                        ErgValues(tCompErg.t_amb_veh)(run) = ErgValues(tCompErg.t_amb_veh)(run) / anz
                        ErgValues(tCompErg.t_amb_stat)(run) = ErgValues(tCompErg.t_amb_stat)(run) / anz
                        ErgValues(tCompErg.p_amb_stat)(run) = ErgValues(tCompErg.p_amb_stat)(run) / anz
                        ErgValues(tCompErg.rh_stat)(run) = ErgValues(tCompErg.rh_stat)(run) / anz
                        ErgValues(tCompErg.v_air_sq)(run) = ErgValues(tCompErg.v_air_sq)(run) / anz
                        ErgValues(tCompErg.beta_abs)(run) = Math.Abs(ErgValues(tCompErg.beta_avg)(run))
                        ErgValues(tCompErg.vp_H2O)(run) = ((ErgValues(tCompErg.rh_stat)(run) / 100) * 611 * 10 ^ ((7.5 * ErgValues(tCompErg.t_amb_stat)(run)) / (237 + ErgValues(tCompErg.t_amb_stat)(run))))
                        ErgValues(tCompErg.rho_air)(run) = (ErgValues(tCompErg.p_amb_stat)(run) * 100 - ErgValues(tCompErg.vp_H2O)(run)) / (287.05 * (ErgValues(tCompErg.t_amb_veh)(run) + 273.15)) + ErgValues(tCompErg.vp_H2O)(run) / (461.9 * (ErgValues(tCompErg.t_amb_veh)(run) + 273.15))
                        If ErgValues(tCompErg.RunID)(run) = IDHS Then
                            ErgValues(tCompErg.F_res_ref)(run) = ErgValues(tCompErg.F_res)(run) * f_rollHS
                        Else
                            ErgValues(tCompErg.F_res_ref)(run) = ErgValues(tCompErg.F_res)(run) * Crt.rr_corr_factor
                        End If
                        ErgValues(tCompErg.t_ground)(run) = ErgValues(tCompErg.t_ground)(run) / anz
                        ErgValues(tCompErg.r_dyn)(run) = (30 * igear * vehicleX.axleRatio * ErgValues(tCompErg.v_veh)(run) / 3.6) / (ErgValues(tCompErg.n_ec)(run) * Math.PI)
                        ErgValues(tCompErg.tq_grd)(run) = ErgValues(tCompErg.F_grd)(run) * ErgValues(tCompErg.r_dyn)(run)

                        ' Add a new Section to the resultfile
                        run += 1
                        anz = 1
                        ErgValues(tCompErg.RunID)(run) = RunIDx
                        ErgValues(tCompErg.n_ec)(run) = (CalcData(tCompCali.n_ec)(i))
                        ErgValues(tCompErg.tq_sum)(run) = (CalcData(tCompCali.tq_sum)(i))
                        ErgValues(tCompErg.tq_sum_1s)(run) = (CalcData(tCompCali.tq_sum_1s)(i))
                        ErgValues(tCompErg.tq_sum_float)(run) = (CalcData(tCompCali.tq_sum_float)(i))
                        ErgValues(tCompErg.t_float)(run) = (CalcData(tCompCali.t_float)(i))
                        ErgValues(tCompErg.F_trac)(run) = (CalcData(tCompCali.F_trac)(i))
                        ErgValues(tCompErg.v_veh_acc)(run) = (CalcData(tCompCali.v_veh_acc)(i))
                        ErgValues(tCompErg.a_veh_avg)(run) = (CalcData(tCompCali.a_veh_avg)(i))
                        ErgValues(tCompErg.F_acc)(run) = (CalcData(tCompCali.F_acc)(i))
                        ErgValues(tCompErg.F_grd)(run) = (CalcData(tCompCali.F_grd)(i))
                        ErgValues(tCompErg.F_res)(run) = (CalcData(tCompCali.F_res)(i))
                        ErgValues(tCompErg.v_veh_1s)(run) = (CalcData(tCompCali.v_veh_1s)(i))
                        ErgValues(tCompErg.v_veh_float)(run) = (CalcData(tCompCali.v_veh_float)(i))
                        ErgValues(tCompErg.t_amb_veh)(run) = (InputData(tComp.t_amb_veh)(i))
                        ErgValues(tCompErg.t_amb_stat)(run) = (CalcData(tCompCali.t_amp_stat)(i))
                        ErgValues(tCompErg.p_amb_stat)(run) = (CalcData(tCompCali.p_amp_stat)(i))
                        ErgValues(tCompErg.rh_stat)(run) = (CalcData(tCompCali.rh_stat)(i))
                        ErgValues(tCompErg.v_air_sq)(run) = (CalcData(tCompCali.vair_c_sq)(i))
                        ErgValues(tCompErg.v_veh_1s_max)(run) = (CalcData(tCompCali.v_veh_1s)(i))
                        ErgValues(tCompErg.v_veh_1s_min)(run) = (CalcData(tCompCali.v_veh_1s)(i))
                        ErgValues(tCompErg.v_veh_float_max)(run) = (CalcData(tCompCali.v_veh_float)(i))
                        ErgValues(tCompErg.v_veh_float_min)(run) = (CalcData(tCompCali.v_veh_float)(i))
                        ErgValues(tCompErg.tq_sum_1s_max)(run) = (CalcData(tCompCali.tq_sum_1s)(i))
                        ErgValues(tCompErg.tq_sum_1s_min)(run) = (CalcData(tCompCali.tq_sum_1s)(i))
                        ErgValues(tCompErg.tq_sum_float_max)(run) = (CalcData(tCompCali.tq_sum_float)(i))
                        ErgValues(tCompErg.tq_sum_float_min)(run) = (CalcData(tCompCali.tq_sum_float)(i))
                        ErgValues(tCompErg.n_ec_1s_max)(run) = (CalcData(tCompCali.n_ec_1s)(i))
                        ErgValues(tCompErg.n_ec_1s_min)(run) = (CalcData(tCompCali.n_ec_1s)(i))
                        ErgValues(tCompErg.n_ec_float_max)(run) = (CalcData(tCompCali.n_ec_float)(i))
                        ErgValues(tCompErg.n_ec_float_min)(run) = (CalcData(tCompCali.n_ec_float)(i))
                        ErgValues(tCompErg.t_ground)(run) = (InputData(tComp.t_ground)(i))

                        ' Last point lies inside of a section (Not allowed only for correct calculation)
                        If i = CalcData(tCompCali.SecID).Count - 1 Then
                            ErgValues(tCompErg.beta_abs)(run) = Math.Abs(ErgValues(tCompErg.beta_avg)(run))
                            ErgValues(tCompErg.vp_H2O)(run) = ((ErgValues(tCompErg.rh_stat)(run) / 100) * 611 * 10 ^ ((7.5 * ErgValues(tCompErg.t_amb_stat)(run)) / (237 + ErgValues(tCompErg.t_amb_stat)(run))))
                            ErgValues(tCompErg.rho_air)(run) = (ErgValues(tCompErg.p_amb_stat)(run) * 100 - ErgValues(tCompErg.vp_H2O)(run)) / (287.05 * (ErgValues(tCompErg.t_amb_veh)(run) + 273.15)) + ErgValues(tCompErg.vp_H2O)(run) / (461.9 * (ErgValues(tCompErg.t_amb_veh)(run) + 273.15))
                            If ErgValues(tCompErg.RunID)(run) = IDHS Then
                                ErgValues(tCompErg.F_res_ref)(run) = ErgValues(tCompErg.F_res)(run) * f_rollHS
                            Else
                                ErgValues(tCompErg.F_res_ref)(run) = ErgValues(tCompErg.F_res)(run) * Crt.rr_corr_factor
                            End If
                            ErgValues(tCompErg.r_dyn)(run) = (30 * igear * vehicleX.axleRatio * ErgValues(tCompErg.v_veh)(run) / 3.6) / (ErgValues(tCompErg.n_ec)(run) * Math.PI)
                            ErgValues(tCompErg.tq_grd)(run) = ErgValues(tCompErg.F_grd)(run) * ErgValues(tCompErg.r_dyn)(run)
                        End If
                    End If
                End If
            Else
                ' Finish calculation after a valid section
                If run > 0 And firstIn = False Then
                    ' Calculate the results from the last section
                    ErgValues(tCompErg.n_ec)(run) = ErgValues(tCompErg.n_ec)(run) / anz
                    ErgValues(tCompErg.tq_sum)(run) = ErgValues(tCompErg.tq_sum)(run) / anz
                    ErgValues(tCompErg.tq_sum_1s)(run) = ErgValues(tCompErg.tq_sum_1s)(run) / anz
                    ErgValues(tCompErg.tq_sum_float)(run) = ErgValues(tCompErg.tq_sum_float)(run) / anz
                    ErgValues(tCompErg.t_float)(run) = ErgValues(tCompErg.t_float)(run) / anz
                    ErgValues(tCompErg.F_trac)(run) = ErgValues(tCompErg.F_trac)(run) / anz
                    ErgValues(tCompErg.v_veh_acc)(run) = ErgValues(tCompErg.v_veh_acc)(run) / anz
                    ErgValues(tCompErg.a_veh_avg)(run) = ErgValues(tCompErg.a_veh_avg)(run) / anz
                    ErgValues(tCompErg.F_acc)(run) = ErgValues(tCompErg.F_acc)(run) / anz
                    ErgValues(tCompErg.F_grd)(run) = ErgValues(tCompErg.F_grd)(run) / anz
                    ErgValues(tCompErg.F_res)(run) = ErgValues(tCompErg.F_res)(run) / anz
                    ErgValues(tCompErg.v_veh_1s)(run) = ErgValues(tCompErg.v_veh_1s)(run) / anz
                    ErgValues(tCompErg.v_veh_float)(run) = ErgValues(tCompErg.v_veh_float)(run) / anz
                    ErgValues(tCompErg.t_amb_veh)(run) = ErgValues(tCompErg.t_amb_veh)(run) / anz
                    ErgValues(tCompErg.t_amb_stat)(run) = ErgValues(tCompErg.t_amb_stat)(run) / anz
                    ErgValues(tCompErg.p_amb_stat)(run) = ErgValues(tCompErg.p_amb_stat)(run) / anz
                    ErgValues(tCompErg.rh_stat)(run) = ErgValues(tCompErg.rh_stat)(run) / anz
                    ErgValues(tCompErg.v_air_sq)(run) = ErgValues(tCompErg.v_air_sq)(run) / anz
                    ErgValues(tCompErg.beta_abs)(run) = Math.Abs(ErgValues(tCompErg.beta_avg)(run))
                    ErgValues(tCompErg.vp_H2O)(run) = ((ErgValues(tCompErg.rh_stat)(run) / 100) * 611 * 10 ^ ((7.5 * ErgValues(tCompErg.t_amb_stat)(run)) / (237 + ErgValues(tCompErg.t_amb_stat)(run))))
                    ErgValues(tCompErg.rho_air)(run) = (ErgValues(tCompErg.p_amb_stat)(run) * 100 - ErgValues(tCompErg.vp_H2O)(run)) / (287.05 * (ErgValues(tCompErg.t_amb_veh)(run) + 273.15)) + ErgValues(tCompErg.vp_H2O)(run) / (461.9 * (ErgValues(tCompErg.t_amb_veh)(run) + 273.15))
                    If ErgValues(tCompErg.RunID)(run) = IDHS Then
                        ErgValues(tCompErg.F_res_ref)(run) = ErgValues(tCompErg.F_res)(run) * f_rollHS
                    Else
                        ErgValues(tCompErg.F_res_ref)(run) = ErgValues(tCompErg.F_res)(run) * Crt.rr_corr_factor
                    End If
                    ErgValues(tCompErg.t_ground)(run) = ErgValues(tCompErg.t_ground)(run) / anz
                    ErgValues(tCompErg.r_dyn)(run) = (30 * igear * vehicleX.axleRatio * ErgValues(tCompErg.v_veh)(run) / 3.6) / (ErgValues(tCompErg.n_ec)(run) * Math.PI)
                    ErgValues(tCompErg.tq_grd)(run) = ErgValues(tCompErg.F_grd)(run) * ErgValues(tCompErg.r_dyn)(run)

                    anz = 0
                    run += 1
                    firstIn = True
                End If
            End If
        Next i

        ' Calculate r_dyn_ref
        anz = 0
        For i = 0 To ErgValues(tCompErg.valid).Count - 1
            If ErgValues(tCompErg.valid)(i) = 1 Then
                r_dyn_ref += ErgValues(tCompErg.r_dyn)(i)
                anz += 1
            End If
        Next i
        If anz > 0 Then
            r_dyn_ref = r_dyn_ref / anz
        Else
            r_dyn_ref = 0
        End If

        Return True
    End Function

    ' Find the position of the Measurement section
    Private Function fMSPos(ByVal orgMSCX As cMSC, ByVal SecID As Integer, ByVal DirID As Integer) As Integer
        ' Deklarations
        Dim i As Integer

        For i = 0 To orgMSCX.meID.Count - 1
            If orgMSCX.meID(i) = SecID And orgMSCX.dID(i) = DirID Then
                Return i
            End If
        Next

        Throw New Exception(format("The section (SecID {0}, DirID {1}) is not inside the declared csms file", SecID, DirID))
    End Function

End Module
