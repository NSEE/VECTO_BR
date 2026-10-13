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

Public Module OutputTest
    Function fOuttest(ByVal Datafile As String) As Boolean
        ' Declaration
        Dim i As Integer
        Dim NameOutFile, key As String
        Using FileOut As New cFile_V3
            Dim first As Boolean
            Dim s As New System.Text.StringBuilder

            ' Initialise
            first = True
            ErgEntriesI = New Dictionary(Of tComp, CResult)
            ErgEntryListI = New List(Of tComp)
            ErgEntriesIU = New Dictionary(Of String, CResult)
            ErgEntryListIU = New List(Of String)
            ErgEntriesC = New Dictionary(Of tCompCali, CResult)
            ErgEntryListC = New List(Of tCompCali)
            GenErgOutDataTest()

            ' Exit function if error is detected
            If BWorker.CancellationPending And FileBlock Then
                Return False
            End If

            ' Write on GUI
            logme(5, False, "Write output-file (*.csv)")

            ' Generate the file name
            NameOutFile = OutFolder & fName(Datafile, False) & "_test.csv"

            ' Anlegen der Datei
            FileOut.OpenWrite(NameOutFile, , False)

            ' Filekopf
            FileOut.WriteLine("# Resultfile Programm " & AppName & " " & AppVers & " Comp " & AppDate)
            FileOut.WriteLine("# Datafile: ", Datafile)
            FileOut.WriteLine("#")

            ' Write the head and units
            FileOut.WriteLine(ErgHead("InputData") + "," + ErgHead("CalcData"))

            ' Write the data
            For i = 0 To InputData.Item(tComp.t).Count - 1
                For Each key In ErgEntryListI
                    If Not first Then s.Append(",")
                    s.Append(InputData(key)(i))
                    first = False
                Next
                For Each key In ErgEntryListC
                    If Not first Then s.Append(",")
                    s.Append(CalcData(key)(i))
                    first = False
                Next
                FileOut.WriteLine(s.ToString)
                s.Clear()
                first = True
            Next i

        End Using


        ' Ausgabe bei blockierter Datei
        If BWorker.CancellationPending And FileBlock Then
            logme(9, False, "Can´t write in file " & NameOutFile & ". File is blocked by another process!")
        End If

        Return True
    End Function

    ' Generate the output dictionary (for calculate)
    Private Sub GenErgOutDataTest()
        ' Declaration
        Dim sKV As New KeyValuePair(Of String, List(Of Double))

        ' Input data
        AddToErg(tComp.t, fCompName(tComp.t), fCompUnit(tComp.t), "InputData")
        AddToErg(tComp.lati, fCompName(tComp.lati), fCompUnit(tComp.lati), "InputData")
        AddToErg(tComp.longi, fCompName(tComp.longi), fCompUnit(tComp.longi), "InputData")

        ' Undefined input data
        For Each sKV In InputUndefData
            AddToErg(sKV.Key, sKV.Key, "", "InputUndefData")
        Next

        ' Calculated data
        AddToErg(tCompCali.zone_UTM, fCompName(tCompCali.zone_UTM), fCompUnit(tCompCali.zone_UTM), "CalcData")
        AddToErg(tCompCali.lati_UTM, fCompName(tCompCali.lati_UTM), fCompUnit(tCompCali.lati_UTM), "CalcData")
        AddToErg(tCompCali.longi_UTM, fCompName(tCompCali.longi_UTM), fCompUnit(tCompCali.longi_UTM), "CalcData")
    End Sub
End Module
