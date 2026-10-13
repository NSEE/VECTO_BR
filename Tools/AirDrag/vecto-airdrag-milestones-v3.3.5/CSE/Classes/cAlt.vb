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

Public Class cAlt
    Public meID As Integer                               ' Measurement ID
    Public dID As Integer                                ' Direction ID
    Public KoordLat As List(Of Double)                   ' Latetude [mm.mm]
    Public KoordLong As List(Of Double)                  ' Longitude [mm.mm]
    Public Altitude As List(Of Double)                   ' Altitude [m]
    Public UTM As List(Of cUTMCoord)                     ' UTM coords [m]
    Public dist As List(Of Double)                       ' Distance from the begin of the altitude file

    ' Initialise the parameter
    Public Sub New()
        KoordLat = New List(Of Double)
        KoordLong = New List(Of Double)
        Altitude = New List(Of Double)
        UTM = New List(Of cUTMCoord)
        dist = New List(Of Double)
    End Sub
End Class
