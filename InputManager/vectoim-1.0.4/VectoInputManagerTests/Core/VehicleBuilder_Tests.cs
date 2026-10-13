using System;
using VectoInputManager.Core;
using VectoObjects;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace VectoInputManager.Tests.Core;

public class VehicleBuilder_Tests
{
    [Test]
    public void VehicleBuilder_Vehicle_HEV_IEPC_S_HeavyLorryDeclarationType_v2_7()
    {
        var testing = typeof(Vehicle_HEV_IEPC_S_HeavyLorryDeclarationType_v2_7);

        var promptAnswerSequence = new PromptAnswer[]
        {
            new("@VehicleType", -1, "Vehicle_HEV_IEPC_S_HeavyLorryDeclarationType_v2_7"),
            new("Manufacturer", 0, "test"),
            new("ManufacturerAddress", 0, "test"),
            new("Model", 0, "test"),
            new("VIN", 0, "1"),
            new("Date", 0, new DateTime(2026,1,1)),
            new("SimulationToolLicenseNumber", 0, "LICNAME"),
            new("LegislativeCategory", 0, "N3"),
            new("ChassisConfiguration", 0, "Rigid_Lorry"),
            new("AxleConfiguration", 0, "Item8x2"),
            new("CorrectedActualMass", 0, 20000),
            new("TechnicalPermissibleMaximumLadenMass", 0, 11),
            new("IdlingSpeed", 0, 11),
            new("RetarderType", 0, "Transmission_Input_Retarder"),
            new("RetarderRatio", 0, 11m),
            new("AngledriveType", 0, "Losses_included_in_Gearbox"),
            new("ZeroEmissionVehicle", 0, true),
            new("VocationalVehicle", 0, false),
            new("NgTankSystem", 0, "Compressed"), //Note: NgTankSystemSpecified
            new("SleeperCab", 0, true),
            new("VehicleTypeApprovalNumber", 0, "1"),
            new("ArchitectureID", 0, "S_IEPC"),
            new("OVC", 0, true),
            new("BatteryOnlyMode", 0, true),
            new("DynamicChargingTechnology", 0, "None"),

            new("ADAS", 0, "ADAS_HEV_Type_v2_7"),
              new("ADAS.EngineStopStart", 1, true),
              new("ADAS.PredictiveCruiseControl", 1, "Item1Comma2"),

            new("ElectricMotorTorqueLimits", 0, "ElectricMachineTorqueLimitsType_v2_7"),
              new("ElectricMotorTorqueLimits.ElectricMachine", 1, "ElectricMachineTorqueLimitsTypeElectricMachine_v2_7"),
                new("ElectricMotorTorqueLimits.ElectricMachine.Position", 2, "Item2Period5"),
                new("ElectricMotorTorqueLimits.ElectricMachine.VoltageLevel[0]", 2, "ElectricMachineTorqueLimitsTypeElectricMachineVoltageLevel_v2_7"),
                    new("ElectricMotorTorqueLimits.ElectricMachine.VoltageLevel[0].Voltage", 3, 11),
                    new("ElectricMotorTorqueLimits.ElectricMachine.VoltageLevel[0].MaxTorqueCurve[0]", 3, "ElectricMachineSystemMaxTorqueEntryType_v2_3"),
                      new("ElectricMotorTorqueLimits.ElectricMachine.VoltageLevel[0].MaxTorqueCurve[0].outShaftSpeed", 4, 1m),
                      new("ElectricMotorTorqueLimits.ElectricMachine.VoltageLevel[0].MaxTorqueCurve[0].maxTorque", 4, 1m),
                      new("ElectricMotorTorqueLimits.ElectricMachine.VoltageLevel[0].MaxTorqueCurve[0].minTorque", 4, 1m),
                    new("ElectricMotorTorqueLimits.ElectricMachine.VoltageLevel[0].MaxTorqueCurve[1]", 3, "ElectricMachineSystemMaxTorqueEntryType_v2_3"),
                      new("ElectricMotorTorqueLimits.ElectricMachine.VoltageLevel[0].MaxTorqueCurve[1].outShaftSpeed", 4, 1m),
                      new("ElectricMotorTorqueLimits.ElectricMachine.VoltageLevel[0].MaxTorqueCurve[1].maxTorque", 4, 1m),
                      new("ElectricMotorTorqueLimits.ElectricMachine.VoltageLevel[0].MaxTorqueCurve[1].minTorque", 4, 1m),
                    new("ElectricMotorTorqueLimits.ElectricMachine.VoltageLevel[0].MaxTorqueCurve[2]", 3, "No"),
                new("ElectricMotorTorqueLimits.ElectricMachine.VoltageLevel[1]", 2, "No"),
        };

        var provider = TestUtils.TestServiceProvider();
        var builder = provider.GetRequiredService<VehicleBuilder>();
        int promptIndex = 0;

        builder.PromptHandler = action =>
        {
            if (action.Type == PromptActionType.Enum
                || action.Type == PromptActionType.Class
                || action.Type == PromptActionType.Collection)
                action.Selection.Should().NotBeEmpty($"Selection should not be empty for action: {action.Query}");
            else
                action.Selection.Should().BeNull($"Selection should be empty for action: {action.Query}");

            if (promptIndex >= promptAnswerSequence.Length)
            {
                action.Answer = null;
                return;
            }

            var (expectedQuery, expectedDepth, answer) = promptAnswerSequence[promptIndex];
            action.Property.Should().Be(expectedQuery, "Line: " + promptIndex + " - " + action.Property);
            action.Depth.Should().Be(expectedDepth, "Line: " + promptIndex);
            promptIndex++;

            action.Answer = answer;
        };

        builder.Run();

        // Assertions
        var veh = builder.Object!;
        var (f, v, p) = PathUtils.FindValue(veh, "ADAS.PredictiveCruiseControl");
        v.Should().Be(PredictiveCruiseControlType_v2_1.Item1Comma2);

        (f, v, p) = PathUtils.FindValue(veh, "ElectricMotorTorqueLimits.ElectricMachine.VoltageLevel[0]");
        var t1 = (ElectricMachineTorqueLimitsTypeElectricMachineVoltageLevel_v2_7)v!;
        t1.Voltage.Should().Be(11);

        (f, v, p) = PathUtils.FindValue(veh, "ElectricMotorTorqueLimits.ElectricMachine.VoltageLevel[0].Voltage");
        var t2 = (int)v!;
        t2.Should().Be(11);
    }
}
