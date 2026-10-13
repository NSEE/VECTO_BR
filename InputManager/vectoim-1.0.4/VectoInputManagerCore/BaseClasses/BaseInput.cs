using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VectoObjects;

namespace VectoInputManager.Core;

/// <summary>
/// Base class for component input data. Contains common properties like manufacturer information
/// and certification details that are required for all component types.
/// </summary>
public abstract class BaseInput
{
    /// <summary>
    /// Gets the component type this input data is for.
    /// </summary>
    public abstract ComponentType Component { get; }

    /// <summary>
    /// Sets example/placeholder values for template purposes.
    /// </summary>
    public virtual void SetExampleValues()
    {
        Manufacturer = "Example Manufacturer";
        Model = "Example Model";
        CertificationNumber = "EX1234567890";
        Date = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the certification number letter code for this component type (e.g., "G" for Gearbox).
    /// </summary>
    [JsonIgnore]
    public string CertificationNumberLetter => Components.ComponentInformation[Component].Code;

    /// <summary>
    /// Gets or sets the manufacturer name.
    /// </summary>
    [MinLength(1)]
    [Required(AllowEmptyStrings = false)]
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the component model designation.
    /// </summary>
    [MinLength(1)]
    [Required(AllowEmptyStrings = false)]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the certification number assigned to this component.
    /// </summary>
    public string CertificationNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date.
    /// </summary>
    [JsonIgnore]
    public DateTime Date { get; internal set; } = DateTime.UtcNow;

    /// <summary>
    /// Initializes common data properties on a component data declaration.
    /// </summary>
    /// <typeparam name="T">The component data declaration type.</typeparam>
    /// <param name="target">The target declaration to initialize.</param>
    public void InitDataDeclaration<T>(T target) where T : IComponentDataDeclaration
    {
        target.Manufacturer = Manufacturer;
        target.Model = Model;
        target.CertificationNumber = CertificationNumber;
        target.Date = Date;
        target.AppVersion = Consts.AppVersion + " *** component not for certification use";
        target.id = $"{Component}-{GlobalCounter++}".ToLower();
    }

    /// <summary>
    /// Global counter for generating unique component IDs.
    /// </summary>
    static int GlobalCounter = 1;
}


