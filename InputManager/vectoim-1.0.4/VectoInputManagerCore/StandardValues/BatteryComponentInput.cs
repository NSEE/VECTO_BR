using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using VectoObjects;

namespace VectoInputManager.Core
{
    /// <summary>
    /// Input parameters for generating battery system standard values (cell configuration, nominal
    /// voltage, and chemistry).
    /// </summary>
    public class BatteryComponentInput : BaseInput
    {
        public override ComponentType Component => ComponentType.Battery;

        [Description("Cell capacity (R100) [Ah]")]
        [Range(0.1, 500.0, ErrorMessage = "Cell capacity must be between 0.1 and 500 Ah.")]
        public decimal CellCapacity { get; set; }

        [Description("Number of cells in series (R100)")]
        [Range(1, 500, ErrorMessage = "Number of series cells must be between 1 and 500.")]
        public int CellsInSeries { get; set; }

        [Description("Number of cells in parallel (R100)")]
        [Range(1, 200, ErrorMessage = "Number of parallel cells must be between 1 and 200.")]
        public int CellsInParallel { get; set; }

        [Description("Nominal voltage [V]")]
        [Range(0.5, 5.0, ErrorMessage = "Nominal voltage must be between 0,5 and 5,0 V.")]
        public decimal NominalVoltage { get; set; }

        [Description("Type of the battery")]
        public BatteryTypeType_v2_3 BatteryType { get; set; }

        public override void SetExampleValues()
        {
            base.SetExampleValues();

            CellCapacity = 100;
            CellsInSeries = 15;
            CellsInParallel = 4;
            NominalVoltage = 3.7m;
        }
    }
}
