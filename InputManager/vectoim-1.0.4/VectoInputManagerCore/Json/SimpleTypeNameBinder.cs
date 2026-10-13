using System;
using Newtonsoft.Json.Serialization;

namespace VectoInputManager.Core;

/// <summary> 
/// Used in JSON serialization to simplify type names by omitting namespaces and
/// assemblies, restricting to a specific allowed namespace and assembly for security, and handling
/// generic types like List<T> with a shortened format. 
/// </summary>
class SimpleTypeNameBinder : ISerializationBinder
{
    private const string AllowedNamespace = "VectoObjects";
    private const string AllowedAssembly = "VectoObjects";

    public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
    {
        // Handle List<T> and similar generics
        if (serializedType.IsGenericType && serializedType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var arg = serializedType.GetGenericArguments()[0];
            if (arg.Namespace == AllowedNamespace && arg.Assembly.GetName().Name == AllowedAssembly)
            {
                // Output: [TypeName]
                typeName = $"[{arg.Name}]";
                assemblyName = null;
                return;
            }
        }

        // Only allow types from the allowed namespace/assembly
        if (serializedType.Namespace == AllowedNamespace && serializedType.Assembly.GetName().Name == AllowedAssembly)
        {
            typeName = serializedType.Name;
            assemblyName = null;
            return;
        }

        // Fallback to default behavior for other types
        typeName = serializedType.Name;
        assemblyName = null;
    }

    public Type BindToType(string? assemblyName, string typeName)
    {
        // Handle List<T> with simplified type name: [TypeName]
        if (typeName.StartsWith("[") && typeName.EndsWith("]"))
        {
            var innerTypeName = typeName.Trim('[', ']');
            var fullTypeName = $"{AllowedNamespace}.{innerTypeName}, {AllowedAssembly}";
            var innerType = Type.GetType(fullTypeName, throwOnError: true)!;
            var listType = typeof(List<>).MakeGenericType(innerType);
            return listType;
        }

        var fullType = $"{AllowedNamespace}.{typeName}, {AllowedAssembly}";
        return Type.GetType(fullType, throwOnError: true)!;
    }
}