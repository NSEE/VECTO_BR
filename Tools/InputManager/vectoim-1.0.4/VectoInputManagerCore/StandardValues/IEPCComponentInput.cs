using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VectoObjects;

namespace VectoInputManager.Core;

public class IEPCComponentInput : BaseInput
{
    public override ComponentType Component => ComponentType.IEPC;

    public GearboxInput Gearbox { get; set; } = new GearboxInput();

    public AxleInput? Axle { get; set; }

    [Description("Electric machine type")]
    public ElectricMachineTypeType_v2_3 ElectricMachineType { get; set; }

    [Description("R85 rated power [W]")]
    [Range(1000, 1000000, ErrorMessage = "R85 rated power must be between 1.000 W and 1.000.000 W.")]
    public int R85RatedPower { get; set; }

    [Description("Rotational inertia [kg·m²]")]
    [Range(0.001, 10.0, ErrorMessage = "Rotational inertia must be between 0,001 and 10,0 kg·m².")]
    public decimal RotationalInertia { get; set; }

    [Description("Design type wheel motor")]
    public bool DesignTypeWheelMotor { get; set; }

    public NrOfDesignTypeWheelMotorMeasuredType_v2_6? NrOfDesignTypeWheelMotorMeasured { get; set; }

    [Description("Disengagement clutch")]
    public bool? DisengagementClutch { get; set; }

    // Voltage level parameters

    [Description("Continuous torque [Nm]")]
    [Range(1, 50000, ErrorMessage = "Continuous torque must be between 1 and 50.000 Nm.")]
    public decimal ContinuousTorque { get; set; }

    [Description("Test speed for continuous torque [1/min]")]
    [Range(1, 30000, ErrorMessage = "Test speed for continuous torque must be between 1 and 30.000 1/min.")]
    public decimal TestSpeedContinuousTorque { get; set; }

    [Description("Overload torque [Nm]")]
    [Range(1, 50000, ErrorMessage = "Overload torque must be between 1 and 50.000 Nm.")]
    public decimal OverloadTorque { get; set; }

    [Description("Test speed for overload torque [1/min]")]
    [Range(1, 30000, ErrorMessage = "Test speed for overload torque must be between 1 and 30.000 1/min.")]
    public decimal TestSpeedOverloadTorque { get; set; }

    [Description("Overload duration [s]")]
    [Range(1, 300, ErrorMessage = "Overload duration must be between 1 and 300 s.")]
    public decimal OverloadDuration { get; set; }

    [Description("Speed setpoints [rpm]")]
    public decimal[] SpeedSetpoints { get; set; } = [];

    [Description("Torque setpoints [Nm]")]
    public decimal[] TorqueSetpoints { get; set; } = [];

    public List<IEPCGearInput> Gears { get; set; } = [];

    [Description("Maximum torque curve entries")]
    [MinLength(2, ErrorMessage = "At least two max torque curve points are required.")]
    public List<MaxTorqueCurveEntry> MaxTorqueCurve { get; set; } = [];

    public override void SetExampleValues()
    {
        base.SetExampleValues();

        SpeedSetpoints = [0, 1000];
        TorqueSetpoints = [400, 800];

        Gearbox = new GearboxInput
        {
            Tmaxin = 10000,
            DirectGear = true,
            MoreThan2FrictionClutches = true,
            ExternalExternalGearMeshes = 1,
            ExternalInternalGearMeshes = 0,
            AngleDriveGearMeshes = 0
        };

        Axle = new AxleInput
        {
            Ratio = 3.5m,
            GearMeshType = AxleGearMeshType.Bevel,
            ExternalExternalGearMeshes = 1,
            ExternalInternalGearMeshes = 0,
            AngleDriveGearMeshes = 0,
            BevelGearMeshes = 0
        };

        Gears =
        [
            new IEPCGearInput {
                Number = 1,
                Ratio = 21.938m,
                MaxOutShaftSpeed = 600,
                MaxOutShaftTorque = 45000
            },
            new IEPCGearInput {
                Number = 2,
                Ratio = 12.276m,
                MaxOutShaftSpeed = 1100,
                MaxOutShaftTorque = 25000
            },
            new IEPCGearInput {
                Number = 3,
                Ratio = 7.234m,
                MaxOutShaftSpeed = 1800,
                MaxOutShaftTorque = 15000
            },
            new IEPCGearInput {
                Number = 4,
                Ratio = 3m,
                MaxOutShaftSpeed = 4400,
                MaxOutShaftTorque = 6000
            },
        ];

        ElectricMachineType = ElectricMachineTypeType_v2_3.PSM;
        R85RatedPower = 120000;
        RotationalInertia = 0.12m;
        DesignTypeWheelMotor = false;
        NrOfDesignTypeWheelMotorMeasured = null;
        DisengagementClutch = true;
        ContinuousTorque = 1273.00m;
        TestSpeedContinuousTorque = 2100.00m;
        OverloadTorque = 1885.13m;
        TestSpeedOverloadTorque = 1793.75m;
        OverloadDuration = 7.50m;

        MaxTorqueCurve =
        [
            new MaxTorqueCurveEntry { OutShaftSpeed = 0, MaxTorque = 2237, MinTorque = -2237 },
            new MaxTorqueCurveEntry { OutShaftSpeed = 300, MaxTorque = 2237, MinTorque = -2237 },
            new MaxTorqueCurveEntry { OutShaftSpeed = 500, MaxTorque = 2237, MinTorque = -2237 },
            new MaxTorqueCurveEntry { OutShaftSpeed = 700, MaxTorque = 2237, MinTorque = -2237 },
            new MaxTorqueCurveEntry { OutShaftSpeed = 900, MaxTorque = 2237, MinTorque = -2237 },
            new MaxTorqueCurveEntry { OutShaftSpeed = 1100, MaxTorque = 2237, MinTorque = -2237 },
            new MaxTorqueCurveEntry { OutShaftSpeed = 1300, MaxTorque = 2237, MinTorque = -2237 },
            new MaxTorqueCurveEntry { OutShaftSpeed = 1400, MaxTorque = 2237, MinTorque = -2237 },
            new MaxTorqueCurveEntry { OutShaftSpeed = 1500, MaxTorque = 2237, MinTorque = -2237 },
            new MaxTorqueCurveEntry { OutShaftSpeed = 1600, MaxTorque = 2094, MinTorque = -2094 },
            new MaxTorqueCurveEntry { OutShaftSpeed = 1700, MaxTorque = 1964, MinTorque = -1964 },
            new MaxTorqueCurveEntry { OutShaftSpeed = 1800, MaxTorque = 1855, MinTorque = -1855 },
            new MaxTorqueCurveEntry { OutShaftSpeed = 2000, MaxTorque = 1630, MinTorque = -1630 },
            new MaxTorqueCurveEntry { OutShaftSpeed = 2100, MaxTorque = 1583, MinTorque = -1583 },
            new MaxTorqueCurveEntry { OutShaftSpeed = 2250, MaxTorque = 1481, MinTorque = -1481 },
        ];
    }
}

public class IEPCGearInput
{
    [Description("Gear number")]
    public int Number { get; set; }

    [Description("Gear ratio")]
    public decimal Ratio { get; set; }

    public int? MaxOutShaftSpeed { get; set; }
    public int? MaxOutShaftTorque { get; set; }
}

/// <summary>
/// Gearbox torque loss calculation inputs.
/// </summary>
public class GearboxInput
{
    [Description("Maximum allowed input torque [Nm]")]
    [Range(0, 100_000_000, ErrorMessage = "Maximum allowed input torque must be greater than 0 Nm.")]
    public decimal Tmaxin { get; set; }

    [Description("Direct gear flag (true only if ratio is 1)")]
    public bool DirectGear { get; set; }

    [Description("Contains more than 2 friction clutches")]
    public bool MoreThan2FrictionClutches { get; set; }

    [Description("Number of external-external gear meshes")]
    [Range(0, 10, ErrorMessage = "Number of external-external gear meshes must be between 0 and 10.")]
    public int ExternalExternalGearMeshes { get; set; }

    [Description("Number of external-internal gear meshes")]
    [Range(0, 10, ErrorMessage = "Number of external-internal gear meshes must be between 0 and 10.")]
    public int ExternalInternalGearMeshes { get; set; }

    [Description("Number of angle drive gear meshes")]
    [Range(0, 10, ErrorMessage = "Number of angle drive gear meshes must be between 0 and 10.")]
    public int AngleDriveGearMeshes { get; set; }
}
public enum AxleGearMeshType
{
    [Description("Bevel gear mesh")]
    Bevel,

    [Description("Spur gear mesh or planetary gearset")]
    SpurPlanetary
}

/// <summary>
/// Axle torque loss calculation inputs.
/// </summary>
public class AxleInput
{
    [Description("Axle gear ratio")]
    [Range(0.1, 100.0, ErrorMessage = "Axle gear ratio must be between 0,1 and 100,0.")]
    public decimal Ratio { get; set; }

    [Description("Axle gear mesh type (only one can be selected)")]
    public AxleGearMeshType GearMeshType { get; set; }

    [Description("Number of external-external gear meshes")]
    [Range(0, 10, ErrorMessage = "Number of external-external gear meshes must be between 0 and 10.")]
    public int ExternalExternalGearMeshes { get; set; }

    [Description("Number of external-internal gear meshes")]
    [Range(0, 10, ErrorMessage = "Number of external-internal gear meshes must be between 0 and 10.")]
    public int ExternalInternalGearMeshes { get; set; }

    [Description("Number of angle drive gear meshes")]
    [Range(0, 10, ErrorMessage = "Number of angle drive gear meshes must be between 0 and 10.")]
    public int AngleDriveGearMeshes { get; set; }

    [Description("Number of bevel gear meshes")]
    [Range(0, 10, ErrorMessage = "Number of bevel gear meshes must be between 0 and 10.")]
    public int BevelGearMeshes { get; set; }
}


/*

public class MaxMinTorquePoint
{
    [Description("Output shaft speed [rpm]")]
    [Range(0, 20000, ErrorMessage = "Output shaft speed must be between 0 and 20.000 rpm.")]
    public decimal OutputShaftSpeed { get; set; }

    [Description("Maximum torque [Nm]")]
    [Range(0, 20000, ErrorMessage = "Maximum torque must be between 0 and 20.000 Nm.")]
    public decimal MaxTorque { get; set; }

    [Description("Minimum torque [Nm]")]
    [Range(-20000, 0, ErrorMessage = "Minimum torque must be between -20.000 and 0 Nm.")]
    public decimal MinTorque { get; set; }         
    
    public MaxMinTorquePoint()
    {
    }

    public MaxMinTorquePoint(decimal outputShaftSpeed, decimal maxTorque, decimal minTorque)
    {
        OutputShaftSpeed = outputShaftSpeed;
        MaxTorque = maxTorque;
        MinTorque = minTorque;
    }
}

public class ElectricPowerPoint
{
    [Description("Output shaft speed [rpm]")]
    [Range(0, 20000, ErrorMessage = "Output shaft speed must be between 0 and 20.000 rpm.")]
    public decimal OutputShaftSpeed { get; set; }

    [Description("Torque [Nm]")]
    [Range(-20000, 20000, ErrorMessage = "Torque must be between -20.000 and 20.000 Nm.")]
    public decimal Torque { get; set; }

    [Description("Electric power [W]")]
    [Range(-1_000_000, 1_000_000, ErrorMessage = "Electric power must be between -1.000.000 and 1.000.000 W.")]
    public decimal ElectricPower { get; set; }

    public ElectricPowerPoint()
    {
    }

    public ElectricPowerPoint(decimal outputShaftSpeed, decimal torque, decimal electricPower)
    {
        OutputShaftSpeed = outputShaftSpeed;
        Torque = torque;
        ElectricPower = electricPower;
    }
}

public class DragTorquePoint
{
    [Description("Speed [rpm]")]
    [Range(0, 20000, ErrorMessage = "Speed must be between 0 and 20.000 rpm.")]
    public decimal Speed { get; set; }

    [Description("Drag torque [Nm]")]
    [Range(0, 1000, ErrorMessage = "Drag torque must be between 0,0 and 1.000,0 Nm.")]
    public decimal Torque { get; set; }

    public DragTorquePoint()
    {
    }

    public DragTorquePoint(decimal speed, decimal torque)
    {
        Speed = speed;
        Torque = torque;
    }
}
*/