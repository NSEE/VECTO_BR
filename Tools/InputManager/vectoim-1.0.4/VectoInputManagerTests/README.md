# VECTO Input Manager Tests

## Overview

This NUnit test project validates the VECTO Input Manager functionality and also includes utilities for code generation.

### Test Namespaces

**VectoInputManager.Tests.Core** - Core functionality tests

- `CorpusBuilder_Tests` - Tests for corpus building functionality (combining component files into vehicles)
- `JsonDeclarationUtils_Tests` - Tests for JSON declaration serialization/deserialization
- `JsonInputUtils_Tests` - Tests for JSON input parsing and utilities
- `ObjectBuilder_JObject_Tests` - Tests for JObject-based object building
- `ObjectBuilder_Tests` - Tests for building objects interactively with prompts
- `SignatureValidation_Tests` - Tests for signature validation actions
- `TypeUtils_Tests` - Tests for type reflection and component conversion utilities
- `VehicleBuilder_Tests` - Tests for interactive vehicle creation workflows
- `XmlSerializer_Tests` - Tests for XML serialization (decimal formatting, culture handling)
- `XmlUtils_Tests` - Tests for XML utility functions and synthetic vehicle generation

**VectoInputManager.Tests.Console** - Console command tests

- `Console_Job_Tests` - Tests for the `job` console command (vehicle creation)
- `Console_StandardValues_Tests` - Tests for the `standardvalues` console command (component generation)
- `Console_Json_Tests` - Tests for the `json` console command (XML to JSON conversion)
- `Console_Extract_Tests` - Tests for the `extract` console command (component extraction from vehicles)

**VectoInputManager.Tests.StandardValues** - Standard value calculation tests

- `StandardValues_Tests` - Tests for component standard values calculation (see next section for details)
	 
**VectoInputManager.Tests.Samples** - Sample processing tests

- `SampleProcessing_Tests` - Tests for validating serialization, deserialization, and schema validation of XML and JSON formats on sample vehicle files

### Utilities

The utilities below are included in the test project but are marked as `[Explicit]` tests. They are not run automatically and must be executed manually when needed.

**VectoInputManager.Tests.CodeGeneration** - Code generation utilities (see next section for details)

- `CodeGeneration_Utility` - Generates C# helper code and JSON schemas for component input types
- `GenerateVectoObjects_Utility` - Generates C# classes from VECTO XSD schemas

**VectoInputManager.Tests.Utilities** - Data Generation Utilities

- `SyntheticData_Utility` - Generates synthetic XML test data from existing samples
- `Helpers_Utility` - Helper code for internal development

# Standard Value Components Calculation Tests

The test class `StandardValues_Tests` validates the standard values calculation methodology for vehicle components. The test calculates component standard values from JSON input files and compares the results against expected XML output files.

**Test cases are created by adding pairs of files**: each JSON input file must have a matching XML expected output file with the same base name.

## Test Structure

### Input Files Location
Test cases are organized in component-specific folders under:
```
Input/StandardValues/{ComponentType}/
```

### Supported Component Types
- **ADC** - Alternating Current/Direct Current converter
- **Angledrive** - Angle drive components
- **Axle** - Axle components
- **Battery** - Battery systems
- **Capacitor** - Capacitor systems
- **EMS** - Energy Management System
- **FuelCell** - Fuel cell systems
- **GearBox** - Gearbox/transmission
- **IEPC** - Integrated Electric Powertrain
- **Retarder** - Retarder/brake systems
- **TC** - Torque Converter

## Test Workflow

For each component type, the test:

1. Loads the input data from `{ComponentName}.json`
2. Instantiates the appropriate calculator and computes component values
3. Creates a component XML with the calculated values
4. Compares recursively the generated XML against the expected `{ComponentName}.xml`

## File Pairing

Each test case consists of **a pair of files** with **matching base names**:
- `{ComponentName}.json` - Input data (test parameters)
- `{ComponentName}.xml` - Expected output (reference result)

The test automatically discovers all JSON files and looks for their corresponding XML pairs.

**To add a new test case, create a pair of files:**

```
Input/StandardValues/{ComponentType}/{TestName}.json
Input/StandardValues/{ComponentType}/{TestName}.xml
```

Running the test again will automatically discover and validate the new pair.

### Example Files
```
Input/StandardValues/Battery/
  ├── Battery_Standard.json
  ├── Battery_Standard.xml
  ├── Battery_HighCapacity.json
  ├── Battery_HighCapacity.xml
  ├── Battery_LowVoltage.json
  └── Battery_LowVoltage.xml
```

## Output

Test output is generated in: `VectoInputManagerTests/Output/{DateTime}/InputOutput_Tests/`

# Code Generation

## XSD Schema Updates

VECTO XSDs are located in the path `VectoCore/VectoCore/Resources/XSD` and are used  to generate C# classes representing VECTO objects. If the XSD files are modified, do the following to update the projects:

1) Execute `GenerateVectoObjects_Utility/GenerateVectoObjects`

This will generate the file `VectoInputManagerTests/Output/{DateTime}/VectoObjects.cs`. Copy this and replace the file `VectoObjects/Generated/VectoObjects.cs`

2) Update the JSON schemas by running `CodeGeneration_Utility/GenerateComponentInputJsonSchemas`

This updates the JSON Schema files in the folders:

- VectoInputManager/ComponentSchemas/*.schema.json
- VectoInputManager/InputSchemas/*.schema.json
- VectoInputManager/VehicleSchemas/*.schema.json

**Important**: These files should be committed to a public repository, and its base URL should be reflected in the file `VectoInputManager\VectoInputManagerCore\Consts.cs`

3) Execute all tests in the `VectoInputManagerTests` project. 

### Extra steps if vehicles are added

If the XSDs include new vehicles, then also do the following after step (1) above and before step (2).

1) Execute `CodeGeneration_Utility/GenerateVehicleTypesData_Utility`

This will generate the file `VectoInputManagerTests/Output/{DateTime}/VehicleTypes.csv`. Edit it and then replace the file `VectoInputManagerCore/Data/VehicleTypes.csv`.

2) Execute `CodeGeneration_Utility/Generate_VehicleTypesCs_Utility`

This will generate the file `VectoInputManagerTests/Output/{DateTime}/VehicleTypes.cs`. Replace the file `VectoObjects/VehicleTypes.cs`

# Vehicle Samples

The `VectoInputManager.Tests.Samples` namespace includes tests that check serialization, deserialization, hash, output xml validation on all the official VECTO Job samples (last updated `23-05-2025`). 

Samples can be added to the folder `VectoInputManager\VectoInputManagerTests\Input\Samples`.
