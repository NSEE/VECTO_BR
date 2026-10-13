using Castle.Core.Internal;
using Fasterflect;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using VectoObjects;

namespace VectoInputManager.Core;

/// <summary>
/// Base class for interactively constructing complex object graphs using a sequence of
/// PromptAction steps. Handles discovery of properties, default/template values, type selection,
/// collections, enums, validation, and conversion. Subclasses override behavior for domain types.
/// </summary>
public abstract class ObjectBuilder(ILogger<ObjectBuilder> _logger)
{
    public object? Object { get; protected set; }
    public IReadOnlyList<PromptAction> Actions => _actions;
    public Action<PromptAction>? PromptHandler { get; set; }
    public object? TemplateObject { get; set; }

    public void Run(object? startObject = null)
    {
        _logger.LogTrace("Starting ObjectBuilder Run.");
        _actions.Clear();
        Object = null;

        // Build recurive does not support version 1.0,
        // so let's just make a copy and don't try to build it.
        if (TemplateObject is VehicleDeclarationType_v1_0)
        {
            Object = TemplateObject;
            _logger.LogWarning("VehicleDeclarationType v1.0 cannot be edited. Skipping any modifications.");
        }
        else
        {
            BuildRecursive(startObject, null, 0);
        }
    }

    protected readonly List<PromptAction> _actions = [];

    protected void PromptQuery(PromptAction action)
    {
        if (PromptHandler == null)
            throw new InvalidOperationException("PromptHandler is not set.");

        if (TemplateObject != null)
        {
            // Try to set a default value from the input object
            SetTemplateValue(action);

            // For collections and classes, if no default value was set from the input object, use
            // "No" as the default
            if (action.Type == PromptActionType.Collection && !action.TemplateValueSpecified)
            {
                action.TemplateValueSpecified = true;
                action.TemplateValue = StrNo;
            }

            if (action.Type == PromptActionType.Class && !action.TemplateValueSpecified)
            {
                action.TemplateValueSpecified = true;
                action.TemplateValue = StrNo;
            }
        }

        BeforePromptHandler(action);
        PromptHandler.Invoke(action);
    }

    /// <summary>
    /// Called before the <see cref="PromptHandler"/> is invoked to allow derived classes to modify 
    /// the <see cref="PromptAction"/> before it's presented to the user.
    /// </summary>
    protected virtual void BeforePromptHandler(PromptAction action)
    {
    }

    protected virtual void SetTemplateValue(PromptAction action)
    {
        var (found, value, _) = PathUtils.FindValue(TemplateObject!, action.Property);

        if (found == null)
            return;

        if (value == null)
        {
            action.TemplateValue = null;
        }
        else
        {
            var vType = value.GetType();

            // In BaseInput descendants, a string is used  for decimal[]. This
            // is an exception to the collection pattern.
            if (vType == typeof(decimal[]))
            {
                var decimalArray = (decimal[])value;
                action.TemplateValue = string.Join(Separator, decimalArray);
            }

            // For classes, use the class name as the default value
            else if (vType.IsClass && vType != typeof(string) && vType != typeof(DateTime))
                action.TemplateValue = vType.Name;

            // Everything else, use the value directly
            else
                action.TemplateValue = value;
        }

        action.TemplateValueSpecified = true;
    }

    protected virtual object BuildRecursive(object? target, PropertyInfo? targetProp,
        int depth, string parentPath = "")
    {
        var objType = target!.GetType();

        _logger.LogTrace("Building object of type '{ObjectType}' at depth {Depth} for property '{PropertyPath}'.",
            objType.Name, depth, parentPath);

        foreach (var prop in objType.GetProperties())
        {
            ProcessProperty(target, prop, depth, parentPath);
        }

        return target;
    }

    private void ProcessProperty(object targetObj, PropertyInfo targetProp, int depth, string parentPath)
    {
        var targetObjType = targetObj.GetType();

        if (!targetProp.CanWrite
            || targetProp.GetAttribute<EditorBrowsableAttribute>()?.State == EditorBrowsableState.Never
            || targetProp.Name.EndsWith("Specified")
            || targetProp.Name == "Signature"
            || targetProp.Name == "MonitoringData"
            || targetProp.Name == "Reference")
            return;

        var targetPropSpecified = targetObjType.GetProperty($"{targetProp.Name}Specified");
        var optional = targetPropSpecified != null;

        string targetPropName = GetDisplayPropertyName(targetProp);
        var fullPath = string.IsNullOrEmpty(parentPath) ? targetProp.Name : $"{parentPath}.{targetProp.Name}";
        var targetRange = targetProp.GetAttribute<RangeAttribute>();

        // Switch based on property type
        switch (targetProp.PropertyType)
        {
            // Handle string properties
            case Type t when t == typeof(string):
                {
                    var action = new PromptAction
                    {
                        Property = fullPath,
                        Query = $"{targetPropName}",
                        Type = PromptActionType.String,
                        Depth = depth,
                        Optional = optional
                    };
                    _actions.Add(action);

                    PromptQuery(action);
                    SetAnswer(action);
                    break;
                }

            // Handle DateTime properties
            case Type t when t == typeof(DateTime):
                {
                    var action = new PromptAction
                    {
                        Property = fullPath,
                        Query = $"{targetPropName} (DateTime)",
                        Type = PromptActionType.DateTime,
                        Depth = depth,
                        Optional = optional
                    };
                    _actions.Add(action);

                    PromptQuery(action);
                    SetAnswer(action);
                    break;
                }

            case Type t when t == typeof(DateTime?):
                {
                    var action = new PromptAction
                    {
                        Property = fullPath,
                        Query = $"{targetPropName} (DateTime)",
                        Type = PromptActionType.DateTime,
                        Depth = depth,
                        Optional = true
                    };
                    _actions.Add(action);

                    PromptQuery(action);
                    SetAnswer(action);
                    break;
                }

            // Handle int properties
            case Type t when t == typeof(int):
                {
                    var action = new PromptAction
                    {
                        Property = fullPath,
                        Query = $"{targetPropName} (int)",
                        Type = PromptActionType.Integer,
                        Depth = depth,
                        Optional = optional,
                        Range = targetRange
                    };
                    _actions.Add(action);

                    PromptQuery(action);
                    SetAnswer(action);
                    break;
                }

            case Type t when t == typeof(int?):
                {
                    var action = new PromptAction
                    {
                        Property = fullPath,
                        Query = $"{targetPropName} (int)",
                        Type = PromptActionType.Integer,
                        Depth = depth,
                        Optional = true,
                        Range = targetRange
                    };
                    _actions.Add(action);

                    PromptQuery(action);
                    SetAnswer(action);
                    break;
                }

            // Handle decimal properties
            case Type t when t == typeof(decimal):
                {
                    var action = new PromptAction
                    {
                        Property = fullPath,
                        Query = $"{targetPropName} (decimal)",
                        Type = PromptActionType.Decimal,
                        Depth = depth,
                        Optional = optional,
                        Range = targetRange
                    };
                    _actions.Add(action);

                    PromptQuery(action);
                    SetAnswer(action);
                    break;
                }

            case Type t when t == typeof(decimal?):
                {
                    var action = new PromptAction
                    {
                        Property = fullPath,
                        Query = $"{targetPropName} (decimal)",
                        Type = PromptActionType.Decimal,
                        Depth = depth,
                        Optional = true,
                        Range = targetRange
                    };
                    _actions.Add(action);

                    PromptQuery(action);
                    SetAnswer(action);
                    break;
                }

            // Handle decimal array properties (used in BaseInput descendants)
            // See also SetDefaultValue method
            case Type t when t == typeof(decimal[]):
                {
                    var action = new PromptAction
                    {
                        Property = fullPath,
                        Query = $"{targetPropName} (separted with {Separator})",
                        Type = PromptActionType.String,
                        Depth = depth,
                        Optional = optional
                    };
                    _actions.Add(action);

                    PromptQuery(action);
                    HandleAnswerDecimalArray(action);
                    SetAnswer(action);
                    break;
                }

            // Handle bool properties
            case Type t when t == typeof(bool):
                {
                    var action = new PromptAction
                    {
                        Property = fullPath,
                        Query = $"{targetPropName}",
                        Type = PromptActionType.Boolean,
                        Depth = depth,
                        Optional = optional
                    };
                    _actions.Add(action);

                    PromptQuery(action);
                    SetAnswer(action);
                    break;
                }

            case Type t when t == typeof(bool?):
                {
                    var action = new PromptAction
                    {
                        Property = fullPath,
                        Query = $"{targetPropName}",
                        Type = PromptActionType.Boolean,
                        Depth = depth,
                        Optional = true
                    };
                    _actions.Add(action);

                    PromptQuery(action);
                    SetAnswer(action);
                    break;
                }

            //Handle Enums
            case Type t when t.IsEnum:
                {
                    var action = new PromptAction
                    {
                        Property = fullPath,
                        Query = $"{targetPropName}",
                        Type = PromptActionType.Enum,
                        Selection = GetEnumSelectionDictionary(targetProp.PropertyType),
                        Depth = depth,
                        Optional = optional
                    };
                    _actions.Add(action);

                    PromptQuery(action);
                    HandleAnswerEnum(action, targetProp.PropertyType);
                    SetAnswer(action);
                    break;
                }

            case Type t when t.IsGenericType
                             && t.GetGenericTypeDefinition() == typeof(Nullable<>)
                             && Nullable.GetUnderlyingType(t)!.IsEnum:
                {
                    var underlying = Nullable.GetUnderlyingType(targetProp.PropertyType)!;
                    var action = new PromptAction
                    {
                        Property = fullPath,
                        Query = $"{targetPropName}",
                        Type = PromptActionType.Enum,
                        Depth = depth,
                        Optional = true,
                        Selection = GetEnumSelectionDictionary(underlying)
                    };
                    _actions.Add(action);

                    PromptQuery(action);
                    HandleAnswerEnum(action, underlying);
                    SetAnswer(action);
                    break;
                }

            // Handle List<T> properties
            case Type t when t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>):
                {
                    var targetItemType = targetProp.PropertyType.GetGenericArguments()[0];
                    var typesStrList = TypeUtils.GetSelfAndDescendantTypes(targetItemType)
                        .OrderByDescending(c => c.Name)
                        .Select(t2 => t2.Name)
                        .ToList();
                    typesStrList.Add(StrNo);

                    var targetCollectionObj = targetProp.GetValue(targetObj);

                    // Create the collection if it does not exist.
                    if (targetCollectionObj == null)
                    {
                        targetCollectionObj = CreateClass(targetProp.PropertyType, t);
                        targetProp.SetValue(targetObj, targetCollectionObj);
                    }

                    int itemIdx = 0;
                    while (true)
                    {
                        var itemPath = $"{fullPath}[{itemIdx}]";

                        var action = new PromptAction
                        {
                            Property = itemPath,
                            Query = $"Add " + (itemIdx == 0 ? "" : "another ") + $"item to {targetPropName}?",
                            Type = PromptActionType.Collection,
                            Depth = depth,
                            Optional = false,
                            Selection = typesStrList.ToDictionary(key => key, value => value),
                        };
                        _actions.Add(action);

                        PromptQuery(action);
                        var answerObject = action.Answer;
                        string? answer = answerObject is string v ? v
                            : answerObject?.GetType().Name;

                        if (answer == StrNo || answer == null)
                            break;

                        var concreteType = GetTypeByName(answer);
                        var instance = CreateClass(targetItemType, concreteType);

                        // All prompts for the item's properties will use itemPath as the parentPath
                        BuildRecursive(instance!, null, depth + 1, itemPath);

                        var addMethod = targetCollectionObj!.GetType().GetMethod("Add");
                        addMethod!.Invoke(targetCollectionObj, [instance]);
                        itemIdx++;
                    }
                    break;
                }

            // Handle complex object properties
            case Type t when t.IsClass && t != typeof(string) && t != typeof(DateTime):
                {
                    var typesStrList = TypeUtils.GetSelfAndDescendantTypes(targetProp.PropertyType)
                        .OrderByDescending(c => c.Name)
                        .Select(t2 => t2.Name)
                        .ToList();
                    typesStrList.Add(StrNo);

                    var action = new PromptAction
                    {
                        Property = fullPath,
                        Query = $"{targetPropName}",
                        Type = PromptActionType.Class,
                        Depth = depth,
                        Optional = optional,
                        Selection = typesStrList.ToDictionary(key => key, value => value),
                    };
                    _actions.Add(action);

                    PromptQuery(action);
                    bool recurse = HandleAnswerClass(action, targetProp, typesStrList);
                    SetAnswer(action);
                    if (recurse)
                    {
                        BuildRecursive(action.Answer, targetProp, depth + 1, fullPath);
                    }

                    break;
                }

            // Unsupported property type
            default:
                throw new InvalidOperationException(
                    $"Unsupported property type: {targetProp.PropertyType} for property {targetProp.Name}.");
        }

        // Internal Method
        void SetAnswer(PromptAction action)
        {
            var answer = action.Answer;

            if (answer == null && action.Optional)
            {
                if (targetPropSpecified != null && targetPropSpecified.CanWrite)
                    targetPropSpecified?.SetValue(targetObj, false);
            }
            else
            {
                if (targetPropSpecified != null && targetPropSpecified.CanWrite)
                    targetPropSpecified?.SetValue(targetObj, true);

                try
                {
                    targetProp.SetValue(targetObj, answer);
                }
                catch
                {
                    _logger.LogWarning("Failed to set property '{Property}' with value '{Value}' to this schema.", targetProp.Name, answer);
                }
            }
        }
    }

    private bool HandleAnswerClass(PromptAction action, PropertyInfo targetProp, List<string> types)
    {
        var answerObject = action.Answer;

        string? answer = answerObject is string v ? v
            : answerObject?.GetType().Name;

        if (answer == StrNo || answer == null)
        {
            action.Answer = null;
            return false;
        }
        else
        {
            // The template had a List<T> but the target is a concrete type?
            // We cannot convert, so set to empty instance of first type and warn the user.
            if (answer == "List`1")
            {
                answer = types.First();
                _logger.LogWarning("Cannot convert value of object '{Property}' to selected schema. Will set to an empty '{answer}'.",
                    targetProp.Name, answer);
            }

            var concreteType = GetTypeByName(answer);
            var instance = CreateClass(targetProp.PropertyType, concreteType);
            action.Answer = instance;
            return true;
        }
    }

    private void HandleAnswerEnum(PromptAction action, Type targetType)
    {
        var answer = action.Answer;
        if (answer == null || answer.ToString() == StrNone)
            action.Answer = null;
        else
        {
            object val = SetEnum();
            action.Answer = val;
        }

        object SetEnum()
        {
            object val;

            // Directly set if the type matches, otherwise parse from string
            if (answer.GetType() == targetType)
            {
                val = answer;
            }
            else
            {
                if (Enum.TryParse(targetType, answer.ToString(), out var enumVal))
                {
                    val = enumVal!;
                }
                else
                {
                    var enumValues = Enum.GetValues(targetType);
                    val = enumValues.GetValue(0)!;

                    _logger.LogWarning("Failed to parse enum value '{EnumValue}' for type '{EnumType}'. Using first available value '{FirstValue}' instead.",
                        answer.ToString(), targetType.Name, val.ToString());
                }
            }

            return val;
        }
    }

    private void HandleAnswerDecimalArray(PromptAction action)
    {
        var answer = action.Answer;
        if (answer == null || string.IsNullOrWhiteSpace(answer.ToString()))
        {
            action.Answer = Array.Empty<decimal>();
        }
        else
        {
            try
            {
                var decimalArray = answer.ToString()!
                    .Split(Separator)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => decimal.Parse(s.Trim()))
                    .ToArray();
                action.Answer = decimalArray;
            }
            catch (FormatException)
            {
                throw new InvalidOperationException($"Invalid decimal format in {Separator}-separated string for {action.Property}");
            }
        }
    }

    private object CreateClass(Type targetType, Type requestedType)
    {
        if (requestedType == null)
        {
            throw new InvalidOperationException("Requested type cannot be null. TargetType: " + targetType.Name);
            //requestedType = targetType;
            //_logger.LogWarning("Requested type is null. Using target type '{TargetType}' instead.", targetType.Name);
        }

        // Check if concrete type can be assigned to property type. This also upgrades like,
        // EngineComponentDeclarationType_v1_0 to "EngineComponentDeclarationType_v2_0
        if (!targetType.IsAssignableFrom(requestedType))
        {
            // Find compatible type using TypeUtils.GetSelfAndDescendantTypes
            var compatibleTypes = TypeUtils.GetSelfAndDescendantTypes(targetType)
                .OrderByDescending(q => q.Name)
                .ToList();

            // Find the best matching type - prefer exact match, then the first compatible type
            var bestType = compatibleTypes.FirstOrDefault(t => t == requestedType)
                          ?? compatibleTypes.FirstOrDefault();

            if (bestType != null)
            {
                _logger.LogWarning("Requested type '{RequestedType}' is not compatible. Using compatible type '{BestType}' instead.",
                    requestedType.Name, bestType.Name);

                requestedType = bestType;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Type '{requestedType.Name}' is not compatible with property type '{targetType.Name}' and no compatible descendant type was found.");
            }
        }

        var instance = requestedType.CreateInstance();
        return instance;
    }

    protected virtual string GetDisplayPropertyName(PropertyInfo prop)
    {
        return Humanizer.StringHumanizeExtensions.Humanize(prop.Name);
    }

    protected static IDictionary<string, string> GetEnumSelectionDictionary(Type enumType)
    {
        return Enum.GetNames(enumType)
            .ToDictionary(
                key => key,
                value => Humanizer.StringHumanizeExtensions.Humanize(value));
    }

    protected abstract Type GetTypeByName(string typeName);

    internal const string StrNo = "No";
    private const string StrNone = "None";

    private readonly string Separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
}
