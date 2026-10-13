using AwesomeAssertions;
using VectoInputManager.Core;
using VectoObjects;

namespace VectoInputManager.Tests.Core;

public class TypeUtils_Tests
{
    [Test]
    public void SetComponentInVectoInputDeclaration_REESS_Convert_Test()
    {
        {
            var component = new REESSBatteryTypeREESS_v2_4();
            component.Data = new BatterySystemDataType_v2_3();
            ((BatterySystemDataType_v2_3)component.Data).BatteryType = BatteryTypeType_v2_3.HPBS;

            var vinput = TypeUtils.CreateVectoInputDeclaration();
            TypeUtils.SetComponentInVectoInputDeclaration(vinput, component);
            vinput.BatterySystem.Data.Should().NotBeNull();
        }

        {
            var component = new REESSCapacitorType_v2_4();
            component.Data = new CapacitorSystemDataType_v2_6();
            ((CapacitorSystemDataType_v2_6)component.Data).Capacitance = 10;

            var vinput = TypeUtils.CreateVectoInputDeclaration();
            TypeUtils.SetComponentInVectoInputDeclaration(vinput, component);
            vinput.CapacitorSystem.Data.Should().NotBeNull();
        }

        {
            var component = new REESSComponentDeclarationType_v2_3();
            component.Data = new BatterySystemDataType_v2_3();
            ((BatterySystemDataType_v2_3)component.Data).BatteryType = BatteryTypeType_v2_3.HPBS;

            var vinput = TypeUtils.CreateVectoInputDeclaration();
            TypeUtils.SetComponentInVectoInputDeclaration(vinput, component);
            vinput.BatterySystem.Data.Should().NotBeNull();
        }

        {
            var component = new REESSComponentDeclarationType_v2_3();
            component.Data = new CapacitorSystemDataType_v2_3();
            ((CapacitorSystemDataType_v2_3)component.Data).Capacitance = 11;

            var vinput = TypeUtils.CreateVectoInputDeclaration();
            TypeUtils.SetComponentInVectoInputDeclaration(vinput, component);
            vinput.CapacitorSystem.Data.Should().NotBeNull();
        }
    }
}