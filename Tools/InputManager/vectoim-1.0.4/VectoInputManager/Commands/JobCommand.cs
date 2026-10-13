using Fasterflect;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using VectoInputManager.Core;
using VectoObjects;
using VectoObjects.Helpers;

namespace VectoInputManager.Commands;

public sealed partial class JobCommand(IAnsiConsole console,
    ILogger<JobCommand> logger, CorpusProcessor corpus, VehicleBuilder builder)
    : Command<JobCommand.Settings>
{
    public sealed class Settings : CommonCommandSettings
    {
        [CommandArgument(0, "[INPUT]")]
        [Description("Input files for the job. You can specify multiple files or glob patterns.")]
        // Square brackets indicate an optional argument.
        public string[] InputFiles { get; init; } = [];

        [CommandOption("-t|--type")]
        [DefaultValue("auto")]
        [Description("The vehicle type to create. For a list of supported vehicles, use the --list argument.")]
        public string? Vehicle { get; set; }

        [CommandOption("-i|--interactive")]
        [DefaultValue(InteractiveModeOptions.none)]
        [Description("Interactive mode for prompting user input. Options:\n"
            + "[bold]vehicle[/] (prompt only for vehicle-level),\n"
            + "[bold]all[/] (prompt for all values),\n"
            + "[bold]auto[/] (prompt only when no value is available).")]
        public InteractiveModeOptions InteractiveMode { get; set; }

        [CommandOption("-o|--output")]
        [Description("Output file for the generated XML or JSON data.")]
        public string? OutputFile { get; set; }

        [CommandOption("-k|--key-value-file")]
        [Description("Generates a values file (.values.txt) containing all keys and values assigned during vehicle creation.")]
        public bool KeyValueFile { get; set; }

        [CommandOption("-m|--modify")]
        [Description("Values to set for the job. Use the format: Path.Key=Value, e.g.,\n"
            + "Components.AxleWheels.Data.Axles[[0]].TwinTyres=True.\n"
            + "You can also assign components to specific vehicle paths, e.g.,\n"
            + "Components.Powertrain[[1]].Axlegear=axlegear.xml.")]
        public string[] Values { get; init; } = [];

        [CommandOption("-e|--example")]
        [Description("Generates an example JSON file for the component.")]
        public bool? Example { get; set; }

        [CommandOption("-l|--list")]
        [Description("Lists available vehicles.")]
        public bool ComponentList { get; set; }

        [CommandOption("-x|--extract")]
        [Description("Output directory where extracted component files will be saved. Cannot be used together with -o.")]
        public string? ExtractDirectory { get; set; }

        [CommandOption("-X|--xformat")]
        [DefaultValue(OutputFormat.Xml)]
        [Description("Output format for extracted components. Valid options: xml, json, both. The default is xml.")]
        public required OutputFormat OutputFormat { get; set; }

        [CommandOption("-b|--broken-sig")]
        [Description("Action to take when signature verification fails. Options:\n"
            + "[bold]auto[/] (default - keeps empty signatures, blocks on mismatched signatures),\n"
            + "[bold]sign[/] (signs missing, blocks on mismatched signatures),\n"
            + "[bold]block[/] (shows error, stops output),\n"
            + "[bold]keep[/] (warning, keeps broken signature),\n"
            + "[bold]re-sign[/] (warning, re-signs component),\n"
            + "[bold]clear[/] (warning, empty signature).")]
        [DefaultValue(BrokenSignatureAction.Auto)]
        [TypeConverter(typeof(EnumCommandLineTypeConverter))]
        public BrokenSignatureAction BrokenSignature { get; set; }

        [CommandOption("-d|--drop-components")]
        [Description("Removes (nullifies) all components from the vehicles before converting them to JSON.")]
        public bool DropComponents { get; set; }
    }

    public enum InteractiveModeOptions
    {
        none,
        vehicle,
        all,
        auto
    }

    public enum OutputFormat
    {
        Xml,
        Json,
        Both
    }

    protected override int Execute([NotNull] CommandContext context, [NotNull] Settings settings, CancellationToken token)
    {
        logger.LogTrace("Command line: {CommandLine}", Environment.CommandLine);
        console.LineToolTitle("Job Creator Tool");

        // Do we want to simply list available vehicles?
        if (settings.ComponentList)
        {
            Vehicles(console);
            return 0;
        }

        var vehicleType = Data.GetVehicleTypes()
            .SingleOrDefault(v => string.Equals(v.Code, settings.Vehicle, StringComparison.OrdinalIgnoreCase));

        // Do we want an example?
        if (settings.Example.GetValueOrDefault())
        {
            if (string.Equals(settings.Vehicle, "auto", StringComparison.OrdinalIgnoreCase))
            {
                console.LineError("For example generation, a specific vehicle type must be provided instead of 'auto'.");
                return -1;
            }

            if (vehicleType == null)
            {
                console.LineError($"Unknown vehicle type '{settings.Vehicle}'. Use --list to see valid vehicle types.");
                return -1;
            }

            var vehicle = vehicleType.FindType().CreateInstance();
            var json = JsonDeclarationUtils.Serialize(vehicle, false);
            console.LineSuccess($"Here is an example JSON for a {settings.Vehicle}:");
            console.WriteLine();
            console.Write(new JsonText(json));
            console.WriteLine();
            console.WriteLine();

            // Prompt the user for an output filename, with a default suggestion.
            var outputFilename = console.Prompt(
                new TextPrompt<string>($"Would you like to save this template? Enter a filename or leave empty to exit:")
                    .AllowEmpty());

            if (!string.IsNullOrWhiteSpace(outputFilename))
            {
                JsonDeclarationUtils.SerializeToFile(vehicle, outputFilename, true);
                console.LineAction("Example JSON saved to", outputFilename);
            }

            return 0;
        }

        bool outputProvided = !string.IsNullOrEmpty(settings.OutputFile);
        bool extractProvided = !string.IsNullOrEmpty(settings.ExtractDirectory);
        bool inputProvided = settings.InputFiles.Length > 0;

        bool interactive = settings.InteractiveMode != InteractiveModeOptions.none;

        // Ensure we have an output XML file or extract directory specified.
        if (!outputProvided && !extractProvided)
        {
            console.LineError("Output file (-o) or extract directory (-x) must be specified.");
            return -1;
        }

        // Validate extract format
        bool extractXml = false;
        bool extractJson = false;

        if (extractProvided)
        {
            extractXml = settings.OutputFormat == OutputFormat.Xml
              || settings.OutputFormat == OutputFormat.Both;
            extractJson = settings.OutputFormat == OutputFormat.Json
              || settings.OutputFormat == OutputFormat.Both;
        }

        // Ensure outputfile directory exists   
        if (outputProvided)
        {
            var outputDir = Path.GetDirectoryName(settings.OutputFile);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
        }

        if (inputProvided)
        {
            ConsoleCommon.UpdateCorpusWithVehicleType(console, corpus, settings.InputFiles!);
            if (corpus.Vehicle == null && corpus.ComponentObjects.Count == 0
                && !corpus.ArbitraryObjects.Any())
            {
                console.LineError("No matching input files found.");
                return -1;
            }
        }

        if (!interactive && vehicleType == null && corpus.Vehicle == null)
        {
            console.LineError("A vehicle type must be specified in not-interactive mode.");
            return -1;
        }

        bool bareVehicle = !interactive && corpus.Vehicle == null;

        // Can be null
        builder.TemplateObject = corpus.Vehicle;

        // Any input values from the command line?
        var paramBuilder = new ParamProcessor();
        paramBuilder.SetParamValues(settings.Values, corpus);

        // Set up the prompt handler for all user interaction
        var pah = new PromptActionHelper(console);

        List<string> keyValueList = [];

        builder.PromptHandler = action =>
        {
            object? val()
            {
                var path = action.Property;

                // The user set a specific vehicle type from the command line? Use it.
                if (action.Property == "@VehicleType" && vehicleType != null)
                    return vehicleType.Code;

                // First try to get value from command line parameters
                if (paramBuilder.TryGetValue(action, out object? obj))
                    return obj;

                // Then try from any raw JSON file
                var corpusFound = corpus.FindFromArbitraryObjectsByPath(action);
                if (corpusFound.found)
                    return corpusFound.value;

                // Then try to find the value in the corpus
                corpusFound = corpus.FindComponentByPath(path);
                if (corpusFound.found)
                    return corpusFound.value;

                // Interactive mode - prompt the user
                if (interactive)
                {
                    // Auto interactive mode - only prompt when no other value is available
                    if (settings.InteractiveMode == InteractiveModeOptions.auto)
                    {
                        if (action.TemplateValueSpecified)
                            return action.TemplateValue;
                    }

                    // If we are only prompting for vehicle-level and this is a component
                    // property, skip it by selecting a nice default to populate it.
                    if (settings.InteractiveMode == InteractiveModeOptions.vehicle
                         && WellKnownStartingRegex.IsMatch(action.Property))
                    {
                        if (!action.TemplateValueSpecified)
                            return GetDefaultValue(action);
                        else
                            return action.TemplateValue;
                    }

                    return pah.PromptAction(action);
                }

                // Non-interactive mode - use default value from template
                else
                {
                    return action.TemplateValue;
                }
            }

            var obj = val();
            keyValueList.Add($"{action.Property}={obj}");
            action.Answer = obj;
        };

        builder.Run();

        var job = VectoXml.CreateVehicleJobDeclaration(
            builder.Object as IVehicleDeclaration);

        corpus.EnforceSignatureAction(job.Vehicle!, settings.BrokenSignature);

        // Extract mode: extract components to the specified directory
        if (extractProvided)
        {
            var extractResult = ConsoleCommon.ExtractComponents(
                console, job.Vehicle!, settings.ExtractDirectory!, extractXml, extractJson);
            if (extractResult != 0)
                return extractResult;
        }

        // Remove components if requested
        if (settings.DropComponents)
        {
            foreach (var component in TypeUtils.FindComponentDelarations(job.Vehicle!, null))
            {
                var (prop, value, parent) = PathUtils.FindValue(job.Vehicle!, component.path);
                if (prop != null)
                {
                    prop.SetValue(parent, null);
                    console.LineInfo($"Removed component at {component.path}");
                }
            }
        }

        if (outputProvided)
        {
            // Determine output format based on file extension
            var outputExtension = Path.GetExtension(settings.OutputFile!).ToLowerInvariant();
            var isJsonOutput = outputExtension == ".json";

            // Always write XML to a file for running the validation
            var xmlFile = settings.OutputFile;

            if (isJsonOutput)
            {
                // Output JSON format
                JsonDeclarationUtils.SerializeToFile(job.Vehicle!, settings.OutputFile!, true);
                xmlFile = Path.GetTempFileName();
            }

            // Output XML format (always create for validation)
            using (var stream = File.Create(xmlFile!))
            {
                corpus.Save(stream, job);
            }

            console.LineAction("Output written to", settings.OutputFile!);

            var reportPath = Path.ChangeExtension(settings.OutputFile, ".report.txt");
            console.LineAction("Report written to", reportPath!);
            console.WriteLine();

            // Generate report
            using (var writer = new StreamWriter(reportPath!))
            {
                foreach (var line in corpus.WriteReport((IVehicleDeclaration)job.Vehicle!,
                    isJsonOutput ? xmlFile! : settings.OutputFile!))
                {
                    writer.WriteLine(line);
                    logger.LogInformation(line);
                }
            }

            // Delete the temporary XML file if we only wanted JSON output
            if (isJsonOutput && File.Exists(xmlFile))
            {
                File.Delete(xmlFile);
            }
        }

        if (paramBuilder.UnusedParams.Any())
        {
            console.LineWarning("The following input values were not found:");
            foreach (var unused in paramBuilder.UnusedParams)
            {
                console.LineWarning(" - " + unused);
            }
        }

        if (settings.KeyValueFile)
        {
            var kvFile = extractProvided
                ? Path.Combine(settings.ExtractDirectory!, "job.values.txt")
                : Path.ChangeExtension(settings.OutputFile, ".values.txt");
            File.WriteAllLines(kvFile!, keyValueList);
            console.LineAction("Parameter log written to", kvFile!);
        }

        return 0;
    }

    // Get a default value for the prompt action based on its type. 
    // Used in vehicle interactive mode to skip component-level prompts.
    private static string? GetDefaultValue(PromptAction action)
    {
        if (action.Type == PromptActionType.Class
             || action.Type == PromptActionType.Enum)
        {
            return action.Selection!.First().Key;
        }
        else
        {
            return null;
        }
    }

    private static void Vehicles(IAnsiConsole console)
    {
        console.LineInfo("Valid vehicles are:");

        var table = new Table();

        table.AddColumn("Code");
        table.AddColumn("Description");
        table.AddColumn("Version");

        foreach (var value in Data.GetVehicleTypes().Where(c => c.Active))
        {
            table.AddRow(
                new Markup($"[bold]{value.Code}[/]"),
                new Markup($"{value.Description}"),
                new Markup($"{value.Version}")
            );
        }

        console.Write(table);
    }

#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
    public static readonly Regex WellKnownStartingRegex = new(
        Consts.WellKnownRegexString, RegexOptions.IgnoreCase);
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
}