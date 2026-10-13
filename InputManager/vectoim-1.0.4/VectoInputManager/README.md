# VECTO Input Manager (vectoim)

A command-line tool for creating, extracting, converting, and manipulating vehicle job components for VECTO simulations.

## Features

- **Component Extraction**: Decompose vehicle XML files into individual component files
- **XML <-> JSON Conversion**: Convert bidirectionally between XML and JSON formats with schema validation
- **Standard Values Generation**: Create certified components with standard values
- **Vehicle Job Creation**: Build new vehicle jobs by combining and replacing components
- **Multi-format Support**: Work with both XML and JSON component files
- **Schema Validation**: Built-in validation against VECTO schemas

## Installation

### Prerequisites

- **.NET 10 Runtime** - Download from: https://dotnet.microsoft.com/en-us/download/dotnet/10.0/runtime

### Download and Install

> **📥 [Download vectoim-1.0.4.zip](https://code.europa.eu/vecto/guests/vecto-sim-harry/-/raw/vectoinputmanager/VectoInputManager/vectoim-1.0.4.zip)**

1. Download the latest release using the link above and extract to a folder of your choice
2. (Recommended) Add the installation folder to your system PATH to run `vectoim` from any directory

### Platform-Specific Setup

#### Windows

Open Command Prompt or PowerShell in the installation folder and run:

```cmd
vectoim --help
```

#### macOS / Linux

Open a terminal in the installation folder and make the executable:

```bash
chmod +x vectoim
./vectoim --help
```

## Main Commands

vectoim provides three main commands:

| Command   | Description                                                        |
| --------- | ------------------------------------------------------------------ |
| `job`     | Manipulate vehicle jobs, extract their components and/or convert   |
| `comp`    | Manipulate components and/or convert to XML & JSON                 |
| `sv`      | Create components with standard values                             |

### Help and Documentation

- Get general help: `vectoim --help`
- Get command-specific help: `vectoim <command> --help`
- List available standard value components: `vectoim sv --list`
- List available vehicles: `vectoim job --list`

## Command Reference

### vectoim job

```text
DESCRIPTION:
Create jobs with components

USAGE:
    vectoim job [INPUT] [OPTIONS]

EXAMPLES:
    vectoim job vehicle.xml component_*.xml -o new_vehicle.xml

ARGUMENTS:
    [INPUT]    Input files for the job. You can specify multiple files or glob patterns

OPTIONS:
                             DEFAULT                                                                
    -h, --help                          Prints help information                                     
    -v, --verbose                       Enables verbose logging                                     
    -t, --type               auto       The vehicle type to create. For a list of supported         
                                        vehicles, use the --list argument                           
    -i, --interactive        none       Interactive mode for prompting user input. Options:         
                                        vehicle (prompt only for vehicle-level),                    
                                        all (prompt for all values),                                
                                        auto (prompt only when no value is available)               
    -o, --output                        Output file for the generated XML or JSON data              
    -k, --key-value-file                Generates a values file (.values.txt) containing all keys   
                                        and values assigned during vehicle creation                 
    -m, --modify                        Values to set for the job. Use the format: Path.Key=Value,  
                                        e.g.,                                                       
                                        Components.AxleWheels.Data.Axles[0].TwinTyres=True.         
                                        You can also assign components to specific vehicle paths,   
                                        e.g.,                                                       
                                        Components.Powertrain[1].Axlegear=axlegear.xml              
    -e, --example                       Generates an example JSON file for the component            
    -l, --list                          Lists available vehicles                                    
    -x, --extract                       Output directory where extracted component files will be    
                                        saved. Cannot be used together with -o                      
    -X, --xformat            Xml        Output format for extracted components. Valid options: xml, 
                                        json, both. The default is xml                              
    -b, --broken-sig         Auto       Action to take when signature verification fails. Options:  
                                        auto (default - keeps empty signatures, blocks on mismatched
                                        signatures),                                                
                                        sign (signs missing, blocks on mismatched signatures),      
                                        block (shows error, stops output),                          
                                        keep (warning, keeps broken signature),                     
                                        re-sign (warning, re-signs component),                      
                                        clear (warning, empty signature)                            
    -d, --drop-components               Removes (nullifies) all components from the vehicles before 
                                        converting them to JSON
```

### vectoim comp

```text
DESCRIPTION:
Modify or convert a component file

USAGE:
    vectoim comp <INPUT> [OPTIONS]

EXAMPLES:
    vectoim component engine.xml engine.json
    vectoim component engine.json engine.xml -m Data.Model=NewModel

ARGUMENTS:
    <INPUT>    Input files. One must be a component (XML or JSON), others are arbitrary JSON        
               modification files                                                                   

OPTIONS:
                         DEFAULT                                                                    
    -h, --help                      Prints help information                                         
    -v, --verbose                   Enables verbose logging                                         
    -o, --output                    Output file for the generated XML or JSON data                  
    -i, --interactive               Interactive mode for prompting user input                       
    -m, --modify                    Values to set for the component. Use the format: Key=Value,     
                                    e.g.,                                                           
                                    Data.Model=MyModel                                              
    -b, --broken-sig     Auto       Action to take when signature verification fails. Options:      
                                    auto (default - re-signs the component),                        
                                    sign (signs if missing, blocks on mismatched),                  
                                    block (shows error, stops output),                              
                                    keep (warning, keeps broken signature),                         
                                    re-sign (warning, re-signs component),                          
                                    clear (warning, empty signature)
```

### vectoim sv

```text
DESCRIPTION:
Create components with standard values

USAGE:
    vectoim sv [INPUT] [OPTIONS]

EXAMPLES:
    vectoim sv battery battery.json -i -o battery.xml

ARGUMENTS:
    [INPUT]    Input file containing component data in JSON format

OPTIONS:
                         DEFAULT                                                                    
    -h, --help                      Prints help information                                         
    -v, --verbose                   Enables verbose logging                                         
    -i, --interactive               Interactive mode                                                
    -t, --type           auto       The component type to create. The default is 'auto', which      
                                    automatically detects the component type from the input JSON.   
                                    For a list of supported components, use the --list argument     
    -o, --output                    Output file for the generated XML or JSON data                  
    -m, --modify                    Values to set for the component. Use the format: Key=Value,     
                                    e.g., R100CellsInSeries=20                                      
    -e, --example                   Generates an example JSON file for the component                
    -l, --list                      Lists available components
```

## Logging

vectoim creates log files for each run to help with troubleshooting and debugging. Each execution generates a separate log file with a timestamp in the filename. You can also increase the verbosity of the output logs by including the `-v` parameter.

### Log File Naming Convention

Log files follow the naming convention / pattern: `vectoim-YYYY-MM-DD-HHMMSS.log`

Example: `vectoim-2025-10-25-143022.log`

#### Windows location

```txt
C:\Users\<YourUsername>\AppData\Local\vecto\
```

**To access:**

1. Press **Win + R**
2. Type: ` %LOCALAPPDATA%\vecto `
3. Press **Enter**

#### Linux location

```txt
/home/<yourusername>/.local/share/vecto/
```

#### macOS location

```txt
/Users/<yourusername>/Library/Application Support/vecto/
```

Logs from the last 100 runs are maintained and older logs are deleted.

## Documentation

For detailed usage instructions and advanced features, see:

- **[Quick Start Guide](Documentation/Quickstart.md)** - Tutorial with step-by-step examples
- **[Use Cases - Job Creator XML Workflow](Documentation/UseCases_JobCreator_XML.md)** - Common scenarios and workflows for XML-based job creation
- **[Use Cases - Job Creator JSON Workflow](Documentation/UseCases_JobCreator_JSON.md)** - Common scenarios and workflows for JSON-based job creation
- **[Use Cases - Standard Values Tool](Documentation/UseCases_StandardValues.md)** - Use cases for creating standard value components
- **[Understanding JSON File Types](Documentation/AboutJson.md)** - Explanation of different JSON file types in the VECTO ecosystem
- **[Validation Actions](Documentation/ValidationActions.md)** - Details on signature validation options
- **[Interactive Mode](Documentation/InteractiveMode.md)** - Guide to using interactive vehicle creation
- **[Dependencies](Documentation/Dependencies.md)** - Third-party libraries and their licenses

## License

This project uses several third-party libraries. See [Dependencies](Documentation/Dependencies.md) for detailed license information.
