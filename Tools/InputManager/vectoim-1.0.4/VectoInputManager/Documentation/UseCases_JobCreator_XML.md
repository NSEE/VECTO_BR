# Use Cases - Job Creator Tool (XML Workflow)

This document covers use cases for the Job Creator Tool (`job` command) when working primarily with XML files.

> 💡 **Download the ready-to-run files:** [UseCases.zip](https://code.europa.eu/vecto/guests/vecto-sim-harry/-/raw/vectoinputmanager/VectoInputManager/UseCases/UseCases.zip). Each use case has its own folder pre-populated with the input files it needs. Extract the archive and either run the included `Run-Demos.ps1` script or `cd` into a use-case folder and run its commands manually.

See also:

- [Use Cases - Job Creator Tool (JSON Workflow)](UseCases_JobCreator_JSON.md) for JSON-based workflows
- [Understanding JSON File Types](AboutJson.md) for clarification on different JSON file types used in the VECTO ecosystem

## Use Case 07: Interactive Vehicle Information Entry with Components

The user has XML component files and wants to create a new vehicle by selecting a vehicle type interactively and incorporating these existing component files. The tool validates that the components match the selected vehicle type, then guides the user through entering the remaining vehicle properties. The result is a complete VECTO job XML file with all components integrated.

**Command example:**

Assume the user would like to create a Heavy Lorry vehicle and has prepared several XML component files in the current folder:

- AirDrag.xml
- Angledrive.xml
- Axlegear.xml
- Tyre1.xml
- Tyre2.xml
- Engine.xml
- Gearbox.xml
- Retarder.xml
- TorqueConverter.xml

The following command will start the interactive vehicle creation process, incorporating the existing components:

```bash
vectoim job *.xml -i vehicle -o output.xml
```

During the interactive session, the user is prompted to select the vehicle type from a list:

```txt
Vehicle Type:

  Conventional Vehicle - Completed Bus (v2.7)
> Conventional Vehicle - Heavy Lorry (v2.7)
  Conventional Vehicle - Medium Lorry (v2.7)
  Conventional Vehicle - Primary Bus (v2.7)
  Exempted Vehicle - Completed Bus (v2.7)
  Exempted Vehicle - Heavy Lorry (v2.7)
  Exempted Vehicle - Medium Lorry (v2.7)
  Exempted Vehicle - Primary Bus (v2.7)
  Fuel Cell Hybrid Vehicle - Completed Bus (v2.7)
  Fuel Cell Hybrid Vehicle - Fuel Cell Variant - Heavy Lorry (v2.7)

(Move up and down to reveal more choices)
```

The user is then prompted for each required vehicle property. Depending on the required data type, the tool shows valid ranges or enumerated options. Example prompts:

```txt
Vehicle Type: Conventional Vehicle - Heavy Lorry (v2.7)
Manufacturer: Test Company
Manufacturer address: Athens
Model: Test Model 1
VIN: 123123123
Date (DateTime): 1/1/2026
Simulation tool license number: 1
Legislative category: N 3
Chassis configuration:

  Tractor
> Rigid Lorry
...
```

**Important:** The `-i vehicle` flag disables interactive prompts for component data. However, for components that can appear multiple times (e.g., tyres for each axle, multiple powertrains), the tool will still prompt for the properties of the structure that holds these components (see the [Interactive Mode Documentation](InteractiveMode.md)).

For example, when creating a vehicle with multiple axles, you'll need to specify the axle configuration. The tool will then automatically assign the tyre components from the input files to each axle in file name order. Example:

```txt
Components: Components_Conventional_LorryType_v2_7
> Axle wheels: AxleWheelsComponentDeclarationType_v2_0
---> Data: AxleWheelsDataDeclarationType_v2_7
------> Add item to Axles: AxleDataDeclarationType_v2_7
---------> Axle type: Vehicle driven
---------> Twin tyres: Yes
---------> Steered: No
---------> Wheel end: WheelEndType_v2_7
------------> Wheel end friction (decimal): 1
------------> Certification number: 1
---------> Axle number (int): 1
------> Add another item to Axles: AxleDataDeclarationType_v2_7
---------> Axle type: Vehicle non driven
---------> Twin tyres: Yes
---------> Steered: Yes
---------> Wheel end: No
---------> Axle number (int): 2
------> Add another item to Axles: No

```

In the above input, we defined the two axles of the vehicle. The tool will then assign the tyre components from the input files to each axle in file name order.

After completing the interactive session, the tool generates the vehicle job XML file (`output.xml`).

## Use Case 08: Replace Components in Existing Vehicle Job

The user would like to replace the Axlegear and its two tyres. The following files exist in the current folder: `Tyre1.xml`, `Tyre2.xml`, `Axlegear.xml` and `Conventional_HeavyLorry.xml`. The tool matches the vehicle type and components, and if there is an invalid input, it prints appropriate error messages and terminates. The result is a valid VECTO job XML where the input components have replaced the respective existing ones.

**Command example:**

```bash
vectoim job *.xml -o modifiedvehicle.xml
```

After the command run, the `modifiedvehicle.report.txt` will display details of the execution. Example output (for a Conventional vehicle):

```txt
=== VEHICLE COMPONENTS ===
Components.Engine                        -> * Not Changed
Components.Gearbox                       -> * Not Changed
Components.TorqueConverter               -> * Not Changed
Components.Angledrive                    -> * Not Changed
Components.Retarder                      -> * Not Changed
Components.Axlegear                      -> Axlegear.xml
Components.AxleWheels.Data.Axles[0].Tyre -> Tyre1.xml
Components.AxleWheels.Data.Axles[1].Tyre -> Tyre2.xml
Components.AirDrag                       -> * Not Changed

=== XML VALIDATION ===
Vehicle XML is valid.

=== INPUT FILE SIGNATURE ERRORS ===
The following signature errors were found in the input XML files.
- Components.Axlegear - Signature does not match computed hash
- Components.AxleWheels.Data.Axles[0].Tyre - Signature does not match computed hash
- Components.AxleWheels.Data.Axles[1].Tyre - Signature does not match computed hash
```

## Use Case 09: Update Component Hashes and Validate Vehicle Job

The user has a vehicle XML job file (`Conventional_HeavyLorry.xml`) that may have outdated component signatures or validation errors due to schema updates or manual edits. The user wants to recreate the vehicle using the latest schema and recalculate all component hashes to ensure integrity. The tool processes the file, updates all signatures, and reports any validation errors or hash mismatches in the output report file.

**Command example:**

```bash
vectoim job -b re-sign Conventional_HeavyLorry.xml -o Conventional_HeavyLorry_fixed.xml
```

## Use Case 10: Overwrite a Value of a Vehicle Job from the Command Line

The user has an existing vehicle XML file (`Conventional_HeavyLorry.xml`) and wants to quickly modify one or more specific properties without manually editing the file. The user specifies property paths and new values directly in the command line, and the tool applies these changes when generating the output file. This works for simple properties, nested values, and even array elements. Setting a property to `null` or "No" removes the entire object.

**Command example:**

```bash
vectoim job ./Conventional_HeavyLorry.xml -o output.xml \
    -m Model=Variation1 \
    -m Components.AxleWheels.Data.Axles[1].Tyre.Data.RRCDeclared=0,0060
```

The command modifies the Model property and a value nested in an array. To remove an entire object, set it to `null` or `No`.

Note: When modifying components that have valid signatures, the `re-sign` or `clear` validation action is required to allow modifications. Otherwise, an error will be raised.

## Use Case 11: Explicitly Assign Components to Specific Vehicle Paths

The user has a vehicle XML file (`Conventional_HeavyLorry.xml`) and a component file (`Tyre.xml`) that should be assigned to a specific location in the vehicle structure. By default, the tool assigns components to the first matching location (e.g., first axle for tyres, first powertrain for engines). However, the user wants to assign the component to a different position (e.g., second axle instead of first). The user specifies the exact vehicle path using the `-m` parameter to override the default behavior.

**Default behavior example:**

Assuming a folder with these files:

- `Conventional_HeavyLorry.xml`
- `Tyre.xml`

**Command:**

```bash
vectoim job Conventional_HeavyLorry.xml Tyre.xml -o v1.xml
```

**Result:**

```txt
=== VEHICLE COMPONENTS ===
...
Components.AxleWheels.Data.Axles[0].Tyre -> Tyre.xml
```

As shown above, the tyre is assigned to the first axle (`Axles[0]`).

**Explicit path assignment example:**

To assign components to specific paths (e.g., second axle):

**Command:**

```bash
vectoim job Conventional_HeavyLorry.xml Tyre.xml -o v2.xml \
    -m Components.AxleWheels.Data.Axles[1].Tyre=Tyre.xml
```

**Result:**

```txt
=== VEHICLE COMPONENTS ===
...
Components.AxleWheels.Data.Axles[1].Tyre -> Tyre.xml
...
```

Now the tyre is assigned to the second axle (`Axles[1]`), while the first axle remains unchanged.


## Full Workflow Example

### Demo 1: Full Workflow Example with XML Components

The user has a complete vehicle XML job file (`Conventional_HeavyLorry.xml`) and wants to extract all components as separate XML files, then recreate the vehicle from a combination of the base vehicle structure and those components. This workflow demonstrates the basic cycle of extraction and reconstruction of vehicle job files using XML format for components.

**Goal:**

1. Extract vehicle information, without components, to JSON format
2. Extract all components to individual XML files in a components folder
3. Optionally edit the vehicle JSON or replace/edit component XML files
4. Rebuild the vehicle from the JSON structure and component XML files

**Command sequence:**

- **Step 1** Create a Vehicle Job in JSON format containing the vehicle structure with all component placeholders set to `null`. This serves as the base vehicle definition.

```bash
vectoim job Conventional_HeavyLorry.xml -o Conventional_HeavyLorry.json --drop-components
```

This creates the file `Conventional_HeavyLorry.json`

- **Step 2** Extract each component from the vehicle and save them as separate files in a `components` subfolder (by default in XML). The tool validates the job XML and creates individual component files.

```bash
vectoim job Conventional_HeavyLorry.xml -x ./components
```

This creates the files in the `components` folder: `Components.Engine.xml`, `Components.Gearbox.xml`, `Components.TorqueConverter.xml`, `Components.Angledrive.xml`, `Components.Retarder.xml`, `Components.Axlegear.xml`, `Components.AxleWheels.Data.Axles[0].Tyre.xml`, `Components.AxleWheels.Data.Axles[1].Tyre.xml`, `Components.AirDrag.xml`.

- **Step 3** (Optional) The user can now:
  - Edit the `Conventional_HeavyLorry.json` file to modify vehicle properties
  - Edit component files in the `components` folder
  - Replace component files from other sources into the `components` folder

- **Step 4** Combine the base vehicle JSON with all component XML files from the `components` folder to create the complete vehicle XML file. The system automatically validates all inputs and matches components to their correct positions in the vehicle structure.

```bash
vectoim job Conventional_HeavyLorry.json ./components/*.xml -o modified_vehicle.xml
```

Final output is `modified_vehicle.xml` along with `modified_vehicle.report.txt` containing details of the operation. The recreated vehicle should be functionally identical to the original (if no changes were made), with updated component signatures.

```txt
=== VEHICLE COMPONENTS ===
Components.Engine                        -> Components.Engine.xml
Components.Gearbox                       -> Components.Gearbox.xml
Components.TorqueConverter               -> Components.TorqueConverter.xml
Components.Angledrive                    -> Components.Angledrive.xml
Components.Retarder                      -> Components.Retarder.xml
Components.Axlegear                      -> Components.Axlegear.xml
Components.AxleWheels.Data.Axles[0].Tyre -> Components.AxleWheels.Data.Axles[0].Tyre.xml
Components.AxleWheels.Data.Axles[1].Tyre -> Components.AxleWheels.Data.Axles[1].Tyre.xml
Components.AirDrag                       -> Components.AirDrag.xml

=== XML VALIDATION ===
Vehicle XML is valid.
```