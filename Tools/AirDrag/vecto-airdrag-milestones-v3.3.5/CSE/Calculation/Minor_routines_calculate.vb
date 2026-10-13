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

Module Minor_routines_calculate
    ' Koordinat calculation
    Public Function KleinPkt(ByVal orgKoordX As Double, ByVal orgKoordY As Double, ByVal Tae As Double, ByVal Saf As Double, ByVal Sfp As Double) As Array
        ' Declaration
        Dim a, o, Koord(1) As Double
        Dim KoordAr As Array

        ' Calculat of the angle terms
        a = Math.Cos(Tae * Math.PI / 180)
        o = Math.Sin(Tae * Math.PI / 180)

        ' Calculation of the Koordinates
        Koord(0) = orgKoordX + o * Saf + a * Sfp
        Koord(1) = orgKoordY + a * Saf - o * Sfp

        ' Return the calculated Koordinates
        KoordAr = ({Koord(0), Koord(1)})
        Return KoordAr
    End Function

    ' Angle calculation with quadrant request
    Public Function QuadReq(ByVal DX As Double, ByVal Dy As Double) As Double
        ' Calculation addicted by the Koordinate difference
        If Dy > 0 And DX > 0 Then
            QuadReq = 90 - Math.Atan(Dy / DX) * 180 / Math.PI
        ElseIf Dy < 0 And DX > 0 Then
            QuadReq = 90 + Math.Abs(Math.Atan(Dy / DX) * 180 / Math.PI)
        ElseIf Dy <= 0 And DX < 0 Then
            QuadReq = 270 - Math.Atan(Dy / DX) * 180 / Math.PI
        ElseIf Dy > 0 And DX < 0 Then
            QuadReq = 270 + Math.Abs(Math.Atan(Dy / DX) * 180 / Math.PI)
        ElseIf Dy = 0 And DX > 0 Then
            QuadReq = 90
        ElseIf Dy >= 0 And DX = 0 Then
            QuadReq = 0
        ElseIf Dy < 0 And DX = 0 Then
            QuadReq = 180
        Else
            logme(9, False, "The angle definition is not possible")
            QuadReq = "x"
        End If
    End Function

    ' Höhe-Höhenfüßpunkt calculation
    Public Function HHF(ByVal KoordA As Array, ByVal KoordE As Array, ByVal KoordP As Array) As cHHF
        ' Declaration
        Dim DXae, DYae, DXap, DYap, DXep, DYep, Aae, Aap, KAae As Double
        Dim result As New cHHF

        ' Calculation of the parameters
        DXae = KoordE(0) - KoordA(0)
        DYae = KoordE(1) - KoordA(1)
        DXap = KoordP(0) - KoordA(0)
        DYap = KoordP(1) - KoordA(1)
        DXep = KoordP(0) - KoordE(0)
        DYep = KoordP(1) - KoordE(1)

        ' Calculate the angles
        Aae = QuadReq(DXae, DYae)
        Aap = QuadReq(DXap, DYap)

        ' Calculate the angle from the direction A-E to A-P
        KAae = Math.Abs(Aae - 360)
        result.KAap = Aap + KAae
        If result.KAap > 360 Then result.KAap = result.KAap - 360

        ' Calculate if the Point lays in the virtual point area
        result.p = ((DXae ^ 2 + DYae ^ 2) + (DXep ^ 2 + DYep ^ 2) - (DXap ^ 2 + DYap ^ 2)) / (2 * Math.Sqrt(DXae ^ 2 + DYae ^ 2))
        result.q = ((DXae ^ 2 + DYae ^ 2) - (DXep ^ 2 + DYep ^ 2) + (DXap ^ 2 + DYap ^ 2)) / (2 * Math.Sqrt(DXae ^ 2 + DYae ^ 2))

        ' Calculate the distance from P to line AE
        result.hp = Math.Sqrt((DXep ^ 2 + DYep ^ 2) - result.p ^ 2)

        Return result
    End Function

    ' Function for the calculation of the moving average
    Public Function fMoveAve(ByVal TimeX As List(Of Double), ByVal ValuesX As List(Of Double), ByRef NewValues As List(Of Double), Optional ByVal Ave_t As Single = AveSec) As Boolean
        ' Declaration
        Dim i, lauf, laufE, zEnd, Anz, pos, PktB, PktE, anzPkt As Integer
        Dim t0, tstep As Double
        Dim Sprung As Boolean
        Dim Summe As Double
        Dim tI As New List(Of Integer)

        ' Initialise
        Sprung = False
        tstep = 0

        'Check whether Time is not reversed
        For i = 1 To TimeX.Count - 1
            If i = 1 Then tstep = TimeX(i) - TimeX(i - 1)
            If tstep + (tstep * Crt.delta_Hz_max / 100) < Math.Abs(TimeX(i) - TimeX(i - 1)) Or tstep - (tstep * Crt.delta_Hz_max / 100) > Math.Abs(TimeX(i) - TimeX(i - 1)) Then
                Sprung = True
                TI.Add(i)
            End If
        Next i

        ' Initialise
        If TI.Count = 0 Then TI.Add(0)
        If Sprung Then TI.Add(TimeX.Count - 1)
        Anz = Math.Round(Ave_t / tstep, 0)
        zEnd = TimeX.Count - 1
        pos = 0
        t0 = 0
        anzPkt = 0
        If Sprung Then
            laufE = ti.Count - 1
        Else
            laufE = 0
        End If

        For lauf = 0 To laufE
            If Sprung And lauf = 0 Then
                zEnd = tI(lauf) - 1
                pos = 0
            ElseIf Sprung And lauf = laufE Then
                zEnd = tI(lauf)
                t0 = tI(lauf - 1)
                pos = tI(lauf - 1)
            ElseIf Sprung And lauf <> 0 Then
                zEnd = TI(lauf) - 1
                t0 = TI(lauf - 1)
                pos = tI(lauf - 1)
            Else
                pos = tI(lauf)
                t0 = tI(lauf)
                zEnd = TimeX.Count - 1
            End If

            For i = pos To zEnd
                ' Set the boundries
                PktB = i - Anz / 2
                PktE = i + Anz / 2
                If PktB < t0 Then PktB = t0
                If PktE > zEnd Then PktE = zEnd

                ' Build the sum
                For j = PktB To PktE
                    Summe += ValuesX(j)
                    anzPkt += 1
                Next j

                NewValues(i) = Summe / anzPkt
                Summe = 0
                anzPkt = 0
            Next i
        Next lauf

        Return True
    End Function

    ' Function for the calculation of the moving average
    Public Function fMoveAve(ByVal TimeX As List(Of Double), ByVal ValuesX As List(Of Double), ByRef NewValues As List(Of Double), ByRef StepValues As List(Of Double)) As Boolean
        ' Declaration
        Dim i, lauf, laufE, zEnd, Anz, pos, PktB, PktE, anzPkt As Integer
        Dim t0, tstep As Double
        Dim Sprung As Boolean
        Dim Summe As Double
        Dim tI As New List(Of Integer)

        ' Initialise
        Sprung = False
        tstep = 0

        'Check whether Time is not reversed
        For i = 1 To TimeX.Count - 1
            If i = 1 Then tstep = TimeX(i) - TimeX(i - 1)
            If tstep + (tstep * Crt.delta_Hz_max / 100) < Math.Abs(TimeX(i) - TimeX(i - 1)) Or tstep - (tstep * Crt.delta_Hz_max / 100) > Math.Abs(TimeX(i) - TimeX(i - 1)) Then
                Sprung = True
                tI.Add(i)
            End If
        Next i

        ' Initialise
        If tI.Count = 0 Then tI.Add(0)
        If Sprung Then tI.Add(TimeX.Count - 1)
        zEnd = TimeX.Count - 1
        pos = 0
        t0 = 0
        anzPkt = 0
        If Sprung Then
            laufE = tI.Count - 1
        Else
            laufE = 0
        End If

        For lauf = 0 To laufE
            If Sprung And lauf = 0 Then
                zEnd = tI(lauf) - 1
                pos = 0
            ElseIf Sprung And lauf = laufE Then
                zEnd = tI(lauf)
                t0 = tI(lauf - 1)
                pos = tI(lauf - 1)
            ElseIf Sprung And lauf <> 0 Then
                zEnd = tI(lauf) - 1
                t0 = tI(lauf - 1)
                pos = tI(lauf - 1)
            Else
                pos = tI(lauf)
                t0 = tI(lauf)
                zEnd = TimeX.Count - 1
            End If

            For i = pos To zEnd
                ' Calculate the step range
                Anz = Math.Round(StepValues(i) / tstep, 0)

                ' Set the boundries
                PktB = i - Math.Round(Anz / 2 - 0.5, 0)
                PktE = i + Math.Round(Anz / 2 - 0.5, 0)
                If PktB < t0 Then PktB = t0
                If PktE > zEnd Then PktE = zEnd

                ' Build the sum
                For j = PktB To PktE
                    Summe += ValuesX(j)
                    anzPkt += 1
                Next j

                NewValues(i) = Summe / anzPkt
                Summe = 0
                anzPkt = 0
            Next i
        Next lauf

        Return True
    End Function

    ' Convert the data to 1Hz (not used at the moment)
    Public Function ConvTo1HzArray(ByRef TimeX() As Double, ByRef ValuesX() As Double) As Boolean
        ' Declaration
        Dim tMin As Double
        Dim tMax As Double
        Dim tMid As Integer
        Dim Anz As Integer
        Dim z, t0, zEnd, pos As Integer
        Dim Time As Double
        Dim tEnd As Double
        Dim Finish, firstIn As Boolean
        Dim NewTime(0) As Double
        Dim NewValues(0) As Double
        Dim Summe As Double

        'Check whether Time is not reversed
        For z = 1 To UBound(TimeX)
            If TimeX(z) < TimeX(z - 1) Then
                logme(9, False, "Time step invalid! t(" & z - 1 & ") = " & TimeX(z - 1) & "[s], t(" & z & ") = " & TimeX(z) & "[s]")
                Return False
            End If
        Next z

        'Define Time-range
        t0 = CInt(Math.Round(TimeX(0), 0, MidpointRounding.AwayFromZero))
        tEnd = TimeX(UBound(TimeX))

        'Start-values
        tMin = TimeX(0)
        tMid = CInt(tMin)
        tMax = tMid + 0.5

        If TimeX(0) >= tMax Then
            tMid = tMid + 1
            tMin = tMid - 0.5
            tMax = tMid + 0.5
            t0 = tMid
        End If

        ' Initialise
        Anz = 0
        zEnd = UBound(TimeX)
        pos = 0
        Finish = False
        firstIn = True

        For z = pos To zEnd
            'Next Time-step
            Time = TimeX(z)

            'If Time-step > tMax:
            Do While (Time >= tMax Or z = zEnd)
                'Conclude Second
                If Not firstIn Then
                    ReDim Preserve NewTime(UBound(NewTime) + 1)
                    ReDim Preserve NewValues(UBound(NewValues) + 1)
                Else
                    firstIn = False
                End If
                NewTime(UBound(NewTime)) = tMid

                'If no values ​​in Sum: Interpolate
                If Anz = 0 Then
                    NewValues(UBound(NewValues)) = ((tMid - TimeX(z - 1)) * (ValuesX(z) - ValuesX(z - 1)) / (TimeX(z) - TimeX(z - 1)) + ValuesX(z - 1))
                Else
                    If Time = tMax Then
                        NewValues(UBound(NewValues)) = ((Summe + ValuesX(z)) / (Anz + 1))
                    Else
                        'If only one Value: Inter- /Extrapolate
                        If Anz = 1 Then
                            If z < 2 OrElse TimeX(z - 1) < tMid Then
                                NewValues(UBound(NewValues)) = ((tMid - TimeX(z - 1)) * (ValuesX(z) - ValuesX(z - 1)) / (TimeX(z) - TimeX(z - 1)) + ValuesX(z - 1))
                            Else
                                NewValues(UBound(NewValues)) = ((tMid - TimeX(z - 2)) * (ValuesX(z - 1) - ValuesX(z - 2)) / (TimeX(z - 1) - TimeX(z - 2)) + ValuesX(z - 2))
                            End If

                        Else
                            NewValues(UBound(NewValues)) = (Summe / Anz)
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
                    Summe = 0
                    Anz = 0
                End If

                ' Exit while after the last calculation
                If Finish And z = zEnd Then
                    Exit Do
                End If
            Loop

            Summe += ValuesX(z)
            Anz = Anz + 1
        Next z

        'Accept New fields
        ValuesX = NewValues

        Return True
    End Function

    ' Detect the length from the right section and direction 
    Public Function fSecLen(ByVal MSCX As cMSC, ByVal Sec As Integer, ByVal Dir As Integer) As Double
        ' Declaration
        Dim i As Integer

        ' Search after the rigt section and direction
        For i = 1 To MSCX.meID.Count - 1
            If MSCX.meID(i) = Sec And MSCX.dID(i) = Dir Then
                Return MSCX.len(i)
            End If
        Next i

        Return 0
    End Function

    ' Detect the position from the right section and direction 
    Public Function fSecPos(ByVal Altdata As List(Of cAlt), ByVal Sec As Integer, ByVal Dir As Integer) As Double
        ' Declaration
        Dim i As Integer

        ' Search after the rigt section and direction
        For i = 1 To Altdata.Count - 1
            If Altdata(i).meID = Sec And Altdata(i).dID = Dir Then
                Return i
            End If
        Next i

        Return 0
    End Function

    ' Calculate the altitude
    Public Function fAltInterp(ByVal Altdata As cAlt, ByVal Lati_root As Double, ByVal Longi_root As Double) As Double
        ' Declaration
        Dim i As Integer
        Dim dist, altitude As Double

        ' Calculate the distance
        dist = Math.Sqrt(Math.Pow(Altdata.UTM(0).Easting - Longi_root, 2) + Math.Pow(Altdata.UTM(0).Northing - Lati_root, 2))

        ' Error if distance is longer then altitude file (Should never happen)
        If dist > Altdata.dist(Altdata.dist.Count - 1) Then
            Throw New Exception(format("The actual distance ({0}) is higher than the range of the given altitude profile ({1}). Please check your altitude profile file!", dist, Altdata.dist(Altdata.dist.Count - 1)))
        End If

        ' Find the right points for interpolation
        For i = 1 To Altdata.dist.Count - 1
            If dist < Altdata.dist(i) And dist >= Altdata.dist(i - 1) Then
                altitude = InterpLinear(Altdata.dist(i - 1), Altdata.dist(i), Altdata.Altitude(i - 1), Altdata.Altitude(i), dist)
                Exit For
            End If
        Next i

        Return altitude
    End Function

    ' Function to find the position
    Public Function fFindPos(ByVal FValue As Double, ByVal ArrayV As List(Of Double)) As Integer
        ' Declaration
        Dim pos As Integer

        ' Find the value
        For pos = 1 To ArrayV.Count - 1
            If FValue <= ArrayV(pos) And FValue > ArrayV(pos - 1) Then
                Return pos
            End If
        Next

        ' Give false back if no value was found
        Return -1
    End Function

    ' Calculate the UTM coordinates
    Function UTM(ByVal Lat As Double, ByVal Lon As Double) As cUTMCoord
        ' Declaration
        Dim LatRad, LonRad As Double
        Dim Sin1Lat, Sin2Lat, Sin4Lat, Sin6Lat As Double
        Dim DistOverMeridian As Double
        Dim Rho, Nu, Psi, Psi2, Psi3, Psi4 As Double
        Dim CosLat, CosLat2, CosLat3, CosLat4, CosLat5, CosLat6, CosLat7 As Double
        Dim TanLat, TanLat2, TanLat4, TanLat6 As Double
        Dim DifLon, DifLon2, DifLon3, DifLon4, DifLon5, DifLon6, DifLon7, DifLon8 As Double
        Dim Zone As Integer
        Dim CentralMeridian As Integer
        Dim East1, East2, East3, East4 As Double
        Dim North1, North2, North3, North4 As Double
        Dim X, Y As Double

        Dim retVal As cUTMCoord
        retVal = New cUTMCoord

        ' Some constants. Uses NAD83/WGS84 for UTM datums
        Const SemiMajorAxis As Double = 6378137
        Const InverseFlattening As Double = 298.257222101
        Const Flattening As Double = 1 / InverseFlattening
        Const Eccent2 As Double = 2 * Flattening - (Flattening * Flattening)
        Const Eccent4 As Double = Eccent2 * Eccent2
        Const Eccent6 As Double = Eccent2 * Eccent4
        Const A0 As Double = 1 - (Eccent2 / 4) - ((3 * Eccent4) / 64) - ((5 * Eccent6) / 256)
        Const A2 As Double = (3 / 8) * (Eccent2 + (Eccent4 / 4) + ((15 * Eccent6) / 128))
        Const A4 As Double = (15 / 256) * (Eccent4 + ((3 * Eccent6) / 4))
        Const A6 As Double = (35 * Eccent6) / 3072
        Const FalseEasting = 500000                                                         ' Adjusts to keep UTM coords from going negative
        Const FalseNorthing = 0                                                             ' False Northing of 10 000 000 for southern hemisphere only
        Const CentralScaleFactor As Double = 0.9996
        Const ZoneWidth = 6
        Dim Zone0WestMeridian = Zone1CentralMeridian - (1.5 * ZoneWidth)
        Dim Zone0CentralMeridian = Zone0WestMeridian + ZoneWidth / 2

        ' Parameters to radians
        LatRad = Lat / 180 * Math.PI
        LonRad = Lon / 180 * Math.PI

        'Zone
        Zone = Int((Lon - Zone0WestMeridian) / ZoneWidth)

        'Sin of latitude and its multiples
        Sin1Lat = Math.Sin(LatRad)
        Sin2Lat = Math.Sin(2 * LatRad)
        Sin4Lat = Math.Sin(4 * LatRad)
        Sin6Lat = Math.Sin(6 * LatRad)

        'Meridian Distance
        DistOverMeridian = SemiMajorAxis * (A0 * LatRad - A2 * Sin2Lat + A4 * Sin4Lat - A6 * Sin6Lat)

        'Radii of Curvature
        Rho = SemiMajorAxis * (1 - Eccent2) / (1 - (Eccent2 * Sin1Lat * Sin1Lat)) ^ 1.5
        Nu = SemiMajorAxis / (1 - (Eccent2 * Sin1Lat * Sin1Lat)) ^ 0.5
        Psi = Nu / Rho
        Psi2 = Psi * Psi
        Psi3 = Psi * Psi2
        Psi4 = Psi * Psi3

        'Powers of cos latitude
        CosLat = Math.Cos(LatRad)
        CosLat2 = CosLat * CosLat
        CosLat3 = CosLat * CosLat2
        CosLat4 = CosLat * CosLat3
        CosLat5 = CosLat * CosLat4
        CosLat6 = CosLat * CosLat5
        CosLat7 = CosLat * CosLat6

        'Powers of tan latitude
        TanLat = Math.Tan(LatRad)
        TanLat2 = TanLat * TanLat
        TanLat4 = TanLat2 * TanLat2
        TanLat6 = TanLat2 * TanLat4

        'Differences in longitude and its powers
        CentralMeridian = (Zone * ZoneWidth) + Zone0CentralMeridian
        DifLon = (Lon - CentralMeridian) / 180 * Math.PI
        DifLon2 = DifLon * DifLon
        DifLon3 = DifLon * DifLon2
        DifLon4 = DifLon * DifLon3
        DifLon5 = DifLon * DifLon4
        DifLon6 = DifLon * DifLon5
        DifLon7 = DifLon * DifLon6
        DifLon8 = DifLon * DifLon7

        'X (Easting)
        East1 = DifLon * CosLat
        East2 = DifLon3 * CosLat3 * (Psi - TanLat2) / 6
        East3 = DifLon5 * CosLat5 * (4 * Psi3 * (1 - 6 * TanLat2) + Psi2 * (1 + 8 * TanLat2) - Psi * (2 * TanLat2) + TanLat4) / 120
        East4 = DifLon7 * CosLat7 * (61 - 479 * TanLat2 + 179 * TanLat4 - TanLat6) / 5040
        X = CentralScaleFactor * Nu * (East1 + East2 + East3 + East4) + FalseEasting

        'Y (Northing)
        North1 = Sin1Lat * DifLon2 * CosLat / 2
        North2 = Sin1Lat * DifLon4 * CosLat3 * (4 * Psi2 + Psi - TanLat2) / 24
        North3 = Sin1Lat * DifLon6 * CosLat5 * (8 * Psi4 * (11 - 24 * TanLat2) - 28 * Psi3 * (1 - 6 * TanLat2) + Psi2 * (1 - 32 * TanLat2) - Psi * (2 * TanLat2) + TanLat4) / 720
        North4 = Sin1Lat * DifLon8 * CosLat7 * (1385 - 3111 * TanLat2 + 543 * TanLat4 - TanLat6) / 40320
        Y = CentralScaleFactor * (DistOverMeridian + Nu * (North1 + North2 + North3 + North4)) + FalseNorthing

        ' UTM return value
        retVal.Zone = Zone
        retVal.Easting = X
        retVal.Northing = Y
        Return retVal
    End Function

End Module
