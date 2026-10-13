using Fasterflect;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using TUGraz.VectoCore.Utils;
using VectoInputManager.Core;

namespace VectoInputManager.Commands;

public sealed class StandardValuesCommand(IAnsiConsole console,
    ILogger<StandardValuesCommand> logger, IStandardValuesFactory standardValuesFactory,
    StandardValuesBuilder builder)
    : Command<StandardValuesCommand.Settings>
{
    public sealed class Settings : CommonCommandSettings
    {
        [CommandArgument(0, "[INPUT]")]
        [Description("Input file containing component data in JSON format.")]
        // Square brackets indicate an optional argument.
        public string? InputFile { get; set; }

        [CommandOption("-i|--interactive")]
        [Description("Interactive mode.")]
        public bool InteractiveMode { get; set; }

        [CommandOption("-t|--type")]
        [DefaultValue("auto")]
        [Description("The component type to create. The default is 'auto', which automatically detects the component type from the input JSON. For a list of supported components, use the --list argument.")]
        public string? Component { get; set; }

        [CommandOption("-o|--output")]
        [Description("Output file for the generated XML or JSON data.")]
        public string? OutputFile { get; set; }

        [CommandOption("-m|--modify")]
        [Description("Values to set for the component. Use the format: Key=Value, e.g., R100CellsInSeries=20.")]
        public required string[] Values { get; set; }

        [CommandOption("-e|--example")]
        [Description("Generates an example JSON file for the component.")]
        public bool? Example { get; set; }

        [CommandOption("-l|--list")]
        [Description("Lists available components.")]
        public bool ComponentList { get; set; }
    }

    protected override int Execute([NotNull] CommandContext context, [NotNull] Settings settings, CancellationToken token)
    {
        logger.LogTrace("Command line: {CommandLine}", Environment.CommandLine);
        console.LineToolTitle("Standard Values Tool");

        ComponentType? componentType = null;

        if (settings.ComponentList)
        {
            PrintValidComponents(console);
            return 0;
        }

        BaseInput? template = null;
        bool isAuto = string.Equals(settings.Component, "auto", StringComparison.OrdinalIgnoreCase);
        bool inputProvided = !string.IsNullOrEmpty(settings.InputFile);
        bool outputProvided = !string.IsNullOrEmpty(settings.OutputFile);
        bool interactive = settings.InteractiveMode;

        // In auto mode, we expect an input file must be provided.
        if (isAuto && !inputProvided)
        {
            console.LineError("For auto component mode, input json file is required.");
            return -1;
        }

        // Not in auto mode? Ensure we get the component type correctly.
        if (!isAuto)
        {
            if (!Enum.TryParse(settings.Component, true, out ComponentType s))
            {
                console.LineError("Invalid component type.");
                PrintValidComponents(console);
                return -1;
            }
            else
            {
                componentType = s;
            }
        }

        // No input file, but output file and not interactive? Error.
        // Exception: -e (example) is valid with -o and no input file.
        if (!inputProvided && outputProvided && !interactive && !settings.Example.GetValueOrDefault())
        {
            console.LineError("No input file specified. Interactive mode is required when no input file is provided.");
            return -1;
        }

        // We have an input file? Deserialize it.
        if (inputProvided)
        {
            if (!File.Exists(settings.InputFile))
            {
                console.LineError($"Input file not found: {settings.InputFile}");

                // Common mistake: passing a component type as the positional argument
                if (Enum.TryParse<ComponentType>(settings.InputFile, true, out _))
                {
                    console.LineInfo($"Did you mean to specify a component type? Use: sv -t {settings.InputFile} ...");
                }

                return -1;
            }

            try
            {
                template = JsonInputUtils.Deserialize(File.ReadAllText(settings.InputFile));
            }
            catch (Exception ex)
            {
                console.LineError($"Failed to read input file: {ex.Message}");
                return -1;
            }

            // Special case: If the user did not choose auto component mode,
            // we expect the input file to match the component type.
            if (!isAuto && componentType != template.Component)
            {
                console.LineError($"Component type mismatch: expected {componentType}, but input file contains {template.Component}.");
                return -1;
            }

            componentType = template.Component;
        }

        // Sanity check: This should never happen.
        if (componentType == null)
            throw new InvalidOperationException("Internal error: component not specified.");

        var componentInfo = Components.ComponentInformation[componentType.Value];
        var componentCalculator = standardValuesFactory.Create(componentType.Value);

        // Do we want an example?
        if (settings.Example.GetValueOrDefault())
        {
            // If the user requested an example, we create a default input for the component.
            template = componentCalculator.CreateInput();
            template.SetExampleValues();

            var json = JsonInputUtils.Serialize(template, false);

            console.LineSuccess($"Here is an example JSON for a {componentInfo.Description}:");
            console.WriteLine();
            console.Write(new JsonText(json));
            console.WriteLine();
            console.WriteLine();

            if (outputProvided)
            {
                File.WriteAllText(settings.OutputFile!, JsonInputUtils.Serialize(template, true));
                console.LineAction("Example JSON saved to", settings.OutputFile!);
            }

            return 0;
        }

        // Ensure we have an output file specified.
        if (!outputProvided)
        {
            console.LineError("Output file must be specified.");
            return -1;
        }

        // We are all set to create the component.
        builder.TemplateObject = template;

        // Any input values from the command line?
        var paramBuilder = new ParamProcessor();
        paramBuilder.SetParamValues(settings.Values, null);

        // Set up the prompt handler for all user interaction
        var pah = new PromptActionHelper(console);
        var finalInput = componentCalculator.CreateInput();

        builder.PromptHandler = action =>
        {
            var path = action.Property;

            // First try to get value from command line parameters
            if (paramBuilder.TryGetValue(action, out object? obj))
            {
                action.Answer = obj;
                return;
            }

            if (interactive)
                action.Answer = pah.PromptAction(action);
            else
                action.Answer = action.TemplateValue;
        };

        builder.Run(finalInput);

        var sv = componentCalculator.Calculate(finalInput);

        // Determine output format based on file extension
        var outputExtension = Path.GetExtension(settings.OutputFile)!.ToLowerInvariant();
        var isJsonOutput = outputExtension == ".json";
        var xmlFile = settings.OutputFile;

        if (isJsonOutput)
        {
            // Output JSON format
            JsonDeclarationUtils.SerializeToFile(sv, settings.OutputFile!, true);
            xmlFile = Path.GetTempFileName();
        }

        // Output XML format
        var vInputDeclaration = TypeUtils.CreateVectoInputDeclaration();
        componentCalculator.SetInput(vInputDeclaration, sv);

        using (var fs = File.Create(xmlFile!))
            XmlUtils.HashAndSerialize(fs, vInputDeclaration, finalInput);

        console.LineAction("Output written to", settings.OutputFile!);

        // Validate the XML output.
        using (var stream = File.OpenRead(xmlFile!))
        using (var xmlReader = XmlReader.Create(stream))
        {
            var validator = new XMLValidator(xmlReader);
            bool isValid = validator.ValidateXML(TUGraz.VectoCore.Utils.XmlDocumentType.DeclarationComponentData);
            if (isValid)
            {
                console.LineSuccess("XML output validated successfully.");
            }
            else
            {
                console.LineError("XML output was NOT validated successfully.");
                console.LineActionError("Error", validator.ValidationError);
                console.WriteLine();
            }
        }

        // Delete the temporary XML file if we only wanted JSON output.
        if (isJsonOutput && File.Exists(xmlFile))
        {
            File.Delete(xmlFile);
        }


        console.WriteLine();
        console.LineWarningBold("Component not for certification use.");

        return 0;
    }

    private static void PrintValidComponents(IAnsiConsole console)
    {
        console.LineInfo("Valid components are:");

        var table = new Table();

        // Add some columns
        table.AddColumn("Code");
        table.AddColumn("Description");
        table.AddColumn(new TableColumn("Cert.\nCode").Centered());
        table.AddColumn("Declaration\nType");

        foreach (var value in Components.ComponentInformation)
        {
            table.AddRow([
                new Markup($"[bold]{value.ComponentType}[/]"),
                        new Markup($"{value.Description}"),
                        new Markup($"{value.Code}"),
                        new Markup(string.Join("\n", value.Declarations
                            .Where(c=> c.StandarValuesEnabled)
                            .Select(c=> c.Description)))]);
        }

        console.Write(table);
    }

}
