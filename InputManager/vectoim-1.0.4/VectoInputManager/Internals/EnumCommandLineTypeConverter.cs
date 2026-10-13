using System.ComponentModel;
using System.Globalization;
using VectoInputManager.Core;

namespace VectoInputManager.Commands;

/// <summary>
/// Type converter for converting command line string inputs to Enum values, allowing variations in formatting.
/// </summary>
public sealed class EnumCommandLineTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string stringValue)
        {
            var normalized = stringValue.Replace("-", "").Replace("_", "").Trim();

            if (Enum.TryParse<BrokenSignatureAction>(normalized, ignoreCase: true, out var result))
            {
                return result;
            }

            throw new FormatException($"Unable to convert '{stringValue}' to {nameof(BrokenSignatureAction)}. " +
                $"Valid values are: {string.Join(", ", Enum.GetNames<BrokenSignatureAction>().Select(c => c.ToLower()))}");
        }

        return base.ConvertFrom(context, culture, value);
    }
}
