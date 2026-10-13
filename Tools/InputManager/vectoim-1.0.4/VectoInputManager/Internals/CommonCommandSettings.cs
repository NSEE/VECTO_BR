using NLog;
using NLog.Config;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;

namespace VectoInputManager.Commands;

/// <summary>
/// Base settings for all CLI commands, including a common verbose logging option. When verbose is
/// enabled, adjusts NLog rules to output Trace-level logs to the console and file.
/// </summary>
public abstract class CommonCommandSettings : CommandSettings
{
    /// <summary>
    /// Enables verbose logging.
    /// </summary>
    [CommandOption("-v|--verbose")]
    [Description("Enables verbose logging.")]
    public bool Verbose { get; set; }

    public override ValidationResult Validate()
    {
        if (Verbose)
        {
            var config = LogManager.Configuration!;

            config.LoggingRules.Clear();
            foreach (var targetName in new string[] { "console", "file" })
            {
                var target = config.FindTargetByName(targetName);
                if (target != null)
                    config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Trace, target));
            }

            LogManager.ReconfigExistingLoggers();
        }

        return ValidationResult.Success();
    }
}