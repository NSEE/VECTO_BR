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
Imports System.Globalization

''' <summary>The parent-class for Head/Body json files.
''' 
''' It is responsible for maintaining a "decent" Header by overlaying infos from subclasses,
''' and delegates the Body-building and its validation almost entirely to sub-classes.
''' </summary>
''' <remarks>
''' JSON files do not support comments, but help is provided by their accompanying json-schemas (see below).  
''' They are splitted in two sections, Header and Body. The Header contains administrational fields or fields 
''' related to the actual parsing of the file.
''' 
''' Of interest are the following 2 of Header’s properties:
''' •	/Header/StrictBody: Controls whether the application will accept any unknown body-properties 
'''     while reading the file. Set it to true to debug malformed input-files, ie to detect accidentally 
'''     renamed properties.
''' •	/Header/BodySchema: The JSON-schema of the body will be placed HERE, for documenting file.  
'''     When true, it is always replaced by the Body's schema on the next save. When false, it overrides 
''' application's choice and is not replaced ever.
''' 
''' 
''' How to Override:
''' ----------------
''' You can extend it to add Body-content in 2 ways (and a combination o0f both is possible):
''' •    With Properties: Add Properties that on Get/Set write/read directly from the body-content,
'''      so the object is always in sync with the JSON-content.
''' •    With public fields: Add fields that do not have any side-effect, but then the JSON-content 
'''     needs to get synced with those fields.  This is achieved by overriding thefollowing 2 methods:
''' 
''' In any case, 
''' </remarks>
Public MustInherit Class cJsonFile
    Implements ICloneable

    Private Const DateFrmt As String = "yyyy/MM/dd HH:mm:ss zzz"

    ''' <summary>The json-content for a json-file structured in Header/Body</summary>
    ''' <remarks>Note that the content is invalid according to the schema, and has to be specified by sub-classers.</remarks>
    Protected Shared Function JsonStr_FileContents() As String
        Return <json>{
            "Header": {
                "Title": null,
                "FileVersion":  null,
                "AppVersion":   null,
                "ModifiedDate": null,
                "CreatedBy": null,
                "StrictBody":   null,
                "BodySchema":   null,
            },
            "Body": null
        }</json>.Value
    End Function

    ''' <summary>The schema for a json-file structured in Header/Body</summary>
    Protected Shared Function JSchemaStr_Header(ByVal isStrict As Boolean) As String
        Dim requireAll As String = IIf(isStrict, "true", "false")
        Return <json>{
            "title": "vecto header/body json-file",
            "type": "object", "additionalProperties": false, 
            "properties": {
                "Header": { 
                    "type": "object", "additionalProperties": true, 
                    "required": true,
                    "properties": {
                        "Title": {
                            "type": "string",
                            "required": <%= requireAll %>,
                        },
                        "FileVersion": {
                            "type": "string",
                            "required": true,
                        },
                        "AppVersion": {
                            "type": "string",
                            "required": <%= requireAll %>,
                        },
                        "ModifiedDate": {
                            "type": "string",
                            "description": "Last-modification date",
                            "required": <%= requireAll %>,
                        },
                        "CreatedBy": {
                            "type": "string",
                            "description": "The persons who executed the application to produce this file, fetched from the license-file.",
                            "required": <%= requireAll %>,
                        },
                        "StrictBody": {
                            "title": "Validate body strictly",
                            "type": ["boolean", "null"],
                            "description": "If set to true, the application will not accept any unknown body-properties while reading this file.
When null/property missing, the application decides what to do.
It is useful for debugging malformed input-files, ie to detect accidentally renamed properties.",
                            "default": null,
                        },
                        "BodySchema": {
                            "title": "Body schema",
                            "type": ["boolean", "object", "null"],
                            "description": "Body schema is included HERE, for documenting file. 
When null/property missing, the application decides what to do. 
When True, it is always replaced by the Body's schema on the next save.
When False, it overrides Application's choice and is not replaced ever.", 
                            "default": null,
                        },
                    }
                },
                "Body": {}
            }
        }</json>.Value
    End Function

    ''' <summary>When a new file is Created or Stored, the contents return from this method is overlayed on /Header/*</summary>
    ''' <remarks>The result json must be valid overlaying this header.</remarks>
    Protected MustOverride Function HeaderOverlay() As JObject

    Protected MustOverride Function BodySchemaStr() As String

    ''' <summary>Invoked by this class for subclasses to validate file</summary>
    ''' <remarks>To signify validation-failure it can throw an exception or add err-messages into the supplied list</remarks>
    Protected MustOverride Sub ValidateBody(ByVal isStrict As Boolean, ByVal validateMsgs As IList(Of String))

    ''' <summary>Override this to read the JSON-content and update any fields on this instance</summary>
    Protected Overridable Sub OnContentUpdated()
    End Sub

    ''' <summary>Override this to update the JSON-content from any fields on this instance before storing it</summary>
    Protected Overridable Sub OnBeforeContentStored()
    End Sub


    ''' <summary>The whole json-content receiving any changes, always ready to be written as is.</summary>
    Private Content As JObject

    ' ''' <summary>Cached instance from 'Content', used (tentatively) for perfomance.</summary>
    'Public ReadOnly Header As JObject

    ' ''' <summary>Cached instance from 'Content', used (tentatively) for perfomance.</summary>
    'Public ReadOnly Body As JObject



    ''' <summary>Reads from a JSON-file (aka "Load")</summary>
    ''' <param name="skipValidation">When false (the default), validates json-contents in both cases (reading or creating-defaults)</param>
    ''' <remarks>It optionally checks version and validates its body with ValidateVersionAndBody().</remarks>
    Protected Sub New(ByVal inputFilePath As String, Optional ByVal skipValidation As Boolean = False)
        Dim strictHeader = False   '' Accept unknown headers.

        logme(4, False, format("Reading JSON-file({0})...", inputFilePath))

        Me.Content = ReadJsonFile(inputFilePath)
        'Me.Header = Content("Header")
        'Me.Body = Content("Body")

        If Not skipValidation Then
            Me.Validate(strictHeader)
        End If

        OnContentUpdated()
    End Sub

    ''' <summary>Creates an instance with defaults</summary>
    ''' <param name="body">The /Body content with defaults</param>
    ''' <param name="skipValidation">When false (the default), validates json-contents in both cases (reading or creating-defaults)</param>
    ''' <remarks>When defaulting, the resulted file-version is retrieved from 'CodeVersion' prop and the body from 'BodyStr' prop.</remarks>
    Protected Sub New(ByVal body As JToken, Optional ByVal skipValidation As Boolean = False)
        Dim strictHeader = False   '' Accept unknown headers.

        Dim jstr = JsonStr_FileContents()
        Me.Content = JObject.Parse(jstr)
        'Me.Header = Content("Header")
        UpdateHeader()

        Me.Content("Body") = body
        'Me.Body = Content("Body")

        If Not skipValidation Then
            Me.Validate(strictHeader)
        End If

        OnContentUpdated()
    End Sub

    ''' <summary>Validates and Writing to the config file</summary>
    Overridable Sub Store(ByVal fpath As String, Optional ByVal prefs As cPreferences = Nothing)
        OnBeforeContentStored()

        logme(4, False, format("Writing JSON-file({0})...", fpath))
        Me.UpdateHeader(prefs)

        Me.Validate(Me.StrictBody)
        WriteJsonFile(fpath, Content)
    End Sub

    ''' <summary>Maintains header's standard props and overlays any props from subclass.</summary>
    ''' <param name="prefs">It is there to be used when storing cPreferences themselfs.</param>
    ''' <remarks>Note that it is invoked early enough, before the new file has acquired a Body and before AppPreferences exist(!).</remarks>
    Sub UpdateHeader(Optional ByVal prefs As cPreferences = Nothing)
        If prefs Is Nothing Then prefs = AirDrag.Prefs
        Dim h As JObject = Me.Header

        h("ModifiedDate") = DateTime.Now.ToString(dateFrmt)

        '' Decide whether to add username in "CreatedBy".
        ''
        Dim username = ""
        If prefs Is Nothing OrElse Not prefs.hideUsername Then
            username = System.Security.Principal.WindowsIdentity.GetCurrent().Name & "@"
        End If
        h("CreatedBy") = format("{0})", username)

        h("AppVersion") = AppVers

        '' Ensure StrictBody element always there.

        If h("StrictBody") Is Nothing Then
            h("StrictBody") = Nothing
        End If

        '' Decide whether to include body-schema in header (for documenting file),
        ''   by checking the folllowing, ordered by priority:
        ''      1.   jsonfile:/Header/BodySchema
        ''      2.   prefs:/Body/IncludeSchemas
        ''      2.b. prefschema:/properties/Body/properties/IncludeSchemas/default   (implict by cPreferences.IncludeSchemas property)
        ''      3.   false
        Dim isIncludeSchema As Boolean
        Dim bodySchema = h("BodySchema")
        If bodySchema IsNot Nothing AndAlso bodySchema.Type = JTokenType.Boolean Then
            isIncludeSchema = bodySchema
        ElseIf prefs IsNot Nothing Then
            isIncludeSchema = prefs.includeSchemas
        Else
            isIncludeSchema = False
        End If
        If isIncludeSchema Then
            h("BodySchema") = New JRaw(Me.BodySchemaStr)
        ElseIf bodySchema Is Nothing Then
            h("BodySchema") = Nothing
        End If

        '' Overlay subclass's properties.
        ''
        For Each child As KeyValuePair(Of String, JToken) In Me.HeaderOverlay
            h(child.Key) = child.Value
        Next
    End Sub

    ''' <exception cref="FormatException">includes all validation errors</exception>
    ''' <param name="strictHeader">when false, relaxes Header's schema (used on Loading to be more accepting)</param>
    ''' <param name="prefs">It is there just to be used when storing cPreferences themselfs.</param>
    Friend Sub Validate(Optional ByVal strictHeader As Boolean = False, Optional ByVal prefs As cPreferences = Nothing)
        If prefs Is Nothing Then prefs = AirDrag.Prefs
        Dim validateMsgs As IList(Of String) = New List(Of String)

        Dim fileSchema = JSchema.Parse(JSchemaStr_Header(strictHeader))

        '' Validate Header
        ''
        ValidateJson(Me.Content, fileSchema, validateMsgs)
        If (validateMsgs.Any()) Then
            Throw New FormatException(format("Validating /Header failed due to: {0}", String.Join(vbCrLf, validateMsgs)))
        End If
        Dim dummy = New cSemanticVersion(Me.FileVersion) '' Just to ensure its syntax.

        '' Validate Body by subclass
        Me.ValidateBody(Me.StrictBody, validateMsgs)

        If (validateMsgs.Any()) Then
            Throw New FormatException(format("Validating /Body failed due to: {0}", String.Join(vbCrLf, validateMsgs)))
        End If
    End Sub



    Public Function Clone() As Object Implements ICloneable.Clone
        Dim nobj As cJsonFile = Me.MemberwiseClone()
        nobj.Content = Me.Content.DeepClone()

        Return nobj
    End Function

    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        If obj Is Nothing OrElse Not Me.GetType().Equals(obj.GetType()) Then
            Return False
        Else
            Dim oj As cJsonFile = DirectCast(obj, cJsonFile)
            If Not JToken.DeepEquals(Me.Body, oj.Body) Then Return False

            '' Compare Headers without the 'ModifiedDate' field 
            '' which would undoublfully different each time.
            Dim mh As New JObject(Me.Header)
            Dim oh As New JObject(oj.Header)

            mh.Remove("ModifiedDate")
            oh.Remove("ModifiedDate")
            Return JToken.DeepEquals(mh, oh)
        End If
    End Function

    ''' <param name="propPath">The JSON.net's XPath for a Body property, with or without a starting dot('.').</param>
    Public Function PropDefault(ByVal propPath As String) As JToken
        If Not propPath.StartsWith(".") Then
            propPath = "." & propPath
        End If
        Dim schemaPath = propPath.Replace(".", ".properties.")
        Return Me.BodySchema.SelectToken(schemaPath & ".default")
    End Function

    ''' <summary>Reads value found by XPath and if nothing there, fetches default-value schema.</summary>
    ''' <param name="propPath">The JSON.net's XPath for a Body property, which must start with a starting dot('.')!!!
    ''' 
    ''' Examples:
    '''   PROP REQUESTED                     'propPath' ARGUMENT
    '''   --------------                     -------------------
    '''   /Body/SomeProp'                --> .SomeProp
    '''   /Body/someGroup/somePropName   --> .someGroup.somePropName'.  
    ''' </param>
    ''' <remarks>Used by sublasses to implement Propety-Get with defaults when non-existent</remarks>
    Protected Function PropOrDefault(ByVal propPath As String) As JToken
        Dim value As JToken = Me.Body.SelectToken(propPath)
        If value Is Nothing Then  '' No prop existed
            '' Return a default from schema (if any).
            ''
            Dim schemaPath = propPath.Replace(".", ".properties.")
            Return Me.BodySchema.SelectToken(schemaPath & ".default")
        Else
            Return value
        End If
    End Function

    Private _BodySchema As JObject
    Public ReadOnly Property BodySchema As JObject
        Get
            If _BodySchema Is Nothing Then
                _BodySchema = JObject.Parse(Me.BodySchemaStr)
            End If
            Return _BodySchema
        End Get
    End Property


#Region "json props"
    Public ReadOnly Property Header() As JObject
        Get
            Return Me.Content("Header")
        End Get
    End Property
    Public ReadOnly Property Body() As JObject
        Get
            Return Me.Content("Body")
        End Get
    End Property


    Public ReadOnly Property Title As String
        Get
            Return Me.Header("Title")
        End Get
    End Property
    Public ReadOnly Property FileVersion As String
        Get
            Return Me.Header("FileVersion")
        End Get
    End Property
    Public ReadOnly Property AppVersion As String
        Get
            Return Me.Header("AppVersion")
        End Get
    End Property
    Public ReadOnly Property ModifiedDate As String
        Get
            Return Me.Header("ModifiedDate")
        End Get
    End Property

    Public ReadOnly Property StrictBody As Boolean
        Get
            Dim value = False
            Dim jt = Me.Header("StrictBody")
            If jt IsNot Nothing AndAlso jt.Type <> JTokenType.Null Then
                value = jt
            ElseIf Prefs IsNot Nothing Then
                value = Prefs.strictBodies
            End If
            Return value
        End Get
    End Property

    '' NO, logic behind it too complex, see UpdateHeader() instead.
    'Public ReadOnly Property BodySchema As Boolean
    '    Get
    '        Dim value = Me.Body("BodySchema")
    '        Return IIf(value Is Nothing OrElse value.Type = JTokenType.Null, False, value)
    '    End Get
    'End Property

#End Region ' "json props"

End Class
