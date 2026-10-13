# VECTO Input Manager

**VECTO Input Manager** is a CLI tool and .NET library project for creating, extracting, converting, and manipulating VECTO (Vehicle Energy Consumption calculation Tool) vehicle job files.

**Executive Summary**

VECTO Input Manager is a cross-platform command line interface (CLI) tool for working with vehicle job files for the EU's official VECTO (Vehicle Energy Consumption Calculation Tool) application. The toolkit provides four core capabilities: building and modifying complete vehicle configurations interactively or by replacing components and modifying arbitrary values, creating certified components interactively or from JSON input files with calculated standardized values, decomposing vehicle XML files into individual component files, and bidirectional XML-JSON conversion with built-in schema validation.

Common workflows include:

- **Component Extraction and Reuse**: Extracting components from existing vehicle XML files into separate XML or JSON files for reuse across multiple vehicle configurations
- **Format Conversion**: Converting vehicle and component files between XML and JSON formats with built-in schema validation, facilitating easier editing and version control
- **Standard Value Component Generation**: Creating certified components with calculated standard values by providing minimal input parameters
- **Vehicle Assembly and Modification**: Building new vehicle jobs by combining components from multiple sources, or modifying existing vehicles by replacing individual components while maintaining structural integrity and validation
- **Interactive Configuration**: Guiding users through vehicle creation with prompted input for missing values, reducing errors and improving usability for less experienced users
- **Signature Validation Management**: Controlling how signature verification failures are handled during vehicle assembly, with options to sign, re-sign, clear, block, or keep signatures based on workflow requirements

The toolkit supports all vehicle type declarations across all VECTO schema versions: conventional vehicles, hybrid electric vehicles, pure electric vehicles, fuel cell hybrid vehicles, and multiple powertrain configurations.

Primary users include Original Equipment Manufacturers (OEMs), Type Approval Authorities (TAA) and engineers & developers who prototype vehicle configurations or build automated VECTO workflows.

The solution consists of the following projects: VectoInputManager (cross-platform command-line tool), VectoInputManagerCore (UI-agnostic business logic library), VectoObjects (strongly-typed .NET classes for all VECTO schema versions with serialization/deserialization functions), and test projects (VectoInputManagerTests & VectoObjectsTests).

---

**User Documentation**

- **[VECTO Input Manager CLI](VectoInputManager/README.md)** - Command-line interface documentation
- **[Quick Start Guide](VectoInputManager/Documentation/Quickstart.md)** - Step-by-step tutorial with practical examples
- **[Use Cases - Job Creator XML Workflow](VectoInputManager/Documentation/UseCases_JobCreator_XML.md)** - Common workflows for XML-based job creation
- **[Use Cases - Job Creator JSON Workflow](VectoInputManager/Documentation/UseCases_JobCreator_JSON.md)** - Common workflows for JSON-based job creation
- **[Use Cases - Standard Values Tool](VectoInputManager/Documentation/UseCases_StandardValues.md)** - Use cases for creating standard value components
- **[Understanding JSON File Types](VectoInputManager/Documentation/AboutJson.md)** - Explanation of different JSON file types in the VECTO ecosystem
- **[Validation Actions](VectoInputManager/Documentation/ValidationActions.md)** - Explanation of signature validation modes
- **[Interactive Mode](VectoInputManager/Documentation/InteractiveMode.md)** - Guide to interactive vehicle and component creation
- **[Dependencies](VectoInputManager/Documentation/Dependencies.md)** - Third-party libraries and licenses

**Developer Documentation**

- **[Architecture](VectoInputManager/Documentation/Architecture.md)** - Architecture Overview
- **[VectoObjects Library](VectoObjects/README.md)** - .NET library for VECTO object manipulation
- **[VectoInputManagerTests](VectoInputManagerTests/README.md)** - Unit tests and code generation for VECTO Input Manager

**Building the Project**

```powershell
# Build the Cli application
dotnet build ./VectoInputManager

# Run all unit tests
dotnet test ./VectoInputManagerTests

# Publish
./publish.ps1 

# Generate documentation (requires Doxygen)
./generatedocs.ps1
```

**License**

This project uses several third-party libraries with different licenses. See [Dependencies](VectoInputManager/Documentation/Dependencies.md) for complete license information.
