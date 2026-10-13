using Spectre.Console;
using System.Text.RegularExpressions;
using VectoInputManager;
using VectoInputManager.Core;

namespace VectoInputManager.Commands;

/// <summary>
/// Provides helper methods to prompt users for values based on <see cref="PromptAction"/> metadata.
/// Renders questions with units, types, and optional flags, and handles defaults and selections.
/// </summary>
public partial class PromptActionHelper(IAnsiConsole console)
{
    private const string NotSet = "<Not Set>";

    // This flag is used by tests to bypass interactive prompts
    internal static bool TestMode = false;

    /// <summary>
    /// Prompts the user to answer a <see cref="PromptAction"/> using Spectre.Console components.
    /// Supports enum, class, collection selections, booleans, and primitive inputs.
    /// </summary>
    /// <param name="action">The prompt action to render.</param>
    /// <returns>
    /// The user's answer converted to the appropriate type (or null if optional and not set).
    /// </returns>
    public object? PromptAction(PromptAction action)
    {
        // Add padding and a symbol for depth for nicer visual hierarchy
        var prefix = action.Depth > 0
            ? new string('-', (action.Depth - 1) * 3) + "> "
            : "";

        var display = action.Query;

        string unit = "";
        string type = "";
        display = UnitRegex().Replace(display, m =>
            {
                unit = m.Groups[1].Value;
                return "";
            });

        display = TypeRegex().Replace(display, m =>
        {
            type = m.Groups[1].Value;
            return "";
        });

        var append = "";

        if (unit != string.Empty)
            append = $"[italic yellow]{unit}[/]";
        if (type != string.Empty)
            append += append != string.Empty ? $" {type}" : $"{type}";
        if (action.Optional)
            append += append != string.Empty ? ", opt." : "opt.";

        if (append != string.Empty)
            display = display.Trim() + $" [italic grey]({append})[/]";

        var displayQuery = $"{prefix}{display}";

        switch (action.Type)
        {
            case PromptActionType.Enum:
            case PromptActionType.Class:
            case PromptActionType.Collection:
                {
                    var choices = action.Selection!.Select(c => c.Value).ToList();

                    if (action.Optional)
                        choices.Add(NotSet);

                    if (action.TemplateValueSpecified)
                    {
                        string? choice = null;
                        if (action.TemplateValue == null)
                            choice = NotSet;
                        else
                            action.Selection!.TryGetValue(action.TemplateValue.ToString()!, out choice);

                        if (choice != null)
                        {
                            displayQuery += $" [green]({choice})[/]";
                            choices.Remove(choice);
                            choices.Insert(0, choice);
                        }
                    }

                    if (!displayQuery.EndsWith(':') && !displayQuery.EndsWith('?'))
                        displayQuery += ":";

                    var prompt = new SelectionPrompt<string>()
                        .Title(displayQuery)
                        .PageSize(10)
                        .AddChoices(choices)
                        .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]");

                    var p = TestMode
                        ? console.Ask<string>(displayQuery)
                        : console.Prompt(prompt);

                    if (displayQuery.EndsWith('?'))
                        displayQuery = displayQuery.TrimEnd('?') + ":";

                    console.MarkupLine($"{displayQuery} {p}");

                    // Find the key for the selected value
                    if (p == NotSet)
                    {
                        p = null;
                    }
                    else
                    {
                        var kv = action.Selection!.SingleOrDefault(c => c.Value == p);
                        if (kv.Key == null)
                            throw new InvalidOperationException($"Selection '{p}' not found in choices.");
                        p = kv.Key;
                    }

                    return p;
                }

            case PromptActionType.Boolean:
                {
                    var boolChoices = new List<string> { "Yes", "No" };
                    if (action.Optional)
                        boolChoices.Add(NotSet);

                    // Move default value to the front if present
                    if (action.TemplateValueSpecified)
                    {
                        var defaultStr = action.TemplateValue == null ? NotSet
                            : (bool)action.TemplateValue == true ? "Yes"
                            : "No";

                        displayQuery += $" [green]({defaultStr})[/]";

                        if (boolChoices.Remove(defaultStr))
                        {
                            boolChoices.Insert(0, defaultStr);
                        }
                    }

                    if (!displayQuery.EndsWith(':'))
                        displayQuery += ":";

                    var promptb = new SelectionPrompt<string>()
                        .Title(displayQuery)
                        .AddChoices(boolChoices);

                    var str = TestMode
                        ? console.Ask<string>(displayQuery)
                        : console.Prompt(promptb);
                    bool? b = str == "Yes" ? true
                        : str == "No" ? false
                        : null;
                    console.MarkupLine($"{displayQuery} {str}");
                    return b;
                }

            case PromptActionType.String:
                return console.AskEx<string>(displayQuery, action);

            case PromptActionType.Decimal:
                return console.AskEx<decimal>(displayQuery, action);

            case PromptActionType.Integer:
                return console.AskEx<int>(displayQuery, action);

            case PromptActionType.DateTime:
                return console.AskEx<DateTime>(displayQuery, action);

            default:
                throw new NotSupportedException($"Unsupported prompt action type: {action.Type}");
        }
    }

    [GeneratedRegex(@"\[([^\[\]]+)\]\s*")]
    private static partial Regex UnitRegex();
    
    [GeneratedRegex(@"\((\w+)\)\s*")]
    private static partial Regex TypeRegex();
}
