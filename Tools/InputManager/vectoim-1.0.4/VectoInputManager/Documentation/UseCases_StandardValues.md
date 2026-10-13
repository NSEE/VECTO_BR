# Use Cases - Standard Values Tool

This document covers use cases for the Standard Values Tool (`sv` command).

> 💡 **Download the ready-to-run files:** [UseCases.zip](https://code.europa.eu/vecto/guests/vecto-sim-harry/-/raw/vectoinputmanager/VectoInputManager/UseCases/UseCases.zip). Each use case has its own folder pre-populated with the input files it needs. Extract the archive and either run the included `Run-Demos.ps1` script or `cd` into a use-case folder and run its commands manually.

## Use Case 12: Create Component with Standard Values from JSON Input

The user has prepared a JSON file (`battery_input.json`) containing all the required information for a specific component type. The user wants to generate a complete XML component file with calculated standard values. The tool validates that all necessary information is present and correct, computes any standard values required by the VECTO specification, and produces a properly formatted and signed XML component file ready for use in vehicle jobs.

**Command example:**

```bash
vectoim sv battery_input.json -o battery_component.xml
```

💡 For an example component input JSON, use the command `vectoim sv -t fuelcell -e`.

For a list of supported components, use the command `vectoim sv -l`.

## Use Case 13: Interactive Creation of Component with Standard Values

The user wants to create a new component (e.g., battery) from scratch without preparing an input file. The tool prompts the user interactively for the component type and then for each required property, showing valid value ranges and enumerated options where applicable. Once all necessary information is provided, the tool calculates standard values and generates a complete XML component file.

**Command example:**

```bash
vectoim sv -i -t battery -o battery_component.xml
```

## Use Case 14: Mix Input File and Interactive Mode

The user has a partially completed JSON input file (`battery_input.json`) containing some component properties and wants to review and potentially modify these values before generating the final component. The tool loads the values from the JSON file as defaults and presents them interactively, allowing the user to accept each value (by pressing Enter) or provide a different value. This is useful for creating variations of existing components.

**Command example:**

```bash
vectoim sv -t battery -i battery_input.json -o battery_component.xml
```

## Use Case 15: Mix Input File and Parameter Value Input

The user has a complete JSON input file (`battery_input.json`) for a component but wants to override one or more specific properties without editing the file. The user specifies the property changes directly on the command line, and the tool applies these overrides to the values loaded from the JSON file before generating the component XML. Multiple property overrides can be specified in a single command.

**Command example:**

```bash
vectoim sv battery_input.json -o battery_component.xml -m CellsInSeries=20
```
