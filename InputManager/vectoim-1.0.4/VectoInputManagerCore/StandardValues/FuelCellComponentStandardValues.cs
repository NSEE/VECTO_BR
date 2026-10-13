using System;
using VectoObjects;

namespace VectoInputManager.Core;

/// <summary>
/// Calculates standard values for a fuel cell system using rated power and standard efficiency
/// curves to generate a power/consumption map. Produces a FuelCellComponentDeclaration.
/// </summary>
public class FuelCellComponentStandardValues : BaseStandardValues
{
    public override void SetInput(VectoInputDeclaration_v2_2 vInputDeclaration, IComponentDeclaration sv)
    {
        vInputDeclaration.FuelCell = (FuelCellComponentDeclarationType_v2_6)sv;
    }

    public override BaseInput CreateInput() => new FuelCellComponentInput();

    public override IComponentDeclaration Calculate(BaseInput inputData) =>
      Update((FuelCellComponentInput)inputData);

    private static FuelCellComponentDeclarationType_v2_6 Update(FuelCellComponentInput input)
    {
        var dt = new FuelCellComponentDeclarationType_v2_6();
        var data = new FuelCellDataDeclarationType_v2_6();
        dt.Data = data;

        input.InitDataDeclaration(data);
        data.CertificationMethod = FuelCellDataDeclarationTypeCertificationMethod_v2_6.Standard_values;

        // Typical values: 50 – 300 kW
        data.FCSRatedPower = input.FCSRatedPower;

        // Hydrogen Net Calorific Value ≈ 120 MJ/kg
        decimal nvc = 120;

        foreach (var map in Data.GetFuelCellEfficiency())
        {
            // Actual electrical power output of the fuel cell, in kilowatts (kW).
            decimal pabs = map.Pnorm * data.FCSRatedPower;

            // Hydrogen mass flow rate in grams per hour (g/h), calculated from power and efficiency.
            decimal massflow = pabs / map.Eta * 100m * 3600m / nvc;

            var item = new FuelCellOutputPowerConsumptionEntryType_v2_6()
            {
                powerOutput = pabs,
                fuelConsumption = massflow
            };

            data.FuelCellPowerOutputConsumptionMap.Add(item);
        }

        return dt;
    }
}
