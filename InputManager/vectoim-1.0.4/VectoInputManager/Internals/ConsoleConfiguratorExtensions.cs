using Spectre.Console.Cli;

namespace VectoInputManager.Commands;

/// <summary>
/// Extension methods for configuring Spectre.Console commands used by the CLI tool.
/// </summary>
public static class ConsoleConfiguratorExtensions
{
    /// <summary>
    /// Registers all commands and provides aliases, descriptions, and examples.
    /// </summary>
    /// <param name="config">Spectre.Console configurator.</param>
    public static void ConfigureProgramCommands(this IConfigurator config)
    {
        config.AddCommand<StandardValuesCommand>("sv")
            .WithAlias("standard-values")
            .WithDescription("Create components with standard values")
            .WithExample(["sv", "battery", "battery.json", "-i", "-o", "battery.xml"]);

        config.AddCommand<JobCommand>("job")
            .WithAlias("job-creator")
            .WithDescription("Create jobs with components")
            .WithExample(["job vehicle.xml component_*.xml -o new_vehicle.xml"]);

        config.AddCommand<ComponentCommand>("comp")
            .WithAlias("component")
            .WithDescription("Modify or convert a component file")
            .WithExample(["component", "engine.xml", "engine.json"])
            .WithExample(["component", "engine.json", "engine.xml", "-m", "Data.Model=NewModel"]);
    }
}