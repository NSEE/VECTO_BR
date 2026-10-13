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

Public Class cVirtMSC
    Public meID As List(Of Integer)                      ' Measurement ID
    Public dID As List(Of Integer)                       ' Direction ID
    Public KoordA As List(Of Array)                      ' Latitude (start) [mm.mm]
    Public KoordE As List(Of Array)                      ' Latitude (end) [mm.mm]
    Public NewSec As List(Of Boolean)                    ' Variable to detect if a new section is started
    Public tUse As Boolean                               ' Variable to detect if a trigger is used
    Public Head As List(Of Double)                       ' Heading

    ' Initialise the parameter
    Public Sub New()
        meID = New List(Of Integer)
        dID = New List(Of Integer)
        KoordA = New List(Of Array)
        KoordE = New List(Of Array)
        NewSec = New List(Of Boolean)
        Head = New List(Of Double)

        meID.Add(0)
        dID.Add(0)
        KoordA.Add(New Integer() {0})
        KoordE.Add(New Integer() {0})
        NewSec.Add(0)
        Head.Add(0)
    End Sub
End Class
