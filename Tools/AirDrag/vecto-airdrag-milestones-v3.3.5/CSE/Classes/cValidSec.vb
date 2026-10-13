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

Public Class cValidSec
    Public NameSec As List(Of String)                   ' Name of the section
    Public AnzSec As List(Of Integer)                   ' Number of valid sections
    Public ValidSec As List(Of Boolean)                 ' Verify if enough sections in both directions
    Public vVeh As List(Of Double)                      ' Average vehicle speed

    ' Initialise the parameter
    Public Sub New()
        NameSec = New List(Of String)
        AnzSec = New List(Of Integer)
        ValidSec = New List(Of Boolean)
        vVeh = New List(Of Double)
    End Sub

    ' calculate the average speed values
    Public Sub calcAveSpeed()
        ' Declaration
        Dim i As Integer

        For i = 0 To AnzSec.Count - 1
            If AnzSec(i) > 0 Then vVeh(i) /= AnzSec(i)
        Next
    End Sub
End Class
