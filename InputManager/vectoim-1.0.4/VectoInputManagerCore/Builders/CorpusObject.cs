using VectoObjects;

namespace VectoInputManager.Core;

/// <summary>
/// Represents a single input artifact in the corpus (file path, deserialized object, inferred
/// property path, and usage flags). Used to track mapping between files and vehicle/component
/// objects during job construction.
/// </summary>
public class CorpusObject(string filePath, object obj)
{
    public object Object { get; init; } = obj;
    public string FileName { get; private set; } = Path.GetFileName(filePath);
    public string? PropertyPath { get; set; }
    public bool? IsUsed { get; set; } = obj is IVehicleDeclaration ? null : false;
    public bool ArbitraryObject { get; set; }

    public override string ToString()
    {
        return $"{FileName} - Type: {Object.GetType().Name} - Used: {IsUsed}"
            + (PropertyPath == null ? string.Empty : $" - Property: {PropertyPath}");
    }
}