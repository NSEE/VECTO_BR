using System;
using VectoObjects;

namespace VectoInputManager.Core;

/// <summary>
/// Calculates standard values for a capacitor system (capacitance, internal resistance, voltage
/// limits, and current limits) and produces a CapacitorSystem declaration.
/// </summary>
public class CapacitorComponentStandardValues : BaseStandardValues
{
    public override void SetInput(VectoInputDeclaration_v2_2 vInputDeclaration, IComponentDeclaration sv)
    {
        vInputDeclaration.CapacitorSystem = (CapacitorSystemComponentDeclarationType_v2_3)sv;
    }

    public override BaseInput CreateInput() => new CapacitorComponentInput();

    public override IComponentDeclaration Calculate(BaseInput inputData) =>
        Update((CapacitorComponentInput)inputData);

    // Reference resistance [Ohm]
    public const decimal R_ref = 0.00375m;
    // Reference voltage [V]
    public const decimal V_ref = 2.7m;
    // Reference capacitance [F]
    public const decimal C_ref = 3000m;

    private static CapacitorSystemComponentDeclarationType_v2_3 Update(CapacitorComponentInput input)
    {
        var declaration = new CapacitorSystemComponentDeclarationType_v2_3();
        var data = new CapacitorSystemDataType_v2_6();
        declaration.Data = data;

        input.InitDataDeclaration(data);
        data.CertificationMethod = CertificationMethodType_v2_3.Standard_values;

        data.Capacitance = input.Capacitance;

        // R_res
        data.InternalResistance = 1000m *
            (R_ref * ((input.MaxVoltage - 0.45m * input.MaxVoltage) / (0.55m * V_ref)) * (C_ref / input.Capacitance) * (1m / input.CellNumberInSeries));

        //V_min
        data.MinVoltage = 0.45m * input.MaxVoltage;

        data.MaxVoltage = input.MaxVoltage;

        // Chrg_max
        data.MaxChargingCurrent = 5m * input.Capacitance;

        data.MaxDischargingCurrent = 5m * input.Capacitance;

        data.TestingTemperature = null;

        return declaration;
    }
}
