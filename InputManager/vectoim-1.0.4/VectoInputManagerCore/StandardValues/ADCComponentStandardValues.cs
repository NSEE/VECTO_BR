using System;
using VectoObjects;

namespace VectoInputManager.Core;

/// <summary>
/// Calculates standard values for an Additional Drivetrain Component (ADC), producing a ratio and
/// torque loss map from input grids, and emits an ADCComponentDeclaration.
/// </summary>
public class ADCComponentStandardValues : BaseStandardValues
{
    public override void SetInput(VectoInputDeclaration_v2_2 vInputDeclaration, IComponentDeclaration sv)
    {
        vInputDeclaration.ADC = (ADCComponentDeclarationType_v2_3)sv;
    }

    public override BaseInput CreateInput() => new ADCComponentInput();

    public override IComponentDeclaration Calculate(BaseInput inputData) =>
        Update((ADCComponentInput)inputData);

    private static ADCComponentDeclarationType_v2_3 Update(ADCComponentInput input)
    {
        var declaration = new ADCComponentDeclarationType_v2_3();
        var data = new ADCDataDeclarationType_v2_3();
        declaration.Data = data;

        input.InitDataDeclaration(data);

        data.Ratio = input.Ratio;
        data.CertificationMethod =  AngledriveCertificationOptionType_v1_0.Standard_values;
        
        var speeds = input.SpeedPoints.Distinct().OrderBy(s => s).ToArray();
        var torques = input.TorquePoints.Distinct().OrderBy(t => t).ToArray();

        var tMaxIn = input.TMaxIn;
        decimal t_addx = 0.005m * tMaxIn;

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
