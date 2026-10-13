using System.Reflection;
using VectoInputManager.Core;
using VectoObjects;

namespace VectoInputManager.Tests.Utilities;

public partial class Helpers_Utility
{
    [Test]
    [Explicit("Finds data declaration classes. Run manually to discover and list data/component classes.")]
    public void Find_DataDeclarationClasses()
    {
        var outputDir = TestUtils.OutputPath;
        using StreamWriter writer = new StreamWriter(Path.Combine(outputDir, "Classes.txt"));

        // Get the assembly containing VectoObjects types
        var xmlDeclAssembly = typeof(AirDragDataDeclarationType_v1_0).Assembly;
        var interfaceType = typeof(IComponentDataDeclaration);

        // Get all public and non-public types in the VectoObjects namespace
        var candidates = xmlDeclAssembly.GetTypes()
            .Where(t =>
                t.IsClass &&
                t.Namespace != null &&
                t.Namespace.Contains("VectoObjects") &&
                !t.IsAbstract &&
                t.GetConstructor(Type.EmptyTypes) != null // must be instantiable
            );

        // IDataDeclaration property names and types
        var requiredProps1 = new (string Name, Type Type)[]
            {
                ("Manufacturer", typeof(string)),
                ("Model", typeof(string)),
                ("AppVersion", typeof(string)),
                ("CertificationNumber", typeof(string)),
                ("Date", typeof(DateTime)),
            };

        var requiredProps2 = new (string Name, Type Type)[]
    {
            ("Signature", typeof(SignatureType_v2_0)),
        };

        foreach (var type in candidates.OrderBy(t => t.Name))
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name, p => p.PropertyType);

            bool matches = requiredProps1.All(rp =>
                props.TryGetValue(rp.Name, out var pt) && pt == rp.Type);
            if (matches)
            {
                //writer.WriteLine($"public partial class {type.Name} : IDataDeclaration {{ }}");
                writer.WriteLine($"{type.Name},DataDeclaration");
            }

            matches = requiredProps2.All(rp =>
                props.TryGetValue(rp.Name, out var pt) && pt == rp.Type);
            if (matches)
            {
                //writer.WriteLine($"public partial class {type.Name} : IComponentDelaration {{ }}");
                writer.WriteLine($"{type.Name},ComponentDelaration");
            }
        }
    }

    [Test]
    [Explicit("Generates a CSV grid of vehicle types and their properties. Run manually to update grid.")]
    public void VehicleGrid_Utility()
    {
        // Get the assembly containing VectoObjects types
        var xmlDeclAssembly = typeof(VectoObjects.AirDragDataDeclarationType_v1_0).Assembly;

        List<Type> types = new List<Type>();

        foreach (var vt in Data.GetVehicleTypes())
        {
            types.Add(TypeUtils.GetTypeByName(vt.TypeName));
        }

        // Get all public instance properties for each type
        var propertyGroups = (types
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .DistinctBy(c => c.Name + c.PropertyType)
            .Select(p => (p.Name, p.PropertyType))
            .OrderBy(p => p.Name + p.PropertyType)
            .ToHashSet());

        // Prepare header
        var header = new List<string> { "TypeName" };
        header.AddRange(propertyGroups.Select(p => $"{p.Name} ({p.PropertyType})"));

        using StreamWriter writer = new StreamWriter(Path.Combine(
            TestUtils.OutputPath, "Grid.csv"));

        writer.WriteLine(string.Join(",", header));

        foreach (var type in types)
        {
            var values = new List<string> { type.Name };
            foreach (var (propName, propType) in propertyGroups)
            {
                var prop = type.GetProperty(propName);
                var value = prop != null && prop.PropertyType == propType ? "X" : "";
                values.Add(value);
            }
            writer.WriteLine(string.Join(",", values.Select(v => v.Replace(",", " "))));
        }
    }
}
