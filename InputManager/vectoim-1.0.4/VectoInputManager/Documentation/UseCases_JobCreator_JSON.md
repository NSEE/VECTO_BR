# Use Cases - Job Creator Tool (JSON Workflow)

This document covers use cases for the Job Creator Tool (`job` command) when working with JSON files and advanced component manipulation.

> 💡 **Download the ready-to-run files:** [UseCases.zip](https://code.europa.eu/vecto/guests/vecto-sim-harry/-/raw/vectoinputmanager/VectoInputManager/UseCases/UseCases.zip). Each use case has its own folder pre-populated with the input files it needs. Extract the archive and either run the included `Run-Demos.ps1` script or `cd` into a use-case folder and run its commands manually.

See also:

- [Use Cases - Job Creator Tool (XML Workflow)](UseCases_JobCreator_XML.md) for XML-based workflows
- [Understanding JSON File Types](AboutJson.md) for clarification on different JSON file types used in the VECTO ecosystem

## Use Case 01: Interactive Vehicle Information Entry

The user wants to create a new vehicle configuration from scratch without any existing files. The tool guides the user through an interactive process, displaying available vehicle types and prompting for each vehicle property with valid value ranges or enumerated options. The result is a JSON file containing the complete vehicle information, without any components.

**Command example:**

```bash
vectoim job -i vehicle -o new_vehicle.json
```

During the interactive session, the user is prompted to select the vehicle type from a list:

```txt
Vehicle Type:

> Conventional Vehicle - Completed Bus (v2.7)
  Conventional Vehicle - Heavy Lorry (v2.7)
  Conventional Vehicle - Medium Lorry (v2.7)
  Conventional Vehicle - Primary Bus (v2.7)
  ...
```

Based on the selected vehicle type, the tool prompts for each required vehicle property. Depending on the required data type, the tool shows valid ranges or enumerated options. Example prompts:

```txt
Vehicle Type: Vehicle - Completed Bus (v2.7)
Manufacturer: Test Bus
Manufacturer address: Athens
VIN: 123123122
Date (DateTime): 1/1/2025
Simulation tool license number: 1
Model: Test 1
Legislative category (opt.): M 3
Corrected actual mass (int, opt.)
Technical permissible maximum laden mass (int, opt.)
Class bus (opt.):

  I
  II
> III
  A
  B
  I plus II
  II plus III
  <Not Set>
```

When all prompts are completed, the tool generates the vehicle job JSON file (`new_vehicle.json`).

💡 Read more about the interactive mode in the [Interactive Mode Documentation](InteractiveMode.md).

## Use Case 02: Interactive Vehicle Information Entry with Components

The user has XML component files and wants to create a new vehicle by selecting a vehicle type interactively and incorporating these existing components. The tool validates that the components match the selected vehicle type, then guides the user through entering the remaining vehicle properties. The result is an intermediate vehicle job JSON file which can be used to create a complete VECTO job XML file with all components integrated.

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
vectoim job *.xml -i vehicle -o output.json
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

**Important:** The `-i vehicle` flag disables interactive prompts for component data.

For example, when creating a vehicle with multiple axles, you'll need to specify the axle configuration. The tool will then automatically assign the tyre components from the input files to each axle in file name order. Example prompt:

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

After completing the interactive session, the tool generates the vehicle job JSON file (`output.json`). The user can then convert this to XML format using the following command:

```bash
vectoim job output.json -o output.xml
```

## Use Case 03: Working with JSON Vehicle and Components

The user has an existing vehicle XML file (`Conventional_HeavyLorry.xml`) or a JSON vehicle definition file (previously created) along with multiple XML component files. The user wants to combine the vehicle information with the component files to produce a complete VECTO job XML. The tool validates that all components are compatible with the vehicle type defined in the JSON file, then assembles them into a valid job file.

**Command example:**

```bash
vectoim job Conventional_HeavyLorry.xml -o Conventional_HeavyLorry.json
vectoim job Conventional_HeavyLorry.json components/*.xml -o output.xml
```

The first command converts the vehicle XML to JSON. The second command combines the JSON vehicle information with the component XML files (kept in a `components` subfolder) to create a new vehicle job.

Note: keep the component XML files in a separate folder (e.g. `components/`) so the glob does not also pick up the original `Conventional_HeavyLorry.xml`, which would otherwise be treated as a second vehicle.

Note that some components are deeply nested into other placeholder objects, like tyres (for each axle), REESS, or even multiple gearboxes, retarders, etc. for multiple powertrain vehicles. In these cases, the vehicle JSON should be prepared with the placeholder objects and the component files will be used in ascending order.

In all cases of vehicle job generation, a `*.report.txt` file is created containing a detailed report of which components were used in the generation plus any other errors and/or warnings observed.

## Use Case 04: Extract Vehicle Information to JSON

The user has a complete vehicle XML job file (`Conventional_HeavyLorry.xml`) that includes all components and vehicle properties. The user wants to extract only the vehicle structure and properties (without the actual component data) to a JSON file for easier editing. The tool creates a JSON file where all component references are set to `null` but the placeholder structure remains intact, making it compatible with subsequent component integration workflows even for complex vehicles.

**Command example:**

```bash
vectoim job .\Conventional_HeavyLorry.xml -o ./Conventional_HeavyLorry.json -d
```

The `--drop-components` parameter will remove all components. The output, however, retains all structures that held the components. Example output:

```json
...
  "Components": {
    "$type": "Components_Conventional_LorryType_v2_7",
    "Engine": null,
    "Gearbox": null,
    "TorqueConverter": null,
    "Angledrive": null,
    "Retarder": null,
    "Axlegear": null,
    "AxleWheels": {
      "$type": "AxleWheelsComponentDeclarationType_v2_0",
      "Data": {
        "$type": "AxleWheelsDataDeclarationType_v2_7",
        "Axles": {
          "$type": "[AxleDataDeclarationType_v2_7]",
          "$values": [
            {
              "$type": "AxleDataDeclarationType_v2_7",
              "AxleType": "VehicleNonDriven",
              "TwinTyres": false,
              "Steered": true,
              "Tyre": null,
              "WheelEnd": {
                "$type": "WheelEndType_v2_7",
                "WheelEndFriction": 3.2,
                "CertificationNumber": "ASDF"
              },
              "axleNumber": 1
            },
            {
              "$type": "AxleDataDeclarationType_v2_7",
              "AxleType": "VehicleDriven",
              "TwinTyres": true,
              "Steered": false,
              "Tyre": null,
              "WheelEnd": null,
              "axleNumber": 2
            }
          ]
        }
      }
    },
...
```


## Use Case 05: Extract All Components to JSON Format

The user has a complete vehicle XML job file (`Conventional_HeavyLorry.xml`) and wants to extract each individual component (Engine, Gearbox, Tyres, etc.) as separate JSON files for easier inspection and editing. The tool validates the job XML, then saves each component as a JSON file with a descriptive filename that indicates the component type, its position in the vehicle structure, and any array indices where applicable.

**Command example:**

```bash
vectoim job Conventional_HeavyLorry.xml -x ./components -X json
```

## Use Case 06: Modify Vehicle Properties Using Partial JSON Update Files

The user has an existing vehicle XML file (`Conventional_HeavyLorry.xml`) and wants to modify several properties without recreating the entire vehicle structure. The user creates a partial JSON file (`modify.json`) containing only the properties to be changed. The tool merges the modification file with the base vehicle file, applying the updates while preserving all other existing properties. Multiple modification files can be combined in sequence, with later files overriding earlier ones.

**Command example:**

```bash
vectoim job Conventional_HeavyLorry.xml modify.json -o output.xml
```

The system processes input files in order, merging them into a single vehicle definition. Each subsequent file can override properties from previous files. This works with both XML and JSON inputs - you can start with an XML vehicle file and modify it with a JSON modification file.

Note: When modifying components that have valid signatures, the `re-sign` or `clear` validation action is required to allow modifications. Otherwise, an error will be raised.

**Example modification file (modify.json):**

```json
{
  "Manufacturer": "XXX",
  "Components": {
    "Axlegear": {
      "Data": {
        "Ratio": 3.64,
        "TorqueLossMap": {
          "$values": [
            {
              "torqueLoss": 15.0
            },
            {
              "torqueLoss": 62.0
            },
            {
              "torqueLoss": 61.0
            }
          ]
        }
      }
    },
    "Retarder": {
      "Data": {
        "CertificationMethod": "Measured",
        "RetarderLossMap": {
          "$values": [
            {
              "retarderSpeed": 42.0,
              "torqueLoss": 42.0
            },
            {
              "$crop": true
            }
          ]
        }
      }
    },
    "AirDrag": {
      "Data": {
        "CdxA_0": 1.0
      }
    }
  }
}

```

**Result:** The output vehicle will have:

- `Manufacturer` changed to "XXX"
- `Components.Axlegear.Data.Ratio` changed to 3.64
- `Components.Axlegear.Data.TorqueLossMap` - first 3 entries modified with new `torqueLoss` values (15.0, 62.0, 61.0), rest of the array unchanged
- `Components.Retarder.Data.CertificationMethod` changed from "Standard values" to "Measured"
- `Components.Retarder.Data.RetarderLossMap` cropped to only 1 entry (all subsequent items removed)
- `Components.AirDrag.Data.CdxA_0` changed to 1.0

**Advanced array manipulation:** To remove all items from an array after a certain index, use `"$crop": true`:

This will keep only the first item in the `RetarderLossMap` array and remove all subsequent items.

## Full Workflow Example

### Demo 2: Full Workflow Example with JSON Components

The user has a complete vehicle XML job file (`Conventional_HeavyLorry.xml`) and wants to separate the vehicle structure from its components, modify them independently as JSON files for easier editing, and then reassemble everything into a new vehicle XML. This workflow demonstrates the full cycle of extraction, modification, and reconstruction using JSON as the intermediate format for maximum flexibility.

**Goal:**

1. Extract vehicle information, without components, to JSON
2. Extract all components to individual JSON files
3. Modify the component and/or vehicle JSON files
4. Build the vehicle XML with modified components

**Command sequence:**

- **Step 1** Create a JSON file containing the vehicle structure with all component placeholders set to `null`. This serves as the base vehicle definition.

```bash
vectoim job Conventional_HeavyLorry.xml -o ./Conventional_HeavyLorry.json -d
```

This creates the file `Conventional_HeavyLorry.json`

- **Step 2** Extract each component from the vehicle and save them as separate JSON files.

```bash
vectoim job Conventional_HeavyLorry.xml -x ./ -X json
```

This creates the files: `Components.Engine.json`, `Components.Gearbox.json`,  `Components.TorqueConverter.json`, `Components.Angledrive.json`, `Components.Retarder.json`, `Components.Axlegear.json`, `Components.AxleWheels.Data.Axles[0].Tyre.json`, `Components.AxleWheels.Data.Axles[1].Tyre.json`, `Components.AirDrag.json`.

- **Step 3** The user can modify the vehicle JSON file and/or any of the component JSON files as needed. It's recommended to use a modern editor that supports JSON schema validation (see supported editors at https://www.schemastore.org), to enable auto-completion and error checking.

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

## Workflow Comparison: XML vs JSON Components

**Use JSON workflow** when:

- You need to make frequent or complex modifications to components
- You want enhanced editing support with auto-completion and real-time validation
- You're working in a team environment with version control
- You prefer modern, readable file formats

**Use XML workflow** when:

- You're exchanging components with other parties who expect XML
- You're already comfortable with XML editing
- Invalid signatures from original XML components must be preserved in the final output (see [Validation Actions Documentation](ValidationActions.md) for signature handling options)

Both workflows produce identical final results with valid VECTO job XML files.
