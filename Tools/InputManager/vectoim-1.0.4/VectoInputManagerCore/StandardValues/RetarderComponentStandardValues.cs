using System;
using VectoObjects;

namespace VectoInputManager.Core;

/// <summary>
/// Calculates standard values for a retarder (hydrodynamic/magnetic) and generates a drag torque
/// loss curve based on step-up ratio and speed. Produces a RetarderComponentDeclaration.
/// </summary>
public class RetarderComponentStandardValues : BaseStandardValues
{
    public override void SetInput(VectoInputDeclaration_v2_2 vInputDeclaration, IComponentDeclaration sv)
    {
        vInputDeclaration.Retarder = (RetarderComponentDeclarationType_v2_0)sv;
    }

    public override BaseInput CreateInput() => new RetarderComponentInput();

    public override IComponentDeclaration Calculate(BaseInput inputData) =>
        Update((RetarderComponentInput)inputData);

    private static RetarderComponentDeclarationType_v2_0 Update(RetarderComponentInput input)
    {
        var declaration = new RetarderComponentDeclarationType_v2_0();
        var data = new RetarderDataDeclarationType_v2_0();
        declaration.Data = data;

        input.InitDataDeclaration(data);
        data.CertificationMethod = RetarderCertificationOptionType_v1_0.Standard_values;

        decimal k1, k2;
        switch (input.Type)
        {
            case RetarderType.HydrodynamicWithVehicleLaunchFunctionality:
                k1 = 20m;
                k2 = 4m;
                break;
            case RetarderType.Hydrodynamic:
                k1 = 10m;
                k2 = 2m;
                break;
            case RetarderType.Magnetic:
                k1 = 12m;
                k2 = 5m;
                break;
            default:
                throw new NotSupportedException("Unsupported retarder type");
        }

        // Retarder loss map speeds
        var speeds = new[] { 200, 400, 600, 900, 1200, 1600, 2000, 2500, 3000, 3500, 4000, 4500, 5000 };

        foreach (var speed in speeds)
        {
            var i_stepup = input.StepUpRatio;
            var speedFactor = (decimal)Math.Pow(speed / 1000.0, 2);

            var entry = new RetarderLossmapEntryType_v2_0()
            {
                retarderSpeed = speed,
                torqueLoss = (k1 / i_stepup)
                    + (k2 / (decimal)Math.Pow((double)i_stepup, 3)) * speedFactor
            };
            data.RetarderLossMap.Add(entry);
        }

        return declaration;
    }

}
