using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("VectoInputManagerTests")]

namespace VectoInputManager.Core;

/// <summary>
/// Application-wide constants including version information and reference data.
/// </summary>
public static class Consts
{
    /// <summary>
    /// The current version number of VectoInputManager.
    /// </summary>
    public const string Version = "1.0.4";

    /// <summary>
    /// The full application version string used in metadata and output files.
    /// </summary>
    public const string AppVersion = "vectoim-" + Version;

    /// <summary>
    /// The schema base URL, used to construct $schema references in JSON files. Include the
    /// trailing slash.
    /// </summary>
    public const string SchemaBaseUrl =
        "https://code.europa.eu/vecto/guests/vecto-sim-harry/-/raw/vectoinputmanager/VectoInputManager/";

    public const string HelpUrl = "https://vecto-sim-harry-b78fb6.pages.code.europa.eu/";

    // Examples of paths:
    // - Components.AirDrag.json
    // - Components.ElectricEnergyStorage.Battery[0].REESS
    // - Components.AxleWheels.Data.Axles[1].Tyre.json
    // - Components.FuelCellSystem[0].FuelCell.json

    public const string WellKnownRegexString =
    @"^Components\.(?:" +
    @"(?<name>Angledrive|AirDrag|Axlegear|Engine|Gearbox|Retarder|TorqueConverter)|" +
    @"ElectricMachine\.(?<name>ElectricMachineSystem)|" +
    @"ElectricMachine\.(?<name>ADC)|" +

    @"Powertrain\[(?<index>\d+)\]\.(?<name>Gearbox|TorqueConverter|Angledrive|Retarder|Axlegear|IEPC)|" +
    @"Powertrain\[(?<index>\d+)\]\.ElectricMachine\.(?<name>ElectricMachineSystem|ADC)|" +

    @"AxleWheels\.Data\.Axles\[(?<index>\d+)\]\.(?<name>Tyre)|" +
    @"FuelCellSystem\[(?<index>\d+)\]\.(?<name>FuelCell)|" +
    @"ElectricEnergyStorage\.Battery\[(?<index>\d+)\]\.(?<name>REESS)" +
    @")";

    /// <summary>
    /// Mapping of country codes to country names for VECTO simulations. These codes are used in
    /// vehicle declarations to specify the country of operation.
    /// </summary>
    public static readonly Dictionary<int, string> CountryCodes = new()
    {
        { 1, "Germany" },
        { 2, "France" },
        { 3, "Italy" },
        { 4, "Netherlands" },
        { 5, "Sweden" },
        { 6, "Belgium" },
        { 7, "Hungary" },
        { 8, "Czech Republic" },
        { 9, "Spain" },
        { 11, "United Kingdom" },
        { 12, "Austria" },
        { 13, "Luxembourg" },
        { 17, "Finland" },
        { 18, "Denmark" },
        { 19, "Romania" },
        { 20, "Poland" },
        { 21, "Portugal" },
        { 23, "Greece" },
        { 24, "Ireland" },
        { 25, "Croatia" },
        { 26, "Slovenia" },
        { 27, "Slovakia" },
        { 29, "Estonia" },
        { 32, "Latvia" },
        { 34, "Bulgaria" },
        { 36, "Lithuania" },
        { 49, "Cyprus" },
        { 50, "Malta" }
    };
}