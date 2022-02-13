<#

.SYNOPSIS
This is a Powershell script to bootstrap a Cake build.

.DESCRIPTION
This Powershell script will download NuGet if missing, restore NuGet tools (including Cake)
and execute your Cake build script with the parameters you provide.

.PARAMETER Script
The build script to execute.
.PARAMETER Target
The build script target to run.
.PARAMETER Configuration
The build configuration to use.
.PARAMETER Verbosity
Specifies the amount of information to be displayed.
.PARAMETER Experimental
Tells Cake to use the latest Roslyn release.
.PARAMETER WhatIf
Performs a dry run of the build script.
No tasks will be executed.
.PARAMETER Mono
Tells Cake to use the Mono scripting engine.

.LINK
http://cakebuild.net
#>

Param(
    [string]$Script = "build.cake",
    [string]$Target = "Default",
    [string]$Configuration = "Release",
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [string]$Verbosity = "Verbose",
    [switch]$Experimental,
    [Alias("DryRun","Noop")]
    [switch]$WhatIf,
    [switch]$Mono,
    [switch]$SkipToolPackageRestore,
    [switch]$Verbose
)

Write-Host "Preparing to run build script..."

# Should we show verbose messages?
if($Verbose.IsPresent)
{
    $VerbosePreference = "continue"
}

$TOOLS_DIR = Join-Path $PSScriptRoot "tools"
$NUGET_EXE = Join-Path $TOOLS_DIR "nuget.exe"
$CAKE_EXE = "dotnet dotnet-cake"
$PACKAGES_CONFIG = Join-Path $TOOLS_DIR "packages.config"
$DOTNET = "dotnet.exe"

# Should we use mono?
$UseMono = "";
if($Mono.IsPresent) {
    Write-Verbose -Message "Using the Mono based scripting engine."
    $UseMono = "-mono"
}

# Should we use the new Roslyn?
$UseExperimental = "";
if($Experimental.IsPresent -and !($Mono.IsPresent)) {
    Write-Verbose -Message "Using experimental version of Roslyn."
    $UseExperimental = "-experimental"
}

# Is this a dry run?
$UseDryRun = "";
if($WhatIf.IsPresent) {
    $UseDryRun = "-dryrun"
}

# Make sure tools folder exists
if ((Test-Path $PSScriptRoot) -and !(Test-Path $TOOLS_DIR)) {
    New-Item -Path $TOOLS_DIR -Type directory | out-null
}

# Try download NuGet.exe if not exists
if (!(Test-Path $NUGET_EXE)) {
    Write-Verbose -Message "Downloading NuGet.exe..."
    Invoke-WebRequest -Uri https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile $NUGET_EXE
}

# Make sure NuGet exists where we expect it.
if (!(Test-Path $NUGET_EXE)) {
    Throw "Could not find NuGet.exe"
}

# Save nuget.exe path to environment to be available to child processed
$ENV:NUGET_EXE = $NUGET_EXE

# Restore tools from NuGet?
if(-Not $SkipToolPackageRestore.IsPresent)
{
    # Restore tools from NuGet.
    Push-Location
    Set-Location $TOOLS_DIR

    Write-Verbose -Message "Restoring tools from NuGet..."

    # Restore packages
    if (Test-Path $PACKAGES_CONFIG)
    {
        $NuGetOutput = Invoke-Expression "&`"$NUGET_EXE`" install -ExcludeVersion"
        Write-Verbose ($NuGetOutput | Out-String)
    }
    # Install just Cake if missing config
    else
    {
        $NuGetOutput = Invoke-Expression "&`"$DOTNET`" tool install Cake.Tool --version 2.0.0"
        Write-Verbose ($NuGetOutput | Out-String)
    }
    Pop-Location
    if ($LASTEXITCODE -ne 0)
    {
        exit $LASTEXITCODE
    }
}


# Start Cake
Write-Host "Running build script..."
Invoke-Expression "$CAKE_EXE `"$Script`" --target=`"$Target`" --configuration=`"$Configuration`" --verbosity=`"$Verbosity`" $UseMono $UseDryRun $UseExperimental"
exit $LASTEXITCODE