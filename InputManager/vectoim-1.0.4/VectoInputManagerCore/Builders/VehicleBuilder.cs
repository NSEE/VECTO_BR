using System;
using System.Reflection;
using Fasterflect;
using Microsoft.Extensions.Logging;

namespace VectoInputManager.Core;

/// <summary>
/// Specializes ObjectBuilder for vehicle declarations. Prompts for vehicle type first, resolves the
/// correct VECTO vehicle class, and then builds out the full object graph with domain-specific
/// defaults and display tweaks.
/// </summary>
public class VehicleBuilder(ILogger<ObjectBuilder> logger) : ObjectBuilder(logger)
{
    protected override object BuildRecursive(object? target, PropertyInfo? targetProp, int depth, string parentPath = "")
    {
        if (target == null && targetProp == null)
        {
            var action = new PromptAction
            {
                Property = "@VehicleType",
                Query = "Vehicle Type:",
                Type = PromptActionType.Enum,
                Selection = Data.GetVehicleTypes()
                    .ToDictionary(c => c.TypeName, v => $"{v.Description} ({v.Version})"),
                Depth = depth,
                Optional = false
            };
            _actions.Add(action);

            PromptQuery(action);
            var answer = action.Answer;

            var vt = Data.GetVehicleTypes()
                .Single(v => v.TypeName == (string)answer! || v.Code == (string)answer!);
            var instance = vt.FindType().CreateInstance();
            Object = instance;

            return BuildRecursive(instance, null, depth + 1, "");
        }

        return base.BuildRecursive(target, targetProp, depth, parentPath);
    }

    protected override void BeforePromptHandler(PromptAction action)
    {
        // For Vehicles, decrease the depth by one as the root vehicle type can be on the same level
        // as the properties
        action.Depth--;
    }

    protected override void SetTemplateValue(PromptAction action)
    {
        if (action.Property == "@VehicleType")
        {
            action.TemplateValue = TemplateObject!.GetType().Name;
            action.TemplateValueSpecified = true;
        }
        else
        {
            base.SetTemplateValue(action);
        }
    }

    protected override Type GetTypeByName(string typeName)
    {
        return TypeUtils.GetTypeByName(typeName);
    }
}
