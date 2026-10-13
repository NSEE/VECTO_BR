using AwesomeAssertions;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using VectoInputManager.Core;
using VectoObjects;
using VectoObjects.Helpers;

namespace VectoInputManager.Tests.Core;

public class XmlSerializer_Tests
{
    [Test]
    public void Decimal_Test()
    {
        var job = TypeUtils.CreateVectoInputDeclaration();

        var data = new BatterySystemStandardValuesDataType_v2_6()
        {
            id = "test",
            RatedCapacity = 100m,
            Date = DateTime.UtcNow.Date,
            Manufacturer = "TestManu",
            Model = "TestModel",
            CertificationNumber = "e12*2017/2400*2025/XXXX*B*00012*00",
            AppVersion = "1.0.0",
            CertificationMethod = BatterySystemStandardValuesDataTypeCertificationMethod_v2_6.Standard_values,
            BatteryType = BatteryTypeType_v2_3.HEBS,
        };

        data.OCV.Add(new BatterySystemOCVEntryType_v2_3
        {
            SoC = 0,
            OCV = 1
        });
        data.OCV.Add(new BatterySystemOCVEntryType_v2_3
        {
            SoC = 1,
            OCV = 1.005m,
        });

        data.InternalResistance.Add(new BatterySystemInternalResistanceEntryType_v2_3
        {
            SoC = 0,
            R_2 = 1.324234231m,
            R_10 = 2.2342342m,
            R_20 = 3.03m,
            R_120 = 4.041m,
        });

        job.BatterySystem = new BatteryComponentDeclarationType_v2_3
        {
            Data = data,
        };

        // Check SerializeFixed 

        var serializer = new XmlSerializer(typeof(VectoInputDeclaration_v2_2));

        var outputFile = Path.Combine(TestUtils.OutputPath, "decimals.xml");
        using (var fs = File.Create(outputFile))
        {
            serializer.SerializeFixed(fs, job);
        }

        var xml = File.ReadAllText(outputFile);

        xml.Should().Contain("<q1:RatedCapacity>100.00</q1:RatedCapacity>");
        xml.Should().Contain("<q1:Entry SoC=\"0\" OCV=\"1.00\" />");
        xml.Should().Contain("<q1:Entry SoC=\"1\" OCV=\"1.01\" />");
        xml.Should().Contain("<q1:Entry SoC=\"0\" R_2=\"1.32\" R_10=\"2.23\" R_20=\"3.03\" R_120=\"4.04\" />");

        //// Check HashAndSerialize

        var outputFile2 = Path.Combine(TestUtils.OutputPath, "decimals2.xml");
        using (var fs1 = File.Create(outputFile2))
            XmlUtils.HashAndSerialize(fs1, job, null);

        xml = File.ReadAllText(outputFile2);

        xml.Should().Contain("<q1:RatedCapacity>100.00</q1:RatedCapacity>");
        xml.Should().Contain("<q1:Entry SoC=\"0\" OCV=\"1.00\" />");
        xml.Should().Contain("<q1:Entry SoC=\"1\" OCV=\"1.01\" />");
        xml.Should().Contain("<q1:Entry SoC=\"0\" R_2=\"1.32\" R_10=\"2.23\" R_20=\"3.03\" R_120=\"4.04\" />");
    }

    [Test]
    public void ModificationOnXmlFile_Test()
    {
        var inputFile = Path.Combine(TestUtils.XMLReaderDeclarationTestFilesPath,
            "SchemaVersion2.7/PrimaryBuses/Multiple_SHEV_S2_IEPC_PrimaryBus.xml");
        var outputFile = Path.Combine(TestUtils.OutputPath,
            "Multiple_SHEV_S2_IEPC_PrimaryBus.xml");

        TestUtils.AssertValidateVehicleXML(inputFile);

        // Load the Input XML file
        using var stream = File.OpenRead(inputFile);
        VectoXml.TryGetSerializer(stream, out var serializer, out var rootType, out var error);

        var job = (VectoDeclarationJobType_v2_7)serializer.Deserialize(stream)!;

        var vehicle = job.Vehicle as Vehicle_Multiple_SHEV_PrimaryBusDeclarationType_v2_7;

        ElectricEnergyStorageType_v2_7 ees = vehicle!.Components.ElectricEnergyStorage;

        // Modify the job to demonstrate serialization
        ees.Battery[0].SOCmin = 9.05m;

        using (var fs = File.Create(outputFile))
        {
            XmlUtils.HashAndSerializeJob(fs, job);
        }

        TestUtils.AssertValidateVehicleXML(outputFile);

        // Read back the output file and verify the modification
        using var outputStream = File.OpenRead(outputFile);

        VectoXml.TryGetSerializer(outputStream, out var serializer2, out var rootType2, out var error2);
        var modifiedJob = (VectoDeclarationJobType_v2_7)serializer2.Deserialize(outputStream)!;

        var modifiedVehicle = modifiedJob.Vehicle as Vehicle_Multiple_SHEV_PrimaryBusDeclarationType_v2_7;
        var modifiedEes = modifiedVehicle!.Components.ElectricEnergyStorage;

        // Assert that the SOCmin value was correctly modified and persisted
        // Note: Due to the rounding during serialization, we expect 9.1 instead of 9.05
        Assert.That(modifiedEes.Battery[0].SOCmin, Is.EqualTo(9.1m));
    }

    [Test]
    public void HashAndSerialize_WritesXmlAndAppendsJsonComment()
    {
        // Arrange
        var declaration = TypeUtils.CreateVectoInputDeclaration();
        declaration.BatterySystem = new BatteryComponentDeclarationType_v2_3()
        {
            Data = new BatterySystemDataType_v2_3()
            {
                id = "BAT-12345",
                AppVersion = Consts.AppVersion,
                BatteryType = BatteryTypeType_v2_3.HEBS,
                RatedCapacity = 156m,
            },
        };

        var inputObj = new BatteryComponentInput()
        {
            Manufacturer = "a",
            Model = "b",
            CertificationNumber = "c"
        };

        using var output = new MemoryStream();

        // Act
        XmlUtils.HashAndSerialize(output, declaration, inputObj);

        output.Seek(0, SeekOrigin.Begin);
        var xml = new StreamReader(output).ReadToEnd();

        // Normalize line endings (ignore CRLF vs LF)
        var normalizedXml = xml.Replace("\r\n", "\n");

        // Parse XML (ignoring comment part)
        // Split before the JSON comment so XDocument.Parse
        // doesn't choke on trailing content formatting differences
        var xmlMainPart = normalizedXml.Split("<!-- Input data:",
            StringSplitOptions.RemoveEmptyEntries)[0].Trim();

        var doc = XDocument.Parse(xmlMainPart);

        // Assertions (relaxed):
        // 1. Root element exists
        doc.Root!.Name.LocalName.Should().Be("VectoInputDeclaration");

        // 2. BatterySystem/Data presence
        var data = doc.Descendants().First(e => e.Name.LocalName == "Data");
        data.Attribute("id")!.Value.Should().Be("BAT-12345");

        // 3. AppVersion matches
        doc.Descendants().First(e => e.Name.LocalName == "AppVersion").Value
            .Should().Be(Consts.AppVersion);

        // 4. RatedCapacity formatted with two decimals
        doc.Descendants().First(e => e.Name.LocalName == "RatedCapacity").Value
            .Should().Be("156.00");

        // 5. DigestValue exists and looks like a base64(SHA-256) (44 chars with = padding)
        var digestValue = doc.Descendants().First(e => e.Name.LocalName == "DigestValue").Value;
        digestValue.Length.Should().Be(44);
        digestValue.Should().MatchRegex("^[A-Za-z0-9+/=]{44}$");

        // 6. JSON comment appended and contains key fields
        normalizedXml.Should().Contain("<!-- Input data:");
        normalizedXml.Should().Contain(@"""Manufacturer"": ""a""");
        normalizedXml.Should().Contain(@"""CertificationNumber"": ""c""");

        // 7. Ensure signature reference points to the id
        var reference = doc.Descendants().First(e => e.Name.LocalName == "Reference");
        reference.Attribute("URI")!.Value.Should().Be("#BAT-12345");
    }

    [Test]
    public void SchemaVersions_Test()
    {
        // Examples from VectoCore\VectoCoreTest\TestData\XML\XMLReaderDeclaration
        var inputDir = Path.Combine(TestUtils.InputPath, "SchemaVersions");
        var outputDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(SchemaVersions_Test));

        var expected = new Dictionary<string,
            (string RootXmlns, string SchemaVersion, string ChildXmlns, string? TypeLocal, string? TypeNs)>
        {
            ["1.0.xml"] = ("urn:tugraz:ivt:VectoAPI:DeclarationInput:v1.0", "1.0",
                           "urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v1.0", null, null),

            ["2.0.xml"] = ("urn:tugraz:ivt:VectoAPI:DeclarationInput:v2.0", "2.0",
                           "urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v2.0", "VehicleDeclarationType",
                           "urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v2.0"),

            ["2.1.xml"] = ("urn:tugraz:ivt:VectoAPI:DeclarationInput:v2.0", "2.0",
                           "urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v2.0", "VehicleDeclarationType",
                           "urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v2.1"),

            ["2.2.xml"] = ("urn:tugraz:ivt:VectoAPI:DeclarationInput:v2.0", "2.0",
                           "urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v2.0", "VehicleDeclarationType",
                           "urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v2.1"),

            ["2.3.xml"] = ("urn:tugraz:ivt:VectoAPI:DeclarationInput:v2.0", "2.0",
                       "urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v2.0", "Vehicle_Conventional_HeavyLorryDeclarationType",
                       "urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v2.4"),

            ["2.4.xml"] = ("urn:tugraz:ivt:VectoAPI:DeclarationInput:v2.0", "2.0",
                           "urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v2.0", "Vehicle_HEV-Sx_HeavyLorryDeclarationType",
                           "urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v2.4"),

            ["2.7.xml"] = ("urn:tugraz:ivt:VectoAPI:DeclarationInput:v3.0", "2.0",
                           "urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v2.7", "Vehicle_Multiple_PEV_HeavyLorryDeclarationType",
                           "urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v2.7"),
        };

        foreach (var xmlFile in Directory.EnumerateFiles(inputDir, "*.xml", SearchOption.AllDirectories))
        {
            using var fs = File.OpenRead(xmlFile);
            VectoXml.TryGetSerializer(fs, out var serializer, out var rootType, out var error);
            var job = (IVehicleJobDeclaration)serializer.Deserialize(fs)!;

            var outputFile = Path.Combine(outputDir, Path.GetFileName(xmlFile));

            // Signing the component of v1.0 is not supported
            using (var stream = File.Create(outputFile))
            {
                XmlUtils.HashAndSerializeJob(stream, job);
            }

            TestUtils.AssertXmlFilesAreEquivalent(xmlFile, outputFile);
            TestUtils.AssertValidateVehicleXML(outputFile);

            // Verify root and first child namespaces, xsi:type and schemaVersion
            var fileName = Path.GetFileName(xmlFile);
            if (expected.TryGetValue(fileName, out var exp))
            {
                var doc = XDocument.Load(outputFile);
                var root = doc.Root!;
                var firstChild = root.Elements().First();

                // Root assertions
                root.Name.LocalName.Should().Be("VectoInputDeclaration");
                root.Name.NamespaceName.Should().Be(exp.RootXmlns);
                root.Attribute("schemaVersion")!.Value.Should().Be(exp.SchemaVersion);

                // First child (Vehicle) namespace assertion
                firstChild.Name.LocalName.Should().Be("Vehicle");
                firstChild.Name.NamespaceName.Should().Be(exp.ChildXmlns);

                // xsi:type resolution and assertion
                var xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
                var xsiTypeAttr = firstChild.Attribute(xsi + "type");

                if (exp.TypeLocal is null)
                {
                    xsiTypeAttr.Should().BeNull($"xsi:type should be absent in {fileName}");
                }
                else
                {
                    xsiTypeAttr.Should().NotBeNull($"xsi:type missing in {fileName}");

                    var val = xsiTypeAttr!.Value; // maybe "q1:Type" or "Type"
                    string local, ns;
                    var colonIdx = val.IndexOf(':');
                    if (colonIdx > 0)
                    {
                        var prefix = val[..colonIdx];
                        local = val[(colonIdx + 1)..];
                        ns = firstChild.GetNamespaceOfPrefix(prefix)?.NamespaceName ?? string.Empty;
                    }
                    else
                    {
                        local = val;
                        ns = firstChild.GetDefaultNamespace().NamespaceName;
                    }

                    local.Should().Be(exp.TypeLocal);
                    ns.Should().Be(exp.TypeNs);
                }
            }

            var outputFileJson = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(xmlFile) + ".json");

            var vehicle = job.Vehicle;
            JsonDeclarationUtils.SerializeToFile(vehicle!, outputFileJson, true);
        }
    }
}