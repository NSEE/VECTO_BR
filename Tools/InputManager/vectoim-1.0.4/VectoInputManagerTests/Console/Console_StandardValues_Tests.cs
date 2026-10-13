using AwesomeAssertions;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using System.Globalization;
using System.Xml.Linq;
using VectoInputManager.Core;
using VectoObjects;

namespace VectoInputManager.Tests.Console;

public class Console_StandardValues_Tests
{
    [Test]
    public void StandardValues_Capacitor_Interactive()
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("el-GR");

        var console = new TestConsole();
        CommandApp app = TestUtils.InitApp();
        app.Configure(cfg =>
        {
            cfg.ConfigureConsole(console);
        });

        // Interactive answers - emulate user input
        console.Input.PushTextWithEnter("2,7");
        console.Input.PushTextWithEnter("3000");
        console.Input.PushTextWithEnter("1");
        console.Input.PushTextWithEnter("Example Manufacturer");
        console.Input.PushTextWithEnter("Example Model");
        console.Input.PushTextWithEnter("EX1234567890");
        console.Input.PushTextWithEnter("1/1/2026");

        var outFile = Path.Combine(Path.GetTempPath(), $"vecto-{Guid.NewGuid()}.xml");

        // Act
        var result = app.Run(["sv", "-t", "Capacitor", "-i", "-o", outFile]);

        // Assert output file        
        File.Exists(outFile).Should().BeTrue();

        var xdoc = XDocument.Load(outFile);

        // Namespaces (root -> v2.2, Data element -> v2.3, children with q1 -> v2.6)
        XNamespace nsRoot = "urn:tugraz:ivt:VectoAPI:DeclarationComponent:v2.2";
        XNamespace nsData = "urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v2.3";
        XNamespace nsDefs = "urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v2.6";

        var dataEl = xdoc
            .Root!
            .Element(nsRoot + "CapacitorSystem")!
            .Element(nsData + "Data")!;

        // Core expected values from standard-values calculation + inputs
        dataEl.Element(nsDefs + "Manufacturer")!.Value.Should().Be("Example Manufacturer");
        dataEl.Element(nsDefs + "Model")!.Value.Should().Be("Example Model");
        dataEl.Element(nsDefs + "CertificationNumber")!.Value.Should().Be("EX1234567890");
        dataEl.Element(nsDefs + "CertificationMethod")!.Value.Should().Be("Standard values");

        // Standard values
        dataEl.Element(nsDefs + "Capacitance")!.Value.Should().Be("3000.00");
        dataEl.Element(nsDefs + "MaxVoltage")!.Value.Should().Be("2.70");
        dataEl.Element(nsDefs + "MinVoltage")!.Value.Should().Be("1.22"); // 0.45 * 2.7
        dataEl.Element(nsDefs + "InternalResistance")!.Value.Should().Be("3.75");
        dataEl.Element(nsDefs + "MaxChargingCurrent")!.Value.Should().Be("15000.00");
        dataEl.Element(nsDefs + "MaxDischargingCurrent")!.Value.Should().Be("15000.00");

        // Comment with JSON present
        var fileText = File.ReadAllText(outFile);
        fileText.Should().Contain(@"""Component"": ""Capacitor""");
        fileText.Should().Contain(@"""MaxVoltage"": 2.7");
        fileText.Should().Contain(@"""Capacitance"": 3000.0");
        fileText.Should().Contain(@"""CellNumberInSeries"": 1");
    }

    [Test]
    public void StandardValues_Angledrive_Interactive()
    {
        // In this test we use en-US culture to ensure decimal points are used
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        var console = new TestConsole();
        CommandApp app = TestUtils.InitApp();
        app.Configure(cfg =>
        {
            cfg.ConfigureConsole(console);
        });

        // Interactive answers - emulate user input
        console.Input.PushTextWithEnter("6000");  // TMaxIn (Maximum allowed input torque)
        console.Input.PushTextWithEnter("2.345");  // Ratio
        console.Input.PushTextWithEnter("0, 500, 1000, 1500");  // SpeedPoints (comma-separated)
        console.Input.PushTextWithEnter("-10000, -5000, 0, 5000, 10000");  // TorquePoints (comma-separated)
        console.Input.PushTextWithEnter("Example Angledrive Manufacturer");  // Manufacturer
        console.Input.PushTextWithEnter("Example Angledrive Model");  // Model
        console.Input.PushTextWithEnter("AD1234567890");  // CertificationNumber
        console.Input.PushTextWithEnter("1/1/2026");  // Date

        var outFile = Path.Combine(Path.GetTempPath(), $"vecto-{Guid.NewGuid()}.xml");

        // Act
        var result = app.Run(["sv", "-t", "Angledrive", "-i", "-o", outFile]);

        // Assert output file        
        File.Exists(outFile).Should().BeTrue();

        var xdoc = XDocument.Load(outFile);

        // Namespaces (root -> v2.2, Data element -> v2.0)
        XNamespace nsRoot = "urn:tugraz:ivt:VectoAPI:DeclarationComponent:v2.2";
        XNamespace nsData = "urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v2.0";

        var dataEl = xdoc
            .Root!
            .Element(nsRoot + "Angledrive")!
            .Element(nsData + "Data")!;

        // Core expected values from inputs
        dataEl.Element(nsData + "Manufacturer")!.Value.Should().Be("Example Angledrive Manufacturer");
        dataEl.Element(nsData + "CertificationMethod")!.Value.Should().Be("Standard values");

        // Input values
        dataEl.Element(nsData + "Ratio")!.Value.Should().Be("2.345");

        // Verify TorqueLossMap exists and has entries
        var torqueLossMap = dataEl.Element(nsData + "TorqueLossMap")!;
        var torqueLossEntries = torqueLossMap.Elements(nsData + "Entry").ToList();
        torqueLossEntries.Count.Should().Be(4 * 5); // 4 speed points * 5 torque points
    }

    [Test]
    public void StandardValues_FuelCell_FileAndCommandlineValues()
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-DE");

        var console = new TestConsole();
        CommandApp app = TestUtils.InitApp();
        app.Configure(cfg =>
        {
            cfg.ConfigureConsole(console);
        });

        var input = Path.Combine(TestUtils.InputPath, "Various", "FuelCell.json");

        var outFile = Path.Combine(Path.GetTempPath(), $"vecto-{Guid.NewGuid()}.json");

        // Act - Using -m parameters instead of interactive mode
        var result = app.Run([
            "sv", "-t", "FuelCell",
            input,
            "-m", "FCSRatedPower=120",
            "-m", "Manufacturer = Example String 2",
            "-m", "Model=Example String 3",
            "-m", "CertificationNumber=ABC",
            "-m", "Date=30.12.2026", // Note DE culture set in test
            "-o", outFile
        ]);

        File.Exists(outFile).Should().BeTrue();

        var outObj = JsonDeclarationUtils.Deserialize(File.ReadAllText(outFile))
            as FuelCellComponentDeclarationType_v2_6;

        var data = outObj!.Data as FuelCellDataDeclarationType_v2_6;

        data!.FCSRatedPower.Should().Be(120);
        data.Manufacturer.Should().Be("Example String 2");
        data.Model.Should().Be("Example String 3");
        data.CertificationNumber.Should().Be("ABC");
        data.Date.Should().Be(new DateTime(2026, 12, 30));
    }

    [Test]
    public void StandardValues_ExportExamples_CreateXML_AllComponents()
    {
        var console = new TestConsole();
        CommandApp app = TestUtils.InitApp();
        app.Configure(cfg =>
        {
            cfg.ConfigureConsole(console);
        });

        var componentTypes = Components.ComponentInformation
            .Select(ct => ct.ComponentType.ToString()).ToArray();

        foreach (var componentType in componentTypes)
        {
            var outputFolder = TestUtils.CombinePath(TestUtils.OutputPath, "StandardValues", componentType);
            // Export JSON example

            var jsonFile = Path.Combine(outputFolder, $"{componentType}.json");

            // Run the standard values command with -e parameter
            var result = app.Run(["sv", "-t", componentType, "-e", "-o", jsonFile]);

            result.Should().Be(0, $"Command should succeed for component type {componentType}");

            // Assert the file contains valid JSON with the correct component type
            var jsonContent = File.ReadAllText(jsonFile);
            jsonContent.Should().Contain($"\"Component\": \"{componentType}\"",
                $"JSON should contain the correct component type for {componentType}");

            // Basic JSON validation - should not throw
            var deserializedContent = JsonInputUtils.Deserialize(jsonContent);
            deserializedContent.Should().NotBeNull();

            // Create Standard Values XML from the JSON example

            var xmlFile = Path.Combine(outputFolder, $"{componentType}.xml");

            // Run the standard values command with -i (input JSON) and -o (output XML) parameters
            result = app.Run(["sv", jsonFile, "-o", xmlFile]);

            // Assert the XML generation succeeded
            result.Should().Be(0, $"XML generation should succeed for component type {componentType}");

            // Assert the XML file was created
            File.Exists(xmlFile).Should().BeTrue($"XML file should be created for {componentType}");

            // Basic XML validation - should be well-formed XML
            var xmlContent = File.ReadAllText(xmlFile);
            xmlContent.Should().NotBeEmpty($"XML content should not be empty for {componentType}");

            // Verify it's valid XML by loading it
            var xmlDoc = XDocument.Load(xmlFile);
            xmlDoc.Root.Should().NotBeNull($"XML should be well-formed for {componentType}");

            // Verify XML contains component-specific content
            xmlContent.Should().Contain("DeclarationComponent",
                $"XML should contain DeclarationComponent element for {componentType}");
        }
    }
}

