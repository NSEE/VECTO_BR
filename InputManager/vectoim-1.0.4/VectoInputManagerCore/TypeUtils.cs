using Fasterflect;
using Newtonsoft.Json;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;

using VectoObjects;

namespace VectoInputManager.Core;

/// <summary>
/// Provides utility methods for working with types, including type discovery, component
/// declarations, and type conversion operations.
/// </summary>
public static partial class TypeUtils
{
    /// <summary>
    /// Retrieves all types that implement <see cref="VectoObjects.AbstractVehicleDeclarationType"/>
    /// and includes <see cref="VehicleDeclarationType"/>.
    /// </summary>
    /// <returns>A list of concrete vehicle declaration types.</returns>
    public static IReadOnlyList<Type> GetAllVehicleTypes()
    {
        var baseType = typeof(AbstractVehicleDeclarationType_v2_0);
        var additionalType = typeof(VehicleDeclarationType_v1_0);

        // Get all types in the current AppDomain that implement the base type or match the additional type
        var vehicleTypes = JobAssembly.GetTypes()
            .Where(type => baseType.IsAssignableFrom(type) || type == additionalType)
            .Where(Type => !Type.IsAbstract)
            .ToList();

        return vehicleTypes;
    }

    /// <summary>
    /// Retrieves all concrete types that implement <see cref="IComponentDeclaration"/>.
    /// </summary>
    /// <returns>A list of all component declaration types.</returns>
    public static IReadOnlyList<Type> GetAllComponentDeclarationTypes()
    {
        var baseType = typeof(IComponentDeclaration);

        // Get all types in the current AppDomain that implement the base type or match the additional type
        var vehicleTypes = JobAssembly
    .GetTypes()
            .Where(type => baseType.IsAssignableFrom(type))
   .Where(Type => !Type.IsAbstract)
     .ToList();

        return vehicleTypes;
    }

    /// <summary>
    /// Gets the specified type and all its concrete descendant types.
    /// </summary>
    /// <param name="type">The base type to find descendants for.</param>
    /// <returns>An enumerable of the type itself and all its non-abstract subclasses.</returns>
    /// <exception cref="ArgumentNullException">Thrown when type is null.</exception>
    public static IEnumerable<Type> GetSelfAndDescendantTypes(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return JobAssembly.GetTypes()
            .Where(t => t.IsSubclassOf(type) || t == type)
            .Where(t => !t.IsAbstract);
    }

    /// <summary>
    /// Recursively finds all component declarations within an object hierarchy. Traverses through
    /// properties and collections to locate IComponentDeclaration instances.
    /// </summary>
    /// <param name="obj">The root object to search within.</param>
    /// <param name="parentPath">
    /// The path prefix for tracking location (optional, for internal recursion).
    /// </param>
    /// <returns>An enumerable of tuples containing each found component and its path.</returns>
    public static IEnumerable<(IComponentDeclaration component, string path)>
        FindComponentDelarations(object obj, string? parentPath = null)
    {
        if (obj == null)
            yield break;

        var type = obj.GetType();

        // If the object itself is an IComponentDeclaration, yield it
        if (typeof(IComponentDeclaration).IsAssignableFrom(type))
        {
            yield return ((IComponentDeclaration)obj, parentPath ?? "");
            yield break; // Exit - don't recurse into component properties
        }

        // If it's a collection, iterate its items
        if (type.IsGenericType &&
          type.GetGenericTypeDefinition() == typeof(List<>))
        {
            int index = 0;
            var path = parentPath;
            foreach (var item in (System.Collections.IEnumerable)obj)
            {
                foreach (var found in FindComponentDelarations(item, path + $"[{index++}]"))
                    yield return found;
            }
            yield break;
        }

        // Otherwise, recurse into its public instance properties
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.PropertyType.IsClass || prop.PropertyType.IsArray)
                continue;

            var value = prop.GetValue(obj);
            if (value == null)
                continue;

            var path = (string.IsNullOrEmpty(parentPath) ? "" : parentPath + ".") + prop.Name;

            foreach (var found in FindComponentDelarations(value, path))
                yield return found;
        }
    }

    /// <summary>
    /// Resolves a type name to a Type object from the VectoObjects assembly.
    /// </summary>
    /// <param name="typeName">The simple name of the type (without namespace).</param>
    /// <returns>The resolved Type.</returns>
    /// <exception cref="TypeLoadException">Thrown when the type cannot be found.</exception>
    public static Type GetTypeByName(string typeName)
    {
        var type = Type.GetType($"VectoObjects.{typeName}, VectoObjects")
            ?? throw new TypeLoadException($"Type '{typeName}' not found.");
        return type;
    }

    /// <summary>
    /// Resolves a standard values type name to a Type object from the VectoInputManagerCore assembly.
    /// </summary>
    /// <param name="typeName">The simple name of the type (without namespace).</param>
    /// <returns>The resolved Type.</returns>
    /// <exception cref="TypeLoadException">Thrown when the type cannot be found.</exception>
    public static Type GetTypeByNameStandardValues(string typeName)
    {
        var type = Type.GetType($"VectoInputManager.Core.{typeName}, VectoInputManagerCore")
            ?? throw new TypeLoadException($"Type '{typeName}' not found.");
        return type;
    }

    /// <summary>
    /// Attempts to resolve a type name to a Type object, returning null if not found.
    /// </summary>
    /// <param name="typeName">The simple name of the type (without namespace).</param>
    /// <returns>The resolved Type, or null if not found.</returns>
    public static Type? GetTypeByNameOrNull(string typeName)
    {
        return Type.GetType($"VectoObjects.{typeName}, VectoObjects");
    }

    /// <summary>
    /// Creates a new VectoInputDeclaration_v2_2 instance with the schema version set.
    /// </summary>
    /// <returns>A new VectoInputDeclaration_v2_2 instance.</returns>
    public static VectoInputDeclaration_v2_2 CreateVectoInputDeclaration()
    {
        return new VectoInputDeclaration_v2_2()
        {
            schemaVersion = 2.2m,
        };
    }

    /// <summary>
    /// Determines if a component is a scaffolding/placeholder component by checking if its Date
    /// property is set to default. Scaffolding components are typically empty templates that
    /// haven't been properly initialized.
    /// </summary>
    /// <param name="cd">The component declaration to check.</param>
    /// <returns>True if the component is a scaffolding component; otherwise, false.</returns>
    public static bool IsScaffoldingComponent(this IComponentDeclaration cd)
    {
        var data = cd.GetPropertyValue("Data");
        if (data != null)
        {
            var date = data.TryGetPropertyValue("Date");
            if (date != null && date is DateTime dt && dt == default)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Ensures that a component has an ID set in its Data property. If no ID exists, generates one
    /// using an auto-incrementing counter.
    /// </summary>
    /// <param name="cd">The component declaration to ensure has an ID.</param>
    public static void EnsureIdSet(this IComponentDeclaration cd)
    {
        var data = cd.GetPropertyValue("Data");
        if (data != null)
        {
            var id = data.GetPropertyValue("id") as string;
            if (string.IsNullOrEmpty(id))
            {
                data.SetPropertyValue("id", "c" + (ComponentAutoIncrement++).ToString());
            }
        }
    }

    /// <summary>
    /// Sets a component declaration into the appropriate property of a VectoInputDeclaration_v2_2.
    /// Handles special REESS component conversion (Battery/Capacitor) automatically.
    /// </summary>
    /// <param name="vectoInputDeclaration">The target declaration to set the component into.</param>
    /// <param name="component">The component to set.</param>
    /// <exception cref="ArgumentNullException">Thrown when either parameter is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no compatible property is found or multiple matches exist.
    /// </exception>
    public static void SetComponentInVectoInputDeclaration(this IVectoInputDeclaration vectoInputDeclaration, IComponentDeclaration component)
    {
        if (component == null || vectoInputDeclaration == null)
            throw new ArgumentNullException(component == null ? nameof(component) : nameof(vectoInputDeclaration));

        // Special handling for REESS components to convert them to types
        // that can exist in VectoInputDeclaration_v2_2
        if (component is IREESSComponentDelaration)
        {
            var dataValue = component.GetPropertyValue("Data");

            string? newType = null;
            if (dataValue.GetType().Name.Contains("Capacitor"))
                newType = nameof(CapacitorSystemComponentDeclarationType_v2_3);
            if (dataValue.GetType().Name.Contains("Battery"))
                newType = nameof(BatteryComponentDeclarationType_v2_3);

            var json = JsonDeclarationUtils.Serialize(component, false);
            var rgx = TypeRegex();
            json = rgx.Replace(json, $@"""$type"": ""{newType}""", 1);

            component = (IComponentDeclaration)JsonDeclarationUtils.Deserialize(json);
        }

        bool found = false;
        var compType = component.GetType();
        var vectoType = vectoInputDeclaration.GetType();

        // Iterate over all public instance properties of VectoInputDeclaration_v2_2
        foreach (var prop in vectoType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propType = prop.PropertyType;

            // Check if the property type is assignable from the component's type
            if (propType.IsAssignableFrom(compType))
            {
                prop.SetValue(vectoInputDeclaration, component);

                if (found)
                    throw new InvalidOperationException($"Multiple compatible properties found in {vectoType.Name} for component of type {compType.Name}.");

                found = true;
            }
        }

        if (!found)
        {
            throw new InvalidOperationException($"No compatible property found in {vectoType.Name} for component of type {compType.Name}.");
        }
    }

    /// <summary>
    /// Upgrades the component to a compatible type for VectoInputDeclaration_v2_2
    /// if it's an older version v1_0.
    /// </summary>
    public static IComponentDeclaration UpgradeComponent(IComponentDeclaration component)
    {
        // Special handling for v1_0 components: convert to v2_0 types
        // that can exist in VectoInputDeclaration_v2_2
        if (component.GetType().Name.EndsWith("_v1_0"))
        {
            var newType = component.GetType().Name.Replace("_v1_0", "_v2_0");
            var json = JsonDeclarationUtils.Serialize(component, false);
            var rgx = TypeRegex();
            json = rgx.Replace(json, $@"""$type"": ""{newType}""", 1);
            component = (IComponentDeclaration)JsonDeclarationUtils.Deserialize(json);
        }

        return component;
    }

    public static object? DeepCopy(object obj)
    {
        if (obj == null)
            return null;

        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
        };

        var json = JsonConvert.SerializeObject(obj, settings);
        return JsonConvert.DeserializeObject(json, settings);
    }

    /// <summary>
    /// The VectoObjects assembly containing all vehicle and component types.
    /// </summary>
    private static readonly Assembly JobAssembly = typeof(VectoDeclarationJobType_v2_0).Assembly;

    /// <summary>
    /// Regular expression to match and replace $type properties in JSON serialization.
    /// </summary>
    [GeneratedRegex(@"""(\$type)""\s*:\s*""[^""]*""")]
    private static partial Regex TypeRegex();

    /// <summary>
    /// Auto-incrementing counter for generating component IDs.
    /// Note: This is perfectly fine for single-threaded use but not thread-safe.
    /// </summary>
    private static int ComponentAutoIncrement = 1;
}