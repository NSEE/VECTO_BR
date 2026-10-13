using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VectoObjects;

namespace VectoInputManager.Core
{
    /// <summary>
    /// Input data for Electric Machine System (EMS) component standard values calculation.
    /// Based on Appendix 8 of Annex Xb of regulation (EU) 2017/2400.
    /// </summary>
    public class EMSComponentInput : BaseInput
    {
        public override ComponentType Component => ComponentType.EMS;

        [Description("Electric machine type")]
        public ElectricMachineTypeType_v2_3 ElectricMachineType { get; set; }

        [Description("R85 rated power [W]")]
        [Range(1000, 1000000, ErrorMessage = "R85 rated power must be between 1.000 W and 1.000.000 W.")]
        public int R85RatedPower { get; set; }

        [Description("Rotational inertia [kg·m²]")]
        [Range(0.001, 10.0, ErrorMessage = "Rotational inertia must be between 0,001 and 10,0 kg·m².")]
        public decimal RotationalInertia { get; set; }

        [Description("DC-DC converter included")]
        public bool DcDcConverterIncluded { get; set; }

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

        [Description("Maximum torque curve entries")]
        [MinLength(2, ErrorMessage = "At least two max torque curve points are required.")]
        public List<MaxTorqueCurveEntry> MaxTorqueCurve { get; set; } = [];

        public override void SetExampleValues()
        {
            base.SetExampleValues();

            ElectricMachineType = ElectricMachineTypeType_v2_3.PSM;
            R85RatedPower = 50000;
            RotationalInertia = 0.10m;
            DcDcConverterIncluded = true;
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

    /// <summary>
    /// Entry in the maximum torque curve.
    /// </summary>
    public class MaxTorqueCurveEntry
    {
        [Description("Output shaft speed [1/min]")]
        [Range(0, 30000, ErrorMessage = "Output shaft speed must be between 0 and 30.000 1/min.")]
        public decimal OutShaftSpeed { get; set; }

        [Description("Maximum torque [Nm]")]
        [Range(-50000, 50000, ErrorMessage = "Maximum torque must be between -50.000 and 50.000 Nm.")]
        public decimal MaxTorque { get; set; }

        [Description("Minimum torque [Nm]")]
        [Range(-50000, 50000, ErrorMessage = "Minimum torque must be between -50.000 and 50.000 Nm.")]
        public decimal MinTorque { get; set; }
    }
}
