param(
    [string]$Version = "",
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$SkipRestore,
    [switch]$SkipBuild,
    [switch]$SkipZip
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectName = "SpineViewer"
$ProjectFile = Join-Path $RepoRoot "$ProjectName\$ProjectName.csproj"
$SolutionFile = Join-Path $RepoRoot "$ProjectName.sln"
$PublishRoot = Join-Path $RepoRoot "publish"
$ReleaseRoot = Join-Path $RepoRoot "release"

function Get-ProjectVersion {
    param([string]$CsprojPath)
    [xml]$proj = Get-Content -Path $CsprojPath
    $versionNode = $proj.Project.PropertyGroup | ForEach-Object { $_.Version } | Where-Object { $_ } | Select-Object -First 1
    if (-not $versionNode) {
        throw "Could not find <Version> in $CsprojPath"
    }
    return $versionNode.Trim()
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet SDK not found. Install .NET 8 SDK first: https://dotnet.microsoft.com/download/dotnet/8.0"
}

if (-not (Test-Path $ProjectFile)) {
    throw "Project file not found: $ProjectFile"
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    $Version = "v$(Get-ProjectVersion -CsprojPath $ProjectFile)"
}

$FrameworkOut = Join-Path $PublishRoot "$ProjectName-$Version"
$SelfContainedOut = Join-Path $PublishRoot "$ProjectName-$Version-SelfContained"
$FrameworkZip = Join-Path $ReleaseRoot "$ProjectName-$Version.zip"
$SelfContainedZip = Join-Path $ReleaseRoot "$ProjectName-$Version-SelfContained.zip"

Write-Host "Repo root: $RepoRoot"
Write-Host "Version: $Version"
Write-Host "Configuration: $Configuration"
Write-Host "Runtime: $Runtime"

Push-Location $RepoRoot
try {
    if (-not $SkipRestore) {
        Write-Host "Restoring solution..."
        dotnet restore $SolutionFile
    }

    if (-not $SkipBuild) {
        Write-Host "Building solution..."
        dotnet build $SolutionFile -c $Configuration
    }

    Write-Host "Publishing framework-dependent build..."
    dotnet publish $ProjectFile -c $Configuration -r $Runtime --self-contained false -o $FrameworkOut

    Write-Host "Publishing self-contained build..."
    dotnet publish $ProjectFile -c $Configuration -r $Runtime --self-contained true -o $SelfContainedOut

    if (-not $SkipZip) {
        Write-Host "Creating zip packages..."
        New-Item -ItemType Directory -Path $ReleaseRoot -Force | Out-Null
        Compress-Archive -Path (Join-Path $FrameworkOut "*") -DestinationPath $FrameworkZip -Force
        Compress-Archive -Path (Join-Path $SelfContainedOut "*") -DestinationPath $SelfContainedZip -Force
    }

    Write-Host ""
    Write-Host "Done."
    Write-Host "Framework-dependent: $FrameworkOut"
    Write-Host "Self-contained:      $SelfContainedOut"
    if (-not $SkipZip) {
        Write-Host "Zip:                 $FrameworkZip"
        Write-Host "Zip:                 $SelfContainedZip"
    }
}
finally {
    Pop-Location
}
