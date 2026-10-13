# Understanding JSON File Types

The VECTO ecosystem uses two distinct types of files that serve different purposes:

## 1. Vehicle and Component JSON Files (based on official VECTO XML schemas)

These are exact JSON representations of VECTO XML declaration files and are used as intermediate files:

- **Purpose**: Creating, editing, and validating XML vehicle job files
- **Format**: JSON representation of XSD-based XML schema of VECTO job files
- **File types**: Always `*.json`
- **Schema**: Strict adherence to VECTO XML schema
- **Tools**: VECTO Input Manager CLI (`vectoim` command)

**Example files:**

- `Conventional_HeavyLorry.json` (vehicle structure with or without its components)
- `Components.EMS.json` (EMS component)
- `Axlegear1.json` (axle gear component)

## 2. VECTO Simulation Files (for simulation runs)

These are configuration files for running simulations in the VECTO simulation tool:

- **Purpose**: Configuring and running vehicle simulations
- **Format**: JSON configuration files with purpose-based schemas (not XSD-based)
- **File types**: Job files (`.vecto`), vehicle files (`.vveh`), engine files (`.veng`), gearbox files (`.vgbx`), etc.
- **Schema**: Focus on simulation parameters, performance data, component behavior
- **Tools**: VECTO GUI

**Example files:**

- `Group5_HEV.vveh` (vehicle definition for simulation)
- `Engine_325kW_12.7l.veng` (engine definition)
- `AMT_12.vgbx` (gearbox definition)
- `LongHaul.vdri` (driving cycle data)

## Why Both Exist

- **VECTO Input Manager JSON files** make it easier to create and edit vehicle and component data (better editing, version control, validation) while maintaining full compatibility with official VECTO XML declaration format
- **VECTO simulation files** are optimized for running simulations with detailed performance maps, driving cycles, and operational parameters
