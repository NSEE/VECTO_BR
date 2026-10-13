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

Public Class cMSC
    Public meID As List(Of Integer)                      ' Measurement ID
    Public dID As List(Of Integer)                       ' Direction ID
    Public len As List(Of Double)                        ' Lenght [m]
    Public head As List(Of Double)                       ' Heading [°]
    Public headID As List(Of Double)                     ' Heading ID
    Public tUse As Boolean                               ' Trigger used (1=Yes, 0=No)
    Public latS As List(Of Double)                       ' Latitude (start) [mm.mm]
    Public longS As List(Of Double)                      ' Longitude (start) [mm.mm]
    Public latE As List(Of Double)                       ' latetude (end) [mm.mm]
    Public longE As List(Of Double)                      ' Longitude (end) [mm.mm]
    Public AltPath As List(Of String)                    ' Path to the altitude file [-]

    ' Initialise the parameter
    Public Sub New()
        meID = New List(Of Integer)
        dID = New List(Of Integer)
        len = New List(Of Double)
        head = New List(Of Double)
        headID = New List(Of Double)
        latS = New List(Of Double)
        longS = New List(Of Double)
        latE = New List(Of Double)
        longE = New List(Of Double)
        AltPath = New List(Of String)

        meID.Add(0)
        dID.Add(0)
        len.Add(0)
        head.Add(0)
        headID.Add(0)
        latS.Add(0)
        longS.Add(0)
        latE.Add(0)
        longE.Add(0)
        AltPath.Add(0)
    End Sub
End Class
