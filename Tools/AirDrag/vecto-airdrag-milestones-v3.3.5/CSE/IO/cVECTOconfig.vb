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

Option Strict Off

Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json.Schema

Public Class cVECTOconfig
    Inherits cJsonFile

    Protected Overrides Function HeaderOverlay() As JObject
        Return JObject.Parse(<json>{
                "Title": "VECTO-Air Drag VECTOconfig",
                "FileVersion":  "1.0.0",
           }</json>.Value)
    End Function

    ' Defaults specified here.
    Protected Shared Function BuildBody() As JObject
        '' Return empty body since all proprs are optional.
        '' They will become concrete on the 1st store.
        Return New JObject()
        'Return JObject.Parse(<json>{
        '        "Manufacturer":   "",
        '        "Model":          "",
        '        "CertNum":        "",
        '        "AppVers":        "",
        '        "CdxA":            0,
        '        "TransfR":         0,
        '        "WorstCase":       0,
        '        "CdxACFD":         null,
        '        "LicenseNumberMethodCFD":       "",
        '    }</json>.Value)
    End Function

    ''' <param name="isStrictBody">when false, more strict validation</param>
    Public Shared Function JSchemaStr(Optional ByVal isStrictBody As Boolean = False) As String
        Dim allowAdditionalProps_str As String = (Not isStrictBody).ToString.ToLower
        Return <json>{
            "title": "Schema for VECTO-Air Drag VECTOconfig",
            "type": "object", "additionalProperties": <%= allowAdditionalProps_str %>, 
            "required": true,
            "properties": {
                "Manufacturer": {
                    "title": "Manufacturer",
                    "type": ["string", "null"], 
                    "default": null,
                    "description": "The manufacturer of the calculated truck.",
                }, 
                "Model": {
                    "title": "Model",
                    "type": ["string", "null"],
                    "default": null,
                    "description": "The model of the truck.",
                }, 
                "CertNum": {
                    "title": "Certification number",
                    "type": ["string", "null"],
                    "default": null,
                    "description": "The certification number of the truck.",
                }, 
                "AppVers": {
                    "title": "VECTO AirDrag version",
                    "type": ["string", "null"],
                    "default": null,
                    "description": "The used VECTO AirDrag version for the calculation.",
                }, 
                "CdxA": {
                    "title": "CdxA value",
                    "type": "number",
                    "default": 0,
                    "description": "The CdxA value",
                }, 
                "TransfR": {
                    "title": "Delta CdxA - transfer rules",
                    "type": "number",
                    "default": 0,
                    "description": "The Delta CdxA - transfer rules.",
                }, 
                "WorstCase": {
                    "title": "Delta CdxA - worst case parent",
                    "type": "number",
                    "minimum": 0,
                    "maximum": 0.2,
                    "default": 0,
                    "description": "The Delta CdxA - worst case parent",
                },  
                "LicenseNumberMethodCFD": {
                    "title": "License Number Method CFD",
                    "type": ["string", "null"],
                    "default": null,
                    "description": "The License Number Method CFD.",
                },
                "CdxACFD": {
                    "title": "Delta CdxA - CFD",
                    "type": "number",
                    "default": null,
                    "minimum": 0,
                    "description": "The Delta CdxA - CFD",
                },
            }
        }</json>.Value
    End Function

    ''' <summary>creates defaults</summary>
    ''' <remarks>See cJsonFile() constructor</remarks>
    Sub New(Optional ByVal skipValidation As Boolean = False)
        MyBase.New(BuildBody, skipValidation)
    End Sub
    ''' <summary>Reads from file or creates defaults</summary>
    ''' <param name="inputFilePath">the fpath of the file to read data from</param>
    Sub New(ByVal inputFilePath As String, Optional ByVal skipValidation As Boolean = False)
        MyBase.New(inputFilePath, skipValidation)
    End Sub


    Protected Overrides Function BodySchemaStr() As String
        Return JSchemaStr()
    End Function

    ''' <exception cref="SystemException">includes all validation errors</exception>
    ''' <param name="isStrictBody">when True, no additional json-properties allowed in the data, when nothing, use value from Header</param>
    Protected Overrides Sub ValidateBody(ByVal isStrictBody As Boolean, ByVal validateMsgs As IList(Of String))
        '' Check version
        ''
        Dim fromVersion = "1.0.0--"
        Dim toVersion = "2.0.0--" ' The earliest pre-release.
        If Not IsSemanticVersionsSupported(Me.FileVersion, fromVersion, toVersion) Then
            validateMsgs.Add(format("Unsupported FileVersion({0}, was not in between [{1}, {2})", FileVersion, fromVersion, toVersion))
            Return
        End If

        '' Check schema
        ''
        Dim schema = JSchema.Parse(JSchemaStr(isStrictBody))
        ValidateJson(Me.Body, schema, validateMsgs)
    End Sub

#Region "json props"
    Public Property Manufacturer As String
        Get
            Return PropOrDefault(".Manufacturer")
        End Get
        Set(ByVal value As String)
            Me.Body("Manufacturer") = value
        End Set
    End Property

    Public Property Model As String
        Get
            Return PropOrDefault(".Model")
        End Get
        Set(ByVal value As String)
            Me.Body("Model") = value
        End Set
    End Property

    Public Property CertNum As String
        Get
            Return PropOrDefault(".CertNum")
        End Get
        Set(ByVal value As String)
            Me.Body("CertNum") = value
        End Set
    End Property

    Public Property AppVers As String
        Get
            Return PropOrDefault(".AppVers")
        End Get
        Set(ByVal value As String)
            Me.Body("AppVers") = value
        End Set
    End Property

    Public Property CdxA As Double
        Get
            Return PropOrDefault(".CdxA")
        End Get
        Set(ByVal value As Double)
            Me.Body("CdxA") = value
        End Set
    End Property

    Public Property TransfR As Double
        Get
            Return PropOrDefault(".TransfR")
        End Get
        Set(ByVal value As Double)
            Me.Body("TransfR") = value
        End Set
    End Property

    Public Property WorstCase As Double
        Get
            Return PropOrDefault(".WorstCase")
        End Get
        Set(ByVal value As Double)
            Me.Body("WorstCase") = value
        End Set
    End Property

    Public Property LicenseNumberMethodCFD As String
        Get
            Return PropOrDefault(".LicenseNumberMethodCFD")
        End Get
        Set(ByVal value As String)
            Me.Body("LicenseNumberMethodCFD") = value
        End Set
    End Property

    Public Property CdxACFD As Double?
        Get
            Return PropOrDefault(".CdxACFD")
        End Get
        Set(ByVal value As Double?)
            Me.Body("CdxACFD") = value
        End Set
    End Property

#End Region ' "json props"
End Class
