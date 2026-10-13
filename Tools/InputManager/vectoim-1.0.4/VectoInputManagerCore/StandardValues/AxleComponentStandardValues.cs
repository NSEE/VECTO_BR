using System;
using System.Linq;
using VectoObjects;

namespace VectoInputManager.Core;

// ANNEX VII - Appendix 3 - p291
/// <summary>
/// Calculates standard values for an axle (axlegear), deriving a torque loss map from ratio, line
/// type, and speed/torque grids. Produces an AxlegearComponentDeclaration.
/// </summary>
public class AxleComponentStandardValues : BaseStandardValues
{
    public override void SetInput(VectoInputDeclaration_v2_2 vInputDeclaration, IComponentDeclaration sv)
    {
        vInputDeclaration.Axlegear = (AxlegearComponentDeclarationType_v2_0)sv;
    }

    public override BaseInput CreateInput() => new AxleComponentInput();

    public override IComponentDeclaration Calculate(BaseInput inputData) =>
        Update((AxleComponentInput)inputData);

    private static AxlegearComponentDeclarationType_v2_0 Update(AxleComponentInput input)
    {
        var declaration = new AxlegearComponentDeclarationType_v2_0();
        var data = new AxlegearDataDeclarationType_v2_0();
        declaration.Data = data;
        input.InitDataDeclaration(data);

        data.CertificationMethod = AxlegearCertificationOptionType_v1_0.Standard_values;
        data.Ratio = input.Ratio;
        data.LineType = input.LineType;

        // Get generic efficiency and drag parameters from Table 1
        var (eta, T0, T1) = GetLineTypeParams(input.LineType);

        var igear = input.Ratio;
        var speeds = input.SpeedPoints.Distinct().OrderBy(s => s).ToArray();
        var torques = input.TorquePoints.Distinct().OrderBy(t => t).ToArray();

        foreach (var speed in speeds)
        {
            foreach (var Tout in torques)
            {
                // The basic drag torque (wheel side)
                var Td0 = T0 + T1 * igear;

                // The standard torque loss on the input side of the axle
                var Tloss = (Td0 + (Tout / eta) - Tout) / igear;
                
                // The corresponding torque (at input side)
                var Tin = Tout / igear + Tloss;
                // Tin = Math.Abs(Tin);

                var entry = new AxlegearTorqueLossMapEntryType_v1_0
                {
                    inputSpeed = speed,
                    inputTorque = Tin,
                    torqueLoss = Tloss,
                };
                data.TorqueLossMap.Add(entry);
                
                entry = new AxlegearTorqueLossMapEntryType_v1_0
                {
                    inputSpeed = speed,
                    inputTorque = -Tin,
                    torqueLoss = Tloss,
                };
                data.TorqueLossMap.Add(entry);
            }
        }

        declaration.Data = data;
        return declaration;
    }

    // Generic efficiency and drag loss
    static (decimal eta, decimal T0, decimal T1)
        GetLineTypeParams(AxlegearLineTypeType_v1_0 lineType) => lineType switch
        {
            AxlegearLineTypeType_v1_0.Single_reduction_axle =>
                (0.98m, 70m, 20m),  // SR
            AxlegearLineTypeType_v1_0.Single_reduction_tandem_axle =>
                (0.96m, 80m, 20m),  // SRT
            AxlegearLineTypeType_v1_0.Single_portal_axle =>
                (0.96m, 80m, 20m),  // SP (like SRT)
            AxlegearLineTypeType_v1_0.Hub_reduction_axle =>
                (0.97m, 70m, 20m),  // HR
            AxlegearLineTypeType_v1_0.Hub_reduction_tandem_axle =>
                (0.95m, 90m, 20m),  // HRT
            _ =>
                (0.90m, 150m, 50m), // All other axle technologies
        };
}
