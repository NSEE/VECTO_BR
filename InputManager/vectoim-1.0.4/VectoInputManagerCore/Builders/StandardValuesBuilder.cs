using System;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace VectoInputManager.Core
{
    public class StandardValuesBuilder : ObjectBuilder
    {
        public StandardValuesBuilder(ILogger<StandardValuesBuilder> logger) : base(logger) { }

        protected override Type GetTypeByName(string typeName)
        {
            return TypeUtils.GetTypeByNameStandardValues(typeName);
        }

        protected override string GetDisplayPropertyName(PropertyInfo prop)
        {
            return prop.GetCustomAttribute<DescriptionAttribute>()?.Description
                           ?? Humanizer.StringHumanizeExtensions.Humanize(prop.Name);
        }
    }
}
