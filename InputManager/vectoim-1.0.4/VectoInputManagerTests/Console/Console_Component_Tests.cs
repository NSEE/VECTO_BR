using AwesomeAssertions;
using Spectre.Console.Cli;
using VectoInputManager.Core;
using VectoObjects;

namespace VectoInputManager.Tests.Console;

public class Console_Component_Tests
{
    private static string ComponentsPath => Path.Combine(TestUtils.InputPath, "Components");

    [Test]
    public void XmlToJson_Retarder()
    {
        CommandApp app = TestUtils.InitApp();

        var inFile = Path.Combine(ComponentsPath, "Components.Retarder.xml");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(Console_Component_Tests));
        var outFile = Path.Combine(outDir, "Retarder.json");

        var result = app.Run(["component", inFile, "-o", outFile]);

        result.Should().Be(0, "Command should succeed");
        File.Exists(outFile).Should().BeTrue("Output JSON file should be created");

        // Verify the JSON is valid by deserializing it
        var obj = JsonDeclarationUtils.DeserializeFromFile(outFile);
        obj.Should().BeAssignableTo<IComponentDeclaration>();
    }

    [Test]
    public void JsonToXml_Battery()
    {
        CommandApp app = TestUtils.InitApp();

        var inFile = Path.Combine(ComponentsPath, "Battery-json.json");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(Console_Component_Tests));
        var outFile = Path.Combine(outDir, "Battery.xml");

        var result = app.Run(["component", inFile, "-o", outFile]);

        result.Should().Be(0, "Command should succeed");
        File.Exists(outFile).Should().BeTrue("Output XML file should be created");

        TestUtils.AssertValidateComponentXML(outFile);
        TestUtils.AssertValidateHash(outFile);

        var component = TestUtils.LoadComponentFromXml(outFile);
        var sig = component.GetSignature();
        sig.IsEmptySignature().Should().BeFalse("Output should have a valid signature");
    }

    [Test]
    public void XmlToXml_Retarder()
    {
        CommandApp app = TestUtils.InitApp();

        var inFile = Path.Combine(ComponentsPath, "Components.Retarder.xml");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(Console_Component_Tests));
        var outFile = Path.Combine(outDir, "Retarder_copy.xml");

        var result = app.Run(["component", inFile, "-o", outFile]);

        result.Should().Be(0, "Command should succeed");
        File.Exists(outFile).Should().BeTrue("Output XML file should be created");

        TestUtils.AssertValidateComponentXML(outFile);
        TestUtils.AssertValidateHash(outFile);

        var component = TestUtils.LoadComponentFromXml(outFile);
        var sig = component.GetSignature();
        sig.IsEmptySignature().Should().BeFalse("Output should have a valid signature");
    }

    [Test]
    public void JsonToJson_Battery()
    {
        CommandApp app = TestUtils.InitApp();

        var inFile = Path.Combine(ComponentsPath, "Battery-json.json");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(Console_Component_Tests));
        var outFile = Path.Combine(outDir, "Battery_copy.json");

        var result = app.Run(["component", inFile, "-o", outFile]);

        result.Should().Be(0, "Command should succeed");
        File.Exists(outFile).Should().BeTrue("Output JSON file should be created");

        var obj = JsonDeclarationUtils.DeserializeFromFile(outFile);
        obj.Should().BeAssignableTo<IComponentDeclaration>();
    }

    [Test]
    public void Modify_RetarderModel()
    {
        CommandApp app = TestUtils.InitApp();

        var inFile = Path.Combine(ComponentsPath, "Components.Retarder.xml");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(Console_Component_Tests));
        var outFile = Path.Combine(outDir, "Retarder_Modified.xml");

        var result = app.Run(["component", inFile, "-o", outFile,
            "-m", "Data.Model=Modified Retarder"]);

        result.Should().Be(0, "Command should succeed");
        File.Exists(outFile).Should().BeTrue("Output XML file should be created");

        TestUtils.AssertValidateComponentXML(outFile);
    }

    [Test]
    public void Modify_RetarderModel_JsonOutput()
    {
        CommandApp app = TestUtils.InitApp();

        var inFile = Path.Combine(ComponentsPath, "Components.Retarder.xml");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(Console_Component_Tests));
        var outFile = Path.Combine(outDir, "Retarder_Modified.json");

        var result = app.Run(["component", inFile, "-o", outFile,
            "-m", "Data.Model=Modified Retarder"]);

        result.Should().Be(0, "Command should succeed");
        File.Exists(outFile).Should().BeTrue("Output JSON file should be created");

        var obj = JsonDeclarationUtils.DeserializeFromFile(outFile);
        obj.Should().BeAssignableTo<IComponentDeclaration>();
    }

    [Test]
    public void Modify_AxleNoSig()
    {
        CommandApp app = TestUtils.InitApp();

        var inFile = Path.Combine(ComponentsPath, "Axle-NoSig.xml");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(Console_Component_Tests));
        var outFile = Path.Combine(outDir, "Axle_Modified.xml");

        var result = app.Run(["component", inFile, "-o", outFile,
            "-m", "Data.Model=Modified Axle"]);

        result.Should().Be(0, "Command should succeed");
        File.Exists(outFile).Should().BeTrue("Output XML file should be created");

        TestUtils.AssertValidateComponentXML(outFile);
    }

    [Test]
    public void Modify_RetarderWithJsonFile()
    {
        CommandApp app = TestUtils.InitApp();

        var inFile = Path.Combine(ComponentsPath, "Components.Retarder.xml");
        var jsonModifyFile = Path.Combine(ComponentsPath, "modify_retarder.json");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(Console_Component_Tests));
        var outFile = Path.Combine(outDir, "Retarder_JsonModified.xml");

        var result = app.Run(["component", "-b", "resign", inFile, "-o", outFile,
            jsonModifyFile]);

        result.Should().Be(0, "Command should succeed");
        File.Exists(outFile).Should().BeTrue("Output XML file should be created");

        // Verify the JSON modifications were applied
        var xmlContent = File.ReadAllText(outFile);
        xmlContent.Should().Contain("Measured", "CertificationMethod should be changed to 'Measured'");
        xmlContent.Should().Contain("retarderSpeed=\"42", "RetarderLossMap should contain the modified entry");
        xmlContent.Should().NotContain("retarderSpeed=\"0.00\"", "Original first entry should be replaced by $crop");
    }

    [Test]
    public void Signature_ValidSig_PassThrough()
    {
        // GearBox-ValidSig.xml has a valid signature.
        // Converting it should produce a valid signed output.
        CommandApp app = TestUtils.InitApp();

        var inFile = Path.Combine(ComponentsPath, "GearBox-ValidSig.xml");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(Console_Component_Tests));
        var outFile = Path.Combine(outDir, "GearBox_ValidSig_Out.xml");

        var result = app.Run(["component", inFile, "-o", outFile]);

        result.Should().Be(0, "Command should succeed");
        File.Exists(outFile).Should().BeTrue("Output XML file should be created");

        TestUtils.AssertValidateComponentXML(outFile);
        TestUtils.AssertValidateHash(outFile);

        var component = TestUtils.LoadComponentFromXml(outFile);
        var sig = component.GetSignature();
        sig.IsEmptySignature().Should().BeFalse("Output should have a valid signature");
    }

    [Test]
    public void Signature_NoSig_OutputHasValidHash()
    {
        // Axle-NoSig.xml has no signature.
        // Output should still have a valid hash (re-signed by HashAndSerialize).
        CommandApp app = TestUtils.InitApp();

        var inFile = Path.Combine(ComponentsPath, "Axle-NoSig.xml");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(Console_Component_Tests));
        var outFile = Path.Combine(outDir, "Axle_NoSig_Out.xml");

        var result = app.Run(["component", inFile, "-o", outFile]);

        result.Should().Be(0, "Command should succeed");
        File.Exists(outFile).Should().BeTrue("Output XML file should be created");

        TestUtils.AssertValidateComponentXML(outFile);
        TestUtils.AssertValidateHash(outFile);

        var component = TestUtils.LoadComponentFromXml(outFile);
        var sig = component.GetSignature();
        sig.IsEmptySignature().Should().BeFalse("Re-signed output should have a valid signature");
    }

    [Test]
    public void Signature_CorruptedSig_OutputIsReSigned()
    {
        // FuelCell-CorruptedSig.xml has a corrupted signature (URI mismatch).
        // The component tool re-signs via HashAndSerialize, producing a valid hash.
        CommandApp app = TestUtils.InitApp();

        var inFile = Path.Combine(ComponentsPath, "FuelCell-CorruptedSig.xml");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(Console_Component_Tests));
        var outFile = Path.Combine(outDir, "FuelCell_CorruptedSig_Out.xml");

        var result = app.Run(["component", inFile, "-o", outFile]);

        result.Should().Be(0, "Command should succeed");
        File.Exists(outFile).Should().BeTrue("Output XML file should be created");

        TestUtils.AssertValidateComponentXML(outFile);
        TestUtils.AssertValidateHash(outFile);

        var component = TestUtils.LoadComponentFromXml(outFile);
        var sig = component.GetSignature();
        sig.IsEmptySignature().Should().BeFalse("Re-signed output should have a valid signature");
    }

    [Test]
    public void NoInput_ReturnsError()
    {
        CommandApp app = TestUtils.InitApp();

        var result = app.Run(["component"]);

        result.Should().NotBe(0, "Command should fail without input file");
    }

    [Test]
    public void NoOutput_ReturnsError()
    {
        CommandApp app = TestUtils.InitApp();

        var inFile = Path.Combine(ComponentsPath, "Components.Retarder.xml");

        var result = app.Run(["component", inFile]);

        result.Should().NotBe(0, "Command should fail without output file");
    }

    [Test]
    public void MissingInputFile_ReturnsError()
    {
        CommandApp app = TestUtils.InitApp();

        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(Console_Component_Tests));
        var outFile = Path.Combine(outDir, "missing.xml");

        var result = app.Run(["component", "nonexistent_file.xml", "-o", outFile]);

        result.Should().NotBe(0, "Command should fail with missing input file");
    }
}
