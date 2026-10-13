using AwesomeAssertions;
using Spectre.Console.Cli;


namespace VectoInputManager.Tests.Console;

public class Console_Job_Extract_Tests
{
    [Test]
    public void JobCreator_ExtractComponents_Vehicle1()
    {
        CommandApp app = TestUtils.InitApp();

        var inDir = Path.Combine(TestUtils.InputPath, "Vehicle1");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(JobCreator_ExtractComponents_Vehicle1));

        // Create a job just to debug the intermediate XML structure 
        var result = app.Run(["job", inDir + "/*.*", "-o",
            Path.Combine(outDir, "Composed.xml"), "-b", "re-sign"]);

        var xmlOut = Path.Combine(outDir, "xml");

        // Extract XML components from the job
        result = app.Run(["job", inDir + "/*.*", "-o", Path.Combine(outDir, "Composed2.xml"),
            "-x", xmlOut, "-b", "re-sign"]);

        result.Should().Be(0, "Command should succeed");

        var xmlFiles = Directory.GetFiles(xmlOut, "*.xml", SearchOption.AllDirectories);
        xmlFiles.Length.Should().BeGreaterThan(0);
        xmlFiles.All(f => f.EndsWith(".xml")).Should().BeTrue();

        // Validate XML and hash of each extracted file
        foreach (var file in xmlFiles)
        {
            TestUtils.AssertValidateComponentXML(file);
            TestUtils.AssertValidateHash(file);
        }
        xmlFiles.Length.Should().Be(10);

        // Extract JSON components from the job
        var jsonOut = Path.Combine(outDir, "json");
        result = app.Run(["job", inDir + "/*.*", "-x", jsonOut, "-X", "json", "-b", "re-sign"]);

        result.Should().Be(0, "Command should succeed");

        var jsonFiles = Directory.GetFiles(jsonOut, "*.json", SearchOption.AllDirectories);
        jsonFiles.Length.Should().Be(10);
    }

    [Test]
    public void JobCreator_Extract_Valid()
    {
        CommandApp app = TestUtils.InitApp();

        var inFile = Path.Combine(TestUtils.InputPath, "Extract", "Conventional_HeavyLorry.xml");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(JobCreator_Extract_Valid));

        var result = app.Run(["job", inFile, "-x", outDir]);
        result.Should().Be(0, "Command should succeed");

        var files = Directory.GetFiles(outDir, "*.*", SearchOption.AllDirectories);
        files.Length.Should().Be(9);
        files.All(f => f.EndsWith(".xml")).Should().BeTrue();

        // Validate XML and hash of each file

        foreach (var file in files)
        {
            TestUtils.AssertValidateComponentXML(file);
            TestUtils.AssertValidateHash(file);
        }

        foreach (string file in new string[]
        {
            "Components.AirDrag.xml",
            "Components.Angledrive.xml",
            "Components.Axlegear.xml",
            "Components.AxleWheels.Data.Axles[0].Tyre.xml",
            "Components.AxleWheels.Data.Axles[1].Tyre.xml",
            "Components.Engine.xml",
            "Components.Gearbox.xml",
            "Components.Retarder.xml",
            "Components.TorqueConverter.xml"})
        {
            var expected = Path.Combine(TestUtils.InputPath, "Extract/ExpectedXml", file);
            var output = Path.Combine(outDir, file);
            TestUtils.AssertTextFilesAreEquivalent(output, expected);
        }

        // Also try to extract JSON components from the job

        app.Run(["job", inFile, "-x", outDir, "-X", "json"]);
        files = Directory.GetFiles(outDir, "*.json", SearchOption.AllDirectories);
        files.Length.Should().Be(9);
    }

    [Test]
    public void JobCreator_Extract_V1_0_ToJson()
    {
        CommandApp app = TestUtils.InitApp();

        var inFile = Path.Combine(TestUtils.InputPath, "SchemaVersions", "1.0.xml");
        var orgFile = Path.Combine(TestUtils.InputPath, "Extract", "Conventional_HeavyLorry.xml");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(JobCreator_Extract_V1_0_ToJson));

        var result = app.Run(["job", inFile, "-x", outDir, "-X", "json"]);

        var files = Directory.GetFiles(outDir, "*.*", SearchOption.AllDirectories);
        files.Length.Should().Be(7);
        files.All(f => f.EndsWith(".json")).Should().BeTrue();

        result.Should().Be(0, "Command should succeed");

        result = app.Run(["job", orgFile, "-b", "resign", outDir + "/*.json", "-o", outDir + "/Updated.xml"]);
        result.Should().Be(0, "Command should succeed");
    }

    [Test]
    public void JobCreator_Extract_V1_0_ToXml()
    {
        CommandApp app = TestUtils.InitApp();

        var inFile = Path.Combine(TestUtils.InputPath, "SchemaVersions", "1.0.xml");
        var orgFile = Path.Combine(TestUtils.InputPath, "Extract", "Conventional_HeavyLorry.xml");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(JobCreator_Extract_V1_0_ToXml));

        var result = app.Run(["job", inFile, "-x", outDir, "-X", "xml"]);

        var files = Directory.GetFiles(outDir, "*.*", SearchOption.AllDirectories);
        files.Length.Should().Be(7);
        files.All(f => f.EndsWith(".xml")).Should().BeTrue();

        result.Should().Be(0, "Command should succeed");

        result = app.Run(["job", orgFile, "-b", "resign", outDir + "/*.xml", "-o", outDir + "/Updated.xml"]);
        result.Should().Be(0, "Command should succeed");
    }
}
