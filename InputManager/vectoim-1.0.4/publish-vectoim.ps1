function Publish-VectoInputManager {
    param (
        [Parameter(Mandatory)]
        [string]$Runtime,

        [Parameter(Mandatory)]
        [string]$OutputPath
    )

    # Construct the base publish command
    $publishArgs = @(
        "publish", "./VectoInputManager",
        "-c", "Release",
        "-f", "net10.0",
        "--output", $OutputPath
    )

    if ($Runtime -ne "portable") {
        $publishArgs += @(
            "-r", $Runtime,
            "--self-contained",
            "/p:PublishSingleFile=true",
            "/p:PublishTrimmed=false",
            "/p:PublishReadyToRun=true"
        )
    }

    dotnet @publishArgs
    # Remove all subfolders in the output directory
    Get-ChildItem -Path $OutputPath -Directory | Remove-Item -Recurse -Force
    # Remove all *.config files in the output directory except nlog.config
    Get-ChildItem -Path $OutputPath -Filter *.config -File | Where-Object { $_.Name -ne "nlog.config" } | Remove-Item -Force
    # Copy the html folder to the output directory
    Copy-Item -Path "./html" -Destination $OutputPath -Recurse -Force
}


Write-Host "Cleaning release folder..."
if (Test-Path "release") {
    Remove-Item "release\*" -Recurse -Force
}

Publish-VectoInputManager -Runtime "portable" -OutputPath "./release"
