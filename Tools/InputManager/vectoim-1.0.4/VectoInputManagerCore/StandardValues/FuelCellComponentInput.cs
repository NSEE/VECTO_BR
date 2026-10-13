using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VectoInputManager.Core
{
    /// <summary>
    /// Input parameters for generating fuel cell standard values (system rated power).
    /// </summary>
    public class FuelCellComponentInput : BaseInput
    {
        public override ComponentType Component => ComponentType.FuelCell;

        [Description("Fuel cell system rated power [kW]")]
        [Range(10, 1000, ErrorMessage = "Rated power must be between 10 and 1000 kW.")]
        public int FCSRatedPower { get; set; }

        public override void SetExampleValues()
        {
            base.SetExampleValues();
            FCSRatedPower = 100;
        }
    }
}
