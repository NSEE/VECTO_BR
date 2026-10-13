param(
    [string]$ProjectFile = "$(Get-Location)\VectoObjects\VectoObjects.csproj",
    [string]$NugetFeed = "https://code.europa.eu/api/v4/projects/1129/packages/nuget/index.json",
    [string]$Username,
    [string]$Token
)

if (-not $Token) {
    Write-Host "You must provide -Token (Personal Access Token)" -ForegroundColor Red
    exit 1
}

Write-Host "Cleaning release folder..."
if (Test-Path "release") {
    Remove-Item "release\*" -Recurse -Force
}

Write-Host "Restoring project..."
dotnet restore $ProjectFile

Write-Host "Building project..."
dotnet build $ProjectFile -c Release

Write-Host "Packing NuGet..."
dotnet pack $ProjectFile `
    -c Release `
    -o "release"

# Resulting package path
$Package = Get-ChildItem "release\*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if (-not $Package) {
    Write-Host "No .nupkg file was created!" -ForegroundColor Red
    exit 1
}

Write-Host "Found package: $($Package.FullName)"

# GitLab NuGet feed endpoint format

Write-Host "Publishing to GitLab: $NugetFeed"

dotnet nuget remove source vecto 2>$null

dotnet nuget add source $NugetFeed `
  --name vecto `
  --username $Username `
  --password $Token 

dotnet nuget push $Package.FullName `
  --source vecto `
  --skip-duplicate

Write-Host "Publish Completed"
