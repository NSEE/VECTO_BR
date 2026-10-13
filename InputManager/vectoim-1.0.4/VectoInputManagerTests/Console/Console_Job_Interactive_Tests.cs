using AwesomeAssertions;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using VectoInputManager.Commands;

namespace VectoInputManager.Tests.Console;

public class Console_Job_Interactive_Tests
{
    [Test]
    public void JobCreator_InteractiveBusCreation_ConventionalCompletedBus()
    {
        PromptActionHelper.TestMode = true;

        var console = new TestConsole();
        CommandApp app = TestUtils.InitApp();
        app.Configure(cfg =>
        {
            cfg.ConfigureConsole(console);
        });

        var outDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(JobCreator_InteractiveBusCreation_ConventionalCompletedBus));
        var outputVehicleFile = Path.Combine(outDir, "ConventionalCompletedBus_Job.xml");

        // Simulate user interactive input based on the provided parameters
        console.Input.PushTextWithEnter("Conventional Vehicle - Completed Bus (v2.7)"); // Vehicle Type selection
        console.Input.PushTextWithEnter("Manu"); // Manufacturer
        console.Input.PushTextWithEnter("ManuAddress"); // Manufacturer address
        console.Input.PushTextWithEnter("VIN1"); // VIN
        console.Input.PushTextWithEnter("1/1/2024"); // Date (DateTime)
        console.Input.PushTextWithEnter("simmodel");
        console.Input.PushTextWithEnter("TestModel"); // Model (providing valid value instead of 'd')
        console.Input.PushTextWithEnter("M 3"); // Legislative category
        console.Input.PushTextWithEnter(""); // Corrected actual mass (empty for optional)
        console.Input.PushTextWithEnter("5000"); // Technical permissible maximum laden mass (valid integer instead of 'd')
        console.Input.PushTextWithEnter("Yes"); // Airdrag modified multistep
        console.Input.PushTextWithEnter("I"); // Class bus
        console.Input.PushTextWithEnter("Compressed"); // Ng tank system
        console.Input.PushTextWithEnter(""); // Number passenger seats lower deck (empty for optional)
        console.Input.PushTextWithEnter(""); // Number passengers standing lower deck (empty for optional)
        console.Input.PushTextWithEnter(""); // Number passenger seats upper deck (empty for optional)
        console.Input.PushTextWithEnter(""); // Number passengers standing upper deck (empty for optional)
        console.Input.PushTextWithEnter("CA"); // Bodywork code
        console.Input.PushTextWithEnter("Yes"); // Low entry
        console.Input.PushTextWithEnter(""); // Height integrated body (empty for optional)
        console.Input.PushTextWithEnter(""); // Vehicle length (empty for optional)
        console.Input.PushTextWithEnter(""); // Vehicle width (empty for optional)
        console.Input.PushTextWithEnter(""); // Entrance height (empty for optional)
        console.Input.PushTextWithEnter("Electric"); // Door drive technology
        console.Input.PushTextWithEnter("Interim"); // Vehicle declaration type
        console.Input.PushTextWithEnter("s"); // Vehicle type approval number

        // ADAS section
        console.Input.PushTextWithEnter("ADAS_Conventional_Type_v2_7");
        console.Input.PushTextWithEnter("Yes"); // Engine stop start
        console.Input.PushTextWithEnter("Yes"); // Eco roll without engine stop
        console.Input.PushTextWithEnter("Yes"); // Eco roll with engine stop
        console.Input.PushTextWithEnter("None"); // Predictive cruise control
        console.Input.PushTextWithEnter("Yes"); // APT eco roll release lockup clutch
        console.Input.PushTextWithEnter("2"); // H2StorageUsableCapacityValue

        //console.Input.PushTextWithEnter("a1"); // Id
        //console.Input.PushTextWithEnter(""); // H2StorageUsableCapacity
        console.Input.PushTextWithEnter("Compressed"); // HydrogenStorageTechnologyValue
        console.Input.PushTextWithEnter("Components_Conventional_CompletedBusType_v2_7"); // c type

        // Auxiliaries component
        console.Input.PushTextWithEnter("AUX_Component_Conventional_CompletedBus_Type_v2_7"); // Auxiliaries
        console.Input.PushTextWithEnter("AUX_Conventional_CompletedBusType_v2_7"); // Data

        // Electric system
        console.Input.PushTextWithEnter("AUX_Conventional_CompletedBusTypeElectricSystem_v2_7");
        console.Input.PushTextWithEnter("BusAuxElectricSystemLightsGroupLEDLights_v2_7"); // LED lights
        console.Input.PushTextWithEnter("Yes"); // Interiorlights (opt.)
        console.Input.PushTextWithEnter("Yes"); // Dayrunninglights (opt.)
        console.Input.PushTextWithEnter("Yes"); // Positionlights (opt.)
        console.Input.PushTextWithEnter("Yes"); // Brakelights (opt.)
        console.Input.PushTextWithEnter("Yes"); // Headlights (opt.)

        // HVAC
        console.Input.PushTextWithEnter("AUX_Conventional_CompletedBusTypeHVAC_v2_7"); // HVAC
        console.Input.PushTextWithEnter(""); // System configuration (int, opt.)

        // Heat pump type driver compartment
        console.Input.PushTextWithEnter("BusAuxHVACConventionalSequenceGroupHeatPumpTypeDriverCompartment_v2_7"); // Heat pump type driver compartment
        console.Input.PushTextWithEnter("not applicable"); // Cooling
        console.Input.PushTextWithEnter("not applicable"); // Heating

        // Heat pump type passenger compartment
        console.Input.PushTextWithEnter("BusAuxHVACConventionalSequenceGroupHeatPumpTypePassengerCompartment_v2_7"); // Heat pump type passenger compartment
        console.Input.PushTextWithEnter("R 744"); // Cooling
        console.Input.PushTextWithEnter("R 744"); // Heating

        console.Input.PushTextWithEnter("424242"); // Auxiliary heater power (int, opt.)
        console.Input.PushTextWithEnter("Yes"); // Double glazing (opt.)
        console.Input.PushTextWithEnter("Yes"); // Adjustable auxiliary heater (opt.)
        console.Input.PushTextWithEnter("Yes"); // Separate air distribution ducts (opt.)

        console.Input.PushTextWithEnter("a2"); // id

        var inputArgs = new List<string> { "job", "-b", "re-sign", "-i", "vehicle", "-o", outputVehicleFile };

        // Act
        var result = app.Run([.. inputArgs]);

        // Assert
        result.Should().Be(0);

        // Verify report file was created
        var reportFile = Path.ChangeExtension(outputVehicleFile, ".report.txt");
        File.ReadLines(reportFile).Should().HaveCountGreaterThan(2);

        var xmlContent = File.ReadAllText(outputVehicleFile);
        xmlContent.Should().Contain("<Model>TestModel</Model>");
        xmlContent.Should().Contain("<AuxiliaryHeaterPower>424242</AuxiliaryHeaterPower>");
        xmlContent.Should().Contain("id=\"a2\"");
        xmlContent.Should().Contain("<APTEcoRollReleaseLockupClutch>true</APTEcoRollReleaseLockupClutch>");
        // We also have an empty AirDrag
        xmlContent.Should().Contain("xsi:type=\"q1:AirDragModifiedUseStandardValueType\"");
    }
}