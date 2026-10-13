using System;

namespace VectoInputManager.Core;

/// <summary>
/// Defines a factory interface for creating standard values component calculators based on
/// component types. This factory provides a centralized mechanism for instantiating the appropriate
/// standard values calculator implementation for different vehicle component types.
/// </summary>
public interface IStandardValuesFactory
{
    /// <summary>
    /// Creates a standard values calculator for the specified component type.
    /// </summary>
    /// <param name="componentType">The type of component to create a calculator for.</param>
    /// <returns>A BaseStandardValues implementation specific to the component type.</returns>
    BaseStandardValues Create(ComponentType componentType);
}

/// <summary>
/// Represents the result of component signature validation.
/// </summary>
public enum SignatureValidation
{
    /// <summary>
    /// Component signature is valid.
    /// </summary>
    Valid,

    /// <summary>
    /// Signature does not match computed hash.
    /// </summary>
    Mismatch,

    /// <summary>
    /// Signature was empty or missing.
    /// </summary>
    Empty,

    /// <summary>
    /// Signature was corrupted (e.g., missing or mismatched id).
    /// </summary>
    Corrupted,

    /// <summary>
    /// Unknown error occurred during validation.
    /// </summary>
    UnknownError
}

/// <summary>
/// Defines actions to take when a component signature verification fails during processing.
/// </summary>
public enum BrokenSignatureAction
{
    /// <summary>
    /// Automatically handle signatures: keep empty signatures as-is, block on 
    /// mismatched signatures (default behavior).
    /// </summary>
    Auto,

    /// <summary>
    /// Treat invalid signature as an error and block output generation.
    /// </summary>
    Block,

    /// <summary>
    /// Treat invalid signature as a warning and keep the old (invalid) signature.
    /// </summary>
    Keep,

    /// <summary>
    /// Sign components that are missing a signature, block if signature is invalid.
    /// </summary>
    Sign,

    /// <summary>
    /// Treat invalid signature as a warning and recompute/re-sign the component.
    /// </summary>
    ReSign,

    /// <summary>
    /// Treat invalid signature as a warning and remove the broken signature completely.
    /// </summary>
    Clear
}