using Fasterflect;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using TUGraz.VectoCore.Utils;
using VectoInputManager.Core;
using VectoObjects;
using VectoObjects.Helpers;

namespace VectoInputManager.Commands;

public sealed partial class ComponentCommand(IAnsiConsole console,
    ILogger<ComponentCommand> logger, ComponentBuilder builder, ComponentProcessor processor)
    : Command<ComponentCommand.Settings>
{
    public sealed class Settings : CommonCommandSettings
    {
        [CommandArgument(0, "<INPUT>")]
        [Description("Input files. One must be a component (XML or JSON), others are arbitrary JSON modification files.")]
        // Angle brackets indicate a required argument.
        public string[] InputFiles { get; init; } = [];

        [CommandOption("-o|--output")]
        [Description("Output file for the generated XML or JSON data.")]
        public required string OutputFile { get; set; }

        [CommandOption("-i|--interactive")]
        [Description("Interactive mode for prompting user input.")]
        public bool InteractiveMode { get; set; }

        [CommandOption("-m|--modify")]
        [Description("Values to set for the component. Use the format: Key=Value, e.g.,\n"
            + "Data.Model=MyModel.")]
        public string[] Values { get; init; } = [];

        [CommandOption("-b|--broken-sig")]
        [Description("Action to take when signature verification fails. Options:\n"
            + "[bold]auto[/] (default - re-signs the component),\n"
            + "[bold]sign[/] (signs if missing, blocks on mismatched),\n"
            + "[bold]block[/] (shows error, stops output),\n"
            + "[bold]keep[/] (warning, keeps broken signature),\n"
            + "[bold]re-sign[/] (warning, re-signs component),\n"
            + "[bold]clear[/] (warning, empty signature).")]
        [DefaultValue(BrokenSignatureAction.Auto)]
        [TypeConverter(typeof(EnumCommandLineTypeConverter))]
        public BrokenSignatureAction BrokenSignature { get; set; }
    }

    protected override int Execute([NotNull] CommandContext context, [NotNull] Settings settings, CancellationToken token)
    {
        logger.LogTrace("Command line: {CommandLine}", Environment.CommandLine);
        console.LineToolTitle("Component Tool");

        bool interactive = settings.InteractiveMode;
        bool hasModifyValues = settings.Values.Length > 0
            && settings.Values.Any(v => !string.IsNullOrWhiteSpace(v));

        if (settings.InputFiles.Length == 0)
        {
            console.LineError("No input files specified.");
            return -1;
        }

        if (settings.OutputFile == null)
        {
            console.LineError("No output file specified.");
            return -1;
        }

        // Validate all input files exist
        foreach (var file in settings.InputFiles)
        {
            if (!File.Exists(file))
            {
                console.LineError($"Input file not found: {file}");
                return -1;
            }
        }

        // Classify input files: one component + zero or more arbitrary JSON modifications
        string? componentFile = null;
        List<JObject> jsonModifyObjects = [];

        foreach (var file in settings.InputFiles)
        {
            if (ComponentProcessor.IsArbitraryJsonFile(file))
            {
                var jsonText = File.ReadAllText(file);
                var jObj = JsonConvert.DeserializeObject<JObject>(jsonText);
                if (jObj != null)
                    jsonModifyObjects.Add(jObj);
            }
            else
            {
                if (componentFile != null)
                {
                    console.LineError("Multiple component files specified. Only one component input is allowed.");
                    return -1;
                }
                componentFile = file;
            }
        }

        if (componentFile == null)
        {
            console.LineError("No component file found in the input files.");
            return -1;
        }

        bool hasJsonModify = jsonModifyObjects.Count > 0;

        // Load the component from the input file
        IComponentDeclaration component;
        try
        {
            component = processor.LoadComponent(componentFile);
            console.LineSuccess($"Loaded component: {component.GetType().Name}");
        }
        catch (Exception ex)
        {
            console.LineError($"Failed to load component: {ex.Message}");
            return -1;
        }

        // Determine formats based on file extension
        bool isXmlInput = componentFile.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);
        var isJsonOutput = settings.OutputFile.EndsWith(".json", StringComparison.OrdinalIgnoreCase);

        var originalComponent = component;
        var originalHash = ObjectHashUtils.MakeHash(component);

        // Apply modifications if interactive mode, -m values, or JSON modification files are provided
        if (interactive || hasModifyValues || hasJsonModify)
        {
            var paramBuilder = new ParamProcessor();
            paramBuilder.SetParamValues(settings.Values!, null);

            var pah = new PromptActionHelper(console);

            builder.TemplateObject = component;
            builder.PromptHandler = action =>
            {
                // First try to get value from command line parameters
                if (paramBuilder.TryGetValue(action, out object? obj))
                {
                    action.Answer = obj;
                    return;
                }

                // Then try from arbitrary JSON modification files
                foreach (var jObj in jsonModifyObjects)
                {
                    var (found, value) = PathUtils.FindJObject(jObj, action, logger);
                    if (found)
                    {
                        action.Answer = value;
                        return;
                    }
                }

                if (interactive)
                    action.Answer = pah.PromptAction(action);
                else
                    action.Answer = action.TemplateValue;
            };

            // Create a new instance of the same component type and build it in-place
            var componentInstance = (IComponentDeclaration)component.GetType().CreateInstance();
            builder.Run(componentInstance);
            component = componentInstance;

            if (paramBuilder.UnusedParams.Any())
            {
                console.LineWarning("The following input values were not found:");
                foreach (var unused in paramBuilder.UnusedParams)
                {
                    console.LineWarning(" - " + unused);
                }
            }
        }

        // Detect if the component was modified
        bool wasModified = ObjectHashUtils.MakeHash(component) != originalHash;

        // Enforce signature action
        bool skipSigning;
        SignatureType_v2_0? signatureOverride;

        if (isJsonOutput)
        {
            skipSigning = true;
            signatureOverride = XmlUtils.CreateEmptySignature();
        }
        else
        {
            try
            {
                (skipSigning, signatureOverride) = processor.EnforceSignatureAction(
                    originalComponent, wasModified, isXmlInput, settings.BrokenSignature);
            }
            catch (InvalidOperationException ex)
            {
                console.LineError(ex.Message);
                return -1;
            }
        }

        // Ensure output directory exists
        var outputDir = Path.GetDirectoryName(settings.OutputFile);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        // Always write XML to a file for running the validation
        string xmlFile = settings.OutputFile;
        if (isJsonOutput)
        {
            xmlFile = Path.GetTempFileName();
        }

        try
        {
            // Upgrade component if needed and write XML
            var upgraded = TypeUtils.UpgradeComponent(component);
            var vInputDeclaration = TypeUtils.CreateVectoInputDeclaration();
            vInputDeclaration.SetComponentInVectoInputDeclaration(upgraded);

            using var fs = File.Create(xmlFile);
            if (!skipSigning)
            {
                XmlUtils.HashAndSerialize(fs, vInputDeclaration, null);
            }
            else
            {
                // Set the override signature and serialize without hash computation
                upgraded.Signature = signatureOverride!;
                var serializer = new XmlSerializer(typeof(VectoInputDeclaration_v2_2));
                serializer.SerializeFixed(fs, vInputDeclaration);
            }

            if (isJsonOutput)
            {
                JsonDeclarationUtils.SerializeToFile(upgraded, settings.OutputFile, true);
            }

            console.LineAction("Output written to", settings.OutputFile);
        }
        catch (Exception ex)
        {
            console.LineError($"Failed to write output: {ex.Message}");
            return -1;
        }

        using (var stream = File.OpenRead(xmlFile))
        {
            using var xmlReader = System.Xml.XmlReader.Create(stream);
            var validator = new XMLValidator(xmlReader);
            bool isValid = validator.ValidateXML(XmlDocumentType.DeclarationComponentData);
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

        // Delete the temporary XML file if we only wanted JSON output
        if (isJsonOutput && File.Exists(xmlFile))
        {
            File.Delete(xmlFile);
        }

        return 0;
    }
}
