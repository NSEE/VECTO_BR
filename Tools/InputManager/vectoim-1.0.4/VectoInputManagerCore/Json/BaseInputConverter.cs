using System;
using Fasterflect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VectoInputManager.Core;

/// <summary>
/// Custom JSON converter for supporting JSON component input files. These files start with a
/// "Component" property that indicates the specific subclass of BaseInput to instantiate.
/// </summary>
class BaseInputConverter : JsonConverter
{
    private static readonly Dictionary<string, Type> Types;

    static BaseInputConverter()
    {
        // Discover all non-abstract subclasses of BaseInput and map them by their Component name.
        Types = typeof(BaseInput).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(BaseInput)) && !t.IsAbstract)
            .ToDictionary(t => ((BaseInput)t.CreateInstance()).Component.ToString(),
                StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Deserializes JSON to the appropriate BaseInput subclass using the 'Component' property.
    /// </summary>
    public override object ReadJson(JsonReader reader, Type type,
        object? value, JsonSerializer serializer)
    {
        var jobject = JObject.Load(reader);

        if (!jobject.TryGetValue("Component", StringComparison.OrdinalIgnoreCase, out JToken? v1Value))
            throw new ArgumentException("Cannot parse json.");

        if (!Types.TryGetValue(v1Value.ToObject<string>()!, out Type? messageType))
            throw new Exception("Cannot find type " + v1Value.ToObject<string>());

        return jobject.ToObject(messageType, serializer)!;
    }

    /// <summary>
    /// Serializes a BaseInput instance to JSON.
    /// </summary>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is not BaseInput _)
            throw new ArgumentException("Value is not a BaseInput.");

        // Create a JObject from the BaseInput instance
        var jobject = JObject.FromObject(value, serializer);
        jobject.WriteTo(writer);
    }

    public override bool CanConvert(Type type) => type == typeof(BaseInput);
    public override bool CanWrite => true;
}
