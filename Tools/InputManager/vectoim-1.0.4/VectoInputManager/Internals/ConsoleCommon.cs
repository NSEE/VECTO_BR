using System;
using Microsoft.Extensions.FileSystemGlobbing;
using Spectre.Console;
using VectoInputManager.Core;

namespace VectoInputManager.Commands;

/// <summary>
/// Console helper utilities for scanning input files and updating the data corpus.
/// </summary>
public static class ConsoleCommon
{
    /// <summary>
    /// Scans input paths, adds files to the <see cref="CorpusProcessor"/>, and prints status messages.
    /// </summary>
    /// <param name="console">The console to write messages to.</param>
    /// <param name="corpus">The corpus to update.</param>
    /// <param name="inputs">Input paths or glob patterns to scan.</param>
    public static void UpdateCorpusWithVehicleType(IAnsiConsole console, CorpusProcessor corpus, string[] inputs)
    {
        foreach (var filePath in ScanFiles(inputs, console))
        {
            console.LineFound("input file", filePath);
            var result = corpus.AddCorpusFile(filePath);
            if (result.IsError)
            {
                console.LineError($"Failed to add file {result.FirstError} to corpus.");
            }
            else
            {
                console.LineSuccess(result.Value);
            }
        }
    }

    /// <summary>
    /// Enumerates files that match the provided input paths and glob patterns.
    /// </summary>
    /// <param name="inputs">Input paths or glob patterns.</param>
    /// <returns>All matching files as full paths.</returns>
    public static IEnumerable<string> ScanFiles(string[] inputs, IAnsiConsole? console)
    {
        foreach (var input in inputs)
        {
            var path = Path.GetDirectoryName(input);
            var file = Path.GetFileName(input);

            Matcher matcher = new();
            matcher.AddIncludePatterns([file]);
            string searchDirectory = string.IsNullOrEmpty(path) ? Environment.CurrentDirectory : path;
            var matchingFiles = matcher.GetResultsInFullPath(searchDirectory);

            if (!matchingFiles.Any())
            {
                console?.LineWarning($"Not found: {input}");
            }
            else
            {
                foreach (var matchingFile in matchingFiles)
                {
                    yield return matchingFile;
                }
            }
        }
    }

    /// <summary>
    /// Extracts components from a vehicle object into the specified directory.
    /// </summary>
    /// <param name="console">The console for user output.</param>
    /// <param name="vehicle">The vehicle object to extract components from.</param>
    /// <param name="outputDirectory">The directory to write extracted files to.</param>
    /// <param name="outputXml">Whether to output XML files.</param>
    /// <param name="outputJson">Whether to output JSON files.</param>
    /// <returns>0 on success, -1 on failure.</returns>
    public static int ExtractComponents(IAnsiConsole console, object vehicle,
        string outputDirectory, bool outputXml, bool outputJson)
    {
        // Ensure output directory exists
        if (!Directory.Exists(outputDirectory))
        {
            try
            {
                Directory.CreateDirectory(outputDirectory);
                console.LineCreated("output directory", outputDirectory);
            }
            catch (Exception ex)
            {
                console.LineError($"Failed to create output directory: {ex.Message}");
                return -1;
            }
        }

        int totalComponentsExtracted = 0;

        foreach (var (component, path) in TypeUtils.FindComponentDelarations(vehicle, null))
        {
            if (component == null)
            {
                console.LineWarning("Skipping null component declaration.");
                continue;
            }

            var name = path ?? "UnknownComponent";

            console.LineAction("Extracting component", name);

            try
            {
                // Output XML file
                if (outputXml)
                {
                    var xmlFilename = $"{name}.xml";
                    var xmlOutput = Path.Combine(outputDirectory, xmlFilename);

                    using (var fs = new FileStream(xmlOutput, FileMode.Create, FileAccess.Write))
                    {
                        var vInputDeclaration = TypeUtils.CreateVectoInputDeclaration();
                        var newComponent = TypeUtils.UpgradeComponent(component);
                        vInputDeclaration.SetComponentInVectoInputDeclaration(newComponent);
                        XmlUtils.HashAndSerialize(fs, vInputDeclaration, null);
                    }

                    console.LineFileOperation("  XML", xmlFilename);
                }

                // Output JSON file
                if (outputJson)
                {
                    var jsonFilename = $"{name}.json";
                    var jsonOutput = Path.Combine(outputDirectory, jsonFilename);

                    JsonDeclarationUtils.SerializeToFile(component, jsonOutput, true);
                    console.LineFileOperation("  JSON", jsonFilename);
                }

                totalComponentsExtracted++;
            }
            catch (Exception ex)
            {
                console.LineError($"Failed to extract component '{name}': {ex.Message}");
            }
        }

        console.LineSuccess($"Successfully extracted {totalComponentsExtracted} components to {outputDirectory}");
        return 0;
    }
}
