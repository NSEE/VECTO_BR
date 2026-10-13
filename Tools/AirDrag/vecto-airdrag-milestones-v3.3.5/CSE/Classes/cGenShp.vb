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

Public Class cGenShp

    Public fa_pe As Double                                  ' Fa_pe
    Public h_ref As Double                                  ' Reference vehicle high
    Public h_min As Double                                  ' Minimum vehicle high
    Public h_max As Double                                  ' Maximum vehicle high
    Public x_val() As Double                                ' X-Coordinates
    Public y_val() As Double                                ' Y-Coordinates
    Public valid As Boolean                                 ' Genshape valid

    ' Get the AirDrag curve parameters
    Private Sub GetAirDragCurvePara()
        Select Case (AirDragCurv_val)
            Case -1
                valid_val = False
            Case 0 ' rigid (solo)
                a1_val = 0.013526
                a2_val = 0.017746
                a3_val = -0.000666
            Case 1 ' rigid & trailer
                a1_val = 0.017125
                a2_val = 0.072275
                a3_val = -0.004148
            Case 2 ' tractor & semitrailer
                a1_val = 0.030042
                a2_val = 0.040817
                a3_val = -0.00213
            Case 3 ' coach / bus (HB)
                a1_val = -0.000794
                a2_val = 0.02109
                a3_val = -0.00109
            Case 4 ' MLr (53)
                a1_val = -0.0015
                a2_val = 0.0086
                a3_val = -0.00029
            Case 5 ' MLvan (54)
                a1_val = 0.0032
                a2_val = 0.00532
                a3_val = -0.00028
        End Select
    End Sub

    ' Get the curve values
    Private Sub GetCurveVal()
        ' Declaration
        Dim i As Integer
        ReDim x_val(10)
        ReDim y_val(10)

        ' Calculate the parameter
        For i = 0 To 10
            x_val(i) = i
            y_val(i) = Math.Round(a1_val * i + a2_val * Math.Pow(i, 2) + a3_val * Math.Pow(i, 3), 6)
        Next i
    End Sub

    ' Get the vehicle class data
    Private Sub GetVehClassData(ByVal VehClass As tVehClass, ByVal VehConf As Integer, ByVal TPMLM As Double, ByVal VehHeight As Double)
        Select Case (VehClass)
            Case tVehClass.Class1 ' rigid or tractor 4x2
                hmin = 3.4
                hmax = 3.6
                href = 3.6
                If VehConf = 0 Then
                    AirDragCurv_val = 0
                Else
                    AirDragCurv_val = -1
                End If
            Case tVehClass.Class2 ' rigid or tractor 4x2
                hmin = 3.5
                hmax = 3.75
                href = 3.75
                If VehConf = 0 Then
                    AirDragCurv_val = 0
                Else
                    AirDragCurv_val = -1
                End If
            Case tVehClass.Class3 ' rigid or tractor 4x2
                hmin = 3.7
                hmax = 3.9
                href = 3.9
                If VehConf = 0 Then
                    AirDragCurv_val = 0
                Else
                    AirDragCurv_val = -1
                End If
            Case tVehClass.Class4 ' rigid 4x2
                hmin = 3.85
                hmax = 4
                href = 4
                If VehConf = 0 Then
                    AirDragCurv_val = 0
                Else
                    If Job.mode = 1 Then
                        AirDragCurv_val = -1
                    Else
                        AirDragCurv_val = 1
                    End If
                End If
            Case tVehClass.Class5 ' tractor 4x2
                hmin = 3.9
                hmax = 4
                href = 4
                If VehConf = 0 Then
                    AirDragCurv_val = -1
                Else
                    AirDragCurv_val = 2
                End If
            Case tVehClass.Class9 ' rigid 6x2/2-4
                If (TPMLM <= 10000) Then
                    ' vehicle class 1
                    hmin = 3.5
                    hmax = 3.75
                    href = 3.75
                ElseIf (TPMLM > 10000 And TPMLM <= 12000) Then
                    ' vehicle class 2
                    hmin = 3.4
                    hmax = 3.6
                    href = 3.6
                ElseIf (TPMLM > 12000 And TPMLM <= 16000) Then
                    ' vehicle class 3
                    hmin = 3.7
                    hmax = 3.9
                    href = 3.9
                Else
                    ' vehicle class 4
                    hmin = 3.85
                    hmax = 4
                    href = 4
                End If
                If VehConf = 0 Then
                    AirDragCurv_val = 0
                Else
                    If Job.mode = 1 Then
                        AirDragCurv_val = -1
                    Else
                        AirDragCurv_val = 1
                    End If
                End If
            Case tVehClass.Class10 ' tractor 6x2/2-4
                hmin = 3.9
                hmax = 4
                href = 4
                If VehConf = 0 Then
                    AirDragCurv_val = -1
                Else
                    AirDragCurv_val = 2
                End If
            Case tVehClass.Class53 ' MLr
                hmin = 2
                hmax = 5
                href = 3.5
                If VehConf = 0 Then
                    AirDragCurv_val = 4
                Else
                    AirDragCurv_val = -1
                End If
            Case tVehClass.Class54 ' MLvan
                hmin = 2
                hmax = 5
                href = VehHeight
                If VehConf = 0 Then
                    AirDragCurv_val = 5
                Else
                    AirDragCurv_val = -1
                End If
            Case tVehClass.Class31b2, ' HB
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
                hmin = 2
                hmax = 5
                href = VehHeight
                If VehConf = 0 Then
                    AirDragCurv_val = 3
                Else
                    AirDragCurv_val = -1
                End If
        End Select
    End Sub

    ' Get the AirDrag curve parameters
    Public Function GetAirDragPara(ByVal VehClass As tVehClass, ByVal VehConf As Integer, ByVal TPMLM As Double, ByVal VehHeight As Double) As Boolean
        ' Set flag
        valid_val = True

        ' Set fa_pe
        fa_pe = 1

        ' Get Vehicle class data
        GetVehClassData(VehClass, VehConf, TPMLM, VehHeight)

        ' Get the curve parameters
        GetAirDragCurvePara()

        ' Calculate the curve
        If valid_val Then GetCurveVal()

        Return True
    End Function

#Region "Properties"
    Private Property hmin As Double
        Get
            Return h_min
        End Get
        Set(ByVal value As Double)
            h_min = value
        End Set
    End Property
    Private Property hmax As Double
        Get
            Return h_max
        End Get
        Set(ByVal value As Double)
            h_max = value
        End Set
    End Property
    Private Property href As Double
        Get
            Return h_ref
        End Get
        Set(ByVal value As Double)
            h_ref = value
        End Set
    End Property
    Private Property valid_val As Boolean
        Get
            Return valid
        End Get
        Set(ByVal value As Boolean)
            valid = value
        End Set
    End Property
    Private a1 As Double
    Private Property a1_val As Double
        Get
            Return a1
        End Get
        Set(ByVal value As Double)
            a1 = value
        End Set
    End Property
    Private a2 As Double
    Private Property a2_val As Double
        Get
            Return a2
        End Get
        Set(ByVal value As Double)
            a2 = value
        End Set
    End Property
    Private a3 As Double
    Private Property a3_val As Double
        Get
            Return a3
        End Get
        Set(ByVal value As Double)
            a3 = value
        End Set
    End Property
    Private AirDragCurv As Double
    Private Property AirDragCurv_val As Double
        Get
            Return AirDragCurv
        End Get
        Set(ByVal value As Double)
            AirDragCurv = value
        End Set
    End Property
#End Region
End Class
