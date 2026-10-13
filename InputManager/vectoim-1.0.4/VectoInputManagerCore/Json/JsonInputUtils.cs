using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace VectoInputManager.Core;

/// <summary>
/// Provides JSON serialization and deserialization utilities for component input data files. These
/// files contain user-provided parameters for generating standard value components.
/// </summary>
public static class JsonInputUtils
{
    static readonly JsonSerializerSettings Settings;

    static JsonInputUtils()
    {
        Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
        };
        Settings.Converters.Add(new StringEnumConverter());
        Settings.Converters.Add(new BaseInputConverter());
    }

    /// <summary>
    /// Serializes a component input object to JSON format.
    /// </summary>
    /// <param name="obj">The BaseInput-derived object to serialize.</param>
    /// <param name="addSchema">
    /// Whether to include a $schema property referencing the JSON schema URL.
    /// </param>
    /// <returns>The JSON string representation.</returns>
    public static string Serialize(BaseInput obj, bool addSchema)
    {
        if (addSchema)
        {
            var ct = Components.ComponentInformation[obj.Component].ComponentType;
            var schema = Consts.SchemaBaseUrl + $"InputSchemas/{ct}.schema.json";

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
    /// Deserializes a JSON string to a component input object. The JSON must contain a "Component"
    /// property to determine the concrete type.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized BaseInput-derived object.</returns>
    public static BaseInput Deserialize(string json)
    {
        return JsonConvert.DeserializeObject<BaseInput>(json, Settings)!;
    }
}

