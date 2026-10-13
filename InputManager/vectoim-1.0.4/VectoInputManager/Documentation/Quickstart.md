# Quick Start Guide

This guide demonstrates vectoim's core functionality using a sample vehicle job file (`Conventional_HeavyLorry.xml`).

## Overview

vectoim is a command-line tool for working with VECTO vehicle job files and components. It enables you to:

- **Extract** individual components from vehicle jobs for inspection and modification
- **Convert** bidirectionally between XML and JSON formats for easier editing
- **Generate** certified components using standard values
- **Build** new vehicle configurations by combining components

This guide uses a complete vehicle job file (`Conventional_HeavyLorry.xml`) to demonstrate a typical workflow: extracting the vehicle structure and components, editing them as JSON files, and reassembling them into a new vehicle XML. This approach provides maximum flexibility while maintaining data integrity through validation and automatic signature generation.

## Prerequisites

Before starting, ensure you have:

- VECTO Input Manager installed and accessible via command line
- Sample vehicle XML files (e.g., `Conventional_HeavyLorry.xml`)
- A text editor that supports JSON schema validation (see supported editors at https://www.schemastore.org)

## Available Commands

vectoim provides 3 main commands:

- **`job`** - Manipulate vehicle jobs, extract their components and/or convert to XML & JSON
- **`comp`** - Manipulate components and/or convert to XML & JSON
- **`sv`** - Create components with standard values

## Example 1: Extracting Components from a Conventional Vehicle

**Setup:** Create a working folder named `Example1` and copy the file `Conventional_HeavyLorry.xml` as an example.

The `job` command with the `-x` (extract) option allows you to decompose a vehicle job into individual component files, making it easier to work with specific parts of the vehicle configuration.

### Extracting to JSON Format

Run the following command to extract all components as JSON files:

```bash
vectoim job ./Conventional_HeavyLorry.xml -x . -X json
```

This command will:

- parse the input vehicle XML file (first argument),
- dump each component found in the vehicle as a separate file in the current directory (the `--extract|-x .` option),
- dump those components as JSON files (the `--xformat|-X json` option).

**Expected output:**

```txt
* Found input file: .\Example1\Conventional_HeavyLorry.xml
+ Vehicle 'Vehicle_Conventional_HeavyLorryDeclarationType_v2_7' added successfully.
Processing vehicle: Vehicle_Conventional_HeavyLorryDeclarationType_v2_7
> Extracting component: Components.Engine
  JSON: Components.Engine.json
> Extracting component: Components.Gearbox
  JSON: Components.Gearbox.json
> Extracting component: Components.TorqueConverter
  JSON: Components.TorqueConverter.json
> Extracting component: Components.Angledrive
  JSON: Components.Angledrive.json
> Extracting component: Components.Retarder
  JSON: Components.Retarder.json
> Extracting component: Components.Axlegear
  JSON: Components.Axlegear.json
> Extracting component: Components.AxleWheels.Data.Axles[0].Tyre
  JSON: Components.AxleWheels.Data.Axles[0].Tyre.json
> Extracting component: Components.AxleWheels.Data.Axles[1].Tyre
  JSON: Components.AxleWheels.Data.Axles[1].Tyre.json
> Extracting component: Components.AirDrag
  JSON: Components.AirDrag.json
+ Successfully extracted 9 components to .
```

### Extracting to XML Format

You can also extract components in XML format:

```bash
vectoim job ./Conventional_HeavyLorry.xml -x ./xml -X xml
```

This will create the same components but in XML format within the `./xml` directory.

### Extracting to Both Formats

To extract in both XML and JSON formats simultaneously:

```bash
vectoim job ./Conventional_HeavyLorry.xml -x ./xmlandjson -X both
```

### Understanding the Extracted Files

The extaction creates files with names that precisely indicate:

- **Component type**: What kind of component it is (Engine, Gearbox, etc.)
- **Component path**: The exact location within the vehicle structure
- **Array indices**: For components that appear multiple times (e.g., tyres, powertrains)

**File naming examples:**

- `Components.Engine.json` - The main engine component
- `Components.AxleWheels.Data.Axles[0].Tyre.json` - First axle's tyre
- `Components.AxleWheels.Data.Axles[1].Tyre.json` - Second axle's tyre

## Example 2: Working with Multiple Powertrains Vehicles

**Setup:** Create a folder named `Example2` and copy the file `Multiple_FCHV_F2_IEPC_HeavyLorry.xml`.

Multiple powertrains vehicles (like hybrid or electric vehicles) have more complex component structures. Let's extract components from this more advanced example:

```bash
vectoim job ./Multiple_FCHV_F2_IEPC_HeavyLorry.xml -x . -X json
```

This vehicle contains two separate powertrains, resulting in components organized as:

**Vehicle components:**

- `Components.AirDrag.json`
- `Components.AxleWheels.Data.Axles[0].Tyre.json`
- `Components.AxleWheels.Data.Axles[1].Tyre.json`
- `Components.ElectricEnergyStorage.Battery[0].REESS.json`
- `Components.FuelCellSystem[0].FuelCell.json`

**Powertrain-specific components:**

- `Components.Powertrain[0].Angledrive.json`
- `Components.Powertrain[0].Axlegear.json`
- `Components.Powertrain[0].ElectricMachine.ADC.json`
- `Components.Powertrain[0].ElectricMachine.ElectricMachineSystem.json`
- `Components.Powertrain[0].Gearbox.json`
- `Components.Powertrain[0].Retarder.json`
- `Components.Powertrain[0].TorqueConverter.json`
- `Components.Powertrain[1].Axlegear.json`
- `Components.Powertrain[1].IEPC.json`
- `Components.Powertrain[1].Retarder.json`

Notice how components are indexed by powertrain (`[0]` and `[1]`), allowing you to work with each powertrain independently.

## Converting Vehicle Jobs to JSON Format

Working with JSON format offers several advantages:

- **Human-readable**: Easier to read and understand than XML
- **Editor support**: Better syntax highlighting and validation in modern editors
- **Flexible editing**: Simpler to modify values and structure
- **Schema validation**: Support for JSON Schema validation in many editors

To convert an entire vehicle job to JSON format:

```bash
vectoim job ./Conventional_HeavyLorry.xml -o ./Conventional_HeavyLorry.json
```

You can also create a JSON file containing the vehicle structure with all component placeholders set to `null`. This serves as the base vehicle definition.

```bash
vectoim job --drop-components .\Conventional_HeavyLorry.xml -o .\Conventional_HeavyLorry.json
```

**Expected output:**

This creates the file `Conventional_HeavyLorry.json`

```txt
Processing .\Example1\Conventional_HeavyLorry.xml
Removed component at Components.Engine
Removed component at Components.Gearbox
Removed component at Components.TorqueConverter
Removed component at Components.Angledrive
Removed component at Components.Retarder
Removed component at Components.Axlegear
Removed component at Components.AxleWheels.Data.Axles[0].Tyre
Removed component at Components.AxleWheels.Data.Axles[1].Tyre
Removed component at Components.AirDrag
> Converted vehicle: .\Conventional_HeavyLorry.json
+ Successfully converted 1 out of 1 files.
```

💡 For the JSON tool, multiple files with glob patterns are also supported, for example:

```bash
vectoim job vehicle.xml Components.*.xml -o vehicle.json
```

## Working with Standard Values

The Standard Values tool creates certified components with calculated standard values based on minimum input parameters.

### Listing Available Components with Standard Values

To see all available component types standard values:

```bash
vectoim sv -l
```

**Output:**

```txt
Valid components are:
┌────────────┬─────────────────────────────────┬───────┬───────────────────────┐
│ Code       │ Description                     │ Cert. │ Declaration           │
│            │                                 │ Code  │ Type                  │
├────────────┼─────────────────────────────────┼───────┼───────────────────────┤
│ GearBox    │ Transmission                    │   G   │ Transmission v2.3     │
│ TC         │ Torque Converter                │   C   │ Torque Converter v2.0 │
│ Retarder   │ Retarder                        │   O   │ Retarder v2.0         │
│ ADC        │ Additional Drivetrain Component │   D   │ ADC v2.3              │
│ Angledrive │ Angle Drive                     │   D   │ Angle Drive v2.0      │
│ Axle       │ Axle                            │   L   │ Axle v2.0             │
│ EMS        │ Electric Motor System           │   M   │ Standard Values v2.3  │
│ IEPC       │ Integrated Electric Powertrain  │   I   │ Standard Values v2.6  │
│ Battery    │ Battery System                  │   B   │ Standard Values v2.6  │
│ Capacitor  │ Capacitor System                │   A   │ Capacitor v2.6        │
│ FuelCell   │ Fuel Cell                       │   F   │ Fuel Cell v2.6        │
└────────────┴─────────────────────────────────┴───────┴───────────────────────┘
```

### Creating a Standard Value Template

Let's create a standard axle gear component. First, generate a template:

```bash
vectoim sv -t axle -e -o axlesv.json
```

This command will:

1. Display a complete JSON template for the axle component
2. Save the template to `axlesv.json` (because `-o` was provided)
3. Include all required fields with example values
4. Add JSON schema reference for validation

> Note: the component type is passed with `-t|--type`. Without `-o`, the template is only printed to the console.

The saved `axlesv.json` includes:

- Component type and basic parameters (ratio, line type)
- Speed and torque curves with example values
- JSON schema reference for editor validation

### Editing the Standard Value Input

Open `axlesv.json` in your preferred editor. If your editor supports JSON Schema validation (recommended), you'll also get immediate feedback on invalid values, auto-completion, suggestions for valid field names and types, and type checking.

Edit the values according to your requirements.

### Generating the Component

Create the final component from your input file:

**XML output:**

```bash
vectoim sv .\axlesv.json -o axle.xml
```

**JSON output:**

```bash
vectoim sv .\axlesv.json -o axle.json
```

**Expected output:**

```txt
──  Standard Values Tool  ──────────────────────────────────────────────────────────────────────────────────────────────

> Output written to: axle.xml
+ XML output validated successfully.
```

The tool will:

- Parse your JSON input
- Validate all values against component requirements
- Generate a properly formatted and signed XML component
- Validate the output against VECTO schemas

## Building Modified Vehicle Jobs

Now let's demonstrate how to edit or replace components in an existing vehicle job.

- **Step 1** Create a JSON file containing the vehicle structure with all component placeholders set to `null`. This serves as the base vehicle definition.

```bash
vectoim job Conventional_HeavyLorry.xml -o Conventional_HeavyLorry.json --drop-components
```

This creates the file `Conventional_HeavyLorry.json`

- **Step 2** Extract each component from the vehicle and save them as separate JSON files.

```bash
vectoim job Conventional_HeavyLorry.xml -x ./ -X json
```

This creates the files: `Components.Engine.json`, `Components.Gearbox.json`,  `Components.TorqueConverter.json`, `Components.Angledrive.json`, `Components.Retarder.json`, `Components.Axlegear.json`, `Components.AxleWheels.Data.Axles[0].Tyre.json`, `Components.AxleWheels.Data.Axles[1].Tyre.json`, `Components.AirDrag.json`.

- **Step 3** You can modify the vehicle JSON file and/or any of the component JSON files as needed. It's recommended to use a modern editor that supports JSON schema validation (see supported editors at https://www.schemastore.org), to enable auto-completion and error checking.

- **Step 4** Combine the base vehicle JSON with all component JSON files (including the modified ones) to create a new vehicle XML file. The system automatically validates all inputs and matches components to their correct positions in the vehicle structure.

```bash
vectoim job *.json -o modified_vehicle.xml
```

Final output is `modified_vehicle.xml` along with `modified_vehicle.report.txt` containing details of the operation. Note that the Vehicle Job XML file will also have updated component signatures.

```txt
=== VEHICLE COMPONENTS ===
Components.Engine                        -> Components.Engine.json
Components.Gearbox                       -> Components.Gearbox.json
Components.TorqueConverter               -> Components.TorqueConverter.json
Components.Angledrive                    -> Components.Angledrive.json
Components.Retarder                      -> Components.Retarder.json
Components.Axlegear                      -> Components.Axlegear.json
Components.AxleWheels.Data.Axles[0].Tyre -> Components.AxleWheels.Data.Axles[0].Tyre.json
Components.AxleWheels.Data.Axles[1].Tyre -> Components.AxleWheels.Data.Axles[1].Tyre.json
Components.AirDrag                       -> Components.AirDrag.json

=== XML VALIDATION ===
Vehicle XML is valid.
```

### Supported Input Types

- **XML Vehicle Jobs**: Complete vehicle definitions
- **XML Components**: Standard XML component files
- **JSON Components**: Components in JSON format (automatically converted)
- **Standard Value Inputs**: JSON input files for standard values (automatically processed through the standard values tool)
- **Glob Patterns**: Use wildcards to include multiple files at once

### Glob Pattern Examples

```bash
# Include all XML components in a directory
vectoim job vehicle.xml Components.*.xml -o output.xml

# Include specific component types
vectoim job vehicle.xml Components.Powertrain[0].*.xml -o output.xml

# Mix different file types
vectoim job vehicle.xml *.xml *.json -o output.xml
```

---

For more detailed information about specific commands, use the help option:

```bash
vectoim --help
vectoim <command> --help
```
