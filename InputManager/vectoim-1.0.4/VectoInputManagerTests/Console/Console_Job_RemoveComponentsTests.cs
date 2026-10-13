using AwesomeAssertions;
using Spectre.Console.Cli;
using VectoInputManager.Core;
using VectoObjects;
using VectoObjects.Helpers;


namespace VectoInputManager.Tests.Console;

public class Console_Job_RemoveComponentsTests
{
    [Test]
    public void JobCreator_RemoveComponents_Json()
    {
        CommandApp app = TestUtils.InitApp();

        var inFile = Path.Combine(TestUtils.InputPath, "Strip", "Conventional_PrimaryBus.xml");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(JobCreator_RemoveComponents_Json));
        var outputVehicleFile = Path.Combine(outDir, "Conventional_PrimaryBus.json");

        // Act - create job with --drop-components
        var result = app.Run(["job", inFile, "-o", outputVehicleFile, "-d", "-b", "re-sign"]);

        // Assert
        result.Should().Be(0, "Command should succeed");
        File.Exists(outputVehicleFile).Should().BeTrue("Output vehicle JSON file should be created");

        // Deserialize and verify all components have been removed (nullified)
        var vehicle = JsonDeclarationUtils.DeserializeFromFile(outputVehicleFile)
            as Vehicle_Conventional_PrimaryBusDeclarationType_v2_7;
        vehicle.Should().NotBeNull("Vehicle should be deserialized successfully");

        var components = vehicle!.Components;
        components.Engine.Should().BeNull("Engine component should be removed");
        components.Gearbox.Should().BeNull("Gearbox component should be removed");
        components.Axlegear.Should().BeNull("Axlegear component should be removed");
        components.Retarder.Should().BeNull("Retarder component should be removed");
        components.TorqueConverter.Should().BeNull("TorqueConverter component should be removed");
        components.Angledrive.Should().BeNull("Angledrive component should be removed");

        // AxleWheels is not a component itself, but the Tyres within it are
        components.AxleWheels.Should().NotBeNull();
        var axleWheelsData = (AxleWheelsDataDeclarationType_v2_0)components.AxleWheels.Data;
        ((AxleDataDeclarationType_v2_0)axleWheelsData.Axles[0]).Tyre.Should().BeNull("Tyre on axle 0 should be removed");
        ((AxleDataDeclarationType_v2_0)axleWheelsData.Axles[1]).Tyre.Should().BeNull("Tyre on axle 1 should be removed");
    }

    [Test]
    public void JobCreator_RemoveComponents_Xml()
    {
        CommandApp app = TestUtils.InitApp();

        var inFile = Path.Combine(TestUtils.InputPath, "Strip", "Conventional_PrimaryBus.xml");
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(JobCreator_RemoveComponents_Xml));
        var outputVehicleFile = Path.Combine(outDir, "Conventional_PrimaryBus.xml");

        // Act - create job with --drop-components and XML output
        var result = app.Run(["job", inFile, "-o", outputVehicleFile, "-d", "-b", "re-sign"]);

        // Assert
        result.Should().Be(0, "Command should succeed");
        File.Exists(outputVehicleFile).Should().BeTrue("Output vehicle XML file should be created");

        using var fileStream = new FileStream(outputVehicleFile, FileMode.Open, FileAccess.Read);
        VectoXml.TryGetSerializer(fileStream, out var serializer, out _, out _).Should().BeTrue();
        var job = serializer.Deserialize(fileStream) as IVehicleJobDeclaration;
        job.Should().NotBeNull();

        var vehicle = job!.Vehicle as Vehicle_Conventional_PrimaryBusDeclarationType_v2_7;
        vehicle.Should().NotBeNull("Vehicle should be deserialized successfully");

        var components = vehicle!.Components;
        components.Engine.Should().BeNull("Engine component should be removed");
        components.Gearbox.Should().BeNull("Gearbox component should be removed");
        components.Axlegear.Should().BeNull("Axlegear component should be removed");
        components.Retarder.Should().BeNull("Retarder component should be removed");
        components.TorqueConverter.Should().BeNull("TorqueConverter component should be removed");
        components.Angledrive.Should().BeNull("Angledrive component should be removed");

        // AxleWheels is not a component itself, but the Tyres within it are
        components.AxleWheels.Should().NotBeNull();
        var axleWheelsData = (AxleWheelsDataDeclarationType_v2_0)components.AxleWheels.Data;
        ((AxleDataDeclarationType_v2_0)axleWheelsData.Axles[0]).Tyre.Should().BeNull("Tyre on axle 0 should be removed");
        ((AxleDataDeclarationType_v2_0)axleWheelsData.Axles[1]).Tyre.Should().BeNull("Tyre on axle 1 should be removed");
    }
}