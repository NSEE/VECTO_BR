using Microsoft.Extensions.Logging;
using VectoObjects;
using VectoObjects.Helpers;
using STJ = System.Text.Json;

namespace VectoInputManager.Core;

/// <summary>
/// Handles component loading from XML/JSON files and signature enforcement logic.
/// Provides reusable operations for the component command pipeline.
/// </summary>
public class ComponentProcessor(ILogger<ComponentProcessor> logger)
{
    /// <summary>
    /// Loads a component declaration from an XML or JSON file.
    /// </summary>
    /// <param name="filePath">Path to the XML or JSON component file.</param>
    /// <returns>The deserialized component declaration.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the file format is unsupported or no component declaration is found.
    /// </exception>
    public IComponentDeclaration LoadComponent(string filePath)
    {
        bool isJson = filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
        bool isXml = filePath.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);

        if (isJson)
        {
            var obj = JsonDeclarationUtils.DeserializeFromFile(filePath);

            if (obj is IComponentDeclaration comp)
                return comp;

            throw new InvalidOperationException(
                $"Expected a component declaration but found {obj.GetType().Name}.");
        }

        if (isXml)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            if (!VectoXml.TryGetSerializer(fileStream, out var serializer, out var rootType, out var error))
                throw new InvalidOperationException(error);

            var item = serializer.Deserialize(fileStream)!;

            // Extract the first component from the VectoInputDeclaration
            foreach (var prop in item.GetType().GetProperties())
            {
                if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    var value = prop.GetValue(item);
                    if (value is IComponentDeclaration comp)
                        return comp;
                }
            }

            throw new InvalidOperationException(
                "No component declaration found in the XML file.");
        }

        throw new InvalidOperationException(
            "Unsupported file format. Only JSON and XML files are supported.");
    }

    /// <summary>
    /// Checks if a JSON file is an arbitrary modification file (not a typed component).
    /// XML files always return false (they are treated as components).
    /// </summary>
    public static bool IsArbitraryJsonFile(string filePath)
    {
        if (!filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            return false;

        var raw = File.ReadAllBytes(filePath);
        var reader = new STJ.Utf8JsonReader(raw);

        if (!reader.Read() || reader.TokenType != STJ.JsonTokenType.StartObject)
            return true;
        if (!reader.Read() || reader.TokenType != STJ.JsonTokenType.PropertyName)
            return true;

        var propertyName = reader.GetString();

        // Skip $schema if present
        if (propertyName == "$schema" && reader.Read() && reader.Read())
            propertyName = reader.GetString() ?? string.Empty;

        // Component files have "$type" or "Component" as the first significant property
        return propertyName != "$type" && propertyName != "Component";
    }

    /// <summary>
    /// Enforces the signature action for a single component.
    /// Returns (skipSigning, signatureOverride): if skipSigning is true, use signatureOverride
    /// instead of computing a fresh signature via HashAndSerialize.
    /// Throws <see cref="InvalidOperationException"/> on block conditions.
    /// </summary>
    public (bool skipSigning, SignatureType_v2_0? signatureOverride) EnforceSignatureAction(
        IComponentDeclaration originalComponent,
        bool wasModified,
        bool isXmlInput,
        BrokenSignatureAction action)
    {
        // Clear always produces an empty signature
        if (action == BrokenSignatureAction.Clear)
        {
            if (isXmlInput)
            {
                var result = XmlUtils.ValidateComponentHashes(originalComponent);
                if (result != SignatureValidation.Valid)
                    logger.LogWarning("Component has {Result} signature. Clearing.", result);
            }
            return (true, XmlUtils.CreateEmptySignature());
        }

        // Auto: always re-sign the component (default for the component tool)
        if (action == BrokenSignatureAction.Auto)
        {
            if (wasModified)
            {
                return (true, XmlUtils.CreateEmptySignature());
            }

            if (isXmlInput)
            {
                var result = XmlUtils.ValidateComponentHashes(originalComponent);
                if (result == SignatureValidation.Mismatch)
                    throw new InvalidOperationException(
                        $"Component: {GetValidationMessage(result)}");
                if (result != SignatureValidation.Valid && result != SignatureValidation.Empty)
                    logger.LogWarning("Component has {Result} signature. Re-signing.", result);
            }
            return (false, null);
        }

        // For JSON inputs, there is no existing XML signature to validate
        if (!isXmlInput)
        {
            if (wasModified && action == BrokenSignatureAction.Block)
                throw new InvalidOperationException(
                    "Component was modified. Use `--broken-sig re-sign` or `clear` to update the signature.");
            return (false, null);
        }

        // XML input: validate the original component's signature
        var validationResult = XmlUtils.ValidateComponentHashes(originalComponent);
        var originalSignature = originalComponent.Signature ?? XmlUtils.CreateEmptySignature();

        if (!wasModified)
        {
            if (validationResult == SignatureValidation.Valid)
                return (false, null);

            switch (action)
            {
                case BrokenSignatureAction.Sign:
                    if (validationResult == SignatureValidation.Empty)
                        return (false, null);
                    throw new InvalidOperationException(
                        $"Component: {GetValidationMessage(validationResult)}");

                case BrokenSignatureAction.Block:
                    throw new InvalidOperationException(
                        $"Component: {GetValidationMessage(validationResult)}");

                case BrokenSignatureAction.Keep:
                    logger.LogWarning("Component has {Result} signature. Keeping original.", validationResult);
                    return (true, originalSignature);

                case BrokenSignatureAction.ReSign:
                    logger.LogWarning("Component has {Result} signature. Re-signing.", validationResult);
                    return (false, null);
            }
        }
        else
        {
            switch (action)
            {
                case BrokenSignatureAction.Sign:
                    if (validationResult == SignatureValidation.Empty)
                        return (false, null);
                    throw new InvalidOperationException(
                        "Component was modified. Use `--broken-sig re-sign` or `clear` to update the signature.");

                case BrokenSignatureAction.Block:
                    throw new InvalidOperationException(
                        "Component was modified. Use `--broken-sig re-sign` or `clear` to update the signature.");

                case BrokenSignatureAction.Keep:
                    logger.LogWarning("Component was modified. Keeping original signature (now mismatched).");
                    return (true, originalSignature);

                case BrokenSignatureAction.ReSign:
                    logger.LogWarning("Component was modified. Re-signing.");
                    return (false, null);
            }
        }

        throw new InvalidOperationException($"Unknown BrokenSignatureAction: {action}");
    }

    private static string GetValidationMessage(SignatureValidation result) => result switch
    {
        SignatureValidation.Valid => "Component signature is valid",
        SignatureValidation.Empty => "Signature was empty",
        SignatureValidation.Mismatch => "Signature does not match computed hash",
        SignatureValidation.Corrupted => "Signature was corrupted",
        SignatureValidation.UnknownError => "Unknown error",
        _ => "Unknown validation result"
    };
}
