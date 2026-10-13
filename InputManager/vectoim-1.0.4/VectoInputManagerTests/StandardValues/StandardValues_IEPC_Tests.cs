using AwesomeAssertions;
using VectoInputManager.Core;

namespace VectoInputManager.Tests.StandardValues;

[TestFixture]
public class StandardValues_IEPC_Tests
{
    [Test]
    public void Gearbox_ColumnB_DirectGear_MoreThan2Clutches()
    {
        var result = IEPCComponentStandardValues.CalculateGearboxLoss(new GearboxInput
        {
            Tmaxin = 10000,
            DirectGear = true,
            MoreThan2FrictionClutches = true,
            ExternalExternalGearMeshes = 1,
            ExternalInternalGearMeshes = 0,
            AngleDriveGearMeshes = 0
        }, speedRpm: 1000, torqueNm: 800);

        result.EffEE.Should().Be(0.986);
        result.Ft.Should().Be(0.007);
        result.Td0.Should().Be(75.0);
        result.TorqueLoss.Should().Be(155.6);
    }

    [Test]
    public void Gearbox_ColumnC_NotDirectGear_MoreThan2Clutches()
    {
        var result = IEPCComponentStandardValues.CalculateGearboxLoss(new GearboxInput
        {
            Tmaxin = 10000,
            DirectGear = false,
            MoreThan2FrictionClutches = true,
            ExternalExternalGearMeshes = 1,
            ExternalInternalGearMeshes = 0,
            AngleDriveGearMeshes = 0
        }, speedRpm: 1000, torqueNm: 800);

        result.EffEE.Should().Be(0.986);
        result.Ft.Should().Be(0.018930000000000002);
        result.Td0.Should().Be(75.0);
        result.TorqueLoss.Should().Be(165.144);
    }

    [Test]
    public void Gearbox_ColumnD_DirectGear_LessThan2Clutches()
    {
        var result = IEPCComponentStandardValues.CalculateGearboxLoss(new GearboxInput
        {
            Tmaxin = 10000,
            DirectGear = true,
            MoreThan2FrictionClutches = false,
            ExternalExternalGearMeshes = 1,
            ExternalInternalGearMeshes = 0,
            AngleDriveGearMeshes = 0
        }, speedRpm: 1000, torqueNm: 800);

        result.Td0.Should().Be(25.0);
        result.TorqueLoss.Should().Be(55.6);
    }

    [Test]
    public void Gearbox_ColumnE_NotDirectGear_LessThan2Clutches()
    {
        var result = IEPCComponentStandardValues.CalculateGearboxLoss(new GearboxInput
        {
            Tmaxin = 10000,
            DirectGear = false,
            MoreThan2FrictionClutches = false,
            ExternalExternalGearMeshes = 1,
            ExternalInternalGearMeshes = 0,
            AngleDriveGearMeshes = 0
        }, speedRpm: 1000, torqueNm: 800);

        result.Td0.Should().Be(25.0);
        result.TorqueLoss.Should().Be(65.144);
    }

    [Test]
    public void Axle_ColumnB_BevelGearMesh()
    {
        var result = IEPCComponentStandardValues.CalculateAxleLoss(new AxleInput
        {
            GearMeshType = AxleGearMeshType.Bevel,
            ExternalExternalGearMeshes = 1,
            ExternalInternalGearMeshes = 0,
            AngleDriveGearMeshes = 0,
            BevelGearMeshes = 0
        }, torqueNm: 800, ratio: 3.0);

        result.EffEE.Should().Be(0.986);
        result.EffEI.Should().Be(1.0);
        result.EffAD.Should().Be(1.0);
        result.EffBG.Should().Be(1.0);
        result.EffTotal.Should().Be(0.986);
        result.Td0.Should().Be(70.0);
        result.TorqueLoss.Should().Be(34.20666666666668);
    }

    /// <summary>
    /// Column C: bevelGearMesh=FALSE (spur/planetary)
    /// </summary>
    [Test]
    public void Axle_ColumnC_SpurPlanetary()
    {
        var result = IEPCComponentStandardValues.CalculateAxleLoss(new AxleInput
        {
            GearMeshType = AxleGearMeshType.SpurPlanetary,
            ExternalExternalGearMeshes = 1,
            ExternalInternalGearMeshes = 0,
            AngleDriveGearMeshes = 0,
            BevelGearMeshes = 0
        }, torqueNm: 800, ratio: 3.0);

        result.Td0.Should().Be(40.0);
        result.TorqueLoss.Should().Be(24.34666666666668);
    }

    /// <summary>
    /// Speed through gearbox: n_GBX = n_EM / i_gear
    /// </summary>
    [Test]
    public void Combination_SpeedThroughGearbox()
    {
        double nGbx = IEPCComponentStandardValues.TransformSpeedThroughGearbox(1000, 2.0);

        nGbx.Should().Be(500.0);
    }

    /// <summary>
    /// Torque through gearbox: T_GBX = (T_EM - T_loss) * i_gear
    /// </summary>
    [Test]
    public void Combination_TorqueThroughGearbox()
    {
        double tGbx = IEPCComponentStandardValues.TransformTorqueThroughGearbox(800, 155.6, 2.0);

        tGbx.Should().Be(1288.8);
    }

    /// <summary>
    /// Speed through axle: n_diff_out = n_diff_in / i_diff
    /// </summary>
    [Test]
    public void Combination_SpeedThroughAxle()
    {
        double nDiff = IEPCComponentStandardValues.TransformSpeedThroughAxle(500, 3.0);

        nDiff.Should().Be(166.66666666666666);
    }

    /// <summary>
    /// Torque through axle: T_diff_out = (T_in - T_loss) * i_diff
    /// </summary>
    [Test]
    public void Combination_TorqueThroughAxle()
    {
        double tDiff = IEPCComponentStandardValues.TransformTorqueThroughAxle(1288.8, 34.2067, 3.0);

        tDiff.Should().Be(3763.7799);
    }
}
