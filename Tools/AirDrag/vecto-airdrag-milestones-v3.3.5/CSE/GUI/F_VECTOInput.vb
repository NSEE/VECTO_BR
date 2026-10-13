Imports System.IO
Imports System.Xml
Imports TUGraz.VectoHashing

Public Class F_VECTOInput
    ' Declaration
    Dim componentNamespace As XNamespace = "urn:tugraz:ivt:VectoAPI:DeclarationComponent:v1.0"
    Dim componentNamespaceV22 As XNamespace = "urn:tugraz:ivt:VectoAPI:DeclarationComponent:v2.2"
    Dim tns As XNamespace = "urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v1.0"

    ' Load GUI 
    Private Sub F_VECTOInput_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Declarations
        Dim configL As Boolean = True

        ' Disable the CdxA box
        TBCdxA.Text = Job.CdxA0
        TBCdxA.Enabled = False

        ' Load the config file
        Try
            VECTOconf = New cVECTOconfig(VECTOconfigPath)
        Catch ex As Exception
            logme(9, False, format(
                    "Failed loading VECTOconfig({0}) due to: {1}\n\iThis is normal the first time you launch the application.",
                    VECTOconfigPath, ex.Message), ex)

            ' Store a default config file
            VECTOconf.Store(VECTOconfigPath)
            logme(7, False, format("Stored new VECTOconfig({0}).", VECTOconfigPath))
        End Try

        ' Write the old Manufacturer
        TBManufacturer.Text = VECTOconf.Manufacturer
        TBTransR.Text = 0
        TBWorstCase.Text = 0

        CBCFD_Enabled.Checked = False
        EnableCFDComponents(CBCFD_Enabled.Checked)
    End Sub

    ' Get values from GUI and put in config values
    Private Sub UI_PopulateTo(ByVal value As cVECTOconfig)
        value.Manufacturer = Me.TBManufacturer.Text
        value.Model = Me.TBModel.Text
        value.CertNum = Me.TBCertNum.Text
        value.CdxA = Me.TBCdxA.Text
        value.AppVers = AppName & " " & AppVers
        value.TransfR = Convert.ToDouble(Me.TBTransR.Text)
        value.WorstCase = Convert.ToDouble(Me.TBWorstCase.Text)
        value.CdxACFD = If(String.IsNullOrEmpty(Me.TBCdACFD.Text), Nothing, Convert.ToDouble(Me.TBCdACFD.Text))
        value.LicenseNumberMethodCFD = Me.TBLicenseCFDMethod.Text
    End Sub

    Private Sub EnableCFDComponents(enabled As Boolean)
        Me.TBCdACFD.Enabled = enabled
        Me.LBCdACFD.Enabled = enabled
        Me.LBDeltaCFD_Unit.Enabled = enabled
        Me.TBLicenseCFDMethod.Enabled = enabled
        Me.LBLicenseCFDMethod.Enabled = enabled
    End Sub

#Region "Buttons"
    ' Cancel
    Private Sub ButtonCancel_Click(sender As Object, e As EventArgs) Handles ButtonCancel.Click
        Me.Close()
    End Sub

    ' OK
    Private Sub ButtonOK_Click(sender As Object, e As EventArgs) Handles ButtonOK.Click
        ' Declaration
        Dim newVECTOconfig As cVECTOconfig = VECTOconf.Clone()
        Dim check As Boolean = False

        ' Get values from GUI to new variable
        UI_PopulateTo(newVECTOconfig)

        ' Check the input
        If TBManufacturer.Text <> "" And
            TBModel.Text <> "" And
            TBCertNum.Text <> "" And
            TBTransR.Text <> "" And
            TBWorstCase.Text <> "" Then
            check = True
        End If

        ' Write the given data into the config file for the next opening
        newVECTOconfig.Store(VECTOconfigPath)
        VECTOconf = newVECTOconfig

        ' Write the config file
        If check Then
            ' Write the xml file and Hashing
            If (TransitionalService.Use2ndAmendment) Then
                Call AirDragDocument()
            Else
                Call AirDragDocumentV26()
            End If

            ' Message
            logme(7, False, "Generation of the VECTO input file successful.")

            ' Close the window
            Me.Close()
        Else
            ' Fail Message
            logme(7, False, "Not all values are given for the correct generation of the VECTO input file. No file is produced.")
        End If
    End Sub

    ' Check the input in TBTransR
    Private Sub TBTransR_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TBTransR.KeyPress
        Select Case Asc(e.KeyChar)
            Case 48 To 57, 8, 46, 45
                ' Zahlen, Backspace und Space zulassen
            Case Else
                ' alle anderen Eingaben unterdrücken
                e.Handled = True
        End Select
    End Sub

    Private Sub TBTransR_Leave(sender As Object, e As EventArgs) Handles TBTransR.Leave
        If TBTransR.Text = "" Then
            TBTransR.Text = "0"
        End If
    End Sub

    ' Check the input in TBWorstCase
    Private Sub TBWorstCase_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TBWorstCase.KeyPress
        Select Case Asc(e.KeyChar)
            Case 48 To 57, 8, 46
                ' Zahlen, Backspace und Space zulassen
            Case Else
                ' alle anderen Eingaben unterdrücken
                e.Handled = True
        End Select
    End Sub

    Private Sub TBWorstCase_Leave(sender As Object, e As EventArgs) Handles TBWorstCase.Leave
        ' Declaration
        Dim input As Double

        If TBWorstCase.Text = "" Then
            TBWorstCase.Text = "0"
        End If

        input = TBWorstCase.Text

        If input < 0 Or input > 0.2 Then
            TBWorstCase.Clear()
            MessageBox.Show(
                "Only values between 0 and 0.2 are allowed. Please correct the input.",
                "Incorrect input value",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
            TBWorstCase.Focus()
        End If
    End Sub

    Private Sub TBCdACFD_Leave(sender As Object, e As EventArgs) Handles TBCdACFD.Leave
        Dim inputValue As Double = TBCdACFD.Text
        If inputValue < 0 Then
            TBCdACFD.Clear()
            MessageBox.Show(
                "Only values greater than 0 are allowed. Please correct the input.",
                "Incorrect input value",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
        End If
    End Sub

    Private Sub CBCFD_Enabled_CheckedChanged(sender As Object, e As EventArgs) Handles CBCFD_Enabled.CheckedChanged
        EnableCFDComponents(CBCFD_Enabled.Checked)
    End Sub

#End Region

#Region "Hashing & xml File"
    Private Function AirDragDocument() As Boolean
        Dim stream As MemoryStream = New MemoryStream()
        Dim writer As StreamWriter = New StreamWriter(stream)

        writer.Write(GenerateComponentDocument(CreateAirdrag(componentNamespace)))
        writer.Flush()
        stream.Seek(0, SeekOrigin.Begin)

        Dim h = VectoHash.Load(stream)
        Dim Report As XDocument = h.AddHash()
        Report.Save(OutFolder & fName(JobFile, False) & ".xml")

        Return True
    End Function

    Private Function AirDragDocumentV26() As Boolean
        Dim stream As MemoryStream = New MemoryStream()
        Dim writer As StreamWriter = New StreamWriter(stream)

        writer.Write(GenerateComponentDocumentV26(CreateAirdragV26(componentNamespaceV22)))
        writer.Flush()
        stream.Seek(0, SeekOrigin.Begin)

        Dim h = VectoHash.Load(stream)
        Dim Report As XDocument = h.AddHash()
        Report.Save(OutFolder & fName(JobFile, False) & "_v26.xml")

        Return True
    End Function

    Protected Function GenerateComponentDocument(content As XElement) As XDocument
        ' Declaration
        Dim xsi = XNamespace.[Get]("http://www.w3.org/2001/XMLSchema-instance")
        Dim SchemaVersion As String = "1.0"
        Dim SchemaLocationBaseUrl As String = "https://webgate.ec.europa.eu/CITnet/svn/VECTO/trunk/Share/XML/XSD/"
        Dim component = New XDocument()

        component.Add(New XElement(componentNamespace + "VectoInputDeclaration",
                      New XAttribute("schemaVersion", SchemaVersion),
                      New XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName),
                      New XAttribute("xmlns", tns),
                      New XAttribute(XNamespace.Xmlns + "tns", componentNamespace),
                      New XAttribute(xsi + "schemaLocation", String.Format("{0} {1}VectoComponent.xsd", componentNamespace, SchemaLocationBaseUrl)),
            content))

        Return component
    End Function

    Protected Function CreateAirdrag(ns As XNamespace)
        Dim AppVersx As String
        If RELEASE_CANDIDATE Then
            AppVersx = AppVers + " - RELEASE CANDIDATE, NOT FOR CERTIFICATION!"
        ElseIf BETA_VERSION Then
            AppVersx = AppVers + " - BETA VERSION, NOT FOR CERTIFICATION!"
        Else
            AppVersx = AppVers
        End If

        Return New XElement(ns + "AirDrag",
                New XElement(tns + "Data",
                    New XElement(tns + "Manufacturer", VECTOconf.Manufacturer),
                    New XElement(tns + "Model", VECTOconf.Model),
                    New XElement(tns + "CertificationNumber", VECTOconf.CertNum),
                    New XElement(tns + "Date", XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Utc)),
                    New XElement(tns + "AppVersion", "VECTOAirDrag_" + AppVersx),
                    New XElement(tns + "CdxA_0", Math.Round(VECTOconf.CdxA, 2, MidpointRounding.AwayFromZero).ToString("0.00")),
                    New XElement(tns + "TransferredCdxA", Math.Round(VECTOconf.CdxA + VECTOconf.TransfR, 2, MidpointRounding.AwayFromZero).ToString("0.00")),
                    New XElement(tns + "DeclaredCdxA", Math.Round((VECTOconf.CdxA + VECTOconf.TransfR + VECTOconf.WorstCase), 2, MidpointRounding.AwayFromZero).ToString("0.00"))))
    End Function

    Protected Function GenerateComponentDocumentV26(content As XElement) As XDocument
        ' Declaration
        Dim xsi = XNamespace.[Get]("http://www.w3.org/2001/XMLSchema-instance")
        Dim SchemaVersion As String = "2.0"
        Dim SchemaLocationBaseUrl As String = "https://webgate.ec.europa.eu/CITnet/svn/VECTO/trunk/Share/XML/XSD/"
        Dim ns = "urn:tugraz:ivt:VectoAPI:DeclarationDefinitions"
        Dim xmlnsComponent = "urn:tugraz:ivt:VectoAPI:DeclarationComponent:v2.2"
        Dim xmlns As XNamespace = "urn:tugraz:ivt:VectoAPI:DeclarationComponent:v2.2"

        Dim component = New XDocument()
        component.Add(New XElement(componentNamespaceV22 + "VectoInputDeclaration",
                      New XAttribute("schemaVersion", SchemaVersion),
                      New XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName),
                      New XAttribute("xmlns", xmlns),
                      New XAttribute(XNamespace.Xmlns + "v2.0", $"{ns}:v2.0"),
                      New XAttribute(XNamespace.Xmlns + "v2.3", $"{ns}:v2.3"),
                      New XAttribute(XNamespace.Xmlns + "v2.6", $"{ns}:v2.6"),
                      New XAttribute(XNamespace.Xmlns + "tns", componentNamespaceV22),
                      New XAttribute(xsi + "schemaLocation", String.Format("{0} {1}VectoComponent.xsd", componentNamespaceV22, SchemaLocationBaseUrl)),
            content))

        Return component
    End Function

    Protected Function CreateAirdragV26(ns As XNamespace)
        Dim AppVersx As String
        Dim xsi = XNamespace.[Get]("http://www.w3.org/2001/XMLSchema-instance")
        If RELEASE_CANDIDATE Then
            AppVersx = AppVers + " - RELEASE CANDIDATE, NOT FOR CERTIFICATION!"
        ElseIf BETA_VERSION Then
            AppVersx = AppVers + " - BETA VERSION, NOT FOR CERTIFICATION!"
        Else
            AppVersx = AppVers
        End If

        Dim v26 = "v2.6"
        Dim v20Namespace As XNamespace = "urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v2.0"
        Dim v26Namespace As XNamespace = $"urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:{v26}"

        Dim cfdXElements
        If (VECTOconf.CdxACFD IsNot Nothing And Not String.IsNullOrEmpty(VECTOconf.LicenseNumberMethodCFD)) Then
            cfdXElements = New XElement() {
                    New XElement(v26Namespace + "DeltaCdxA_CFD", Math.Round((VECTOconf.CdxACFD.Value), 2, MidpointRounding.AwayFromZero).ToString("0.00")),
                    New XElement(v26Namespace + "LicenseNumberCFDMethod", VECTOconf.LicenseNumberMethodCFD)}
        Else
            cfdXElements = New XElement() {}
        End If

        Return New XElement(ns + "AirDrag",
                    New XElement(v20Namespace + "Data",
                    New XAttribute(xsi + "type", $"{v26}:AirDragDataDeclarationType"),
                    New XElement(v26Namespace + "Manufacturer", VECTOconf.Manufacturer),
                    New XElement(v26Namespace + "Model", VECTOconf.Model),
                    New XElement(v26Namespace + "CertificationNumber", VECTOconf.CertNum),
                    New XElement(v26Namespace + "Date", XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Utc)),
                    New XElement(v26Namespace + "AppVersion", "VECTOAirDrag_" + AppVersx),
                    New XElement(v26Namespace + "CdxA_0", Math.Round(VECTOconf.CdxA, 2, MidpointRounding.AwayFromZero).ToString("0.00")),
                    cfdXElements,
                    New XElement(v26Namespace + "DeltaCdxA_declared", Math.Round((VECTOconf.WorstCase), 2, MidpointRounding.AwayFromZero).ToString("0.00")),
                    New XElement(v26Namespace + "DeltaTransferredCdxA", Math.Round(VECTOconf.TransfR, 2, MidpointRounding.AwayFromZero).ToString("0.00"))))
    End Function

#End Region
End Class