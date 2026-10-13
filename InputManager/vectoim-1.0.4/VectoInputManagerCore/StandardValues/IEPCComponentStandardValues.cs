using System;
using VectoObjects;

namespace VectoInputManager.Core;

public class IEPCComponentStandardValues : BaseStandardValues
{
    public override void SetInput(VectoInputDeclaration_v2_2 vInputDeclaration, IComponentDeclaration sv)
    {
        vInputDeclaration.IEPC = (IEPCComponentDeclarationType_v2_3)sv;
    }

    public override BaseInput CreateInput() => new IEPCComponentInput();

    public override IComponentDeclaration Calculate(BaseInput inputData) =>
        Update((IEPCComponentInput)inputData);

    private static IEPCComponentDeclarationType_v2_3 Update(IEPCComponentInput input)
    {
        var declaration = new IEPCComponentDeclarationType_v2_3();

        // Compare to IEPCMeasuredDataDeclarationType_v2_6
        var data = new IEPCStandardValuesDataDeclarationType_v2_6();
        declaration.Data = data;

        input.InitDataDeclaration(data);

        data.CertificationMethod = IEPCStandardValuesDataDeclarationTypeCertificationMethod_v2_6
            .Standard_values_for_all_components;
        // Other choices: Measured_for_complete_component,
        // Measured_for_EM_and_standard_values_for_other_components

        data.ElectricMachineType = input.ElectricMachineType;

        data.R85RatedPower = input.R85RatedPower;

        data.RotationalInertia = input.RotationalInertia;

        bool differentialIncluded = input.Axle != null;

        data.DifferentialIncluded = differentialIncluded;

        data.DesignTypeWheelMotor = input.DesignTypeWheelMotor;

        data.NrOfDesignTypeWheelMotorMeasured = input.NrOfDesignTypeWheelMotorMeasured;

        data.DisengagementClutch = input.DisengagementClutch;

        // Gears – populated from input

        var ratioMultiplier = differentialIncluded
            ? input.Axle!.Ratio
            : 1.0m;

        var gears = new IEPCGearsDeclarationType_v2_6();
        foreach (var g in input.Gears)
        {
            gears.Gear.Add(new IEPCGearDeclarationType_v2_6
            {
                number = g.Number,
                Ratio = g.Ratio * ratioMultiplier,
                MaxOutShaftSpeed = g.MaxOutShaftSpeed,
                MaxOutShaftTorque = g.MaxOutShaftTorque
            });
        }
        data.Gears = gears;

        // Axle ratio = gear ratio closest to 1
        double axleRatio = (double)input.Gears.OrderBy(g => Math.Abs(g.Ratio - 1.0m)).First().Ratio;

        // VoltageLevel

        var voltageLevel = new IEPCSystemVoltageLevelStandardValuesType_v2_6();
        data.VoltageLevel = voltageLevel;

        voltageLevel.ContinuousTorque = input.ContinuousTorque;
        voltageLevel.TestSpeedContinuousTorque = input.TestSpeedContinuousTorque;
        voltageLevel.OverloadTorque = input.OverloadTorque;
        voltageLevel.TestSpeedOverloadTorque = input.TestSpeedOverloadTorque;
        voltageLevel.OverloadDuration = input.OverloadDuration;

        // VoltageLevel.MaxTorqueCurve: Copy max torque curve from input

        foreach (var entry in input.MaxTorqueCurve)
        {
            voltageLevel.MaxTorqueCurve.Add(new IEPCSystemMaxTorqueEntryType_v2_3()
            {
                outShaftSpeed = entry.OutShaftSpeed,
                maxTorque = entry.MaxTorque,
                minTorque = entry.MinTorque
            });
        }

        // VoltageLevel.PowerMap – one per gear, populated via the Combination tab pipeline

        // Parameters for EMS calculations (power map & drag curve)
        var Ti = (double)voltageLevel.OverloadTorque;
        var Tmax = Ti;
        var nj = (double)voltageLevel.TestSpeedOverloadTorque;
        bool psm = input.ElectricMachineType == ElectricMachineTypeType_v2_3.PSM;

        foreach (var gearInput in gears.Gear)
        {
            var powerMap = new IEPCSystemVoltageLevelStandardValuesTypePowerMap_v2_6
            {
                gear = gearInput.number,
            };

            double gearRatio = (double)gearInput.Ratio;

            foreach (var speed in input.SpeedSetpoints)
            {
                foreach (var torque in input.TorqueSetpoints)
                {
                    double s = (double)speed;
                    double t = (double)torque;

                    // Step 2 – Gearbox transform
                    var gbResult = CalculateGearboxLoss(input.Gearbox, s, t);
                    double gbxSpeed = TransformSpeedThroughGearbox(s, gearRatio);
                    double gbxTorque = TransformTorqueThroughGearbox(t, gbResult.TorqueLoss, gearRatio);

                    double finalSpeed = gbxSpeed;
                    double finalTorque = gbxTorque;

                    // Step 3 – Axle transform (only when differential is included)
                    if (differentialIncluded)
                    {
                        var axResult = CalculateAxleLoss(input.Axle!, gbxTorque, axleRatio);
                        finalSpeed = TransformSpeedThroughAxle(gbxSpeed, axleRatio);
                        finalTorque = TransformTorqueThroughAxle(gbxTorque, axResult.TorqueLoss, axleRatio);
                    }

                    // Electric power from EM-side values using normalized speed/torque
                    double wNorm = finalSpeed / nj;
                    double tNorm = finalTorque / Tmax;
                    double elPower = EMSComponentStandardValues.P_el(wNorm, nj, tNorm, Tmax, psm);

                    powerMap.Entry.Add(new IEPCSystemPowerMapEntryType_v2_3
                    {
                        outShaftSpeed = (decimal)finalSpeed,
                        torque = (decimal)finalTorque,
                        electricPower = (decimal)elPower,
                    });
                }
            }

            voltageLevel.PowerMap.Add(powerMap);
        }

        // DragCurve - Same calculations as in EMSComponentStandardValues

        double tDrag1 = EMSComponentStandardValues.StandardValues(Ti, Tmax, nj, nj / 1d, psm);
        double tDrag4 = EMSComponentStandardValues.StandardValues(Ti, Tmax, nj, nj / 4d, psm);
        // A third value of drag torque at zero rotational speed shall be
        // calculated by means of linear extrapolation.
        double tDrag0 = EMSComponentStandardValues.LinearInterpolate(1.0, tDrag1, 4.0, tDrag4, 0.0);

        // At w_norm = 0: speed = 0
        data.DragCurve.Add(new IEPCSystemDragTorqueEntryType_v2_6()
        {
            outShaftSpeed = 0m,
            dragTorque = (decimal)tDrag0,
        });
        // At w_norm = 1.0: speed = nj
        data.DragCurve.Add(new IEPCSystemDragTorqueEntryType_v2_6()
        {
            outShaftSpeed = (decimal)nj,
            dragTorque = (decimal)tDrag1,
        });
        // At w_norm = 4.0: speed = 4 * nj
        data.DragCurve.Add(new IEPCSystemDragTorqueEntryType_v2_6()
        {
            outShaftSpeed = (decimal)(4d * nj),
            dragTorque = (decimal)tDrag4,
        });

        return declaration;
    }

    /// <summary>
    /// Calculates the torque loss through a gearbox for a single operating point.
    ///
    /// Formulas
    ///   eff_ee = IF(numEE > 0, numEE * 0.986, 1)
    ///   eff_ei = IF(numEI > 0, numEI * 0.993, 1)
    ///   eff_ad = IF(numAD > 0, numAD * 0.970, 1)
    ///
    ///   If directGear:
    ///     Ft  = 0.007   (constant)
    ///     T_d0 = 0.0075 * Tmaxin   (with >2 clutches)
    ///     T_d0 = 0.0025 * Tmaxin   (without)
    ///   Else:
    ///     Ft  = 1 - (eff_ee * eff_ei * eff_ad * 0.995)
    ///     T_d0 = 0.0075 * Tmaxin   (with >2 clutches)
    ///     T_d0 = 0.0025 * Tmaxin   (without)
    ///
    ///   T_loss = T_d0 + T_d0 * (n_in / 1000) + Ft * T_in
    /// </summary>
    internal static GearboxResult CalculateGearboxLoss(GearboxInput input, double speedRpm, double torqueNm)
    {
        double effEE = input.ExternalExternalGearMeshes > 0
            ? input.ExternalExternalGearMeshes * 0.986
            : 1.0;

        double effEI = input.ExternalInternalGearMeshes > 0
            ? input.ExternalInternalGearMeshes * 0.993
            : 1.0;

        double effAD = input.AngleDriveGearMeshes > 0
            ? input.AngleDriveGearMeshes * 0.970
            : 1.0;

        // Ft depends on whether it's a direct gear
        double ft;
        if (input.DirectGear)
            ft = 0.007;
        else
            ft = 1.0 - (effEE * effEI * effAD * 0.995);

        // T_d0 depends on number of friction clutches
        double td0;
        if (input.MoreThan2FrictionClutches)
            td0 = 0.0075 * (double)input.Tmaxin;
        else
            td0 = 0.0025 * (double)input.Tmaxin;

        // T_loss = T_d0 + T_d0 * (n_in / 1000) + Ft * T_in
        double tLoss = td0 + td0 * (speedRpm / 1000.0) + ft * torqueNm;

        return new GearboxResult
        {
            EffEE = effEE,
            EffEI = effEI,
            EffAD = effAD,
            Ft = ft,
            Td0 = td0,
            TorqueLoss = tLoss
        };
    }

    /// <summary>
    /// Calculates the torque loss through an axle/differential for a single operating point.
    ///
    /// Formulas:
    ///   eff_ee = IF(numEE > 0, numEE * 0.986, 1)
    ///   eff_ei = IF(numEI > 0, numEI * 0.993, 1)
    ///   eff_ad = IF(numAD > 0, numAD * 0.970, 1)
    ///   eff_bg = IF(numBG > 0, numBG * 0.980, 1)
    ///   eff_total = eff_ee * eff_ei * eff_ad * eff_bg
    ///
    ///   If bevelGearMesh:
    ///     T_d0 = 25 + 15 * ratio
    ///   Else (spur/planetary):
    ///     T_d0 = 25 + 5 * ratio
    ///
    ///   T_loss = eff_total * T_d0 / ratio + (1 - eff_total) * T_in
    /// </summary>
    internal static AxleResult CalculateAxleLoss(AxleInput input, double torqueNm, double ratio)
    {
        double effEE = input.ExternalExternalGearMeshes > 0
            ? input.ExternalExternalGearMeshes * 0.986
            : 1.0;

        double effEI = input.ExternalInternalGearMeshes > 0
            ? input.ExternalInternalGearMeshes * 0.993
            : 1.0;

        double effAD = input.AngleDriveGearMeshes > 0
            ? input.AngleDriveGearMeshes * 0.970
            : 1.0;

        double effBG = input.BevelGearMeshes > 0
            ? input.BevelGearMeshes * 0.980
            : 1.0;

        double effTotal = effEE * effEI * effAD * effBG;

        double td0;
        if (input.GearMeshType == AxleGearMeshType.Bevel)
            td0 = 25.0 + 15.0 * ratio;
        else
            td0 = 25.0 + 5.0 * ratio;

        // T_loss = eff_total * T_d0 / ratio + (1 - eff_total) * T_in
        double tLoss = effTotal * td0 / ratio + (1.0 - effTotal) * torqueNm;

        return new AxleResult
        {
            EffEE = effEE,
            EffEI = effEI,
            EffAD = effAD,
            EffBG = effBG,
            EffTotal = effTotal,
            Td0 = td0,
            TorqueLoss = tLoss
        };
    }

    /// <summary>
    /// Scales the EM power map for multiple identical electric machines.
    /// when multiple EMs are part of the IEPC, multiply
    /// all torque and power figures by the machine count.
    /// </summary>
    internal static (double[] scaledTorques, double[] scaledPowers) ScaleEmPowerMap(
        double[] torques, double[] powers, int machineCount)
    {
        var scaledT = torques.Select(t => t * machineCount).ToArray();
        var scaledP = powers.Select(p => p * machineCount).ToArray();
        return (scaledT, scaledP);
    }

    /// <summary>
    /// Transforms EM speed setpoints through the gearbox.
    /// n_GBX = n_EM / i_gear
    /// </summary>
    internal static double TransformSpeedThroughGearbox(double speedEm, double gearRatio)
    {
        return speedEm / gearRatio;
    }

    /// <summary>
    /// Transforms EM torque setpoints through the gearbox.
    /// T_GBX = (T_EM - T_loss(n_EM, T_EM, gear)) * i_gear
    /// where T_loss is calculated by CalculateGearboxLoss.
    /// </summary>
    internal static double TransformTorqueThroughGearbox(
        double torqueEm, double gearboxTorqueLoss, double gearRatio)
    {
        return (torqueEm - gearboxTorqueLoss) * gearRatio;
    }

    /// <summary>
    /// Transforms speed setpoints through the differential/axle.
    /// n_diff_out = n_diff_in / i_diff
    /// </summary>
    internal static double TransformSpeedThroughAxle(double speedIn, double axleRatio)
    {
        return speedIn / axleRatio;
    }

    /// <summary>
    /// Transforms torque setpoints through the differential/axle.
    /// Equation (image5): T_diff_out = (T_diff_in - T_loss(T_diff_in)) * i_diff
    /// where T_loss is calculated by CalculateAxleLoss.
    /// </summary>
    internal static double TransformTorqueThroughAxle(
        double torqueIn, double axleTorqueLoss, double axleRatio)
    {
        return (torqueIn - axleTorqueLoss) * axleRatio;
    }

    /// <summary>
    /// Result of a gearbox torque loss calculation, including intermediates.
    /// </summary>
    internal class GearboxResult
    {
        public double EffEE { get; set; }
        public double EffEI { get; set; }
        public double EffAD { get; set; }
        public double Ft { get; set; }
        public double Td0 { get; set; }
        public double TorqueLoss { get; set; }
    }

    /// <summary>
    /// Result of an axle torque loss calculation, including intermediates.
    /// </summary>
    internal class AxleResult
    {
        public double EffEE { get; set; }
        public double EffEI { get; set; }
        public double EffAD { get; set; }
        public double EffBG { get; set; }
        public double EffTotal { get; set; }
        public double Td0 { get; set; }
        public double TorqueLoss { get; set; }
    }
}
