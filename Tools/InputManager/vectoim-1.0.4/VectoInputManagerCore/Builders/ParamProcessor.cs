namespace VectoInputManager.Core
{
    /// <summary>
    /// Parses and supplies key=value overrides for prompt answers during interactive building.
    /// Matches keys to prompt property paths and converts string values to the expected types,
    /// tracking which parameters were consumed.
    /// </summary>
    public class ParamProcessor
    {
        public IReadOnlyList<string> UnusedParams =>
            [.. _params!.Keys.Except(_usedParams)];

        public Dictionary<string, string> Files = new(StringComparer.OrdinalIgnoreCase);

        public bool TryGetValue(PromptAction action, out object? obj)
        {
            if (_params!.TryGetValue(action.Property, out var value))
            {
                obj = ConvertFromString(action, value);
                _usedParams.Add(action.Property);
                return true;
            }

            obj = null;
            return false;
        }

        public void SetParamValues(string[] values, CorpusProcessor? corpus)
        {
            _params = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (values == null || values.Length == 0)
            {
                return;
            }

            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                var parts = value.Trim().Split('=', 2);
                if (parts.Length != 2)
                {
                    throw new FormatException($"Invalid value format '{value}'. Expected format: Key=Value");
                }

                var key = parts[0].Trim();
                var val = parts[1].Trim();

                var compObj = corpus?.Components.FirstOrDefault(c =>
                        c.FileName.Equals(val, StringComparison.OrdinalIgnoreCase));

                if (compObj != null)
                {
                    // Component already mapped to another path — duplicate it in the corpus
                    if (compObj.PropertyPath != null)
                    {
                        compObj = corpus!.DuplicateComponent(compObj);
                    }

                    compObj.PropertyPath = key;
                }
                else if (val.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
                         || val.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Component file '{val}' referenced in '-m {value}' was not found in the input files. "
                        + $"Make sure to include it as an input argument: '{val}'.");
                }
                else
                {
                    _params[key] = val;
                }
            }
        }

        private static object? ConvertFromString(PromptAction action, string str)
        {
            // Handle null or empty string for optional fields
            if (string.IsNullOrEmpty(str) && action.Optional)
                return null;

            switch (action.Type)
            {
                case PromptActionType.String:
                    return str;

                case PromptActionType.Integer:
                    if (int.TryParse(str, out int intValue))
                        return intValue;
                    throw new FormatException($"Cannot convert '{str}' to integer for property '{action.Property}'");

                case PromptActionType.Decimal:
                    if (decimal.TryParse(str, out decimal decimalValue))
                        return decimalValue;
                    throw new FormatException($"Cannot convert '{str}' to decimal for property '{action.Property}'");

                case PromptActionType.DateTime:
                    if (DateTime.TryParse(str, out DateTime dateTimeValue))
                        return dateTimeValue;
                    throw new FormatException($"Cannot convert '{str}' to DateTime for property '{action.Property}'");

                case PromptActionType.Boolean:
                    // Handle various boolean representations
                    var lowerStr = str.ToLowerInvariant();
                    if (lowerStr == "yes" || lowerStr == "true" || lowerStr == "1")
                        return true;
                    if (lowerStr == "no" || lowerStr == "false" || lowerStr == "0")
                        return false;
                    throw new FormatException($"Cannot convert '{str}' to boolean for property '{action.Property}'. Expected: yes/no, true/false, or 1/0");

                case PromptActionType.Enum:
                case PromptActionType.Class:
                case PromptActionType.Collection:
                    // For enum/class/collection types, the string should be a key in the Selection dictionary
                    if (action.Selection != null && action.Selection.ContainsKey(str))
                        return str; // Return the key, not the display value

                    // If not found as a key, try to find by value (display text)
                    if (action.Selection != null)
                    {
                        var kvp = action.Selection.FirstOrDefault(s => s.Value.Equals(str, StringComparison.OrdinalIgnoreCase));
                        if (!kvp.Equals(default(KeyValuePair<string, string>)))
                            return kvp.Key;
                    }

                    throw new FormatException($"Invalid selection '{str}' for property '{action.Property}'. Valid options: {string.Join(", ", action.Selection?.Keys ?? Array.Empty<string>())}");

                default:
                    throw new NotSupportedException($"Unsupported prompt action type: {action.Type} for property '{action.Property}'");
            }
        }

        private Dictionary<string, string>? _params;

        private readonly HashSet<string> _usedParams = new(StringComparer.OrdinalIgnoreCase);
    }
}
