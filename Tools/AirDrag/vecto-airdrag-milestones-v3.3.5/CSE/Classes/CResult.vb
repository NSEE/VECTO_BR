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

Public Class CResult
    ' Declarations
    Public Head As String
    Public Unit As String
    Public Dic As String

    ' Bild new
    Public Sub New(ByVal HeadStr As String, ByVal UnitStr As String, ByVal DicStr As String)
        Head = HeadStr
        Unit = UnitStr
        Dic = DicStr
    End Sub
End Class
