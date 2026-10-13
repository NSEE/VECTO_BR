using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VectoObjects;

namespace VectoInputManager.Core
{
    /// <summary>
    /// Input parameters for generating axle (axlegear) standard values, including ratio, line type,
    /// and speed/torque grid for loss mapping.
    /// </summary>
    public class AxleComponentInput : BaseInput
    {
        public override ComponentType Component => ComponentType.Axle;

        [Description("Ratio")]
        [Range(0.5, 30.0, ErrorMessage = "Ratio must be between 0,5 and 30,0.")]
        public decimal Ratio { get; set; }

        [Description("Line Type")]
        public AxlegearLineTypeType_v1_0 LineType { get; set; }

        [Description("List of speed setpoints [rpm]")]
        [MinLength(2, ErrorMessage = "At least two speed points are required.")]
        public decimal[] SpeedPoints { get; set; } = [];

        [Description("List of torque setpoints [Nm]")]
        [MinLength(2, ErrorMessage = "At least two torque points are required.")]
        public decimal[] TorquePoints { get; set; } = [];

        public override void SetExampleValues()
        {
            base.SetExampleValues();
            Manufacturer = "Generic Gearbox Manufacturer";
            Model = "Generic primary bus 41 AxleGear";
            CertificationNumber = "e12*0800/8000*201-/05E0000*00";
            Ratio = 4m;
            LineType = AxlegearLineTypeType_v1_0.Single_reduction_axle;

            SpeedPoints = [0.00m, 325.00m, 650.00m, 975.00m, 1300.00m, 1625.00m, 1950.00m, 2275.00m, 2600.00m, 2925.00m, 3250.00m, 3750.00m];

            // Should only be positive values
            TorquePoints = [
                31.96m, 70.42m, 110.07m, 149.72m, 189.37m, 268.68m, 347.98m, 506.58m, 665.19m, 823.79m, 982.39m, 1141.00m, 1299.60m, 1458.21m,
                1616.81m, 1934.02m, 2251.23m, 2568.44m, 2885.65m, 3202.85m, 3520.06m, 3837.27m, 4154.48m, 4471.69m, 4788.90m, 5106.11m,
                5423.31m, 5740.52m, 6057.73m, 6374.94m, 6692.15m, 7009.36m, 7326.57m, 7643.77m, 7960.98m];
        }
    }

}
