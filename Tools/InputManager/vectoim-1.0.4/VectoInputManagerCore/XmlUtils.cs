using Fasterflect;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using TUGraz.VectoHashing;
using VectoObjects;
using VectoObjects.Helpers;

namespace VectoInputManager.Core;

/// <summary>
/// Provides utilities for XML serialization, hashing, and validation of VECTO vehicle declarations
/// and components. Handles signature generation using VECTO hashing algorithms.
/// </summary>
public static class XmlUtils
{
    /// <summary>
    /// Hashes and serializes a complete vehicle job declaration to XML format. Computes
    /// cryptographic signatures for each component before serialization.
    /// </summary>
    /// <param name="output">The output stream to write the serialized XML to.</param>
    /// <param name="vectoInputDeclaration">The vehicle job declaration to serialize.</param>
    public static void HashAndSerializeJob(Stream output, IVehicleJobDeclaration vectoInputDeclaration)
    {
        HashAndSerializeJobEx(output, vectoInputDeclaration, null, []);
    }

    public static void HashAndSerializeJobEx(Stream output, IVehicleJobDeclaration vectoInputDeclaration,
        ILogger? logger, IEnumerable<(string path, ISignatureType sig)> sigOverrides)
    {
        // First pass: Compute signatures for each component
        // Note: v1_0 components are not supported

        foreach (var (component, path) in TypeUtils.FindComponentDelarations(
            vectoInputDeclaration.Vehicle, null))
        {
            var data = component.GetPropertyValue("Data");

            // Hashing requires an id for the component.
            component.EnsureIdSet();

            var sigOverride = sigOverrides.FirstOrDefault(s => s.path == path);

            bool skipSigning = component.IsScaffoldingComponent() || data == null
                || (sigOverride.path != null && sigOverride.sig == null);

            // Skip signing for scaffolding components or if override is explicitly null
            if (skipSigning)
            {
                component.Signature = CreateEmptySignature();
            }

            // Otherwise, compute or set the signature
            else
            {
                // Create a temporary VectoInputDeclaration with only this component
                var tempVectoDeclaration = (IVectoInputDeclaration)
                    Activator.CreateInstance(vectoInputDeclaration.VectoInputDeclarationType)!;

                // Set this component in the temporary declaration
                tempVectoDeclaration.SetComponentInVectoInputDeclaration(component);

                // Use override signature if provided
                if (sigOverride.path != null && sigOverride.sig != null)
                {
                    component.Signature = (SignatureType_v2_0)sigOverride.sig;
                    logger?.LogInformation("Using signature override for component at path: {path}", path);
                }
                // Otherwise, compute the signature
                else
                {
                    var serializer = new XmlSerializer(vectoInputDeclaration.VectoInputDeclarationType);

                    using var stream = new MemoryStream();

                    serializer.SerializeFixed(stream, tempVectoDeclaration);
                    stream.Seek(0, SeekOrigin.Begin);

                    // Use the VectoHash class to compute the hash
                    var h = VectoHash.Load(stream);
                    var hashXElement = h.ComputeXmlHash(
                        VectoHash.DefaultCanonicalizationMethod, VectoHash.DefaultDigestMethod);

                    // Serialize XElement to string
                    string xml = hashXElement.ToString(SaveOptions.DisableFormatting);

                    // Deserialize the string back into a ReferenceType object
                    var refTypeSerializer = new XmlSerializer(typeof(ReferenceType));
                    using var stringReader = new StringReader(xml);
                    var refType = (ReferenceType)refTypeSerializer.Deserialize(stringReader)!;

                    // Create a SignatureType_v2_0 object and set the Reference property
                    var signature = new SignatureType_v2_0()
                    {
                        Reference = refType
                    };

                    // Set signature for this component
                    component.Signature = signature;
                }
            }
        }

        VectoXml.SerializeJob(output, vectoInputDeclaration);
    }

    public static SignatureType_v2_0 CreateEmptySignature()
    {
        var s = new SignatureType_v2_0()
        {
            Reference = new ReferenceType()
            {
                DigestMethod = new DigestMethodType()
                {
                    Algorithm = VectoHash.DefaultDigestMethod
                },
                DigestValue = []
            }
        };

        foreach (var c in VectoHash.DefaultCanonicalizationMethod)
        {
            s.Reference.Transforms.Add(new TransformType()
            {
                Algorithm = c
            });
        }

        return s;
    }

    /// <summary>
    /// Hashes and serializes a single component declaration to XML format. Optionally embeds input
    /// data as an XML comment.
    /// </summary>
    /// <param name="output">The output stream to write the serialized XML to.</param>
    /// <param name="vectoInputDeclaration">The component declaration container.</param>
    /// <param name="input">Optional input data to embed as a comment.</param>
    public static void HashAndSerialize(Stream output,
        VectoInputDeclaration_v2_2 vectoInputDeclaration, BaseInput? input)
    {
        var serializer = new XmlSerializer(typeof(VectoInputDeclaration_v2_2));

        // First pass: Serialize the object and compute the signature.
        using (var stream = new MemoryStream())
        {
            serializer.SerializeFixed(stream, vectoInputDeclaration);
            stream.Seek(0, SeekOrigin.Begin);

            // Use the VectoHash class to compute the hash
            // link: VectoHash.cs:GetComponentQueryString()

            var h = VectoHash.Load(stream);
            var hashXElement = h.ComputeXmlHash(
                VectoHash.DefaultCanonicalizationMethod, VectoHash.DefaultDigestMethod);

            // Serialize XElement to string
            string xml = hashXElement.ToString(SaveOptions.DisableFormatting);

            // Deserialize the string back into a ReferenceType object
            var refTypeSerializer = new XmlSerializer(typeof(ReferenceType));
            using var stringReader = new StringReader(xml);
            var refType = (ReferenceType)refTypeSerializer.Deserialize(stringReader)!;

            // Create a SignatureType_v2_0 object and set the Reference property
            var signature = new SignatureType_v2_0()
            {
                Reference = refType
            };

            var inputDeclarationType = typeof(VectoInputDeclaration_v2_2);

            // Find the first non-null property that has a Signature property.
            // This is what the user is interested in.
            bool found = false;
            foreach (var prop in inputDeclarationType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propType = prop.PropertyType;
                var value = prop.GetValue(vectoInputDeclaration);
                if (value != null)
                {
                    if (value.TrySetPropertyValue("Signature", signature))
                    {
                        if (found)
                            throw new InvalidOperationException("Multiple components with Signature property found in VectoInputDeclaration.");

                        found = true;
                    }
                }
            }

            if (!found)
                throw new InvalidOperationException("No component with Signature property found in VectoInputDeclaration.");

            // Now vectoInputDeclaration includes the signature.
        }

        // Second pass: Serialize the object to the final output stream.

        using (var stream = new MemoryStream())
        {
            serializer.SerializeFixed(stream, vectoInputDeclaration);
            stream.Seek(0, SeekOrigin.Begin);

            // If no input object is provided, just copy the stream to output.
            if (input == null)
            {
                stream.CopyTo(output);
            }

            // If an input object is provided, serialize it to JSON and append it as a comment.
            else
            {
                var xdoc = XDocument.Load(stream);
                var settings = new JsonSerializerSettings
                {
                    Formatting = Newtonsoft.Json.Formatting.Indented
                };
                settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                var json = JsonInputUtils.Serialize(input, false);
                var src = " Input data:\n" + json + "\n";
                xdoc.Root!.AddAfterSelf(new XComment(src));
                xdoc.Save(output);
            }
        }
    }

    /// <summary>
    /// Validates the cryptographic hashes of all components in a vehicle job declaration.
    /// </summary>
    /// <param name="job">The vehicle job declaration to validate.</param>
    /// <returns>A list of component paths with their validation results.</returns>
    public static IList<(string path, SignatureValidation result)> ValidateVehicleJobHashes(IVehicleJobDeclaration job)
    {
        var results = new List<(string, SignatureValidation)>();

        foreach (var (component, path) in TypeUtils.FindComponentDelarations(job.Vehicle, null))
        {
            var result = ValidateComponentHashes(component);
            results.Add((path, result));
        }
        return results;
    }

    /// <summary>
    /// Validates the cryptographic hash of the component.
    /// </summary>
    /// <param name="component">The component to validate.</param>
    /// <returns>
    /// A ValidationResult indicating whether the hash is valid or the type of error encountered.
    /// </returns>
    public static SignatureValidation ValidateComponentHashes(IComponentDeclaration component)
    {
        var serializer = new XmlSerializer(typeof(VectoInputDeclaration_v2_2));
        try
        {
            var signature = component.GetSignature();
            if (signature?.Reference == null || signature.Reference.DigestMethod == null
               || signature.Reference.DigestValue == null
               || signature.Reference.DigestValue.Length == 0
               || string.IsNullOrEmpty(signature.Reference.DigestMethod.Algorithm))
            {
                return SignatureValidation.Empty;
            }

            // Set this component in the temporary declaration
            var tempVectoDeclaration = TypeUtils.CreateVectoInputDeclaration();

            // Create a temporary VectoInputDeclaration with only this component
            tempVectoDeclaration.SetComponentInVectoInputDeclaration(component);

            // Serialize the component and compute its hash
            using var stream = new MemoryStream();
            serializer.SerializeFixed(stream, tempVectoDeclaration);
            stream.Seek(0, SeekOrigin.Begin);

            // Validate the hash using VectoHash ValidateHash method
            var h = VectoHash.Load(stream);
            var isValid = h.ValidateHash();

            if (!isValid)
            {
                return SignatureValidation.Mismatch;
            }
        }
        catch (NullReferenceException)
        {
            // Hash validation requires an id for the component, or
            // or the `id` in the `Data` is different than the URI referenced in the signature. 
            return SignatureValidation.Corrupted;
        }
        catch
        {
            // Unknown error occurred during validation.
            return SignatureValidation.UnknownError;
        }

        return SignatureValidation.Valid;
    }

}

