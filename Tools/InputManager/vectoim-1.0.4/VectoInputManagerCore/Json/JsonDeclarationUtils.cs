using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using VectoObjects;

namespace VectoInputManager.Core;

/// <summary>
/// Provides JSON serialization and deserialization utilities for VECTO vehicle and component
/// declarations. Handles type information, schema references, and specialized converters for
/// VECTO-specific patterns.
/// </summary>
public static partial class JsonDeclarationUtils
{
    static readonly JsonSerializerSettings Settings;

    static JsonDeclarationUtils()
    {
        Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.All,
            SerializationBinder = new SimpleTypeNameBinder(),
            // Ensure metadata properties like $type are read even if they are not at the start
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            ContractResolver = new IgnoreStringPropertiesResolver(),
        };

        Settings.Converters.Add(new StringEnumConverter());
        Settings.Converters.Add(new ValueSpecifiedConverter());
    }

    /// <summary>
    /// Serializes a vehicle or component declaration to JSON format.
    /// </summary>
    /// <param name="obj">The vehicle or component declaration object to serialize.</param>
    /// <param name="addSchema">
    /// Whether to include a $schema property referencing the JSON schema URL.
    /// </param>
    /// <returns>The JSON string representation.</returns>
    /// <exception cref="ArgumentException">Thrown when obj is not a vehicle or component declaration.</exception>
    public static string Serialize(object obj, bool addSchema)
    {
        if (obj is not IVehicleDeclaration
            && obj is not IComponentDeclaration)
        {
            throw new ArgumentException($"Found object {obj.GetType().Name}. " +
                 $"Must be of type {nameof(IVehicleDeclaration)} or {nameof(IComponentDeclaration)}",
               nameof(obj));
        }

        if (addSchema)
        {
            // Figure out schema URL 
            var path = obj is IVehicleDeclaration
                ? "VehicleSchemas"
                : "ComponentSchemas";

            var typeName = obj.GetType().Name;
            var schema = Consts.SchemaBaseUrl + $"{path}/{typeName}.schema.json";

            var jsonString = JsonConvert.SerializeObject(obj, Settings);
            var jsonObject = JObject.Parse(jsonString);

            // Add the $schema property at the beginning
            jsonObject.AddFirst(new JProperty("$schema", schema));

            return jsonObject.ToString(Formatting.Indented);
        }
        else
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }
    }

    /// <summary>
    /// Serializes a vehicle or component declaration to a JSON file.
    /// </summary>
    /// <param name="obj">The vehicle or component declaration object to serialize.</param>
    /// <param name="filePath">The target file path.</param>
    /// <param name="addSchema">
    /// Whether to include a $schema property referencing the JSON schema URL.
    /// </param>
    public static void SerializeToFile(object obj, string filePath, bool addSchema)
    {
        var json = Serialize(obj, addSchema);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Deserializes a JSON string to a vehicle or component declaration object.
    /// The JSON must contain a $type property to determine the concrete type.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized vehicle or component declaration object.</returns>
    public static object Deserialize(string json)
    {
        return JsonConvert.DeserializeObject<object>(json, Settings)!;
    }

    /// <summary>
    /// Deserializes a JSON file to a vehicle or component declaration object.
    /// </summary>
    /// <param name="filePath">The source file path.</param>
    /// <returns>The deserialized vehicle or component declaration object.</returns>
    public static object DeserializeFromFile(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return Deserialize(json);
    }

}
