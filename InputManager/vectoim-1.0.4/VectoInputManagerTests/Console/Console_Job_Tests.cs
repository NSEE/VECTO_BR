using AwesomeAssertions;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using VectoInputManager.Core;
using VectoObjects;


namespace VectoInputManager.Tests.Console;

public class Console_Job_Tests
{
    [TestCase("Vehicle1")]
    [TestCase("Vehicle2")]
    [TestCase("Vehicle3")]
    [TestCase("Vehicle5")]
    public void JobCreator_CreateVehicleFromComponents_Vehicles(string vehicle)
    {
        CommandApp app = TestUtils.InitApp();

        var inDir = Path.Combine(TestUtils.InputPath, vehicle);
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(JobCreator_CreateVehicleFromComponents_Vehicles));
        var outputVehicleFile = Path.Combine(outDir, $"{vehicle}_Job.xml");

        var inputArgs = new List<string> { "job", inDir + "/*.*",
            "-o", outputVehicleFile,
            "-b", "re-sign" };

        // Act
        var result = app.Run([.. inputArgs]);

        // Assert
        result.Should().Be(0, "Command should succeed");
        File.Exists(outputVehicleFile).Should().BeTrue("Output vehicle XML file should be created");

        // Report File
        var reportFile = Path.ChangeExtension(outputVehicleFile, ".report.txt");

        // Compare with expected report
        var expectedReportFile = Path.Combine(inDir, "Report.txt");

        TestUtils.AssertTextFilesAreEquivalent(reportFile, expectedReportFile);
    }

    [Test]
    public void JobCreator_CreateVehicleFromComponents_Vehicle4()
    {
        CommandApp app = TestUtils.InitApp();

        var inDir = Path.Combine(TestUtils.InputPath, "Vehicle4");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(JobCreator_CreateVehicleFromComponents_Vehicle4));
        var outputVehicleFile = Path.Combine(outDir, "Vehicle4_Job.json");

        var inputArgs = new List<string> { "job",
            inDir + "/*.*", "-o", outputVehicleFile,
            "-m", "Manufacturer=ManufacturerX",
            "-m", "Components.Powertrain[0].ElectricMachine.ElectricMachineSystem.Data.ElectricMachineType=RM",
            "-m", "Components.AxleWheels.Data.Axles[0].TwinTyres=True",
            "-k",
            "-b", "re-sign"
        };

        var result = app.Run([.. inputArgs]);

        var job = JsonDeclarationUtils.DeserializeFromFile(outputVehicleFile) as
            Vehicle_Multiple_PEV_HeavyLorryDeclarationType_v2_7;

        job!.Manufacturer.Should().Be("ManufacturerX");
        var em = (xEV_EM3_Powertrain_Type_v2_7)job.Components.Powertrain[0];
        var type = em.ElectricMachine.ElectricMachineSystem.Data as ElectricMachineSystemMeasuredDataDeclarationType_v2_3;
        type!.ElectricMachineType.Should().Be(ElectricMachineTypeType_v2_3.RM);

        // Report File
        var reportFile = Path.ChangeExtension(outputVehicleFile, ".report.txt");

        // Compare with expected report
        var expectedReportFile = Path.Combine(inDir, "Report.txt");
        TestUtils.AssertTextFilesAreEquivalent(reportFile, expectedReportFile);
    }

    [Test]
    public void JobCreator_CreateVehicleFromComponents_Vehicle6()
    {
        CommandApp app = TestUtils.InitApp();

        var inDir = Path.Combine(TestUtils.InputPath, "Vehicle6");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(JobCreator_CreateVehicleFromComponents_Vehicle6));
        var outputVehicleFile = Path.Combine(outDir, "Vehicle6_Job.json");

        Environment.CurrentDirectory = inDir;

        var inputArgs = new List<string> { "job", inDir + "/*.*",
            "-o", outputVehicleFile,
            "-m", "Components.Powertrain[1].Axlegear=axlegear.xml",
            "-m", "Components.AxleWheels.Data.Axles[1].Tyre=tyre.json",
            "-b", "re-sign" };

        var result = app.Run([.. inputArgs]);

        // Report File
        var reportFile = Path.ChangeExtension(outputVehicleFile, ".report.txt");

        // Compare with expected report
        var expectedReportFile = Path.Combine(inDir, "Report.txt");
        TestUtils.AssertTextFilesAreEquivalent(reportFile, expectedReportFile);
    }

    [Test]
    public void JobCreator_CreateVehicleFromComponents_Vehicle7_Modification()
    {
        CommandApp app = TestUtils.InitApp();

        var inDir = Path.Combine(TestUtils.InputPath, "Vehicle7");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(JobCreator_CreateVehicleFromComponents_Vehicle7_Modification));
        var outputVehicleFile = Path.Combine(outDir, "Vehicle7_Job.json");

        var inputArgs = new List<string> { "job", inDir + "/*.json",
            "-o", outputVehicleFile,
            "-b", "re-sign" };

        // Act
        var result = app.Run([.. inputArgs]);

        // Assert
        result.Should().Be(0, "Command should succeed");
        File.Exists(outputVehicleFile).Should().BeTrue("Output vehicle JSON file should be created");

        // Deserialize and verify the AirDrag modification
        var job = JsonDeclarationUtils.DeserializeFromFile(outputVehicleFile) as Vehicle_Conventional_HeavyLorryDeclarationType_v2_7;
        job.Should().NotBeNull("Vehicle should be deserialized successfully");

        // The Modify.json has a Components.Retarder section with no class specified,
        // and the only possible class is RetarderComponentDeclarationType_v2_0
        job!.Components.Retarder.Should().BeOfType<RetarderComponentDeclarationType_v2_0>();

        // The Modify.json has a Components.Retarder.Data section with various overrides,
        // and the class $type is specified.
        job!.Components.Retarder.Data.Should().BeOfType<RetarderDataDeclarationType_v2_0>();

        // The Modify.json has a Components.Axlegear section with no class specified,
        // and the only possible class is AxlegearComponentDeclarationType_v2_0
        job!.Components.Axlegear.Should().BeOfType<AxlegearComponentDeclarationType_v2_0>();

        // The Modify.json has a Components.Axlegear.Data section with various overrides,
        // but the class is not specified, and what to create is vague. So we set to null and give a warning.
        job!.Components.Axlegear.Data.Should().BeNull();

        AirDragComponentDeclarationType_v2_0 airDrag = job!.Components.AirDrag;

        airDrag.Data.Should().BeOfType<AirDragDataDeclarationType_v2_0>();
        var airDragData = (AirDragDataDeclarationType_v2_0)airDrag.Data;

        airDragData.CdxA_0.Should().Be(1.0m, "CdxA_0 should be modified to 1.0");

        // Report File
        var reportFile = Path.ChangeExtension(outputVehicleFile, ".report.txt");

        // Compare with expected report
        var expectedReportFile = Path.Combine(inDir, "Report.txt");
        TestUtils.AssertTextFilesAreEquivalent(reportFile, expectedReportFile);
    }

    [Test]
    public void JobCreator_ErrorWhenModifyReferencesFileNotInInputs()
    {
        var console = new TestConsole();
        CommandApp app = TestUtils.InitApp();
        app.Configure(cfg =>
        {
            cfg.ConfigureConsole(console);
            cfg.PropagateExceptions();
        });

        var inDir = Path.Combine(TestUtils.InputPath, "Vehicle6");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(JobCreator_ErrorWhenModifyReferencesFileNotInInputs));
        var outputVehicleFile = Path.Combine(outDir, "Vehicle_Error.xml");

        // Only include the vehicle XML, NOT the component file "missing_iepc.xml"
        var vehicleXml = Path.Combine(inDir, "Multiple_FCHV_F2_IEPC_HeavyLorry.xml");

        var inputArgs = new List<string> { "job",
            vehicleXml,
            "-o", outputVehicleFile,
            "-m", "Components.Powertrain[1].IEPC=missing_iepc.xml",
            "-b", "re-sign" };

        // Act & Assert — should throw because missing_iepc.xml was not included as an input
        var ex = Assert.Throws<InvalidOperationException>(() => app.Run([.. inputArgs]));
        ex!.Message.Should().Contain("missing_iepc.xml");
        ex.Message.Should().Contain("Make sure to include it as an input argument");
    }

    [Test]
    public void JobCreator_DuplicateComponentMappedToMultiplePaths()
    {
        CommandApp app = TestUtils.InitApp();

        var inDir = Path.Combine(TestUtils.InputPath, "Vehicle8");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(JobCreator_DuplicateComponentMappedToMultiplePaths));
        var outputVehicleFile = Path.Combine(outDir, "Vehicle8_Job.xml");

        Environment.CurrentDirectory = inDir;

        // Include the vehicle + axlegear + axlegear2 + tyre as positional inputs
        // Map axlegear.xml to Powertrain[0] and axlegear2-valid.xml to Powertrain[1]
        var inputArgs = new List<string> { "job", inDir + "/*.*",
            "-o", outputVehicleFile,
            "-m", "Components.Powertrain[0].Axlegear=axlegear.xml",
            "-m", "Components.Powertrain[1].Axlegear=axlegear2-valid.xml",
            "-m", "Components.AxleWheels.Data.Axles[1].Tyre=tyre.json",
            "-b", "re-sign" };

        var result = app.Run([.. inputArgs]);

        // Assert
        result.Should().Be(0, "Command should succeed");

        // Report File
        var reportFile = Path.ChangeExtension(outputVehicleFile, ".report.txt");

        // Compare with expected report
        var expectedReportFile = Path.Combine(inDir, "Report.txt");
        TestUtils.AssertTextFilesAreEquivalent(reportFile, expectedReportFile);
    }

    [Test]
    public void JobCreator_SameFileForMultipleComponents()
    {
        CommandApp app = TestUtils.InitApp();

        var inDir = Path.Combine(TestUtils.InputPath, "Vehicle8");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(JobCreator_SameFileForMultipleComponents));
        var outputVehicleFile = Path.Combine(outDir, "Vehicle8_SameFile_Job.xml");

        Environment.CurrentDirectory = inDir;

        // Map the SAME axlegear.xml to BOTH powertrains — this tests the duplicate mechanism
        var inputArgs = new List<string> { "job", inDir + "/*.*",
            "-o", outputVehicleFile,
            "-m", "Components.Powertrain[0].Axlegear=axlegear.xml",
            "-m", "Components.Powertrain[1].Axlegear=axlegear.xml",
            "-m", "Components.AxleWheels.Data.Axles[1].Tyre=tyre.json",
            "-b", "re-sign" };

        var result = app.Run([.. inputArgs]);

        // Assert
        result.Should().Be(0, "Command should succeed when same file is mapped to multiple paths");

        // Verify the report file was created and contains both axlegear mappings
        var reportFile = Path.ChangeExtension(outputVehicleFile, ".report.txt");
        File.Exists(reportFile).Should().BeTrue("Report file should be created");

        var reportContent = File.ReadAllText(reportFile);
        reportContent.Should().Contain("Components.Powertrain[0].Axlegear");
        reportContent.Should().Contain("Components.Powertrain[1].Axlegear");
    }

    [Test]
    public void JobCreator_VehicleWithNoComponents_Vehicle()
    {
        CommandApp app = TestUtils.InitApp();

        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(JobCreator_VehicleWithNoComponents_Vehicle));
        var outXml = Path.Combine(outDir, "VehicleEmpty_Job.json");

        var result = app.Run(["job", "-t", "MultiFCHVPrimaryBus_v2_7", "-o", outXml]);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public void JobCreator_ListVehicles()
    {
        var console = new TestConsole();
        CommandApp app = TestUtils.InitApp();
        app.Configure(cfg =>
        {
            cfg.ConfigureConsole(console);
        });

        // Act
        var result = app.Run(["job", "--list"]);

        // Assert
        result.Should().Be(0, "List command should succeed");

        var output = console.Output;
        output.Should().Contain("Valid vehicles are:", "Should display vehicle list header");
        output.Should().Contain("Code", "Should contain table headers");
        output.Should().Contain("Version", "Should contain table headers");
        output.Should().Contain("Description", "Should contain table headers");
    }

    [Test]
    public void JobCreator_ErrorHandling_NoOutputFile()
    {
        CommandApp app = TestUtils.InitApp();

        var inFile = Path.Combine(TestUtils.InputPath, "Vehicle1", "FCHV_F2_HeavyLorry.xml");

        // Act - Missing output file should cause error
        var result = app.Run(["job", inFile]);

        // Assert
        result.Should().Be(-1, "Should return error code when output file is missing");
    }

    [Test]
    public void JobCreator_ErrorHandling_InvalidVehicleTypeForExample()
    {
        CommandApp app = TestUtils.InitApp();

        // Try to generate example with auto (should fail)
        var result = app.Run(["job", "-t", "auto", "--example"]);

        // Assert
        result.Should().Be(-1, "Should return error code for auto vehicle type in example mode");
    }

    [Test]
    public void JobCreator_ErrorHandling_UnknownVehicleType()
    {
        CommandApp app = TestUtils.InitApp();

        // Try to generate example with unknown vehicle type
        var result = app.Run(["job", "-t", "UnknownVehicleType", "--example"]);

        // Assert
        result.Should().Be(-1, "Should return error code for unknown vehicle type");
    }

    [Test]
    public void Upgrade_Test()
    {
        CommandApp app = TestUtils.InitApp();

        var inputDir = Path.Combine(TestUtils.InputPath, "SchemaVersions");
        var outputDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(Upgrade_Test));

        foreach (var xmlFile in Directory.EnumerateFiles(inputDir, "*.xml", SearchOption.AllDirectories))
        {
            //if (!xmlFile.Contains("2.7.xml")) continue;

            var filename = "ConvHeavyLorry_v2_7-From-" + Path.GetFileName(xmlFile);
            var outXml = Path.Combine(outputDir, filename);
            var result = app.Run(["job", "-t", "ConvHeavyLorry_v2_7", "-b", "re-sign", xmlFile, "-o", outXml]);
            result.Should().Be(0);
        }
    }

    [Test]
    public void ObjectUpdate_Test()
    {
        CommandApp app = TestUtils.InitApp();

        var inputDir = Path.Combine(TestUtils.InputPath, "Objects");
        var outputDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(ObjectUpdate_Test));

        var outFile = Path.Combine(outputDir, "vehicle.json");
        var result = app.Run(["job", "-b", "reSign", inputDir + "/*.json", "-o", outFile]);
        result.Should().Be(0);

        var job = JsonDeclarationUtils.DeserializeFromFile(outFile) as Vehicle_IEPC_HeavyLorryDeclarationType_v2_7;
        job!.Manufacturer.Should().Be("XXX");

        var axGearData = (AxlegearDataDeclarationType_v2_0)job.Components.Axlegear.Data;
        axGearData.Ratio.Should().Be(3.64m);
        axGearData.TorqueLossMap.Count.Should().Be(10);
        axGearData.TorqueLossMap[0].torqueLoss.Should().Be(15m);
        axGearData.TorqueLossMap[1].torqueLoss.Should().Be(62m);
        axGearData.TorqueLossMap[2].torqueLoss.Should().Be(61m);

        var retarderData = (RetarderDataDeclarationType_v2_0)job.Components.Retarder.Data;
        retarderData.CertificationMethod.Should().Be(RetarderCertificationOptionType_v1_0.Measured);
        retarderData.RetarderLossMap[0].retarderSpeed.Should().Be(42m);
        retarderData.RetarderLossMap.Count.Should().Be(1);

        var airDragData = (AirDragDataDeclarationType_v1_0)job.Components.AirDrag.Data;
        airDragData.CdxA_0.Should().Be(1m);
    }
}
