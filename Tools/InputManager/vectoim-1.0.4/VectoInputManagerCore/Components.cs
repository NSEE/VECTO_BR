using System;
using System.Collections.ObjectModel;
using Castle.Core.Internal;
using VectoObjects;

namespace VectoInputManager.Core;

/// <summary>
/// Provides metadata and configuration for all supported component types.
/// </summary>
public static class Components
{
    /// <summary>
    /// Collection of all supported component types with their metadata, calculators, and
    /// declaration types.
    /// </summary>
    public static ComponentInformationCollection ComponentInformation = new()
    {
        new ComponentTypeInfo()
        {
            ComponentType = ComponentType.GearBox,
            Type = typeof(GearboxComponentDeclarationType_v2_0),
            Calc = typeof(GearboxComponentStandardValues),
            InputType = typeof(GearboxComponentInput),
            Code = "G",
            Description = "Transmission",
            Declarations = [
                new ComponentDeclarationInfo()
                {
                    Type = typeof(GearboxDataDeclarationType_v2_3),
                    Description = "Transmission v2.3",
                    StandarValuesEnabled = true
                },
                new ComponentDeclarationInfo()
                {
                    Type = typeof(GearboxFWDDataDeclarationType_v2_3),
                    Description = "Transmission (FWD) v2.3"
                }
            ]
        },

        new ComponentTypeInfo()
        {
            ComponentType = ComponentType.TC,
            Type = typeof(TorqueConverterComponentDeclarationType_v2_0),
            Calc = typeof(TorqueConverterComponentStandardValues),
            InputType = typeof(TorqueConverterComponentInput),
            Code = "C",
            Description = "Torque Converter",
            Declarations = [
                new ComponentDeclarationInfo()
                {
                    Type = typeof(TorqueConverterDataDeclarationType_v2_0),
                    Description = "Torque Converter v2.0",
                    StandarValuesEnabled = true
                }
            ]
        },

        new ComponentTypeInfo()
        {
            ComponentType = ComponentType.Retarder,
            Type = typeof(RetarderComponentDeclarationType_v2_0),
            Calc = typeof(RetarderComponentStandardValues),
            InputType = typeof(RetarderComponentInput),
            Code = "O",
            Description = "Retarder",
            Declarations = [
                new ComponentDeclarationInfo()
                {
                    Type = typeof(RetarderDataDeclarationType_v2_0),
                    Description = "Retarder v2.0",
                    StandarValuesEnabled = true
                }
            ]
        },

        new ComponentTypeInfo()
        {
            ComponentType = ComponentType.ADC,
            Type = typeof(ADCComponentDeclarationType_v2_3),
            Calc = typeof(ADCComponentStandardValues),
            InputType = typeof(ADCComponentInput),
            Code = "D",
            Description = "Additional Drivetrain Component",
            Declarations = [
                new ComponentDeclarationInfo()
                {
                    Type = typeof(ADCDataDeclarationType_v2_3),
                    Description = "ADC v2.3",
                    StandarValuesEnabled = true
                }
            ]
        },

        new ComponentTypeInfo()
        {
            ComponentType = ComponentType.Angledrive,
            Type = typeof(AngledriveComponentDeclarationType_v2_0),
            Calc = typeof(AngledriveComponentStandardValues),
            InputType = typeof(AngledriveComponentInput),
            Code = "D",
            Description = "Angle Drive",
            Declarations = [
                new ComponentDeclarationInfo()
                {
                    Type = typeof(AngledriveDataDeclarationType_v2_0),
                    Description = "Angle Drive v2.0",
                    StandarValuesEnabled = true
                }
            ]
        },

        new ComponentTypeInfo()
        {
            ComponentType =  ComponentType.Axle,
            Type = typeof(AxlegearComponentDeclarationType_v2_0),
            Calc = typeof(AxleComponentStandardValues),
            InputType = typeof(AxleComponentInput),
            Code = "L",
            Description = "Axle",
            Declarations = [
                new ComponentDeclarationInfo()
                {
                    Type = typeof(AxlegearDataDeclarationType_v2_0),
                    Description = "Axle v2.0",
                    StandarValuesEnabled = true
                }
            ]
        },

        new ComponentTypeInfo()
        {
            ComponentType = ComponentType.EMS,
            Type = typeof(ElectricMachineSystemComponentDeclarationType_v2_3),
            Calc = typeof(EMSComponentStandardValues),
            InputType = typeof(EMSComponentInput),
            Code = "M",
            Description = "Electric Motor System",
            Declarations = [
                new ComponentDeclarationInfo()
                {
                    Type = typeof(ElectricMachineSystemStandardValuesDataDeclarationType_v2_3),
                    Description = "Standard Values v2.3",
                    StandarValuesEnabled = true
                },
                new ComponentDeclarationInfo()
                {
                    Type = typeof(ElectricMachineSystemMeasuredDataDeclarationType_v2_3),
                    Description = "Measured v2.3"
                },
                new ComponentDeclarationInfo()
                {
                    Type = typeof(ElectricMachineSystemIHPCMeasuredDataDeclarationType_v2_3),
                    Description = "IHPC Measured v2.3"
                },
            ]
        },

        new ComponentTypeInfo()
        {
            ComponentType = ComponentType.IEPC,
            Type = typeof(IEPCComponentDeclarationType_v2_3),
            Calc = typeof(IEPCComponentStandardValues),
            InputType = typeof(IEPCComponentInput),
            Code = "I",
            Description = "Integrated Electric Powertrain",
            Declarations = [
                new ComponentDeclarationInfo()
                {
                    Type = typeof(IEPCStandardValuesDataDeclarationType_v2_6),
                    Description = "Standard Values v2.6",
                    StandarValuesEnabled = true
                },
                new ComponentDeclarationInfo()
                {
                    Type = typeof(IEPCMeasuredDataDeclarationType_v2_6),
                    Description = "Measured v2.6"
                },
            ]
        },

        new ComponentTypeInfo()
        {
            ComponentType = ComponentType.Battery,
            Type = typeof(BatteryComponentDeclarationType_v2_3),
            Calc = typeof(BatteryComponentStandardValues),
            InputType = typeof(BatteryComponentInput),
            Code = "B",
            Description = "Battery System",
            Declarations = [
                new ComponentDeclarationInfo()
                {
                    Type = typeof(BatterySystemStandardValuesDataType_v2_6),
                    Description = "Standard Values v2.6",
                    StandarValuesEnabled = true
                },
                new ComponentDeclarationInfo()
                {
                    Type = typeof(BatterySystemDataType_v2_3),
                    Description = "Measured v2.3"
                },
            ]
        },

        new ComponentTypeInfo()
        {
            ComponentType = ComponentType.Capacitor,
            Type = typeof(CapacitorSystemComponentDeclarationType_v2_3),
            Calc = typeof(CapacitorComponentStandardValues),
            InputType = typeof(CapacitorComponentInput),
            Code = "A",
            Description = "Capacitor System",
            Declarations = [
                new ComponentDeclarationInfo()
                {
                    Type = typeof(CapacitorSystemDataType_v2_6),
                    Description = "Capacitor v2.6",
                    StandarValuesEnabled = true
                }
            ]
        },

        new ComponentTypeInfo()
        {
            ComponentType = ComponentType.FuelCell,
            Type = typeof(FuelCellComponentDeclarationType_v2_6),
            Calc = typeof(FuelCellComponentStandardValues),
            InputType = typeof(FuelCellComponentInput),
            Code = "F",
            Description = "Fuel Cell",
            Declarations = [
                new ComponentDeclarationInfo()
                {
                    Type = typeof(FuelCellDataDeclarationType_v2_6),
                    Description = "Fuel Cell v2.6",
                    StandarValuesEnabled = true
                }
            ]
        }
    };

    /// <summary>
    /// Metadata describing a component type including its calculator, input type, and available declarations.
    /// </summary>
    public record ComponentTypeInfo
    {
        /// <summary>Gets or sets the component type identifier.</summary>
        public ComponentType ComponentType { get; set; }

        /// <summary>Gets or sets the runtime type for this component.</summary>
        public required Type Type { get; set; }

        /// <summary>Gets or sets the calculator type for generating standard values.</summary>
        public required Type Calc { get; set; }

        /// <summary>Gets or sets the input data type for user-provided values.</summary>
        public required Type InputType { get; set; }

        /// <summary>
        /// Gets or sets the single-letter code used in certification numbers (e.g., "G" for Gearbox).
        /// </summary>
        public required string Code { get; set; }

        /// <summary>Gets or sets the human-readable description of this component type.</summary>
        public required string Description { get; set; }

        /// <summary>Gets or sets the available declaration schema versions for this component.</summary>
        public required ComponentDeclarationInfo[] Declarations { get; set; }
    }

    /// <summary>
    /// Metadata for a specific component declaration schema version.
    /// </summary>
    public record ComponentDeclarationInfo
    {
        /// <summary>Runtime type for this declaration version.</summary>
        public required Type Type { get; set; }

        /// <summary>Human-readable description (e.g., "Transmission v2.3").</summary>
        public required string Description { get; set; }

        /// <summary>
        /// Whether this declaration version supports standard values generation. 
        /// </summary>
        public bool StandarValuesEnabled { get; set; }
    }

    /// <summary>
    /// Keyed collection of component information indexed by ComponentType.
    /// </summary>
    public class ComponentInformationCollection : KeyedCollection<ComponentType, ComponentTypeInfo>
    {
        protected override ComponentType GetKeyForItem(ComponentTypeInfo item)
        {
            return item.ComponentType;
        }
    }
}

/// <summary>
/// Defines the types of vehicle components supported for Standard Values.
/// </summary>
public enum ComponentType
{
    GearBox, // G
    TC, // TorqueConverter - C
    Retarder, // Other Torque Transferring Component - O
    ADC, //  Additional Drivetrain Component - D
    Angledrive, //- D
    Axle, // L
    EMS, // ElectricMachineSystem - M
    IEPC, // Integrated Electric Powertrain Component - I
    Battery, // Battery System - B
    Capacitor, // Capacitor System - A
    FuelCell, // Fuel Cell - F

    //AirDrag, // P
    //Tyre, // T
    //Engine, // E
    //IHPC, // Integrated HEV Powertrain Component Type 1 - H
}
