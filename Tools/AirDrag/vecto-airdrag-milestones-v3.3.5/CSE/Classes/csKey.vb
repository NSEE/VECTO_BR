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

Public Class csKey
    Public Weather As New csKeyWeather
    Public Meas As New csKeyMeas
End Class

Public Class csKeyWeather
    Public t As String = "<T>"
    Public t_amb_stat As String = "<T_AMB_STAT>"
    Public p_amb_stat As String = "<P_AMB_STAT>"
    Public rh_stat As String = "<RH_STAT>"
End Class

Public Class csKeyMeas
    Public t As String = "<T>"
    Public lati As String = "<LAT>"
    Public longi As String = "<LONG>"
    Public lati_D As String = "<LAT_D>"
    Public longi_D As String = "<LONG_D>"
    Public hdg As String = "<HDG>"
    Public v_veh_GPS As String = "<V_VEH_GPS>"
    Public v_veh_CAN As String = "<V_VEH_CAN>"
    Public v_air As String = "<V_AIR>"
    Public beta As String = "<BETA>"
    Public n_eng As String = "<N_ENG>"
    Public n_card As String = "<N_CARD>"
    Public n_wheel_ave As String = "<N_WHEEL_AVE>"
    Public n_EM As String = "<N_EM>"
    Public tq_l As String = "<TQ_L>"
    Public tq_r As String = "<TQ_R>"
    Public t_ground As String = "<T_GROUND>"
    Public t_amb_veh As String = "<T_AMB_VEH>"
    Public trigger As String = "<TRIGGER>"
    Public valid As String = "<VALID>"
    Public s_brake As String = "<S_BRAKE>"
End Class