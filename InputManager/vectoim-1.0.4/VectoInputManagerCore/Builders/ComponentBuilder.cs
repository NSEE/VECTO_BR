using Microsoft.Extensions.Logging;

namespace VectoInputManager.Core;

/// <summary>
/// Specializes ObjectBuilder for component declarations. Builds the object graph of an existing
/// component allowing interactive or programmatic modification of its properties.
/// </summary>
public class ComponentBuilder(ILogger<ComponentBuilder> logger) : ObjectBuilder(logger)
{
    protected override Type GetTypeByName(string typeName)
    {
        return TypeUtils.GetTypeByName(typeName);
    }
}
