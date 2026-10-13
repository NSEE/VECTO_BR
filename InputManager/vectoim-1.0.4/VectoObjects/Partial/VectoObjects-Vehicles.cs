using System;
using VectoObjects;

namespace VectoObjects
{
    public interface IVehicleJobDeclaration
    {
        IVehicleDeclaration Vehicle { get; set; }
        decimal schemaVersion { get; set; }
        Type VectoInputDeclarationType { get; }
        decimal SchemaVersionInternal { get; }
    }

    public interface IVectoInputDeclaration {
        decimal schemaVersion { get; set; }
    }

    public interface IVehicleDeclaration
    {
        string Manufacturer { get; set; }
        string ManufacturerAddress { get; set; }
        string Model { get; set; }
        string VIN { get; set; }
        DateTime Date { get; set; }
    }

    // DeclarationInput:v3.0
    public partial class VectoDeclarationJobType_v2_7 : IVehicleJobDeclaration
    {
        Type IVehicleJobDeclaration.VectoInputDeclarationType => typeof(VectoInputDeclaration_v2_2);
        decimal IVehicleJobDeclaration.SchemaVersionInternal => 2;

        IVehicleDeclaration IVehicleJobDeclaration.Vehicle
        {
            get => (IVehicleDeclaration)Vehicle;
            set => Vehicle = (AbstractVehicleDeclarationType_v2_0)value;
        }
    }

    // DeclarationInput:v2.0
    public partial class VectoDeclarationJobType_v2_0 : IVehicleJobDeclaration
    {
        Type IVehicleJobDeclaration.VectoInputDeclarationType => typeof(VectoInputDeclaration_v2_2);
        decimal IVehicleJobDeclaration.SchemaVersionInternal => 2;
        IVehicleDeclaration IVehicleJobDeclaration.Vehicle
        {
            get => (IVehicleDeclaration)Vehicle;
            set => Vehicle = (AbstractVehicleDeclarationType_v2_0)value;
        }
    }

    // DeclarationInput:v1.0
    public partial class VectoDeclarationJobType_v1_0 : IVehicleJobDeclaration
    {
        Type IVehicleJobDeclaration.VectoInputDeclarationType => typeof(VectoInputDeclaration_v1_0);
        decimal IVehicleJobDeclaration.SchemaVersionInternal => 1;
        IVehicleDeclaration IVehicleJobDeclaration.Vehicle
        {
            get => (IVehicleDeclaration)Vehicle;
            set => Vehicle = (VehicleDeclarationType_v1_0)value;
        }
    }

    public partial class VectoInputDeclaration_v2_2 : IVectoInputDeclaration
    {
    }
    public partial class VectoInputDeclaration_v1_0 : IVectoInputDeclaration
    {
    }
    public partial class VectoInputDeclaration_v2_0 : IVectoInputDeclaration
    {
    }
    public partial class VectoInputDeclaration_v2_1 : IVectoInputDeclaration
    {
    }
}
