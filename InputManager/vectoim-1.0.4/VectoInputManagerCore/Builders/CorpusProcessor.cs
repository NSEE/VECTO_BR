using ErrorOr;
using Fasterflect;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using TUGraz.VectoCore.Utils;
using VectoObjects;
using VectoObjects.Helpers;

namespace VectoInputManager.Core;

/// <summary>
/// Builds a corpus from mixed input files (vehicle XML/JSON, component XML/JSON, and
/// arbitrary JSON fragments). Maps components to well-known property paths, exposes helper lookups
/// used by interactive prompts, and produces summary/report outputs including hash error fixes.
/// </summary>
public class CorpusProcessor(IStandardValuesFactory standardValuesFactory,
    ILogger<CorpusProcessor> logger)
{
    public IReadOnlyList<CorpusObject> Objects =>
        [.. _corpusObjects.OrderBy(c => c.FileName)];

    public IEnumerable<CorpusObject> Components => _corpusObjects
        .Where(c => c.ArbitraryObject == false);

    public IEnumerable<CorpusObject> ArbitraryObjects => _corpusObjects
        .Where(c => c.ArbitraryObject == true);

    public IReadOnlyList<object> ComponentObjects => [.. Components.Select(c => c.Object)];

    public required IVehicleDeclaration Vehicle { get; set; }
    public required string VehiclePath { get; set; }

    // Original components by data hash for signature validation.
    // Populated from the vehicle and component input files.
    readonly private Dictionary<string, (string path, IComponentDeclaration component, bool isXML)>
        _originalComponents = [];

    readonly private List<(string path, SignatureValidation result)> _hashErrors = [];

    public IReadOnlyList<(string path, SignatureValidation result)> HashErrors => _hashErrors;

    private readonly List<(string path, ISignatureType? sig)> _signatureOverrides = [];

    private readonly List<CorpusObject> _corpusObjects = [];

    /// <summary>
    /// Creates a duplicate <see cref="CorpusObject"/> in the corpus so the same component
    /// can be mapped to more than one vehicle path (e.g. two powertrains sharing the same IEPC file).
    /// </summary>
    public CorpusObject DuplicateComponent(CorpusObject original)
    {
        var duplicate = new CorpusObject(original.FileName, original.Object);
        _corpusObjects.Add(duplicate);
        return duplicate;
    }

    // This is designed to be called by the VehicleBuilder.PromptHandler
    public (bool found, object? value) FindFromArbitraryObjectsByPath(PromptAction action)
    {
        foreach (var arbObj in ArbitraryObjects)
        {
            var jObject = (JObject)arbObj.Object;
            var (found, value) = PathUtils.FindJObject(jObject, action, logger);

            if (found)
            {
                logger.LogTrace(
                    "Providing arbitrary object property '{Property}' from file '{Component}' for path '{Path}'",
                        action.Property, arbObj.FileName, action.Property);
                return (true, value);
            }
        }

        return (false, null);
    }

    // This is designed to be called by the VehicleBuilder.PromptHandler
    public (bool found, object? value) FindComponentByPath(string path)
    {
        // Is it requesting an exact top-level component?
        var match = WellKnownRegex.Match(path);

        if (match.Success)
        {
            // We have a path that looks like "Components.AirDrag" or
            // "Components.ElectricEnergyStorage.Battery[0].REESS". Do we already have a component
            // mapped to this path?
            if (!Components.Any(c =>
                string.Equals(path, c.PropertyPath, StringComparison.OrdinalIgnoreCase)))
            {
                // No? Try to find a matching component by well-known pattern name.
                var componentName = match.Groups["name"].Value;

                // What could it be from the loaded and unused yet components?
                // (unused means PropertyPath == null)
                var possibleComponents = Components
                    .Where(c => c.PropertyPath == null)
                    .Where(c => IsComponentNameMatch(c.Object, componentName, path))
                    .ToList();

                // We'll choose the first one that matches. We assume the user has named the files
                // in a sorted manner in cases of arrays that take components (like the FuelCellSystem).
                var componentCorpus = possibleComponents.FirstOrDefault();
                if (componentCorpus != null)
                {
                    // This is what we wanted. Now the corpus object knows where it belongs.
                    componentCorpus.PropertyPath = match.Groups[0].Value;
                    logger.LogTrace(
                        "Mapped component '{Component}' to path '{Path}'",
                            componentCorpus.FileName, componentCorpus.PropertyPath);
                }
            }
        }

        // Let's try to map the path to a component or it's properies.
        var pathsAndObjects = Components.Where(c => !string.IsNullOrEmpty(c.PropertyPath));

        // We will iterate over all known paths and objects, and see if the requested path
        // starts with any of them. If it does, we will try to get the relative
        // path and find the value inside the object.
        foreach (var kvp in pathsAndObjects)
        {
            if (!path.StartsWith(kvp.PropertyPath!, StringComparison.OrdinalIgnoreCase))
                continue;

            var relativePath = path[kvp.PropertyPath!.Length..];

            // Exact match: we are at the component root.
            if (string.IsNullOrEmpty(relativePath))
            {
                // Mark the component as used.
                // Use the corpus object matched by path (kvp) directly rather than
                // searching by ReferenceEquals, because duplicate corpus objects
                // may share the same underlying Object reference.
                if (kvp.IsUsed == false)
                {
                    kvp.IsUsed = true;
                }
                else if (kvp.IsUsed == true)
                {
                    throw new InvalidOperationException($"Object '{kvp}' is already used.");
                }

                // This is important: We need to set the existing component in the vehicle to null
                // else larger, existing arrays will not be cropped down.
                var (prop, value, parent) = PathUtils.FindValue(Vehicle, path);
                if (prop != null && value != null)
                    prop.SetValue(parent, null);

                logger.LogTrace(
                    "Providing component '{Component}' for path '{Path}'", kvp.FileName, path);

                return (true, kvp.Object);
            }
            else
            {
                relativePath = relativePath[1..];
                var (prop, value, parent) = PathUtils.FindValue(kvp.Object, relativePath);
                if (prop != null)
                {
                    kvp.IsUsed = true;

                    logger.LogTrace(
                        "Providing property '{Property}' from component '{Component}' for path '{Path}'",
                            relativePath, kvp.FileName, path);

                    return (true, value);
                }
            }
        }

        return (false, null);
    }

    public ErrorOr<string> AddCorpusFile(string filePath)
    {
        bool isJson = filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
        bool isXml = filePath.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);

        if (isJson)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(fs);
            var raw = new ReadOnlySpan<byte>(System.Text.Encoding.UTF8.GetBytes(reader.ReadToEnd()));
            var json = System.Text.Encoding.UTF8.GetString(raw);

            var jsonReader = new System.Text.Json.Utf8JsonReader(raw);

            if (!jsonReader.Read() || jsonReader.TokenType != System.Text.Json.JsonTokenType.StartObject)
                return Error.Failure("Invalid JSON format: Expected start of object.");

            if (!jsonReader.Read() || jsonReader.TokenType != System.Text.Json.JsonTokenType.PropertyName)
                return Error.Failure("Invalid JSON format: Expected property name.");

            string? propertyName = jsonReader.GetString();

            // Skip $schema if present
            if (propertyName == "$schema" && jsonReader.Read() && jsonReader.Read())
            {
                propertyName = jsonReader.GetString() ?? string.Empty;
            }

            // PropertyName should be either "$type" or "Component". We are expecting this
            // as the first property in the JSON object.

            if (propertyName == "$type")
            {
                var obj = JsonDeclarationUtils.Deserialize(json);

                if (obj is IVehicleDeclaration vehicle)
                {
                    SetVehicleInput(filePath, vehicle, false);
                    return $"Vehicle '{vehicle.GetType().Name}' added successfully.";
                }
                else
                {
                    AddComponentInput(filePath, obj, false);
                    return $"Component Input '{obj.GetType().Name}' added successfully.";
                }
            }
            else if (propertyName == "Component")
            {
                var component = JsonInputUtils.Deserialize(json);

                var componentCalculator = standardValuesFactory.Create(component.Component);
                var declaration = componentCalculator.Calculate(component);

                AddComponentInput(filePath, declaration, false);
                return $"Component Input '{declaration.GetType().Name}' added successfully.";
            }
            else
            {
                var arbitraryObj = JsonConvert.DeserializeObject<JObject>(json);
                AddArbitraryInput(filePath, arbitraryObj!);
                return $"Arbitrary JSON object from '{Path.GetFileName(filePath)}' added successfully.";
            }
        }

        if (isXml)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            if (!VectoXml.TryGetSerializer(fileStream, out var serializer, out var rootType, out var error))
                return Error.Unexpected(error);

            var item = serializer.Deserialize(fileStream)!;

            if (item is IVehicleJobDeclaration job)
            {
                if (item.GetPropertyValue("Vehicle") is not IVehicleDeclaration vehicle)
                    return Error.Failure("Vehicle job does not contain a vehicle.");

                SetVehicleInput(filePath, vehicle, true);
                return $"Vehicle '{vehicle.GetType().Name}' added successfully.";
            }
            else
            {
                // We have an VectoInputDeclaration? Try to find the first component inside it.
                var component = ExtractFirstComponent(item);
                if (component != null)
                {
                    AddComponentInput(filePath, component, true);
                    return $"Component '{component.GetType().Name}' added successfully.";
                }
            }
        }

        return Error.Failure("Unsupported file format. Only JSON and XML files are supported.");
    }

    // Should be called before serialization to enforce the signature rule.
    public void EnforceSignatureAction(IVehicleDeclaration veh,
        BrokenSignatureAction signatureAction)
    {
        _hashErrors.Clear();
        _signatureOverrides.Clear();

        foreach (var component in TypeUtils.FindComponentDelarations(veh, null))
        {
            // Special case: For Clear, we always want to clear the signature of every component.
            if (signatureAction == BrokenSignatureAction.Clear)
            {
                _signatureOverrides.Add((component.path, null));
            }

            // We'll create a hash of the new component data for comparison.
            var newComponentHash = ObjectHashUtils.MakeHash(component.component);

            // The new component is exactly the same as the original? Enfore business rules.
            if (_originalComponents.TryGetValue(newComponentHash, out var originalComponent))
            {
                // Ignore JSON components marked to skip signature validation
                if (!originalComponent.isXML)
                {
                    // For JSON inputs, an exception for Auto - we won't sign it.
                    if (signatureAction != BrokenSignatureAction.Auto)
                        continue;
                }

                var result = XmlUtils.ValidateComponentHashes(originalComponent.component);
                bool componentHasValidSignature = result == SignatureValidation.Valid;

                if (!componentHasValidSignature)
                {
                    var originalSignature = originalComponent.component.Signature;

                    switch (signatureAction)
                    {
                        case BrokenSignatureAction.Auto:
                        case BrokenSignatureAction.Sign:
                            if (result == SignatureValidation.Empty)
                            {
                                if (signatureAction == BrokenSignatureAction.Auto)
                                    _signatureOverrides.Add((component.path, null));

                                logger.LogInformation("Component '{Path}' has empty signature and will be kept as-is.",
                                    component.path);
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    $"Component '{component.path}': {_validationResultMessages[result]}");
                            }
                            break;

                        case BrokenSignatureAction.Block:
                            throw new InvalidOperationException(
                                $"Component at path '{component.path}': {_validationResultMessages[result]}");

                        case BrokenSignatureAction.Keep:
                            _hashErrors.Add((component.path, result));
                            _signatureOverrides.Add((component.path, originalSignature));
                            break;

                        case BrokenSignatureAction.ReSign:
                            _hashErrors.Add((component.path, result));
                            break;

                        case BrokenSignatureAction.Clear:
                            _hashErrors.Add((component.path, result));
                            // See also above: We always clear signatures in this mode.
                            break;

                        default:
                            throw new InvalidOperationException(
                                $"Unknown BrokenSignatureAction: {signatureAction}");
                    }
                }
                else
                {
                    logger.LogTrace("Component '{Path}' has a valid signature.", component.path);
                }
            }
            else
            {
                // Component was modified via JSON modification input, or --modify options.
                // Find original component by path to check its original signature state
                var originalByPath = _originalComponents.Values
                    .FirstOrDefault(c => string.Equals(c.path, component.path, StringComparison.OrdinalIgnoreCase));

                switch (signatureAction)
                {
                    case BrokenSignatureAction.Auto:
                        // For any modifications, always clear the signature.
                        _signatureOverrides.Add((component.path, null));
                        break;

                    case BrokenSignatureAction.Sign:
                        if (originalByPath.component != null)
                        {
                            var originalResult = XmlUtils.ValidateComponentHashes(originalByPath.component);
                            if (originalResult == SignatureValidation.Empty)
                            {
                                logger.LogInformation("Component '{Path}' was modified, has empty signature and will be kept as-is.",
                                    component.path);
                                break;
                            }
                        }
                        throw new InvalidOperationException(
                            $"Component '{component.path}' was modified. Use `--broken-sig re-sign` or `clear` to update the signature.");

                    case BrokenSignatureAction.Block:
                        throw new InvalidOperationException(
                            $"Component '{component.path}' was modified. Use `--broken-sig re-sign` or `clear` to update the signature.");

                    case BrokenSignatureAction.Keep:
                        // Use original component found by path to get its signature
                        if (originalByPath.component != null)
                        {
                            _hashErrors.Add((component.path, SignatureValidation.Mismatch));
                            _signatureOverrides.Add((component.path, originalByPath.component.Signature));
                            logger.LogWarning("Component '{Path}' was modified. Keeping original signature (now mismatched).",
                                component.path);
                        }
                        else
                        {
                            logger.LogTrace("Component '{Path}' not found in original components for signature validation.",
                                component.path);
                        }
                        break;

                    case BrokenSignatureAction.ReSign:
                        logger.LogTrace("Component '{Path}' not found in original components for signature validation.",
                            component.path);
                        break;
                    case BrokenSignatureAction.Clear:
                        logger.LogTrace("Component '{Path}' not found in original components for signature validation.",
                            component.path);
                        break;
                }
            }

        } //  Iteration for each component
    }

    public void Save(Stream stream, IVehicleJobDeclaration job)
    {
        XmlUtils.HashAndSerializeJobEx(stream, job, logger, _signatureOverrides!);
    }

    public IEnumerable<string> WriteReport(IVehicleDeclaration finalVehicle, string xmlPath)
    {
        var list = TypeUtils.FindComponentDelarations(finalVehicle, null).ToList();

        yield return "=== VEHICLE COMPONENTS ===";

        if (list.Count > 0)
        {
            var maxCharSize = list.Max(c => c.path.Length);

            foreach (var (component, path) in list)
            {
                var newComponent = Components.SingleOrDefault(c =>
                    string.Equals(c.PropertyPath, path, StringComparison.OrdinalIgnoreCase));

                var hash = ObjectHashUtils.MakeHash(component);
                bool modified = _originalComponents.ContainsKey(hash) == false;

                string r;

                if (newComponent != null)
                    r = newComponent.FileName;
                else if (modified)
                    r = "Modified";
                else
                    r = "* Not Changed";

                yield return $"{path.PadRight(maxCharSize)} -> {r}";
            }
        }
        else
        {
            yield return "No components added in vehicle!";
        }

        yield return string.Empty;
        yield return "=== XML VALIDATION ===";
        using (var stream = File.OpenRead(xmlPath))
        using (var xmlReader = XmlReader.Create(stream))
        {
            var validator = new XMLValidator(xmlReader);
            bool isValid = validator.ValidateXML(TUGraz.VectoCore.Utils.XmlDocumentType.DeclarationJobData);
            if (!isValid)
            {
                yield return "Vehicle XML is INVALID.";
                yield return validator.ValidationError;
            }
            else
            {
                yield return "Vehicle XML is valid.";
            }
        }

        var unusedComponents = Objects.Where(obj => obj.IsUsed == false && !obj.ArbitraryObject).ToList();
        if (unusedComponents.Count != 0)
        {
            yield return string.Empty;
            yield return "=== UNUSED COMPONENT FILES ===";
            foreach (var component in unusedComponents)
            {
                yield return $"{component.FileName}";
            }
        }

        if (_hashErrors.Count != 0)
        {
            yield return string.Empty;
            yield return "=== INPUT FILE SIGNATURE ERRORS ===";
            yield return "The following signature errors were found in the input XML files.";
            foreach (var (path, result) in _hashErrors)
            {
                yield return "- " + path + " - " + _validationResultMessages[result];
            }
        }

        yield return string.Empty;
    }

    private static bool IsComponentNameMatch(object component, string componentName, string path)
    {
        var typeName = component.GetType().Name;

        // Exceptions to naming: See link:TypeUtils.cs:SetComponentInVectoInputDeclaration

        if (path.Contains("Battery"))
        {
            if (componentName == "REESS" && typeName.StartsWith("Battery", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        if (path.Contains("Capacitor"))
        {
            if (componentName == "REESS" && typeName.StartsWith("Capacitor", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return typeName.StartsWith(componentName, StringComparison.OrdinalIgnoreCase);
    }

#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

    public static readonly Regex WellKnownRegex = new(
        Consts.WellKnownRegexString + "$", RegexOptions.IgnoreCase);

#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

    private static object? ExtractFirstComponent(object item)
    {
        foreach (var prop in item.GetType().GetProperties())
        {
            if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
            {
                var component = prop.GetValue(item);
                if (component != null)
                {
                    if (component is not IComponentDeclaration)
                        throw new InvalidOperationException($"Expected {component.GetType().Name} to implement IComponentDeclaration");

                    return component;
                }
            }
        }
        return null;
    }

    private void AddComponentInput(string filePath, object obj, bool isXML)
    {
        var filePathNode = Path.GetFileNameWithoutExtension(filePath);
        var corpusObject = new CorpusObject(filePath, obj);

        // If the filename actually contains a path, use it so we don't try to figure it out.
        if (WellKnownRegex.IsMatch(filePathNode))
        {
            corpusObject.PropertyPath = filePathNode;
        }

        _corpusObjects.Add(corpusObject);

        if (obj is IComponentDeclaration compDecl)
        {
            var hash = ObjectHashUtils.MakeHash(compDecl);
            _originalComponents[hash] = (Path.GetFileName(filePath), compDecl, isXML);
        }
    }

    private void AddArbitraryInput(string filePath, JObject obj)
    {
        var corpusObject = new CorpusObject(filePath, obj)
        {
            ArbitraryObject = true
        };
        _corpusObjects.Add(corpusObject);
    }

    private void SetVehicleInput(string filePath, IVehicleDeclaration vehicle, bool isXML)
    {
        if (Vehicle != null)
            throw new InvalidOperationException("Found more that one vehicle jobs. Only one can be added.");
        Vehicle = vehicle;
        VehiclePath = filePath;

        foreach (var (component, path) in TypeUtils.FindComponentDelarations(vehicle, null))
        {
            var compObj = component;
            var hash = ObjectHashUtils.MakeHash(compObj);

            if (!_originalComponents.ContainsKey(hash))
                _originalComponents[hash] = (path, compObj, isXML);
        }
    }

    private static readonly Dictionary<SignatureValidation, string> _validationResultMessages = new()
    {
        { SignatureValidation.Valid, "Component signature is valid" },
        { SignatureValidation.Empty, "Signature was empty" },
        { SignatureValidation.Mismatch, "Signature does not match computed hash" },
        { SignatureValidation.Corrupted, "Signature was corrupted" },
        { SignatureValidation.UnknownError, "Unknown error." }
    };
}
