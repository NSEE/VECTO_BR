using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VectoInputManager.Core
{
    /// <summary>
    /// Input parameters for generating capacitor system standard values (max voltage, capacitance,
    /// and series cell count).
    /// </summary>
    public class CapacitorComponentInput : BaseInput
    {
        public override ComponentType Component => ComponentType.Capacitor;

        [Description("Maximum voltage [V]")]
        [Range(0.5, 5.0, ErrorMessage = "Maximum voltage must be between 0,5 and 5,0 V.")]
        public decimal MaxVoltage { get; set; }

        [Description("Capacitance [F]")]
        [Range(1, 10000, ErrorMessage = "Capacitance must be between 1 and 10000 F.")]
        public decimal Capacitance { get; set; }

        [Description("Number of cells in series")]
        [Range(1, 500, ErrorMessage = "Number of cells in series must be between 1 and 500.")]
        public int CellNumberInSeries { get; set; }

        public override void SetExampleValues()
        {
            base.SetExampleValues();
            MaxVoltage = 2.7m;
            Capacitance = 3000m;
            CellNumberInSeries = 1;
        }
    }
}
