using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VectoInputManager.Core;

/// <summary>
/// Input parameters for generating torque converter standard values (rated engine speed and maximum
/// engine torque).
/// </summary>
public class TorqueConverterComponentInput : BaseInput
{
    public override ComponentType Component => ComponentType.TC;

    [Description("Nominal engine speed at maximum power [rpm]")]
    [Range(600, 4000, ErrorMessage = "Rated speed must be between 600 and 4.000 rpm.")]
    public decimal RatedSpeed { get; set; }

    [Description("Maximum engine torque [Nm]")]
    [Range(100, 10000, ErrorMessage = "Maximum torque must be between 100 and 10.000 Nm.")]
    public decimal MaxTorque { get; set; }

    public override void SetExampleValues()
    {
        base.SetExampleValues();
        RatedSpeed = 2200m;
        MaxTorque = 1666m;
    }
}
