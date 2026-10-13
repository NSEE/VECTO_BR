using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VectoInputManager.Core
{
    /// <summary>
    /// Input parameters for generating retarder standard values (retarder type and mechanical
    /// step-up ratio).
    /// </summary>
    public class RetarderComponentInput : BaseInput
    {
        public override ComponentType Component => ComponentType.Retarder;

        [Description("Type of retarder")]
        public RetarderType Type { get; set; }

        [Description("Step-up ratio")]
        [Range(1.0, 6.0, ErrorMessage = "Step-up ratio must be between 1,0 and 6,0.")]
        public decimal StepUpRatio { get; set; }

        public override void SetExampleValues()
        {
            base.SetExampleValues();

            Type = RetarderType.Hydrodynamic;
            StepUpRatio = 1.5m;
        }
    }

    public enum RetarderType
    {
        HydrodynamicWithVehicleLaunchFunctionality,
        Hydrodynamic,
        Magnetic
    }
}
