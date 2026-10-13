using System;
using VectoObjects;

namespace VectoObjects
{
    /// <summary>
    /// VectoObjects.EngineComponentDeclarationType_v2_0
    ///  Types inheriting from AbstractCombustionEngineDataDeclarationType_v1_0:
    ///    VectoObjects.EngineDataDeclarationType_v1_0
    ///    VectoObjects.EngineDataDeclarationType_v2_0
    ///    VectoObjects.EngineDataDeclarationType_v2_1
    ///    VectoObjects.EngineDataDeclarationType_v2_3
    ///    VectoObjects.EngineDataDeclarationType_v2_6
    /// </summary>
    public partial class EngineComponentDeclarationType_v2_0 : IComponentDeclaration { }
    public partial class EngineDataDeclarationType_v2_6 : IComponentDataDeclaration { }

    /// <summary>
    /// VectoObjects.GearboxComponentDeclarationType_v2_0
    ///  Types inheriting from AbstractTransmissionDataDeclarationType_v1_0:
    ///    VectoObjects.GearboxDataDeclarationType_v1_0
    ///    VectoObjects.GearboxDataDeclarationType_v2_0
    ///    VectoObjects.GearboxDataDeclarationType_v2_3
    ///    VectoObjects.GearboxFWDDataDeclarationType_v2_3
    /// </summary>
    public partial class GearboxComponentDeclarationType_v2_0 : IComponentDeclaration { }
    public partial class GearboxDataDeclarationType_v2_3 : IComponentDataDeclaration { }
    public partial class GearboxFWDDataDeclarationType_v2_3 : IComponentDataDeclaration { }

    /// <summary>
    /// VectoObjects.TorqueConverterComponentDeclarationType_v2_0
    ///  Types inheriting from AbstractTorqueConverterDataDeclarationType_v1_0:
    ///    VectoObjects.TorqueConverterDataDeclarationType_v1_0
    ///    VectoObjects.TorqueConverterDataDeclarationType_v2_0
    /// </summary>
    public partial class TorqueConverterComponentDeclarationType_v2_0 : IComponentDeclaration { }
    public partial class TorqueConverterDataDeclarationType_v2_0 : IComponentDataDeclaration { }

    /// <summary>
    /// VectoObjects.RetarderComponentDeclarationType_v2_0
    ///  Types inheriting from AbstractRetarderDataDeclarationType_v1_0:
    ///    VectoObjects.RetarderDataDeclarationType_v1_0
    ///    VectoObjects.RetarderDataDeclarationType_v2_0
    /// </summary>
    public partial class RetarderComponentDeclarationType_v2_0 : IComponentDeclaration { }
    public partial class RetarderDataDeclarationType_v2_0 : IComponentDataDeclaration { }

    // ADC
    /// <summary>
    /// VectoObjects.ADCComponentDeclarationType_v2_3
    ///  Types inheriting from AbstractADCDataDeclarationType_v2_3:
    ///    VectoObjects.ADCDataDeclarationType_v2_3
    /// </summary>
    public partial class ADCComponentDeclarationType_v2_3 : IComponentDeclaration { }
    public partial class ADCDataDeclarationType_v2_3 : IComponentDataDeclaration { }

    /// <summary>
    /// VectoObjects.AngledriveComponentDeclarationType_v2_0
    ///  Types inheriting from AbstractAngledriveDataDeclarationType_v1_0:
    ///    VectoObjects.AbstractADCDataDeclarationType_v2_3
    ///    VectoObjects.ADCDataDeclarationType_v2_3
    ///    VectoObjects.AngledriveDataDeclarationType_v1_0
    ///    VectoObjects.AngledriveDataDeclarationType_v2_0
    /// </summary>
    public partial class AngledriveComponentDeclarationType_v2_0 : IComponentDeclaration { }
    public partial class AngledriveDataDeclarationType_v2_0 : IComponentDataDeclaration { }

    /// <summary>
    /// VectoObjects.AxlegearComponentDeclarationType_v2_0
    ///  Types inheriting from AbstractAxlegearDataDeclarationType_v1_0:
    ///    VectoObjects.AxlegearDataDeclarationType_v1_0
    ///    VectoObjects.AxlegearDataDeclarationType_v2_0
    /// </summary>
    public partial class AxlegearComponentDeclarationType_v2_0 : IComponentDeclaration { }
    public partial class AxlegearDataDeclarationType_v2_0 : IComponentDataDeclaration { }

    // AirDrag
    /// <summary>
    /// VectoObjects.AirDragComponentDeclarationType_v2_0
    ///  Types inheriting from AbstractAirdragDataDeclarationType_v1_0:
    ///    VectoObjects.AirDragDataDeclarationType_v1_0
    ///    VectoObjects.AirDragDataDeclarationType_v2_0
    ///    VectoObjects.AirDragDataDeclarationType_v2_6
    ///    VectoObjects.AirDragModifiedUseStandardValueType_v2_4
    /// </summary>
    public partial class AirDragComponentDeclarationType_v2_0 : IComponentDeclaration { }
    public partial class AirDragDataDeclarationType_v2_6 : IComponentDataDeclaration { }

    /// <summary>
    /// VectoObjects.TyreComponentDeclarationType_v2_0
    ///  Types inheriting from AbstractTyreDataDeclarationType_v1_0:
    ///    VectoObjects.TyreDataDeclarationType_v1_0
    ///    VectoObjects.TyreDataDeclarationType_v2_0
    ///    VectoObjects.TyreDataDeclarationType_v2_2
    ///    VectoObjects.TyreDataDeclarationType_v2_3
    ///    VectoObjects.TyreDataDeclarationType_v2_5
    /// </summary>
    public partial class TyreComponentDeclarationType_v2_0 : IComponentDeclaration { }

    public partial class TyreDataDeclarationType_v2_5 : IComponentDataDeclaration { }

    // EMS
    /// <summary>
    /// VectoObjects.ElectricMachineSystemComponentDeclarationType_v2_3
    ///  Types inheriting from AbstractElectricMachineSystemDataType_v2_3:
    ///    VectoObjects.ElectricMachineSystemIHPCMeasuredDataDeclarationType_v2_3
    ///    VectoObjects.ElectricMachineSystemMeasuredDataDeclarationType_v2_3
    ///    VectoObjects.ElectricMachineSystemStandardValuesDataDeclarationType_v2_3
    /// </summary>
    public partial class ElectricMachineSystemComponentDeclarationType_v2_3 : IComponentDeclaration { }
    public partial class ElectricMachineSystemIHPCMeasuredDataDeclarationType_v2_3 : IComponentDataDeclaration { }
    public partial class ElectricMachineSystemMeasuredDataDeclarationType_v2_3 : IComponentDataDeclaration { }
    public partial class ElectricMachineSystemStandardValuesDataDeclarationType_v2_3 : IComponentDataDeclaration { }

    // IEPC
    /// <summary>
    /// VectoObjects.IEPCComponentDeclarationType_v2_3
    ///  Types inheriting from AbstractIEPCDataType_v2_3:
    ///    VectoObjects.IEPCMeasuredDataDeclarationType_v2_3
    ///    VectoObjects.IEPCMeasuredDataDeclarationType_v2_6
    ///    VectoObjects.IEPCStandardValuesDataDeclarationType_v2_3
    ///    VectoObjects.IEPCStandardValuesDataDeclarationType_v2_6
    /// </summary>
    public partial class IEPCComponentDeclarationType_v2_3 : IComponentDeclaration { }
    public partial class IEPCMeasuredDataDeclarationType_v2_6 : IComponentDataDeclaration { }
    public partial class IEPCStandardValuesDataDeclarationType_v2_6 : IComponentDataDeclaration { }


    // Battery
    /// <summary>
    /// VectoObjects.BatteryComponentDeclarationType_v2_3
    ///  Types inheriting from AbstractBatterySystemDataType_v2_3:
    ///    VectoObjects.BatterySystemDataType_v2_3
    ///    VectoObjects.BatterySystemStandardValuesDataType_v2_3
    ///    VectoObjects.BatterySystemStandardValuesDataType_v2_6
    /// </summary>
    public partial class BatteryComponentDeclarationType_v2_3 : IComponentDeclaration { }
    public partial class BatterySystemDataType_v2_3 : IComponentDataDeclaration { }
    public partial class BatterySystemStandardValuesDataType_v2_6 : IComponentDataDeclaration { }

    // Capacitor
    /// <summary>
    /// VectoObjects.CapacitorSystemComponentDeclarationType_v2_3
    ///  Types inheriting from AbstractCapacitorSystemDataType_v2_3:
    ///    VectoObjects.CapacitorSystemDataType_v2_3
    ///    VectoObjects.CapacitorSystemDataType_v2_6
    /// </summary>
    public partial class CapacitorSystemComponentDeclarationType_v2_3 : IComponentDeclaration { }
    public partial class CapacitorSystemDataType_v2_6 : IComponentDataDeclaration { }

    // FuelCell
    /// <summary>
    /// VectoObjects.FuelCellComponentDeclarationType_v2_6
    ///  Types inheriting from AbstractFuelCellDataType_v2_6:
    ///    VectoObjects.FuelCellDataDeclarationType_v2_6
    /// </summary>

    public partial class FuelCellComponentDeclarationType_v2_6 : IComponentDeclaration { }
    public partial class FuelCellDataDeclarationType_v2_6 : IComponentDataDeclaration { }

    // Marker interface for REESS Components (rechargeable electric energy storage system)
    public interface IREESSComponentDelaration : IComponentDeclaration { }

    public partial class REESSBatteryTypeREESS_v2_4 : IREESSComponentDelaration { }
    // AbstractBatterySystemDataType_v2_3 Data
    // > BatterySystemDataType_v2_3
    // > BatterySystemStandardValuesDataType_v2_3
    // > BatterySystemStandardValuesDataType_v2_6

    public partial class REESSCapacitorType_v2_4 : IREESSComponentDelaration { }
    // AbstractCapacitorSystemDataType_v2_3 Data
    // > CapacitorSystemDataType_v2_3
    // > CapacitorSystemDataType_v2_6

    public partial class REESSComponentDeclarationType_v2_3 : IREESSComponentDelaration { }
    // AbstractREESSDataType_v2_3 Data
    // > BatterySystemDataType_v2_3
    // > BatterySystemStandardValuesDataType_v2_6
    // > BatterySystemStandardValuesDataType_v2_3
    // > CapacitorSystemDataType_v2_3
    // > CapacitorSystemDataType_v2_6

    //////////////////////////
    // Support for v1.0 schema
    //////////////////////////

    public partial class AirDragComponentDeclarationType_v1_0 : IComponentDeclaration
    {
        SignatureType_v2_0 IComponentDeclaration.Signature { get; set; }
    }
    public partial class AngledriveComponentDeclarationType_v1_0 : IComponentDeclaration
    {
        SignatureType_v2_0 IComponentDeclaration.Signature { get; set; }
    }
    public partial class TyreComponentDeclarationType_v1_0 : IComponentDeclaration
    {
        SignatureType_v2_0 IComponentDeclaration.Signature { get; set; }
    }
    public partial class AxlegearComponentDeclarationType_v1_0 : IComponentDeclaration
    {
        SignatureType_v2_0 IComponentDeclaration.Signature { get; set; }
    }
    public partial class EngineComponentDeclarationType_v1_0 : IComponentDeclaration
    {
        SignatureType_v2_0 IComponentDeclaration.Signature { get; set; }
    }
    public partial class GearboxComponentDeclarationType_v1_0 : IComponentDeclaration
    {
        SignatureType_v2_0 IComponentDeclaration.Signature { get; set; }
    }
    public partial class TorqueConverterComponentDeclarationType_v1_0 : IComponentDeclaration
    {
        SignatureType_v2_0 IComponentDeclaration.Signature { get; set; }
    }
    public partial class RetarderComponentDeclarationType_v1_0 : IComponentDeclaration
    {
        SignatureType_v2_0 IComponentDeclaration.Signature { get; set; }
    }
}