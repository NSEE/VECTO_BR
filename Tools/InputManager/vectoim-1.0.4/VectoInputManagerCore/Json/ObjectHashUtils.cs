using System;
using Newtonsoft.Json;
using VectoObjects;

namespace VectoInputManager.Core;

/// <summary>
/// Utility class for generating cryptographic hashes of component objects. Provides a deterministic
/// way to fingerprint object state via JSON serialization + SHA-256.
/// </summary>
public static partial class ObjectHashUtils
{
    static readonly JsonSerializerSettings Settings;

    static ObjectHashUtils()
    {
        Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            TypeNameHandling = TypeNameHandling.All,
            ContractResolver = new IgnoreStringPropertiesResolver(),
        };

        Settings.Converters.Add(new FixedDecimalConverter(6));
    }

    /// <summary>
    /// Generates a SHA-256 hash of the provided component object.
    /// </summary>
    /// <param name="component">The object to hash (must implement IComponentDeclaration)</param>
    /// <returns>Lowercase hexadecimal string representation of the SHA-256 hash</returns>
    public static string MakeHash(IComponentDeclaration component)
    {
        var json = JsonConvert.SerializeObject(component, Settings);

        // Compute SHA256 hash
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var hashBytes = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToHexStringLower(hashBytes);
    }
}
