# Runs each use case by executing its command(s) inside its pre-populated folder.
# Interactive use cases require user input and are commented out by default.

$ErrorActionPreference = 'Stop'

function Invoke-UseCase {
    param(
        [Parameter(Mandatory)][string]$Folder,
        [Parameter(Mandatory)][scriptblock]$Run
    )
    Write-Host ""
    Write-Host "=== $Folder ===" -ForegroundColor Cyan
    Push-Location (Join-Path $PSScriptRoot $Folder)
    try { & $Run }
    finally { Pop-Location }
}

# Echoes the command to the console, then executes it.
function Show-Run {
    param([Parameter(Mandatory)][string]$Command)
    Write-Host "> $Command" -ForegroundColor Yellow
    Invoke-Expression $Command
}


# ---------------------------------------------------------------------------
# Job Creator - JSON workflow
# ---------------------------------------------------------------------------

# Interactive:
# Invoke-UseCase 'UseCase01_InteractiveVehicleEntry' {
#     Show-Run 'vectoim job -i vehicle -o new_vehicle.json'
# }

# Interactive:
# Invoke-UseCase 'UseCase02_InteractiveWithComponentsJson' {
#     Show-Run 'vectoim job *.xml -i vehicle -o output.json'
#     Show-Run 'vectoim job output.json -o output.xml'
# }

Invoke-UseCase 'UseCase03_JsonVehicleAndComponents' {
    Show-Run 'vectoim job Conventional_HeavyLorry.xml -o Conventional_HeavyLorry.json'
    Show-Run 'vectoim job Conventional_HeavyLorry.json components/*.xml -o output.xml'
}

Invoke-UseCase 'UseCase04_ExtractVehicleToJson' {
    Show-Run 'vectoim job Conventional_HeavyLorry.xml -o Conventional_HeavyLorry.json -d'
}

Invoke-UseCase 'UseCase05_ExtractComponentsToJson' {
    Show-Run 'vectoim job Conventional_HeavyLorry.xml -x ./components -X json'
}

Invoke-UseCase 'UseCase06_ModifyViaPartialJson' {
    Show-Run 'vectoim job Conventional_HeavyLorry.xml modify.json -o output.xml'
}

# ---------------------------------------------------------------------------
# Job Creator - XML workflow
# ---------------------------------------------------------------------------

# Interactive:
# Invoke-UseCase 'UseCase07_InteractiveWithComponentsXml' {
#     Show-Run 'vectoim job *.xml -i vehicle -o output.xml'
# }

Invoke-UseCase 'UseCase08_ReplaceComponents' {
    Show-Run 'vectoim job Conventional_HeavyLorry.xml Axlegear.xml Tyre1.xml Tyre2.xml -o modifiedvehicle.xml'
}

Invoke-UseCase 'UseCase09_UpdateComponentHashes' {
    Show-Run 'vectoim job -b re-sign Conventional_HeavyLorry.xml -o Conventional_HeavyLorry_fixed.xml'
}

Invoke-UseCase 'UseCase10_OverwriteValue' {
    Show-Run 'vectoim job Conventional_HeavyLorry.xml -o output.xml -m Model=Variation1 -m Components.AxleWheels.Data.Axles[1].Tyre.Data.RRCDeclared=0,0060'
}

Invoke-UseCase 'UseCase11_AssignComponentPaths' {
    Show-Run 'vectoim job Conventional_HeavyLorry.xml Tyre.xml -o v1.xml'
    Show-Run 'vectoim job Conventional_HeavyLorry.xml Tyre.xml -o v2.xml -m Components.AxleWheels.Data.Axles[1].Tyre=Tyre.xml'
}

# ---------------------------------------------------------------------------
# Standard Values
# ---------------------------------------------------------------------------

Invoke-UseCase 'UseCase12_StandardValuesFromJson' {
    Show-Run 'vectoim sv battery_input.json -o battery_component.xml'
}

# Interactive:
# Invoke-UseCase 'UseCase13_StandardValuesInteractive' {
#     Show-Run 'vectoim sv -i -t battery -o battery_component.xml'
# }

# Interactive:
# Invoke-UseCase 'UseCase14_StandardValuesMixInteractive' {
#     Show-Run 'vectoim sv -t battery -i battery_input.json -o battery_component.xml'
# }

Invoke-UseCase 'UseCase15_StandardValuesMixParams' {
    Show-Run 'vectoim sv battery_input.json -o battery_component.xml -m CellsInSeries=20'
}

# ---------------------------------------------------------------------------
# Full workflow demos
# ---------------------------------------------------------------------------

Invoke-UseCase 'Demo1_XmlWorkflow' {
    Show-Run 'vectoim job Conventional_HeavyLorry.xml -o Conventional_HeavyLorry.json --drop-components'
    Show-Run 'vectoim job Conventional_HeavyLorry.xml -x ./components'
    Show-Run 'vectoim job Conventional_HeavyLorry.json ./components/*.xml -o modified_vehicle.xml'
}

Invoke-UseCase 'Demo2_JsonWorkflow' {
    Show-Run 'vectoim job Conventional_HeavyLorry.xml -o Conventional_HeavyLorry.json -d'
    Show-Run 'vectoim job Conventional_HeavyLorry.xml -x ./ -X json'
    Show-Run 'vectoim job *.json -o modified_vehicle.xml'
}
