using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Globalization;
using System.Reflection;
using VectoObjects;

namespace VectoInputManager.Core;

/// <summary>
/// Provides access to embedded CSV data files containing standard values and reference data for
/// VECTO component calculations.
/// </summary>
public static class Data
{
    // Embedded resource namespace + folder with CSV files
    const string Namespace = "VectoInputManager.Core.Data";

    /// <summary>
    /// Retrieves battery current limit data indexed by battery type and state of charge. Used for
    /// determining maximum charging and discharging rates.
    /// </summary>
    /// <returns>A list of current limit specifications.</returns>
    public static IList<CurrentLimitItem> GetCurrentLimits()
    {
        return GetRecordsFromEmbeddedCsv<CurrentLimitItem>(Namespace + ".CurrentLimits.csv")
    .ToList();
    }

    /// <summary>
    /// Retrieves battery Direct Current Internal Resistance (DCIR) data. Used for calculating
    /// battery power losses and heat generation.
    /// </summary>
    /// <returns>A list of DCIR specifications by battery type.</returns>
    public static IList<DCIRItem> GetBatteryResistance()
    {
        return GetRecordsFromEmbeddedCsv<DCIRItem>(Namespace + ".DCIR.csv")
 .ToList();
    }

    /// <summary>
    /// Retrieves Open Circuit Voltage (OCV) data as a function of State of Charge. Used for battery
    /// voltage modeling.
    /// </summary>
    /// <returns>A list of OCV factors indexed by SOC percentage.</returns>
    public static IList<OCVItem> GetOCV()
    {
        return GetRecordsFromEmbeddedCsv<OCVItem>(Namespace + ".OCV.csv")
       .ToList();
    }

    /// <summary>
    /// Retrieves fuel cell efficiency data as a function of normalized power output. Used for fuel
    /// cell performance modeling.
    /// </summary>
    /// <returns>A list of fuel cell efficiency values.</returns>
    public static IList<FuelCellEfficiencyItem> GetFuelCellEfficiency()
    {
        return GetRecordsFromEmbeddedCsv<FuelCellEfficiencyItem>(Namespace + ".FuelCellEfficiency.csv")
    .ToList();
    }

    /// <summary>
    /// Retrieves all available vehicle types with their codes, versions, and descriptions.
    /// </summary>
    /// <returns>A list of vehicle type definitions, sorted by description and version.</returns>
    public static IList<VehicleType> GetVehicleTypes()
    {
        return GetRecordsFromEmbeddedCsv<VehicleType>(Namespace + ".VehicleTypes.csv")
            .OrderByDescending(q=>q.Active).ThenBy(q => q.Description).ThenBy(q => q.Version)
            .ToList();
    }

    /// <summary>
    /// Retrieves standard electric machine (motor) efficiency data. Provides normalized torque and
    /// speed values with corresponding efficiency factors.
    /// </summary>
    /// <returns>A list of electric machine standard efficiency values.</returns>
    public static IList<EMStandardItem> GetEMStandard()
    {
        return GetRecordsFromEmbeddedCsv<EMStandardItem>(Namespace + ".EM_standard.csv")
          .ToList();
    }

    /// <summary>
    /// Loads records from an embedded CSV resource file.
    /// </summary>
    /// <typeparam name="T">The type to deserialize CSV records into.</typeparam>
    /// <param name="resourceName">The fully qualified resource name.</param>
    /// <returns>A list of deserialized records.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the resource is not found.</exception>
    private static IList<T> GetRecordsFromEmbeddedCsv<T>(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourceName)
    ?? throw new InvalidOperationException($"Resource '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return csv.GetRecords<T>().ToList();
    }

    /// <summary>
    /// Represents a vehicle type definition with its code, version, and display information.
    /// </summary>
    public class VehicleType
    {
        /// <summary>Short alphanumeric code identifying the vehicle type.</summary>
        public required string Code { get; set; }

        /// <summary>The CLR type name for this vehicle type.</summary>
        public required string TypeName { get; set; }

        /// <summary>The schema version (e.g., "v2.7").</summary>
        public required string Version { get; set; }

        /// <summary>Human-readable description of the vehicle type.</summary>
        public required string Description { get; set; }

        /// <summary>Indicates if this is the currently active/recommended version.</summary>
        public bool Active => Version == "v2.7";

        /// <summary>
        /// Resolves the TypeName to an actual Type object.
        /// </summary>
        /// <returns>The resolved Type.</returns>
        public Type FindType()
        {
            return TypeUtils.GetTypeByName(TypeName);
        }
    }

    /// <summary>
    /// Represents fuel cell electrical efficiency at a specific normalized power output.
    /// </summary>
    public class FuelCellEfficiencyItem
    {
        /// <summary>
        /// Normalized Power Output (0.0 to 1.0).
        /// </summary>
        public decimal Pnorm { get; set; }

        /// <summary>
        /// Fuel Cell Electrical Efficiency as a percentage.
        /// </summary>
        public decimal Eta { get; set; }
    }

    /// <summary>
    /// Represents Open Circuit Voltage factor at a specific State of Charge.
    /// </summary>
    public class OCVItem
    {
        /// <summary>State of Charge as a percentage (0-100).</summary>
        [Name("SoC [%]")]
        public int SOC_Percent { get; set; }

        /// <summary>OCV multiplication factor.</summary>
        public decimal Factor { get; set; }
    }

    /// <summary>
    /// Represents Direct Current Internal Resistance data for a battery type.
    /// </summary>
    public class DCIRItem
    {
        /// <summary>The battery chemistry type.</summary>
        [Name("Battery Type")]
        public BatteryTypeType_v2_3 BatteryType { get; set; }

        /// <summary>DCIR category/identifier.</summary>
        [Name("DCIR")]
        public required string DCIR { get; set; }

        /// <summary>Specific resistance value in Ohm·Ah.</summary>
        [Name("Specific resistance")]
        public decimal SpecificResistance { get; set; }
    }

    /// <summary>
    /// Represents current limits for charging and discharging at a specific State of Charge.
    /// </summary>
    public class CurrentLimitItem
    {
        /// <summary>The battery chemistry type.</summary>
        [Name("Battery Type")]
        public BatteryTypeType_v2_3 BatteryType { get; set; }

        /// <summary>State of Charge as a percentage (0-100).</summary>
        [Name("SOC [%]")]
        public int SOC_Percent { get; set; }

        /// <summary>Maximum charging current as a C-rate multiplier (e.g., 0.5C, 1C, 2C).</summary>
        [Name("C-rate (nC) for maximum charging current")]
        public decimal MaxChargingCRate { get; set; }

        /// <summary>Maximum discharging current as a C-rate multiplier (e.g., 0.5C, 1C, 2C).</summary>
        [Name("C-rate (nC) for maximum discharging current")]
        public decimal MaxDischargingCRate { get; set; }
    }

    /// <summary>
    /// Represents standard electric machine efficiency at normalized speed and torque.
    /// </summary>
    public class EMStandardItem
    {
        /// <summary>Normalized rotational speed (0.0 to 1.0).</summary>
        [Name("n_norm [-]")]
        public decimal NNorm { get; set; }

        /// <summary>Normalized torque (0.0 to 1.0).</summary>
        [Name("T_norm [-]")]
        public decimal TNorm { get; set; }

        /// <summary>Efficiency factor (0.0 to 1.0).</summary>
        [Name("Eta [-]")]
        public decimal Eta { get; set; }
    }
}