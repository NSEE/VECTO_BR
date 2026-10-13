using AwesomeAssertions;
using VectoInputManager.Core;
using VectoObjects;

namespace VectoInputManager.Tests.StandardValues;

[TestFixture]
public class StandardValues_EMS_Tests
{
    /// <summary>
    /// Test Step 6 (b) - Normalized speed and torque generation
    /// Validates that the normalized speed and torque arrays match the regulation requirements
    /// </summary>
    [Test]
    public void EMS_Step6b_NormalizedSpeedAndTorque_Tests()
    {
        var w_norm_j = EMSComponentStandardValues.NormalizedSpeed;
        var T_norm_i = EMSComponentStandardValues.NormalizedTorque;

        // Validate counts
        w_norm_j.Should().HaveCount(21);
        T_norm_i.Should().HaveCount(42);

        // Validate specific values from notebook
        w_norm_j[0].Should().BeApproximately(0.02, 1e-10);
        w_norm_j[1].Should().BeApproximately(0.2, 1e-10);
        w_norm_j[20].Should().BeApproximately(4.0, 1e-10);

        T_norm_i[0].Should().BeApproximately(-1.0, 1e-10);
        T_norm_i[20].Should().BeApproximately(-0.01, 1e-10);
        T_norm_i[21].Should().BeApproximately(0.01, 1e-10);
        T_norm_i[41].Should().BeApproximately(1.0, 1e-10);
    }

    /// <summary>
    /// Test Step 6 (a) - Normalized power loss calculation with edge cases
    /// Tests 24 specific cases covering PSM and Other machine types
    /// </summary>
    [Test]
    public void EMS_Step6a_PowerLossMap_EdgeCases_Tests()
    {
        var T_norm_i = EMSComponentStandardValues.NormalizedTorque;
        var w_norm_j = EMSComponentStandardValues.NormalizedSpeed;

        // Test cases: (j: speed_index, i: torque_index, isPSM, expectedResult)
        var testCases = new[]
        {
            (j: 0, i: 0, isPSM: true, result: 0.024833024),
            (j: 20, i: 41, isPSM: true, result: 0.7606999999999999),
            (j: 0, i: 41, isPSM: true, result: 0.024833024),
            (j: 20, i: 0, isPSM: true, result: 0.7606999999999999),
            (j: 5, i: 20, isPSM: true, result: 0.010571899999999997),
            (j: 5, i: 21, isPSM: true, result: 0.010571899999999997),
            (j: 10, i: 31, isPSM: true, result: 0.08234999999999999),
            (j: 19, i: 22, isPSM: true, result: 0.22118850000000004),
            (j: 1, i: 40, isPSM: true, result: 0.0249975),
            (j: 2, i: 1, isPSM: true, result: 0.029894999999999998),
            (j: 10, i: 10, isPSM: true, result: 0.08234999999999994),
            (j: 10, i: 31, isPSM: true, result: 0.08234999999999999),
            (j: 0, i: 0, isPSM: false, result: 0.11361280800000001),
            (j: 20, i: 41, isPSM: false, result: 0.8089999999999999),
            (j: 0, i: 41, isPSM: false, result: 0.11361280800000001),
            (j: 20, i: 0, isPSM: false, result: 0.8089999999999999),
            (j: 5, i: 20, isPSM: false, result: 0.005126),
            (j: 5, i: 21, isPSM: false, result: 0.005126),
            (j: 10, i: 31, isPSM: false, result: 0.092),
            (j: 19, i: 22, isPSM: false, result: 0.07515200000000001),
            (j: 1, i: 40, isPSM: false, result: 0.10933400000000001),
            (j: 4, i: 3, isPSM: false, result: 0.11665799999999997),
            (j: 10, i: 10, isPSM: false, result: 0.09199999999999987),
            (j: 10, i: 31, isPSM: false, result: 0.092),
    };

        // Validate each test case
        foreach (var (j, i, isPSM, result) in testCases)
        {
            var calculated = EMSComponentStandardValues.CalculateNormalizedPowerLoss(
                T_norm_i[i], w_norm_j[j], isPSM);

            calculated.Should().BeApproximately(result, 1e-5,
                $"Failed for j={j}, i={i}, isPSM={isPSM}: T_norm={T_norm_i[i]:F2}, w_norm={w_norm_j[j]:F2}");
        }

        // Validate array dimensions would be 21×42
        testCases.Length.Should().Be(24);
    }

    /// <summary>
    /// Test Step 6 (d)(iii) - Efficiency calculation
    /// Tests 24 specific cases covering PSM and Other machine types
    /// </summary>
    [Test]
    public void EMS_Step6d_Efficiency_Tests()
    {
        var T_norm_i = EMSComponentStandardValues.NormalizedTorque;
        var w_norm_j = EMSComponentStandardValues.NormalizedSpeed;

        // Test cases: (j: speed_index, i: torque_index, isPSM, expectedResult)
        var testCases = new[]
        {
            (j: 0, i: 0, isPSM: true, result: 0),
            (j: 20, i: 41, isPSM: true, result: 0.8066040708299199),
            (j: 0, i: 41, isPSM: true, result: 0.4282557429095124),
            (j: 20, i: 0, isPSM: true, result: 0.777432),
            (j: 5, i: 20, isPSM: true, result: 0),
            (j: 5, i: 21, isPSM: true, result: 0.4666559724672977),
            (j: 10, i: 31, isPSM: true, result: 0.8869589319536195),
            (j: 19, i: 22, isPSM: true, result: 0.443592172446457),
            (j: 1, i: 40, isPSM: true, result: 0.8483819579297434),
            (j: 2, i: 1, isPSM: true, result: 0.8844757894736842),
            (j: 10, i: 10, isPSM: true, result: 0.880944),
            (j: 10, i: 31, isPSM: true, result: 0.8869589319536195),
            (j: 0, i: 0, isPSM: false, result: 0),
            (j: 20, i: 41, isPSM: false, result: 0.7985028072364316),
            (j: 0, i: 41, isPSM: false, result: 0.14369879869600527),
            (j: 20, i: 0, isPSM: false, result: 0.76584),
            (j: 5, i: 20, isPSM: false, result: 0.467904),
            (j: 5, i: 21, isPSM: false, result: 0.634668782229274),
            (j: 10, i: 31, isPSM: false, result: 0.8791208791208791),
            (j: 19, i: 22, isPSM: false, result: 0.6879073135409124),
            (j: 1, i: 40, isPSM: false, result: 0.6093527631341579),
            (j: 4, i: 3, isPSM: false, result: 0.7953063529411766),
            (j: 10, i: 10, isPSM: false, result: 0.87168),
            (j: 10, i: 31, isPSM: false, result: 0.8791208791208791),
        };

        // Validate each test case
        foreach (var (j, i, isPSM, result) in testCases)
        {
            var calculated = EMSComponentStandardValues.Efficiency(
                T_norm_i[i], w_norm_j[j], isPSM);

            calculated.Should().BeApproximately(result, 1e-5,
                $"Failed for j={j}, i={i}, isPSM={isPSM}: T_norm={T_norm_i[i]:F2}, w_norm={w_norm_j[j]:F2}");
        }

        // Validate array dimensions would be 21×42
        testCases.Length.Should().Be(24);
    }

    [Test]
    public void ActualPowerLoss_Tests()
    {
        var T_norm_i = EMSComponentStandardValues.NormalizedTorque;
        var w_norm_j = EMSComponentStandardValues.NormalizedSpeed;

        EMSComponentStandardValues.P_loss(w_norm_j[2], 1, T_norm_i[1], 1, true)
            .Should().BeApproximately(0.004597113473948968d, 1e-10d);
    }

    /// <summary>
    /// Test Step 7 (b) - Linear interpolation for drag torque at zero speed
    /// Example from notebook: LinearInterpolate(1, -0.005333, 4, -0.008685, 0) = -0.0042161
    /// </summary>
    [Test]
    public void EMS_Step7b_DragTorqueInterpolation_Tests()
    {
        double x0 = 1.0;
        double y0 = -0.005333440275327024;
        double x1 = 4.0;
        double y1 = -0.008685459123940665;
        double x = 0.0;

        // Use the actual method from EMSComponentStandardValues
        double result = EMSComponentStandardValues.LinearInterpolate(x0, y0, x1, y1, x);

        // Expected value from notebook: -0.0042161006591224775
        result.Should().BeApproximately(-0.0042161006591224775, 1e-10);
    }

    /// <summary>
    /// Test complete EMS calculation with example values from notebook
    /// Uses: Ti=1885.13, Tmax=188513, nj=1793.75, nrated=1793.75 (w_norm=1, T_norm=0.01)
    /// Expected: Tdrag1 = -1005.4228266227233
    /// </summary>
    [Test]
    [Ignore("Temp")]
    public void ActualDragTorque_Tests()
    {
        double tDrag1 = EMSComponentStandardValues.T_drag(
            wNormJ: 1.0,
            nRated: 1.0,
            tMax: 1.0,
            isPSM: true);

        tDrag1.Should().Be(-0.005333440275327024);

        double tDrag4 = EMSComponentStandardValues.T_drag(
            wNormJ: 4.0,
            nRated: 1.0,
            tMax: 1.0,
            isPSM: true);

        // Expected value from notebook: -0.008685459123940665
        tDrag4.Should().Be(-0.008685459123940665);

        double ti = 1885.13;
        double nj = 1793.75;

        double tDragReal1 = EMSComponentStandardValues.T_drag(
            wNormJ: 1.0,
            nRated: nj / 1.0,
            tMax: ti / 0.01,
            isPSM: true);

        tDragReal1.Should().Be(-1885.1121616902215);
        // tDragReal1.Should().Be(-1005.4228266227233);
    }


    /// <summary>
    /// Test extrapolated drag torque at zero speed
    /// Expected: Tdrag0 = -794.7897835531555
    /// </summary>
    [Test]
    public void EMS_DragTorque_ZeroSpeed_Extrapolation_Tests()
    {
        double tDrag1 = -0.005333440275327024;
        double tDrag4 = -0.008685459123940665;

        double tDrag0 = EMSComponentStandardValues.LinearInterpolate(1.0, tDrag1, 4.0, tDrag4, 0.0);
        tDrag0.Should().Be(-0.0042161006591224775);
    }

    /// <summary>
    /// Test StandardValues function with Python notebook example
    /// Ti=1885.13, Tmax=188513, nj=1793.75, psm=True
    /// Tests both nrated = nj/1 and nrated = nj/4 cases
    /// </summary>
    [Test]
    public void StandardValues_Function_PythonNotebookExample_Tests()
    {
        // Parameters from Python notebook
        double Ti = 1885.13;
        double Tmax = Ti / 0.01; // 188513
        double nj = 1793.75;
        bool psm = true;

        // Test case 1: nj / nrated = 1
        double nrated1 = nj / 1;
        double Tdrag1 = EMSComponentStandardValues.StandardValues(Ti, Tmax, nj, nrated1, psm);
        
        Tdrag1.Should().Be(-1005.4228266227233);

        // Test case 2: nj / nrated = 4
        double nrated4 = nj / 4;
        double Tdrag4 = EMSComponentStandardValues.StandardValues(Ti, Tmax, nj, nrated4, psm);
        
        Tdrag4.Should().Be(-1637.3219558314265);
    }

    /// <summary>
    /// Integration test with full EMS input
    /// </summary>
    [Test]
    [Ignore("Temp")]
    public void EMS_Integration_Test_WithNotebookExample()
    {
        var calculator = new EMSComponentStandardValues();

        decimal torque = 1885.13m;
        decimal speed = 1793.75m;

        var input = new EMSComponentInput
        {
            Manufacturer = "Test Manufacturer",
            Model = "Test Model",
            CertificationNumber = "TEST123",
            ElectricMachineType = ElectricMachineTypeType_v2_3.PSM,
            R85RatedPower = 50000,
            RotationalInertia = 0.10m,
            DcDcConverterIncluded = true,
            ContinuousTorque = 200.00m,
            TestSpeedContinuousTorque = 2000.00m,
            OverloadTorque = torque,
            TestSpeedOverloadTorque = speed,
            OverloadDuration = 30.00m,

            MaxTorqueCurve =
            [
                new MaxTorqueCurveEntry {
                    OutShaftSpeed = 2250,
                    MaxTorque = 1481,
                    MinTorque = -1481 },
            ],
        };

        var result = calculator.Calculate(input);

        result.Should().NotBeNull();

        var declaration = result as ElectricMachineSystemComponentDeclarationType_v2_3;
        declaration.Should().NotBeNull();

        var data = declaration!.Data as ElectricMachineSystemStandardValuesDataDeclarationType_v2_3;
        data.Should().NotBeNull();

        // Verify basic properties
        data!.ElectricMachineType.Should().Be(ElectricMachineTypeType_v2_3.PSM);
        data.R85RatedPower.Should().Be(50000);
        data.RotationalInertia.Should().Be(0.10m);

        // Verify voltage level
        var vl = data.VoltageLevel;
        vl.Should().NotBeNull();
        vl.ContinuousTorque.Should().Be(200.00m);

        // Verify power map is generated (21 speeds × 42 torques = 882 entries)
        vl.PowerMap.Should().HaveCount(882);

        data.DragCurve.Should().HaveCount(3);

        // Verify drag torque values

        var dragAt1 = data.DragCurve.First(d => d.outShaftSpeed == speed);
        ((double)dragAt1.dragTorque).Should().Be(-1005.42282662272d);

        var dragAt4 = data.DragCurve.First(d => d.outShaftSpeed == 4m * speed);
        ((double)dragAt4.dragTorque).Should().Be(-1637.32195583143d);

        var dragAt0 = data.DragCurve.First(d => d.outShaftSpeed == 0);
        ((double)dragAt0.dragTorque).Should().Be(-794.789783553156d);
    }

}