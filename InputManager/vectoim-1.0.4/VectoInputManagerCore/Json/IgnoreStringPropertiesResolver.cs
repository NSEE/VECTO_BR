using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VectoInputManager.Core;

/// <summary>
/// Custom contract resolver to ignore properties from declaration files that are not needed for
/// job creation.
/// </summary>
class IgnoreStringPropertiesResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var props = base.CreateProperties(type, memberSerialization)!;

        // Get all property names for pattern matching
        var propertyNames = new HashSet<string>(props.Select(p => p.PropertyName!));

        var list = props.Where(p => !p.PropertyName!.Contains("_String")
            && p.PropertyName != "Signature"
            && p.PropertyName != "MonitoringData"
            && !ShouldExcludeProperty(p.PropertyName, propertyNames));
            //&& !HasXmlIgnoreAttribute(p, type));

        // For properties that have a ValueSpecified, set ShouldSerialize accordingly
        list = list.Select(p => ConfigureProperty(p, type));

        return [.. list];
    }

    private static bool ShouldExcludeProperty(string propertyName, HashSet<string> allPropertyNames)
    {
        // Exclude properties ending with "ValueSpecified"
        if (propertyName.EndsWith("ValueSpecified"))
            return true;

        // Exclude properties ending with "Specified" also.
        if (propertyName.EndsWith("Specified"))
            return true;

        // Exclude properties ending with "Value" only if there's a corresponding base property.
        // "Value" by itself is not excluded.
        if (propertyName.EndsWith("Value") && propertyName.Length > 5) // "Value" is 5 characters
        {
            var baseName = propertyName[..^5]; // Remove "Value"
            return allPropertyNames.Contains(baseName);
        }

        return false;
    }

    private static JsonProperty ConfigureProperty(JsonProperty property, Type type)
    {
        // Check if this property has a corresponding ValueSpecified property
        var valueSpecifiedPropertyName = property.PropertyName + "ValueSpecified";
        var valueSpecifiedProperty = type.GetProperty(valueSpecifiedPropertyName);

        if (valueSpecifiedProperty != null && valueSpecifiedProperty.PropertyType == typeof(bool))
        {
            // Configure the property to be ignored if the ValueSpecified is false
            property.ShouldSerialize = instance =>
            {
                var valueSpecifiedValue = valueSpecifiedProperty.GetValue(instance);
                return valueSpecifiedValue is bool specified && specified;
            };
        }

        return property;
    }
}