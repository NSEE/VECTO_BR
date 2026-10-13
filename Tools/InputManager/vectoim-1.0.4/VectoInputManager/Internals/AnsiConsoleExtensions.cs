using Fasterflect;
using Spectre.Console;
using System;
using VectoInputManager.Core;

namespace VectoInputManager.Commands;

/// <summary>
/// Extensions for Spectre.Console to render consistent prompts and status messages. Provides input
/// parsing with validation and common line helpers.
/// </summary>
public static partial class AnsiConsoleExtensions
{
    /// <summary>
    /// Prompts for a value with validation and optional default handling based on <see
    /// cref="PromptAction"/>. Supports string, integer, decimal, and DateTime input types.
    /// </summary>
    public static object? AskEx<T>(this IAnsiConsole console, string displayQuery, PromptAction action)
    {
        if (action.Type != PromptActionType.Enum &&
        action.Type != PromptActionType.Class &&
        action.Type != PromptActionType.Collection &&
        !action.Optional)
            displayQuery += ":";

        while (true)
        {
            var tp = new TextPrompt<string>(displayQuery);

            if (action.Optional)
                tp.AllowEmpty();

            // Default value handling (shown if user just presses enter)
            if (action.TemplateValueSpecified && action.TemplateValue is not null)
            {
                tp.DefaultValue(action.TemplateValue.ToString()!);
            }

            var raw = tp.Show(console);

            // Handle empty input
            if (string.IsNullOrWhiteSpace(raw))
            {
                if (action.Optional)
                    return null;

                console.LineError("A value is required.");
                continue;
            }

            switch (typeof(T))
            {
                // String
                case Type t when t == typeof(string):
                    return raw;

                // Integers
                case Type t when t == typeof(int):
                    if (int.TryParse(raw, out var iv))
                    {
                        if (action.Range is not null && !action.Range.IsValid(iv))
                        {
                            console.LineError($"Value out of range. {action.Range.ErrorMessage}");
                            continue;
                        }
                        return iv;
                    }
                    console.LineError($"'{raw}' is not a valid integer.");
                    continue;

                // Decimal
                case Type t when t == typeof(decimal):
                    if (decimal.TryParse(raw, out var dv))
                    {
                        if (action.Range is not null && !action.Range.IsValid(dv))
                        {
                            console.LineError($"Value out of range. {action.Range.ErrorMessage}");
                            continue;
                        }
                        return dv;
                    }
                    console.LineError($"'{raw}' is not a valid decimal.");
                    continue;

                // DateTime
                case Type t when t == typeof(DateTime):
                    if (DateTime.TryParse(raw, out var dtv))
                    {
                        return dtv;
                    }
                    console.LineError("Invalid date/time. Use formats like 'yyyy-MM-dd' or 'yyyy-MM-dd HH:mm:ss'.");
                    continue;

                default:
                    throw new NotSupportedException($"Unsupported prompt type: {typeof(T).Name}");
            }
        }
    }

    // =======================================================================
    // Status Messages
    // =======================================================================

    public static void LineError(this IAnsiConsole console, string message)
    {
        console.MarkupLine($"[red]{Markup.Escape(message)}[/]");
    }

    public static void LineWarning(this IAnsiConsole console, string message)
    {
        console.MarkupLine($"[yellow]{Markup.Escape(message)}[/]");
    }

    public static void LineWarningBold(this IAnsiConsole console, string message)
    {
        console.MarkupLine($"[yellow][bold]{Markup.Escape(message)}[/][/]");
    }

    public static void LineWarningBlink(this IAnsiConsole console, string message)
    {
        console.MarkupLine($"[yellow blink]{Markup.Escape(message)}[/]");
    }

    public static void LineSuccess(this IAnsiConsole console, string message)
    {
        console.MarkupLine($"[green]+ {Markup.Escape(message)}[/]");
    }

    // =======================================================================
    // Information and Processing Messages
    // =======================================================================

    public static void LineProcessing(this IAnsiConsole console, string message)
    {
        console.MarkupLine($"[cyan]{Markup.Escape(message)}[/]");
    }

    public static void LineInfo(this IAnsiConsole console, string message)
    {
        console.MarkupLine(Markup.Escape(message));
    }

    // =======================================================================
    // Action Messages (Label: Content)
    // =======================================================================

    public static void LineAction(this IAnsiConsole console, string action, string target)
    {
        console.MarkupLine($"[green]> {Markup.Escape(action)}:[/] {Markup.Escape(target)}");
    }

    public static void LineActionError(this IAnsiConsole console, string action, string error)
    {
        console.MarkupLine($"[red]{Markup.Escape(action)}:[/] {Markup.Escape(error)}");
    }

    public static void LineFileOperation(this IAnsiConsole console, string operation, string filename)
    {
        console.MarkupLine($"[blue]{Markup.Escape(operation)}:[/] {Markup.Escape(filename)}");
    }

    public static void LineCreated(this IAnsiConsole console, string what, string path)
    {
        console.MarkupLine($"[green]+ Created {Markup.Escape(what)}:[/] {Markup.Escape(path)}");
    }

    public static void LineFound(this IAnsiConsole console, string what, string path)
    {
        console.MarkupLine($"[blue]* Found {Markup.Escape(what)}:[/] {Markup.Escape(path)}");
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Tool Headers
    // ═══════════════════════════════════════════════════════════════════════

    public static void LineToolTitle(this IAnsiConsole console, string title)
    {
        var rule = new Rule($"[white on navy] {Markup.Escape(title)} [/]")
        {
            Justification = Justify.Left
        };
        console.Write(rule);
        console.WriteLine();
    }
}