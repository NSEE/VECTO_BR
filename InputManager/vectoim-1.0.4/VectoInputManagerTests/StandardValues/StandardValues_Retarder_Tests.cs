using NUnit.Framework;
using VectoInputManager.Core;

namespace VectoInputManager.Tests.StandardValues;

[TestFixture]
public class StandardValues_Retarder_Tests
{
    private const decimal StepUpRatio = 1.5m;
    private const int TestRpm = 2000;
    private const decimal Tolerance = 0.00000001m;

    [Test]
    public void HydrodynamicWithVehicleLaunchFunctionality_At2000Rpm_ReturnsCorrectTorque()
    {
        // Arrange
        var input = new RetarderComponentInput
        {
            Type = RetarderType.HydrodynamicWithVehicleLaunchFunctionality,
            StepUpRatio = StepUpRatio
        };
        var calculator = new RetarderComponentStandardValues();

        // Act
        var result = calculator.Calculate(input);
        var declaration = (VectoObjects.RetarderComponentDeclarationType_v2_0)result;
        var data = (VectoObjects.RetarderDataDeclarationType_v2_0)declaration.Data;
        var entryAt2000 = data.RetarderLossMap.FirstOrDefault(e => e.retarderSpeed == TestRpm);

        // Assert
        Assert.That(entryAt2000, Is.Not.Null, "Entry for 2000 rpm should exist");
        Assert.That(entryAt2000!.torqueLoss, Is.EqualTo(18.07407407m).Within(Tolerance));
    }

    [Test]
    public void Hydrodynamic_At2000Rpm_ReturnsCorrectTorque()
    {
        // Arrange
        var input = new RetarderComponentInput
        {
            Type = RetarderType.Hydrodynamic,
            StepUpRatio = StepUpRatio
        };
        var calculator = new RetarderComponentStandardValues();

        // Act
        var result = calculator.Calculate(input);
        var declaration = (VectoObjects.RetarderComponentDeclarationType_v2_0)result;
        var data = (VectoObjects.RetarderDataDeclarationType_v2_0)declaration.Data;
        var entryAt2000 = data.RetarderLossMap.FirstOrDefault(e => e.retarderSpeed == TestRpm);

        // Assert
        Assert.That(entryAt2000, Is.Not.Null, "Entry for 2000 rpm should exist");
        Assert.That(entryAt2000!.torqueLoss, Is.EqualTo(9.037037037m).Within(Tolerance));
    }

    [Test]
    public void Magnetic_At2000Rpm_ReturnsCorrectTorque()
    {
        // Arrange
        var input = new RetarderComponentInput
        {
            Type = RetarderType.Magnetic,
            StepUpRatio = StepUpRatio
        };
        var calculator = new RetarderComponentStandardValues();

        // Act
        var result = calculator.Calculate(input);
        var declaration = (VectoObjects.RetarderComponentDeclarationType_v2_0)result;
        var data = (VectoObjects.RetarderDataDeclarationType_v2_0)declaration.Data;
        var entryAt2000 = data.RetarderLossMap.FirstOrDefault(e => e.retarderSpeed == TestRpm);

        // Assert
        Assert.That(entryAt2000, Is.Not.Null, "Entry for 2000 rpm should exist");
        Assert.That(entryAt2000!.torqueLoss, Is.EqualTo(13.92592593m).Within(Tolerance));
    }
}
