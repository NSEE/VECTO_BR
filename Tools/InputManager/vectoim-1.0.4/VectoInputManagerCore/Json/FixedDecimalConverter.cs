using Newtonsoft.Json;
using System.Globalization;

namespace VectoInputManager.Core;

public class FixedDecimalConverter(int decimalPlaces = 6) : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(decimal) ||
               objectType == typeof(double) ||
               objectType == typeof(float) ||
               objectType == typeof(int) ||
               objectType == typeof(long) ||
               objectType == typeof(short) ||
               objectType == typeof(byte) ||
               objectType == typeof(uint) ||
               objectType == typeof(ulong) ||
               objectType == typeof(ushort) ||
               objectType == typeof(sbyte);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        double number;

        // Convert to double (handles all numeric types)
        number = Convert.ToDouble(value);

        // Format with exactly 6 decimal places
        string formatted = number.ToString($"F{_decimalPlaces}", CultureInfo.InvariantCulture);

        writer.WriteRawValue(formatted);
    }

    public override object? ReadJson(JsonReader reader, Type objectType,
        object? existingValue, JsonSerializer serializer)
    {
        // For deserialization, just use the default behavior
        if (reader.Value == null) return null;
        return Convert.ChangeType(reader.Value, objectType);
    }

    private readonly int _decimalPlaces = decimalPlaces;
}
