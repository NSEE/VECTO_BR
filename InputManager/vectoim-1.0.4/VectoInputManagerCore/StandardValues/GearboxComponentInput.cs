using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VectoObjects;

namespace VectoInputManager.Core
{
    /// <summary>
    /// Input parameters for generating standard values for a gearbox/transmission.
    /// Includes speed/torque grids and per-gear configuration.
    /// </summary>
    public class GearboxComponentInput : BaseInput
    {
        public override ComponentType Component => ComponentType.GearBox;

        [Description("Maximum allowed input torque [Nm]")]
        [Range(100, 50000, ErrorMessage = "Maximum input torque must be between 100 and 50.000 Nm.")]
        public decimal TMaxIn { get; set; }

        [Description("Transmission type")]
        public GearboxTransmissionTypeType_v2_3 TransmissionType { get; set; }

        [Description("Contains angle drive")]
        public bool ContainsAngleDrive { get; set; }

        [Description("List of speed setpoints [rpm]")]
        [MinLength(2, ErrorMessage = "At least two speed points are required.")]
        public decimal[] SpeedPoints { get; set; } = [];

        [Description("List of torque setpoints [Nm]")]
        [MinLength(2, ErrorMessage = "At least two torque points are required.")]
        public decimal[] TorquePoints { get; set; } = [];

        public List<GearComponent> Gear { get; set; } = [];

        public override void SetExampleValues()
        {
            base.SetExampleValues();

            TMaxIn = 10000;
            TransmissionType = GearboxTransmissionTypeType_v2_3.AMT;
            ContainsAngleDrive = true;

            SpeedPoints = [0, 1000];
            TorquePoints = [-10000, -1000, 1000, 0];

            Gear.Add(new GearComponent
            {
                Number = 1,
                Ratio = 4.0m,
                DirectGear = false,
            });
            Gear.Add(new GearComponent
            {
                Number = 2,
                Ratio = 2.0m,
                DirectGear = false,
            });
            Gear.Add(new GearComponent
            {
                Number = 3,
                Ratio = 1.0m,
                DirectGear = true,
            });
        }
    }

    /// <summary>
    /// Represents a single gearbox gear configuration (index, ratio, and whether it is direct).
    /// </summary>
    public class GearComponent
    {
        [Description("Gear number")]
        [Range(1, 20, ErrorMessage = "Gear number must be between 1 and 20.")]
        public int Number { get; set; }

        [Description("Gear ratio")]
        [Range(0.5, 20.0, ErrorMessage = "Gear ratio must be between 0,5 and 20,0.")]
        public decimal Ratio { get; set; }

        [Description("Direct gear")]
        public bool DirectGear { get; set; }
    }
}
