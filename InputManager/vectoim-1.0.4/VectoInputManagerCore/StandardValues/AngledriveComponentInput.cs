using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VectoInputManager.Core
{
    /// <summary>
    /// Input parameters for generating angle drive standard values (ratio, max input torque,
    /// and speed/torque grid for loss mapping).
    /// </summary>
    public class AngledriveComponentInput : BaseInput
    {
        public override ComponentType Component => ComponentType.Angledrive;

        [Description("Maximum allowed input torque [Nm]")]
        [Range(0, 200000, ErrorMessage = "Maximum input torque must be between 0 and 200.000 Nm.")]
        public decimal TMaxIn { get; set; }

        [Description("Ratio")]
        [Range(0.1, 20.0, ErrorMessage = "Ratio must be between 0,1 and 20,0.")]
        public decimal Ratio { get; set; }

        [Description("List of speed setpoints [rpm]")]
        [MinLength(2, ErrorMessage = "At least two speed points are required.")]
        public decimal[] SpeedPoints { get; set; } = [];

        [Description("List of torque setpoints [Nm]")]
        [MinLength(2, ErrorMessage = "At least two torque points are required.")]
        public decimal[] TorquePoints { get; set; } = [];

        public override void SetExampleValues()
        {
            base.SetExampleValues();
            Ratio = 2.345m;
            TMaxIn = 6000;
            SpeedPoints = [0, 1000];
            TorquePoints = [-10000, -1000, 1000, 0];
        }
    }
}
