using System;
using VectoObjects;

namespace VectoInputManager.Core;

#pragma warning disable IDE1006 // Naming Styles

public class EMSComponentStandardValues : BaseStandardValues
{
    public override void SetInput(VectoInputDeclaration_v2_2 vInputDeclaration, IComponentDeclaration sv)
    {
        vInputDeclaration.ElectricMachineSystem = (ElectricMachineSystemComponentDeclarationType_v2_3)sv;
    }

    public override BaseInput CreateInput() => new EMSComponentInput();

    public override IComponentDeclaration Calculate(BaseInput inputData) =>
        Update((EMSComponentInput)inputData);

    /// <summary>
    /// Generates EMS standard values declaration based on input parameters.
    /// Implements calculations from Appendix 8 of Annex Xb of regulation (EU) 2017/2400.
    /// </summary>
    private static ElectricMachineSystemComponentDeclarationType_v2_3 Update(EMSComponentInput input)
    {
        var declaration = new ElectricMachineSystemComponentDeclarationType_v2_3();
        var data = new ElectricMachineSystemStandardValuesDataDeclarationType_v2_3();
        declaration.Data = data;

        input.InitDataDeclaration(data);
        data.CertificationMethod = ElectricMachineSystemStandardValuesDataDeclarationTypeCertificationMethod_v2_3.Standard_values;

        // Basic properties
        data.ElectricMachineType = input.ElectricMachineType;
        data.R85RatedPower = input.R85RatedPower;
        data.RotationalInertia = input.RotationalInertia;
        data.DcDcConverterIncluded = input.DcDcConverterIncluded;
        data.IHPCType = ElectricMachineSystemStandardValuesDataDeclarationTypeIHPCType_v2_3.None;

        // Voltage level parameters
        data.VoltageLevel = new ElectricMachineSystemVoltageLevelStandardValuesType_v2_3()
        {
            ContinuousTorque = input.ContinuousTorque,
            TestSpeedContinuousTorque = input.TestSpeedContinuousTorque,
            OverloadTorque = input.OverloadTorque,
            TestSpeedOverloadTorque = input.TestSpeedOverloadTorque,
            OverloadDuration = input.OverloadDuration
        };

        // Parameters
        var Ti = (double)input.OverloadTorque;
        var Tmax = Ti;
        var nj = (double)input.TestSpeedOverloadTorque;
        bool psm = input.ElectricMachineType == ElectricMachineTypeType_v2_3.PSM;

        double tDrag1 = StandardValues(Ti, Tmax, nj, nj / 1d, psm);
        double tDrag4 = StandardValues(Ti, Tmax, nj, nj / 4d, psm);
        // A third value of drag torque at zero rotational speed shall be
        // calculated by means of linear extrapolation.
        double tDrag0 = LinearInterpolate(1.0, tDrag1, 4.0, tDrag4, 0.0);

        // Add drag curve points
        // At w_norm = 0: speed = 0
        data.DragCurve.Add(new ElectricMachineSystemDragTorqueEntryType_v2_3()
        {
            outShaftSpeed = 0m,
            dragTorque = (decimal)tDrag0,
        });
        // At w_norm = 1.0: speed = nj
        data.DragCurve.Add(new ElectricMachineSystemDragTorqueEntryType_v2_3()
        {
            outShaftSpeed = (decimal)nj,
            dragTorque = (decimal)tDrag1,
        });
        // At w_norm = 4.0: speed = 4 * nj
        data.DragCurve.Add(new ElectricMachineSystemDragTorqueEntryType_v2_3()
        {
            outShaftSpeed = (decimal)(4d * nj),
            dragTorque = (decimal)tDrag4,
        });

        // Populate power map
        // Normalised torque and speed values - Step 6 (e) (i) & (ii)
        // Current implementation generates 21 (speeds) 42 (torques) = 882 power map entries

        foreach (double w_norm in NormalizedSpeed)
        {
            foreach (double T_norm in NormalizedTorque)
            {
                double actualSpeed = n_j(w_norm, nj);
                double actualTorque = T_i(T_norm, Tmax);
                double electricPower = P_el(w_norm, nj, T_norm, Tmax, psm);

                var pwEntry = new ElectricMachineSystemPowerMapEntryType_v2_3()
                {
                    outShaftSpeed = (decimal)actualSpeed,
                    torque = (decimal)actualTorque,
                    electricPower = (decimal)electricPower
                };

                data.VoltageLevel.PowerMap.Add(pwEntry);
            }
        }

        // Copy max torque curve from input
        foreach (var entry in input.MaxTorqueCurve)
        {
            data.VoltageLevel.MaxTorqueCurve.Add(new ElectricMachineSystemMaxTorqueEntryType_v2_3()
            {
                outShaftSpeed = entry.OutShaftSpeed,
                maxTorque = entry.MaxTorque,
                minTorque = entry.MinTorque
            });
        }

        return declaration;
    }

    /// <summary>
    /// Step 6 (b)(i) - Generate normalized speed values.
    /// </summary>
    private static double[] GenerateNormalizedSpeed()
    {
        var speeds = new List<double> { 0.02 };
        for (int i = 1; i <= 20; i++)
        {
            speeds.Add(i * 0.2);
        }
        return [.. speeds];
    }

    /// <summary>
    /// Step 6 (b)(ii) - Generate normalized torque values.
    /// </summary>
    private static double[] GenerateNormalizedTorque()
    {
        var torques = new List<double>();

        // -1.00 to -0.05 in steps of 0.05
        for (int i = 0; i < 20; i++)
        {
            torques.Add(-1.0 + i * 0.05);
        }

        // Add -0.01 and 0.01
        torques.Add(-0.01);
        torques.Add(0.01);

        // 0.05 to 1.00 in steps of 0.05
        for (int i = 1; i <= 20; i++)
        {
            torques.Add(i * 0.05);
        }

        return [.. torques];
    }

    private static double[]? _normalizedSpeed;
    public static double[] NormalizedSpeed => _normalizedSpeed ??= GenerateNormalizedSpeed();

    private static double[]? _normalizedTorque;
    public static double[] NormalizedTorque => _normalizedTorque ??= GenerateNormalizedTorque();

    /// <summary>
    /// Step 6 (c) - Loss coefficient matrices
    /// </summary>
    private static readonly double[,] KPsm = new double[,]
    {
        { 0,      0.005,  0.0025, 0.003 },
        { 0.0067, 0,      0,      0     },
        { 0.018,  0.001,  0.03,   0     },
        { 0,      0,      0,      0     }
    };

    private static readonly double[,] KOther = new double[,]
    {
        { 0.003, 0,    0.001, 0.001 },
        { 0.01,  0,    0.001, 0     },
        { 0.1,   0.03, 0.03,  0     },
        { 0,     0,    0,     0     }
    };

    /// <summary>
    /// Step 6 (a) - Calculate normalized power loss map.
    /// Formula: P_loss,norm = Σ(m,n=0 to 3) k_mn * |T_norm|^m * |w_norm|^n
    /// </summary>
    internal static double CalculateNormalizedPowerLoss(double tNorm, double wNorm, bool isPSM)
    {
        var k = isPSM ? KPsm : KOther;
        double sum = 0;

        for (int m = 0; m < 4; m++)
        {
            for (int n = 0; n < 4; n++)
            {
                sum += k[m, n] * Math.Pow(Math.Abs(tNorm), m) * Math.Pow(Math.Abs(wNorm), n);
            }
        }

        return sum;
    }

    /// <summary>
    /// Step 6 (d)(iii) - Calculate efficiency map. (e)
    /// Calculated as a function of normalised speed and torque values
    /// </summary>
    internal static double Efficiency(double T_norm_i, double w_norm_j, bool psm)
    {
        double PlossN = CalculateNormalizedPowerLoss(T_norm_i, w_norm_j, psm);
        double Tw = T_norm_i * w_norm_j;
        double aux = T_norm_i < 0d
            ? (Tw + PlossN) / Tw * 0.96
            : Tw / (Tw + PlossN) * 0.96;

        return aux < 0 ? 0 : aux;
    }

    /// <summary>
    /// Step 6 (e)(i) - Actual speed
    /// Formula: n_j = w_norm,j * n_rated
    /// </summary>
    private static double n_j(double wNormJ, double nRated)
    {
        return wNormJ * nRated;
    }

    /// <summary>
    /// Step 6 (e)(ii) - Actual torque
    /// Formula: T_i = T_norm,i * T_max
    /// </summary>
    private static double T_i(double tNormI, double tMax)
    {
        return tNormI * tMax;
    }

    /// <summary>
    /// Step 6 (e)(iii) - Actual power loss
    /// Formula: P_loss(T_i, n_j) = (1 - η(T_i/T_max, n_j/n_rated)) * |T_i| * n_j * 2π/60
    /// </summary>
    internal static double P_loss(double wNormJ, double nRated, double tNormI, double tMax, bool isPSM)
    {
        double ti = T_i(tNormI, tMax);
        double nj = n_j(wNormJ, nRated);

        // Efficiency is calculated using normalized values
        double eta = Efficiency(tNormI, wNormJ, isPSM);

        return ((1d - eta) * Math.Abs(ti) * nj * 2d * Math.PI / 60d);
    }

    /// <summary>
    /// Step 6 (e)(iv) - Actual electric inverter power
    /// Formula: P_el(T_i, n_j) = P_loss(T_i, n_j) + T_i * n_j * 2π/60
    /// </summary>
    internal static double P_el(double wNormJ, double nRated, double tNormI, double tMax, bool isPSM)
    {
        double ti = T_i(tNormI, tMax);
        double nj = n_j(wNormJ, nRated);
        double ploss = P_loss(wNormJ, nRated, tNormI, tMax, isPSM);
        //  Electric inverter power - Step 6 (e) (iv)
        return ploss + (ti * nj * 2d * Math.PI / 60d);
    }

    /// <summary>
    /// Step 7 (a) - Actual drag torque at normalized speed points (1.0 or 4.0)
    /// Formula: T_drag = -P_loss * 60 / (2π * n_j)
    /// </summary>
    internal static double T_drag(double wNormJ, double nRated, double tMax, bool isPSM)
    {
        if (wNormJ != 1 && wNormJ != 4)
            throw new ArgumentException("Normalized speed should be 1 or 4.");

        const double tNorm_i = 0.01;
        double nj = n_j(wNormJ, nRated);

        // Calculate power loss using normalized values
        double Ploss = P_loss(wNormJ, nRated, tNorm_i, tMax, isPSM);

        // Calculate drag torque
        double dragTorque = -Ploss * 60d / (2d * Math.PI * nj);

        return dragTorque;
    }

    /// <summary>
    /// Linear interpolation/extrapolation helper.
    /// </summary>
    internal static double LinearInterpolate(double x0, double y0, double x1, double y1, double x)
    {
        return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
    }

    internal static double StandardValues(double Ti, double Tmax, double nj, double nrated, bool psm)
    {
        double Tnormi = Ti / Tmax;
        double wnormj = nj / nrated;

        if (!NormalizedTorque.Contains(Tnormi))
            throw new ArgumentException($"T_norm_i = {Tnormi} is not in the expected normalized torque array.");
        if (!NormalizedSpeed.Contains(wnormj))
            throw new ArgumentException($"w_norm_j = {wnormj} is not in the expected normalized speed array.");

        double actualSpeed = n_j(wnormj, nrated);

        // Use P_loss which now correctly calculates with normalized values for efficiency
        double ploss = P_loss(wnormj, nrated, Tnormi, Tmax, psm);

        return -ploss * 60d / (2d * Math.PI * actualSpeed);
    }
}

#pragma warning restore IDE1006 // Naming Styles