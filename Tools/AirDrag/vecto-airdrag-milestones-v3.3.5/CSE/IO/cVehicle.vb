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

Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json.Schema

Public Enum VehicleConfig
    No
    Yes
End Enum

Public Enum gearBoxConfig
    AT
    APT
    MT_AMT
    SMT
    AMT
    DCT
    No
    Yes
End Enum

Public Enum tVehClass
    ' Heavy lorries
    Class1         ' rigid or tractor 4x2
    Class2         ' rigid or tractor 4x2
    Class3         ' rigid or tractor 4x2
    Class4         ' rigid 4x2
    Class5         ' tractor 4x2
    Class9         ' rigid 6x2/2-4
    Class10        ' tractor 6x2/2-4

    ' Medium lorries
    Class53        ' MLr - medium regid lorries
    Class54        ' MLvan - medium van lorries

    ' Primary busses
    Class31b2
    Class32a
    Class32b
    Class32c
    Class32d
    Class32e
    Class32f
    Class33b2
    Class34a
    Class34b
    Class34c
    Class34d
    Class34e
    Class34f
    Class35b2
    Class36a
    Class36b
    Class36c
    Class36d
    Class36e
    Class36f
    Class37b2
    Class38a
    Class38b
    Class38c
    Class38d
    Class38e
    Class38f
    Class39b2
    Class40a
    Class40b
    Class40c
    Class40d
    Class40e
    Class40f
End Enum

Public Class cVehicle
    Inherits cJsonFile
    Private IsOldVeh As Boolean

    Protected Overrides Function HeaderOverlay() As JObject
        Return JObject.Parse(<json>{
                "Title": "VECTO-Air Drag VEHICLE",
                "FileVersion":  "1.0.1",
           }</json>.Value)
    End Function


    ' Defaults specified here.
    Protected Shared Function BuildBody() As JObject
        Return JObject.Parse(<json>{
                "vehClass":         null,
                "configuration":    null,
                "TPMLM":            null,
                "vVehMax":          null,
                "vehHeight":        null,
                "anemometerHeight": null,
                "testMass":         null,
                "gearRatio_low":    null,
                "gearRatio_high":   null,
                "axleRatio":        null,
                "fixedTransmission":     null,
                "tqDriftLeft":      null,
                "tqDriftRight":     null,
                "tDriftZero":       null,
                "tDriftCheck":      null,
            }</json>.Value)
    End Function

    ''' <param name="isStrictBody">when true, more strict validation</param>
    Public Shared Function JSchemaStrV102(Optional ByVal isStrictBody As Boolean = False) As String
        Dim allowAdditionalProps_str As String = (Not isStrictBody).ToString.ToLower
        Return <json>{
            "title": "Schema for VECTO-Air Drag VEHICLE",
            "type": "object", "additionalProperties": <%= allowAdditionalProps_str %>, 
            "required": true,
            "properties": {
                "classCode": {
                    "title": "Class code",
                    "type": "string",
                    "required": true,
                    "description": "The class the vehicle belongs to according to the legislation.
The generic parameters for classes are stored in the GenShape.shp",
                }, 
                "configuration with trailer": {
                    "title": "Vehicle Configuration with trailer", 
                    "enum": ["no", "yes"],
                    "required": true,
                    "title": "Vehicle contains trailer?", 
                }, 
                "TPMLM": {
                    "title": "Technically permissible maximum laden mass [kg]", 
                    "type":"number",                     
                    "required": false,
                },
                "vVehMax": {
                    "title": "Vehicle maximum design speed [km/h]", 
                    "type":"number",                     
                    "required": false,
                },
                "vehHeight": {
                    "title": "Vehicle height [m]", 
                    "type":"number",                     
                    "required": true,
                } ,
                "anemometerHeight": {
                    "title": "Anemomenter height [m]", 
                    "type":"number",                     
                    "required": true,
                }, 
                "testMass": {
                    "title": "Vehicle test mass [kg]", 
                    "type":"number",                   
                    "required": true,
                }, 
                "gearRatio_low": {
                    "title": "Gear ratio low speed", 
                    "type":"string",                     
                    "required": true,
                }, 
                "gearRatio_high": {
                    "title": "Gear ratio high speed", 
                    "type":"string",                    
                    "required": true,
                }, 
                "axleRatio": {
                    "title": "Axle ratio", 
                    "type":"string",                    
                    "required": true,
                }, 
                "fixedTransmission": {
                    "title": "Fixed transmission ratio in low speed test", 
                    "enum": ["no", "yes"],
                    "required": true,
                    "title": "Fixed transmission ratio in low speed test?",
                },
                "tqDriftLeft": {
                    "title": "tq Drift Left", 
                    "type":"string",                    
                    "required": true,
                }, 
                "tqDriftRight": {
                    "title": "tq Drift Right", 
                    "type":"string",                    
                    "required": true,
                }, 
                "tDriftZero": {
                    "title": "t Drift Zero", 
                    "type":"string",                    
                    "required": true,
                }, 
                "tDriftCheck": {
                    "title": "t Drift Check", 
                    "type":"string",                    
                    "required": true,
                }, 
            }
        }</json>.Value
    End Function
    Public Shared Function JSchemaStrV101(Optional ByVal isStrictBody As Boolean = False) As String
        Dim allowAdditionalProps_str As String = (Not isStrictBody).ToString.ToLower
        Return <json>{
            "title": "Schema for VECTO-Air Drag VEHICLE",
            "type": "object", "additionalProperties": <%= allowAdditionalProps_str %>, 
            "required": true,
            "properties": {
                "classCode": {
                    "title": "Class code",
                    "type": "number",
                    "required": true,
                    "description": "The class the vehicle belongs to according to the legislation.
The generic parameters for classes are stored in the GenShape.shp",
                }, 
                "configuration with trailer": {
                    "title": "Vehicle Configuration with trailer", 
                    "enum": ["no", "yes"],
                    "required": true,
                    "title": "Vehicle contains trailer?", 
                }, 
                "GVMMax": {
                    "title": "Maximum gross vehicle mass [kg]", 
                    "type":"number",                     
                    "required": true,
                },
                "vVehMax": {
                    "title": "Vehicle maximum design speed [km/h]", 
                    "type":"number",                     
                    "required": false,
                },
                "vehHeight": {
                    "title": "Vehicle height [m]", 
                    "type":"number",                     
                    "required": true,
                } ,
                "anemometerHeight": {
                    "title": "Anemomenter height [m]", 
                    "type":"number",                     
                    "required": true,
                }, 
                "testMass": {
                    "title": "Vehicle test mass [kg]", 
                    "type":"number",                   
                    "required": true,
                }, 
                "gearRatio_low": {
                    "title": "Gear ratio low speed", 
                    "type":"string",                     
                    "required": true,
                }, 
                "gearRatio_high": {
                    "title": "Gear ratio high speed", 
                    "type":"string",                    
                    "required": true,
                }, 
                "axleRatio": {
                    "title": "Axle ratio", 
                    "type":"string",                    
                    "required": true,
                }, 
                "fixedTransmission": {
                    "title": "gearBox type", 
                    "enum": ["SMT", "AMT", "DCT", "APT"],
                    "required": true,
                    "title": "Gear box type is SMT, AMT, DCT or APT?",
                },
                "tqDriftLeft": {
                    "title": "tq Drift Left", 
                    "type":"string",                    
                    "required": true,
                }, 
                "tqDriftRight": {
                    "title": "tq Drift Right", 
                    "type":"string",                    
                    "required": true,
                }, 
                "tDriftZero": {
                    "title": "t Drift Zero", 
                    "type":"string",                    
                    "required": true,
                }, 
                "tDriftCheck": {
                    "title": "t Drift Check", 
                    "type":"string",                    
                    "required": true,
                }, 
            }
        }</json>.Value
    End Function
    Public Shared Function JSchemaStrV100(Optional ByVal isStrictBody As Boolean = False) As String
        Dim allowAdditionalProps_str As String = (Not isStrictBody).ToString.ToLower
        Return <json>{
            "title": "Schema for VECTO-Air Drag VEHICLE",
            "type": "object", "additionalProperties": <%= allowAdditionalProps_str %>, 
            "required": true,
            "properties": {
                "classCode": {
                    "title": "Class code [1-16]",
                    "type": "integer", 
                    "required": true,
                    "description": "The class the vehicle belongs to according to the legislation.
The generic parameters for classes are stored in the GenShape.shp",
                }, 
                "configuration with trailer": {
                    "title": "Vehicle Configuration with trailer", 
                    "enum": ["no", "yes"],
                    "required": true,
                    "title": "Vehicle contains trailer?", 
                }, 
                "GVMMax": {
                    "title": "Maximum gross vehicle mass [kg]", 
                    "type":"number",                     
                    "required": true,
                },
                "vVehMax": {
                    "title": "Vehicle maximum design speed [km/h]", 
                    "type":"number",                     
                    "required": false,
                },
                "vehHeight": {
                    "title": "Vehicle height [m]", 
                    "type":"number",                     
                    "required": true,
                } ,
                "anemometerHeight": {
                    "title": "Anemomenter height [m]", 
                    "type":"number",                     
                    "required": true,
                }, 
                "testMass": {
                    "title": "Vehicle test mass [kg]", 
                    "type":"number",                   
                    "required": true,
                }, 
                "gearRatio_low": {
                    "title": "Gear ratio low speed", 
                    "type":"string",                     
                    "required": true,
                }, 
                "gearRatio_high": {
                    "title": "Gear ratio high speed", 
                    "type":"string",                    
                    "required": true,
                }, 
                "axleRatio": {
                    "title": "Axle ratio", 
                    "type":"string",                    
                    "required": true,
                }, 
                "fixedTransmission": {
                    "title": "gearBox type", 
                    "enum": ["MT_AMT", "AT"],
                    "required": true,
                    "title": "Gear box type is MT_AMT or AT?",
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
        Return JSchemaStrV102()
    End Function

    ''' <exception cref="SystemException">includes all validation errors</exception>
    ''' <param name="isStrictBody">when true, no additional json-properties allowed in the data, when nothing, use value from Header</param>
    Protected Overrides Sub ValidateBody(ByVal isStrictBody As Boolean, ByVal validateMsgs As IList(Of String))
        '' Check version
        ''
        Dim fromVersion = "1.0.0--"
        Dim toVersion = "2.0.0--" ' The earliest pre-release.
        If Not IsSemanticVersionsSupported(Me.FileVersion, fromVersion, toVersion) Then
            validateMsgs.Add(format("Unsupported FileVersion({0}, was not in between [{1}, {2})", Me.FileVersion, fromVersion, toVersion))
            Return
        End If

        ' Check schema
        If (Me.FileVersion = "1.0.0") Then
            Dim schema = JSchema.Parse(JSchemaStrV100(isStrictBody))
            ValidateJson(Body, schema, validateMsgs)
            IsOldVeh = True
        ElseIf (Me.FileVersion = "1.0.1") Then
            Dim schema = JSchema.Parse(JSchemaStrV101(isStrictBody))
            ValidateJson(Body, schema, validateMsgs)
            IsOldVeh = False
        Else
            Dim schema = JSchema.Parse(JSchemaStrV102(isStrictBody))
            ValidateJson(Body, schema, validateMsgs)
            IsOldVeh = False
        End If

        If validateMsgs.Any() Then Return

        ' Set transmission
        If IsAPT Then
            APT = True
            SMT_AMT_DCT = False
        ElseIf IsSMT_AMT_DCT Then
            APT = False
            SMT_AMT_DCT = True
        End If

        ' Set Vehicle maximum speed
        If IsNothing(Me.Body("vVehMax")) Then
            Select Case Me.classCode
                Case tVehClass.Class1, tVehClass.Class2, tVehClass.Class3, tVehClass.Class4, tVehClass.Class5, tVehClass.Class9, tVehClass.Class10, tVehClass.Class53, tVehClass.Class54
                    Me.Body("vVehMax") = If(TransitionalService.Use2ndAmendment, 95, 92)
                Case Else
                    Me.Body("vVehMax") = If(TransitionalService.Use2ndAmendment, 103, 102)
            End Select
        End If

        ' TPMLM check
        If IsNothing(Me.Body("TPMLM")) Then
            If Me.classCode = tVehClass.Class9 Then
                validateMsgs.Add(format("TPMLM not given but needed vor class9!"))
                Return
            Else
                Me.Body("TPMLM") = 0
            End If
        End If

        ' Check others
        ' Check if vehicle class with the given configuration class is available
        Call GenShape.GetAirDragPara(Me.classCode, Me.configuration, Me.TPMLM, Me.vehHeight)
        If GenShape.valid Then
            Job.fa_pe = GenShape.fa_pe
        End If
    End Sub

#Region "json props"
    Public Property classCode As tVehClass
        Get
            Const Prefix = "Class"
            Dim value As String = Me.Body("classCode")

            Return [Enum].Parse(GetType(tVehClass), Prefix + value, True)
        End Get
        Set(ByVal value As tVehClass)
            Me.Body("classCode") = value.ToString()
        End Set
    End Property
    Public Property configuration As VehicleConfig
        Get
            Dim value As String = Me.Body("configuration with trailer")

            Return [Enum].Parse(GetType(VehicleConfig), value, True)
        End Get
        Set(ByVal value As VehicleConfig)
            Me.Body("configuration with trailer") = value.ToString()
        End Set
    End Property
    Public Property TPMLM As Double
        Get
            Return Me.Body("TPMLM")
        End Get
        Set(ByVal value As Double)
            Me.Body("TPMLM") = value
        End Set
    End Property
    Public Property vVehMax As Double
        Get
            Return Me.Body("vVehMax")
        End Get
        Set(ByVal value As Double)
            Me.Body("vVehMax") = value
        End Set
    End Property
    Public Property vehHeight As Double
        Get
            Return Me.Body("vehHeight")
        End Get
        Set(ByVal value As Double)
            Me.Body("vehHeight") = value
        End Set
    End Property
    Public Property anemometerHeight As Double
        Get
            Return Me.Body("anemometerHeight")
        End Get
        Set(ByVal value As Double)
            Me.Body("anemometerHeight") = value
        End Set
    End Property
    Public Property testMass As Double
        Get
            Return Me.Body("testMass")
        End Get
        Set(ByVal value As Double)
            Me.Body("testMass") = value
        End Set
    End Property
    Public Property gearRatio_low As String
        Get
            Return Me.Body("gearRatio_low")
        End Get
        Set(ByVal value As String)
            Me.Body("gearRatio_low") = value
        End Set
    End Property
    Public Property gearRatio_high As String
        Get
            Return Me.Body("gearRatio_high")
        End Get
        Set(ByVal value As String)
            Me.Body("gearRatio_high") = value
        End Set
    End Property
    Public Property axleRatio As String
        Get
            Return Me.Body("axleRatio")
        End Get
        Set(ByVal value As String)
            Me.Body("axleRatio") = value
        End Set
    End Property
    Public Property fixedTransmission As gearBoxConfig
        Get
            Dim value As String = Me.Body("fixedTransmission")

            Return [Enum].Parse(GetType(gearBoxConfig), value, True)
        End Get
        Set(ByVal value As gearBoxConfig)
            Me.Body("fixedTransmission") = value.ToString()
        End Set
    End Property
    Public Property tqDriftLeft As String
        Get
            Return Me.Body("tqDriftLeft")
        End Get
        Set(ByVal value As String)
            Me.Body("tqDriftLeft") = value
        End Set
    End Property
    Public Property tqDriftRight As String
        Get
            Return Me.Body("tqDriftRight")
        End Get
        Set(ByVal value As String)
            Me.Body("tqDriftRight") = value
        End Set
    End Property
    Public Property tDriftZero As String
        Get
            Return Me.Body("tDriftZero")
        End Get
        Set(ByVal value As String)
            Me.Body("tDriftZero") = value
        End Set
    End Property
    Public Property tDriftCheck As String
        Get
            Return Me.Body("tDriftCheck")
        End Get
        Set(ByVal value As String)
            Me.Body("tDriftCheck") = value
        End Set
    End Property
    Public Property OldVehicle As Boolean
        Get
            Return IsOldVeh
        End Get
        Set(ByVal value As Boolean)
            IsOldVeh = value
        End Set
    End Property
#End Region ' "json props"

    Public ReadOnly Property IsWithTrailer As Boolean
        Get
            Return Me.configuration = VehicleConfig.No
        End Get
    End Property

    Public ReadOnly Property IsAPT As Boolean
        Get
            Return (Me.fixedTransmission = gearBoxConfig.APT Or Me.fixedTransmission = gearBoxConfig.AT Or Me.fixedTransmission = gearBoxConfig.No)
        End Get
    End Property

    Public ReadOnly Property IsSMT_AMT_DCT As Boolean
        Get
            Return (Me.fixedTransmission = gearBoxConfig.SMT Or Me.fixedTransmission = gearBoxConfig.AMT Or Me.fixedTransmission = gearBoxConfig.DCT Or Me.fixedTransmission = gearBoxConfig.MT_AMT Or Me.fixedTransmission = gearBoxConfig.Yes)
        End Get
    End Property
End Class
