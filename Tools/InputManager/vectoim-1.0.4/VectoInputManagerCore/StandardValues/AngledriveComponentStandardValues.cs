using System;
using VectoObjects;

namespace VectoInputManager.Core;

// ANNEX VI - Appendix 11 - p266

/// <summary>
/// Calculates standard values for an angle drive from ratio, input torque and speed/torque grids,
/// producing an AngledriveComponentDeclaration and loss map.
/// </summary>
public class AngledriveComponentStandardValues : BaseStandardValues
{
    public override void SetInput(VectoInputDeclaration_v2_2 vInputDeclaration, IComponentDeclaration sv)
    {
        vInputDeclaration.Angledrive = (AngledriveComponentDeclarationType_v2_0)sv;
    }

    public override BaseInput CreateInput() => new AngledriveComponentInput();

    public override IComponentDeclaration Calculate(BaseInput inputData) =>
        Update((AngledriveComponentInput)inputData);

    private static AngledriveComponentDeclarationType_v2_0 Update(AngledriveComponentInput input)
    {
        var declaration = new AngledriveComponentDeclarationType_v2_0();
        var data = new AngledriveDataDeclarationType_v2_0();
        input.InitDataDeclaration(data);

        data.CertificationMethod = AngledriveCertificationOptionType_v1_0.Standard_values;
        data.Ratio = input.Ratio;

        var tMaxIn = input.TMaxIn;
        decimal t_addx = 0.005m * tMaxIn;

        // Build TorqueLossMap from provided speed & torque grids
        var speeds = input.SpeedPoints.Distinct().OrderBy(s => s).ToArray();
        var torques = input.TorquePoints.Distinct().OrderBy(t => t).ToArray();

        foreach (var t in torques)
        {
            foreach (var n in speeds)
            {
                var loss = TorqueLoss(n, t);
                var entry = new AngledriveLossMapEntryType_v2_0
                {
                    inputSpeed = n,
                    inputTorque = t,
                    torqueLoss = loss
                };
                data.TorqueLossMap.Add(entry);
            }
        }

        declaration.Data = data;
        return declaration;

        // Helper to compute loss:
        decimal TorqueLoss(decimal speedRpm, decimal torqueNm)
        {
            // Using |torque| to make symmetric 
            return t_addx
                 + t_addx * (speedRpm / 1000m)
                 + 0.04m * Math.Abs(torqueNm);
        }
    }
}
