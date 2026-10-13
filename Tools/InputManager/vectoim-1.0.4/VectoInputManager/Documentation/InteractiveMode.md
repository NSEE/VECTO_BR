# Interactive Mode

## Overview

Interactive mode provides a flexible approach to creating vehicle configurations in VECTO Input Manager. Instead of requiring all vehicle and component data to be predefined in input files, interactive mode allows users to build configurations step-by-step through guided prompts. This is particularly valuable when creating new vehicles, filling in missing data from partial configurations, or understanding the structure and making small modifications of complex vehicle jobs.

The `--interactive` (`-i`) flag enables interactive mode for vehicle and component creation. The behavior varies depending on the mode selected.

### Interactive Mode Options

| Option | Description | Use Case |
|--------|-------------|----------|
| **`vehicle`** | Prompt for vehicle-level only | Prompts for vehicle structure but not component properties |
| **`all`** | Prompt for all values | Interactive creation with prompts for every property |
| **`auto`** | Prompt only when no value available | Smart prompting - only asks when values are missing |

### Usage Examples

```bash
# Interactive mode for all values in the input file
vectoim job -i all Conventional_PrimaryBus.xml -o Conventional_PrimaryBus_Modified.xml

# Interactive mode for vehicle structure only
vectoim job -i vehicle components/*.xml -o output.xml

# Auto mode - force v2.7 schema on a v2.4 input and prompt only for properties of newer schema
vectoim job -t ConvHeavyLorry_v2_7 -i auto ConvHeavyLorry_2_4.xml -o ConvHeavyLorry_2_7.xml
```

## Mode-Specific Behavior

### Mode: `all`

**Behavior:** Prompts for every vehicle property and every component property, showing value ranges and enumerated options.

⚠️ Can be time-consuming as the user is prompted to create even complex components with collections of objects.

💡 If an input vehicle job is used, then for each property, the corresponding value of the original file will be preselected and can be selected by pressing only "Enter".

### Mode: `auto`

**Behavior:**  Prompts only for values that are missing and accepts default values from input files.

### Mode: `vehicle`

**Behavior:** Prompts for vehicle-level properties and structure, prompts for array-based component structures (axles, powertrains, etc.), and assigns components from input files to the defined structure. Does NOT prompt for individual component properties.

**Example:**

```bash
vectoim job -i vehicle components/*.xml -o output.json
vectoim job -t ConvHeavyLorry_v2_7 -i vehicle -o new_vehicle.json
```

This is the recommended mode for creating vehicles with existing component files, or creating vehicles without components.

When creating a new vehicle with `-i vehicle` mode, below is how different component types are handled:

#### 1. Single Instance Root Components

**Path pattern:** `Components.{ComponentName}`

**Components:**

- `Angledrive`
- `AirDrag`
- `Axlegear`
- `Engine`
- `Gearbox`
- `Retarder`
- `TorqueConverter`

**Interactive Behavior:**  Assigned directly from input files (if provided). These components appear once per vehicle and do not require array indices or other structures to hold them.

💡 The properties for the `Auxiliaries` component are always requested in `vehicle` mode.

#### 2. Electric Machine Components

**Path pattern:** `Components.ElectricMachine.{ComponentName}`

**Components:**

- `ElectricMachineSystem`
- `ADC` (Additional Drivetrain Component)

**Interactive Behavior:** ElectricMachine structure is prompted for, but not the two components; they are assigned directly from input files (if provided).

#### 3. Multiple Powertrain Components

**Path pattern:** `Components.Powertrain[{index}].{ComponentName}`

**Components:**

- `Gearbox`
- `TorqueConverter`
- `Angledrive`
- `Retarder`
- `Axlegear`
- `IEPC` (Integrated Electric Powertrain Component)

**Interactive Behavior:**  Prompts for powertrain structure (number of powertrains and configuration of each powertrain), then assigns components from input files to each powertrain based on the defined structure. Does NOT prompt for individual component property values.

#### 4. Multiple Powertrain Electric Machine Components

**Path pattern:** `Components.Powertrain[{index}].ElectricMachine.{ComponentName}`

**Components:**

- `ElectricMachineSystem`
- `ADC` (Additional Drivetrain Component)

**Interactive Behavior:**  Prompts for powertrain and electric machine structure, then assigns components from input files to each powertrain's electric machine. Does NOT prompt for individual component property values.

#### 5. Axle-Specific Components (Tyres)

**Path pattern:** `Components.AxleWheels.Data.Axles[{index}].Tyre`

**Components:**

- `Tyre` (one per axle)

**Interactive Behavior:**  Prompts for axle structure, then assigns tyre components from input files to each axle based on the defined structure. Does NOT prompt for individual tyre component property values.

#### 6. Fuel Cell System Components

**Path pattern:** `Components.FuelCellSystem[{index}].FuelCell`

**Components:**

- `FuelCell`

**Interactive Behavior:**  Prompts for fuel cell system structure, then assigns fuel cell components from input files to each system. Does NOT prompt for individual component property values.

#### 7. Battery/Capacitor Energy Storage Components

**Path pattern:** `Components.ElectricEnergyStorage.Battery[{index}].REESS`

**Components:**

- `REESS` (Rechargeable Energy Storage System)

**Interactive Behavior:**  Prompts for battery structure, then assigns REESS components from input files to each battery pack. Does NOT prompt for individual component property values.


## Examples of Interactive Mode Behaviors

Assume we have a valid XML file named `Conventional_HeavyLorry.xml`.

### Case 1: `vehicle`

```bash
vectoim job -i vehicle Conventional_HeavyLorry.xml -o New_HeavyLorry.json
```
Expected Behavior:

- Prompts for vehicle-level properties (e.g., manufacturer, ADAS).
- Prompts for Axle Wheels structure (number of axles, driven axles) without prompting for the tyre components.
- Prompts for Auxiliaries component.

Does NOT prompt for individual component properties.

If the user accepts all defaults (by pressing "Enter"), the resulting `New_HeavyLorry.json` will be an exact copy of `Conventional_HeavyLorry.xml`.

### Case 2: `all`

```bash
vectoim job -i all Conventional_HeavyLorry.xml -o New_HeavyLorry.json
```

Expected Behavior:
- Prompts for every vehicle property and every component property.
- The number of prompts will be very large, as every property is requested.

If the user accepts all defaults (by pressing "Enter"), the resulting `New_HeavyLorry.json` will be an exact copy of `Conventional_HeavyLorry.xml`.

### Case 3: `auto`

```bash
vectoim job -i auto Conventional_HeavyLorry.xml -o New_HeavyLorry.json
```

Expected Behavior:
- Prompts only for missing vehicle properties and missing component properties. In our case, as the input file is complete, no prompts will be shown.

### Case 3: `auto` with schema change

```bash
vectoim job -t ConvMediumLorry_v2_7 -i auto Conventional_HeavyLorry.xml -o New_MediumLorry.xml
```

Expected Behavior:
- Prompts only for properties that are new in the v2.7 schema compared to the v2.4 input file.
