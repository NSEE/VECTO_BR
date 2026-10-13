using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace VectoInputManager.Core;

/// <summary>
/// Custom JSON converter that handles ValueSpecified properties during deserialization.
/// When a property exists in the JSON, it sets the corresponding ValueSpecified property to true.
/// </summary>
public class ValueSpecifiedConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        // Only apply to types that actually have ValueSpecified properties
        if (objectType == typeof(object)) return false; // Don't interfere with polymorphic deserialization

        var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        return properties.Any(p => p.Name.EndsWith("ValueSpecified") && p.PropertyType == typeof(bool));
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        // Create a new serializer without this converter to avoid infinite recursion
        var settings = new JsonSerializerSettings
        {
            Formatting = serializer.Formatting,
            TypeNameHandling = serializer.TypeNameHandling,
            SerializationBinder = serializer.SerializationBinder,
            MetadataPropertyHandling = serializer.MetadataPropertyHandling,
            ContractResolver = serializer.ContractResolver,
            // Copy all converters except this one
            Converters = [.. serializer.Converters
            .Where(c => c.GetType() != typeof(ValueSpecifiedConverter))]
        };

        var newSerializer = JsonSerializer.Create(settings);
        var token = JToken.FromObject(value!, newSerializer);
        token.WriteTo(writer);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var jsonObject = JObject.Load(reader);
        var target = existingValue ?? Activator.CreateInstance(objectType)!;

        // Create a new serializer without this converter to avoid infinite recursion during population
        var settings = new JsonSerializerSettings
        {
            Formatting = serializer.Formatting,
            TypeNameHandling = serializer.TypeNameHandling,
            SerializationBinder = serializer.SerializationBinder,
            MetadataPropertyHandling = serializer.MetadataPropertyHandling,
            ContractResolver = serializer.ContractResolver,
            // Copy all converters except this one
            Converters = [.. serializer.Converters
                .Where(c => c.GetType() != typeof(ValueSpecifiedConverter))]
        };

        var newSerializer = JsonSerializer.Create(settings);
        newSerializer.Populate(jsonObject.CreateReader(), target);

        // Now handle ValueSpecified properties
        SetValueSpecifiedProperties(target!, jsonObject);

        return target;
    }

    private static void SetValueSpecifiedProperties(object? target, JObject jsonObject)
    {
        var type = target!.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Create a case-insensitive lookup of JSON property names
        var jsonProperties = new HashSet<string>(jsonObject.Properties().Select(p => p.Name), StringComparer.OrdinalIgnoreCase);

        foreach (var property in properties)
        {
            // Look for ValueSpecified properties
            if (property.Name.EndsWith("ValueSpecified") && property.PropertyType == typeof(bool))
            {
                // Find the corresponding base property name
                var baseName = property.Name[..^"ValueSpecified".Length];

                // Check if the base property exists in the JSON (case-insensitive)
                var basePropertyExists = jsonProperties.Contains(baseName);

                // Set the ValueSpecified property
                property.SetValue(target, basePropertyExists);
            }
        }
    }
}