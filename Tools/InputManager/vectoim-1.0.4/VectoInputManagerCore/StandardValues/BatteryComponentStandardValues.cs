using System;
using Fasterflect;
using VectoObjects;

namespace VectoInputManager.Core;

/// <summary>
/// Calculates standard values for a battery system (OCV curve, internal resistance, current limits)
/// from high-level inputs, producing a BatteryComponentDeclaration.
/// </summary>
public class BatteryComponentStandardValues : BaseStandardValues
{
    public override void SetInput(VectoInputDeclaration_v2_2 vInputDeclaration, IComponentDeclaration sv)
    {
        vInputDeclaration.BatterySystem = (BatteryComponentDeclarationType_v2_3)sv;
    }

    public override BaseInput CreateInput() => new BatteryComponentInput();

    public override IComponentDeclaration Calculate(BaseInput inputData) =>
        Update((BatteryComponentInput)inputData);

    private static BatteryComponentDeclarationType_v2_3 Update(BatteryComponentInput input)
    {
        var declaration = new BatteryComponentDeclarationType_v2_3();
        var data = new BatterySystemStandardValuesDataType_v2_6();
        declaration.Data = data;

        input.InitDataDeclaration(data);
        data.CertificationMethod = BatterySystemStandardValuesDataTypeCertificationMethod_v2_6.Standard_values;

        var totalCapacity = input.CellCapacity * input.CellsInParallel;
        data.BatteryType = input.BatteryType;
        data.RatedCapacity = totalCapacity * 0.9m;

        // OCV

        foreach (var item in Data.GetOCV())
        {
            data.OCV.Add(new BatterySystemOCVEntryType_v2_3()
            {
                SoC = item.SOC_Percent,
                OCV = item.Factor * input.NominalVoltage
            });
        }

        // Internal Resistance

        var brDic = Data.GetBatteryResistance()
            .Where(c => c.BatteryType == input.BatteryType)
            .ToDictionary(c => c.DCIR, StringComparer.OrdinalIgnoreCase);

        var res = new BatterySystemInternalResistanceEntryType_v2_3()
        {
            SoC = 0,
            R_2 = brDic["Ri2"].SpecificResistance /
                data.RatedCapacity * input.CellsInSeries,

            R_10 = brDic["Ri10"].SpecificResistance /
                data.RatedCapacity * input.CellsInSeries,

            R_20 = brDic["Ri20"].SpecificResistance /
                data.RatedCapacity * input.CellsInSeries
        };

        if (brDic.TryGetValue("Ri120", out var ri120))
        {
            res.R_120 = brDic["Ri120"].SpecificResistance /
                data.RatedCapacity * input.CellsInSeries;
        }
        else
            res.R_120 = null;

        data.InternalResistance.Add(res);

        var res100 = res.DeepClone();
        res100.SoC = 100;
        data.InternalResistance.Add(res100);

        // Current Limits

        foreach (var soc in Data.GetCurrentLimits()
            .Where(c => c.BatteryType == input.BatteryType))
        {
            data.CurrentLimits.Add(new BatterySystemCurrentLimitsEntryType_v2_3()
            {
                SoC = soc.SOC_Percent,
                maxChargingCurrent = soc.MaxChargingCRate * data.RatedCapacity,
                maxDischargingCurrent = soc.MaxDischargingCRate * data.RatedCapacity,
            });
        }

        return declaration;
    }

}
