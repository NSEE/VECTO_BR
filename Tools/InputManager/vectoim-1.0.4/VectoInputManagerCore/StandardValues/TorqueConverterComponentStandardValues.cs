using System;
using VectoObjects;

namespace VectoInputManager.Core;

/// <summary>
/// Calculates standard torque converter characteristics (speed/torque ratio curves) from rated
/// speed and torque inputs. Produces a TorqueConverterComponentDeclaration.
/// </summary>
public class TorqueConverterComponentStandardValues : BaseStandardValues
{
    public override void SetInput(VectoInputDeclaration_v2_2 vInputDeclaration, IComponentDeclaration sv)
    {
        vInputDeclaration.TorqueConverter = (TorqueConverterComponentDeclarationType_v2_0)sv;
    }

    public override BaseInput CreateInput() => new TorqueConverterComponentInput();

    public override IComponentDeclaration Calculate(BaseInput inputData) =>
        Update((TorqueConverterComponentInput)inputData);

    private static TorqueConverterComponentDeclarationType_v2_0 Update(TorqueConverterComponentInput input)
    {
        var declaration = new TorqueConverterComponentDeclarationType_v2_0();
        var data = new TorqueConverterDataDeclarationType_v2_0();
        declaration.Data = data;
        input.InitDataDeclaration(data);
        data.CertificationMethod = TorqueConverterCertificationOptionType_v1_0.Standard_values;

        // calculations
        decimal nRated = input.RatedSpeed;
        decimal tMax = input.MaxTorque;

        decimal factor = tMax * 0.8m * (1000m / 0.7m / nRated) * (1000m / 0.7m / nRated);

        decimal[] inputTorqueRefs = [factor, factor * 0.8m, factor * 0.5m, 0];
        decimal[] speedRatios = [0m, 0.6m, 0.9m, 1.0m];
        decimal[] torqueRatios = [1.8m, 1.23m, 0.95m, 0.95m];

        for (int i = 0; i < 4; i++)
        {
            data.Characteristics.Add(new TorqueConverterCharacteristicsEntryType_v1_0()
            {
                inputTorqueRef = inputTorqueRefs[i],
                speedRatio = speedRatios[i],
                torqueRatio = torqueRatios[i],
            });
        }

        return declaration;
    }
}
