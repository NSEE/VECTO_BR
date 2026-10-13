using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace VectoInputManager.Core;

/// <summary>
/// Provides utility methods for navigating and extracting values from JSON objects (JObject) and
/// strongly-typed .NET objects using path expressions.
/// </summary>
public static partial class PathUtils
{
    /// <summary>
    /// Searches for a value in a JObject using a property path from a PromptAction. Handles special
    /// cases like "$values" arrays and indexed array access.
    /// </summary>
    /// <param name="jsonObject">The JObject to search within.</param>
    /// <param name="action">The PromptAction containing the property path and type information.</param>
    /// <returns>A tuple indicating if the value was found and the extracted value (or default).</returns>
    public static (bool found, object? value) FindJObject(JObject jsonObject, PromptAction action, ILogger? logger)
    {
        var propertyPath = action.Property;
        if (jsonObject == null || string.IsNullOrWhiteSpace(propertyPath))
            return (false, null);

        var token = jsonObject.SelectToken(propertyPath);

        if (token == null)
        {
            /* 
            Cover cases like: "TorqueLossMap": { "$values": [ { ...
            Instead of "TorqueLossMap": [ { ...
            Note that for nested arrays, the user will have to use one or the other convention consistently.
            */

            var adjustedPath = propertyPath.Replace("[", ".$values[");
            token = jsonObject.SelectToken(adjustedPath);
        }

        if (token == null)
            return (false, null);

        var matchPropertyPath = PropertyPathRegex().Match(propertyPath);
        bool isArrayElement = matchPropertyPath.Success && matchPropertyPath.Groups["index"].Success;
        JToken? parentToken = token.Parent;

        if (isArrayElement)
        {
            if (parentToken is not JArray jArray)
                throw new InvalidOperationException(
                    $"Expected an array at path '{propertyPath}' but found '{parentToken?.Type}'.");

            var index = int.Parse(matchPropertyPath.Groups["index"].Value);

            // Check if this item should be cropped/excluded from the result
            var cropToken = token["$crop"];
            if (cropToken != null && cropToken.Type == JTokenType.Boolean && cropToken.Value<bool>() == true)
            {
                return (true, ObjectBuilder.StrNo);
            }

            // Return "No" if index is out of bounds, otherwise return default value for the action
            if (index < 0 || index >= jArray.Count)
            {
                return (true, ObjectBuilder.StrNo);
            }
            else
            {
                return GetDefaultValue(action);
            }
        }

        // For object types (classes/collections), return the default value
        if (token.Type == JTokenType.Object)
        {
            // If there is a specific type defined, return that
            var specificType = token["$type"];
            if (specificType != null)
            {
                return (true, specificType.Value<string>());
            }

            return GetDefaultValue(action);
        }

        // Convert JToken to its underlying primitive value based on the action type
        if (action.Type == PromptActionType.Integer)
            return (true, token.Value<int>());
        else if (action.Type == PromptActionType.Decimal)
            return (true, token.Value<decimal>());
        else if (action.Type == PromptActionType.String)
            return (true, token.Value<string>());
        else if (action.Type == PromptActionType.Boolean)
            return (true, token.Value<bool>());
        else if (action.Type == PromptActionType.DateTime)
            return (true, token.Value<DateTime>());
        else if (action.Type == PromptActionType.Enum)
            return (true, token.Value<string>());
        else
            return (false, null);

        // Internal Method
        /// <summary>
        /// Gets the default value for complex types (Class or Collection) from the action's template or selection.
        /// </summary>
        (bool found, object? value) GetDefaultValue(PromptAction action)
        {
            if (action.Type != PromptActionType.Class && action.Type != PromptActionType.Collection)
                throw new InvalidOperationException("GetDefaultValue can only be called for classes or collections.");

            // If there is an actual class name in the template, use that (ignore "No")
            if (action.TemplateValue != null && action.TemplateValue.ToString() != ObjectBuilder.StrNo)
                return (true, action.TemplateValue);

            // Nothing? Use the first selection option only if it's the only one available.
            var candidates = action.Selection!.Where(c => c.Value != ObjectBuilder.StrNo).ToArray();
            if (candidates.Length == 1)
            {
                return (true, action.Selection!.First().Key);
            }
            else
            {
                if (candidates.Length == 0)
                    logger?.LogTrace(
                        "No options available for property '{Property}', setting to null.",
                        action.Property);
                else
                    logger?.LogWarning(
                        "Multiple options available for property '{Property}', setting to null. Add a '$type' property with one of the possible types: {Types}. ",
                    action.Property, string.Join(", ", candidates.Select(c => c.Value)));

                return (false, null);
            }
        }
    }

    /// <summary>
    /// Navigates through a .NET object hierarchy using a dot-notation property path with optional array indexing.
    /// </summary>
    /// <param name="rootObject">The root object to start navigation from.</param>
    /// <param name="propertyPath">The property path (e.g., "ElectricMotorTorqueLimits.ElectricMachine.VoltageLevel[0].Voltage").</param>
    /// <returns>A tuple containing the final PropertyInfo, its value, and the parent object.</returns>
    // Examples of valid paths:
    // - "ElectricMotorTorqueLimits.ElectricMachine"
    // - "ElectricMotorTorqueLimits.ElectricMachine.VoltageLevel[0]"
    // - "ElectricMotorTorqueLimits.ElectricMachine.VoltageLevel[0].Voltage"
    public static (PropertyInfo? prop, object? value, object? parent) FindValue(
        object rootObject, string propertyPath)
    {
        if (rootObject == null || string.IsNullOrWhiteSpace(propertyPath))
            return (null, null, null);

        var pathSegments = propertyPath.Split('.');
        object? currentObject = rootObject;
        object? parentObject = null;
        PropertyInfo? lastPropertyInfo = null;

        foreach (var segment in pathSegments)
        {
            if (currentObject == null)
                return (null, null, null);

            var match = PropertyPathRegex().Match(segment);
            if (!match.Success)
                return (null, null, null);

            var propertyName = match.Groups["name"].Value;
            var propertyInfo = currentObject.GetType().GetProperty(
                propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (propertyInfo == null)
                return (null, null, null);

            var propertyValue = propertyInfo.GetValue(currentObject);

            // Handle array or collection index access
            if (propertyValue != null && match.Groups["index"].Success)
            {
                int index = int.Parse(match.Groups["index"].Value);

                if (propertyValue is System.Collections.IList list)
                {
                    if (index < 0 || index >= list.Count)
                        return (null, null, null);

                    parentObject = currentObject;
                    currentObject = list[index];
                    lastPropertyInfo = propertyInfo;
                }
                else if (propertyValue.GetType().IsArray)
                {
                    var array = (Array)propertyValue;
                    if (index < 0 || index >= array.Length)
                        return (null, null, null);

                    parentObject = currentObject;
                    currentObject = array.GetValue(index);
                    lastPropertyInfo = propertyInfo;
                }
                else
                {
                    return (null, null, null);
                }
            }
            else
            {
                parentObject = currentObject;
                currentObject = propertyValue;
                lastPropertyInfo = propertyInfo;
            }
        }

        // Found final property - now check for the "Specified" pattern
        bool? isSpecified = null;

        // Note the pattern:
        // [System.Xml.Serialization.XmlElementAttribute("MaxTorque")]
        // public int MaxTorqueValue { get; set; }
        // [System.Xml.Serialization.XmlIgnoreAttribute()]
        // public bool MaxTorqueValueSpecified { get; set; }

        var specifiedPropertyInfo = parentObject!.GetType().GetProperty($"{lastPropertyInfo!.Name}ValueSpecified",
       BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (specifiedPropertyInfo != null)
            isSpecified = (bool)specifiedPropertyInfo.GetValue(parentObject)!;

        // If the "Specified" flag is explicitly false, treat the value as not set
        if (isSpecified.HasValue && isSpecified == false)
            return (lastPropertyInfo, null, parentObject); // Property exists but not specified

        return (lastPropertyInfo, currentObject, parentObject);
    }

    /// <summary>
    /// Regular expression to parse property path segments that may include array indices.
    /// Matches patterns like: "PropertyName" or "PropertyName[123]"
    /// </summary>
    [GeneratedRegex(@"^(?<name>[^\[]+)(\[(?<index>\d+)\])?$")]
    private static partial Regex PropertyPathRegex();
}