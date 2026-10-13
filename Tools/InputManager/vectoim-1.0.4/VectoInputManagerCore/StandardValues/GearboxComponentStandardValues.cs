using System;
using VectoObjects;

namespace VectoInputManager.Core;

/// <summary>
/// Calculates standard values for a gearbox based on input parameters and Annex VI guidance.
/// Produces a GearboxComponentDeclaration and sets it into the VectoInputDeclaration container.
/// </summary>
public class GearboxComponentStandardValues : BaseStandardValues
{
    public override void SetInput(VectoInputDeclaration_v2_2 vInputDeclaration, IComponentDeclaration sv)
    {
        vInputDeclaration.Gearbox = (GearboxComponentDeclarationType_v2_0)sv;
    }

    public override BaseInput CreateInput() => new GearboxComponentInput();

    public override IComponentDeclaration Calculate(BaseInput inputData) =>
        Update((GearboxComponentInput)inputData);

    private static GearboxComponentDeclarationType_v2_0 Update(GearboxComponentInput input)
    {
        var declaration = new GearboxComponentDeclarationType_v2_0
        {
            Data = new GearboxDataDeclarationType_v2_3
            {
                TransmissionType = input.TransmissionType,
                Gears = new GearsDeclarationType_v2_0()
            }
        };

        /*
        var data2 = new GearboxFWDDataDeclarationType_v2_3
        {
            TransmissionType = GearboxTransmissionTypeType_v1_0.APT_P,
            AxlegearRatio = 6.0m,
            DifferentialIncluded = true,
            Gears = new GearsDeclarationType_v2_0(),
            MainCertificationMethod = GearboxCertificationOptionType_v1_0.Standard_values,
        };
        */

        var data = (GearboxDataDeclarationType_v2_3)declaration.Data;
        data.MainCertificationMethod = GearboxCertificationOptionType_v1_0.Standard_values;

        input.InitDataDeclaration(data);

        // First category (inherited from AbstractAMTGearbox)
        bool isAMT = input.TransmissionType == GearboxTransmissionTypeType_v2_3.SMT
            || input.TransmissionType == GearboxTransmissionTypeType_v2_3.AMT
            || input.TransmissionType == GearboxTransmissionTypeType_v2_3.APT_N;

        // Second category (higher losses):
        // GearboxTransmissionTypeType_v2_3.APT_S
        // GearboxTransmissionTypeType_v2_3.APT_P;

        if (input.TransmissionType == GearboxTransmissionTypeType_v2_3.IHPC_Type_1)
            throw new NotImplementedException("Standard values for IHPC Type 1 not supported.");

        var p = isAMT ? 0.005m
            : 0.015m;

        var gears = input.Gear;
        var dGears = (GearsDeclarationType_v2_0)data.Gears;

        var speeds = input.SpeedPoints.Distinct().OrderBy(s => s).ToArray();
        var torques = input.TorquePoints.Distinct().OrderBy(t => t).ToArray();

        foreach (var gearInput in gears)
        {
            var gear = new GearDeclarationType_v2_0
            {
                number = gearInput.Number,
                Ratio = gearInput.Ratio,
                MaxTorque = null,
                MaxSpeed = null,
            };
            dGears.Gear.Add(gear);

            gear.TorqueLossMap = new GearDeclarationTypeTorqueLossMap_v2_0();
            var entries = gear.TorqueLossMap.Entry;

            foreach (var tlEntry in torques)
            {
                foreach (var n in speeds)
                {
                    var loss = TorqueLoss(n, tlEntry, gearInput);
                    var entry = new TorqueLossMapEntryType_v2_0
                    {
                        inputSpeed = n,
                        inputTorque = tlEntry,
                        torqueLoss = loss,
                    };
                    entries.Add(entry);
                }
            }
        }

        return declaration;

        decimal TorqueLoss(decimal n_in, decimal t_in, GearComponent g)
        {
            bool directGear = g.DirectGear;
            bool containsAngleDrive = input.ContainsAngleDrive;

            decimal ft = directGear && containsAngleDrive ? 0.05m
                : !directGear && containsAngleDrive ? 0.08m
                : directGear && !containsAngleDrive ? 0.01m
                : !directGear && !containsAngleDrive ? 0.04m
                : throw new InvalidOperationException();

            var t_addx = containsAngleDrive
                ? p * input.TMaxIn + 0.005m * input.TMaxIn
                : p * input.TMaxIn;

            var t_in_abs = Math.Abs(t_in);

            return t_addx + t_addx * (t_in_abs / 1000) + ft * t_in_abs;
        }
    }
}
