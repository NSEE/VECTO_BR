using AwesomeAssertions;
using VectoInputManager.Core;

namespace VectoInputManager.Tests.Core;

public class JsonInputUtils_Tests
{
    [Test]
    public void Deserialize_BaseInput_ReturnsComponent()
    {
        var json = """
        {
            "Component": "Battery",
            "Manufacturer": "a",
        }
        """;

        // Act
        var result = JsonInputUtils.Deserialize(json);

        // Assert
        result.Should().BeOfType<BatteryComponentInput>();
        var battery = (BatteryComponentInput)result;
        battery.Component.Should().Be(ComponentType.Battery);
        battery.Manufacturer.Should().Be("a");
    }

    [Test]
    public void Deserialize_BaseInput_ReturnsComponent_CaseInsensitiveProperties()
    {
        var json = """
        {
            "component": "FUELCELL",
            "manufacturer": "a",
        }
        """;

        // Act
        var result = JsonInputUtils.Deserialize(json);

        // Assert
        result.Should().BeOfType<FuelCellComponentInput>();
        var battery = (FuelCellComponentInput)result;
        battery.Component.Should().Be(ComponentType.FuelCell);
        battery.Manufacturer.Should().Be("a");
    }
}