#! /usr/bin/env pwsh
<#

.SYNOPSIS
This is a PowerShell script to bootstrap a Cake build.

.DESCRIPTION
This PowerShell script will restore NuGet tools (including Cake)
and execute your Cake build script with the parameters you provide.

.PARAMETER Script
The build script to execute.
.PARAMETER Target
The build script target to run.
.PARAMETER Configuration
The build configuration to use.
.PARAMETER Verbosity
Specifies the amount of information to be displayed.
.PARAMETER WhatIf
Performs a dry run of the build script.
No tasks will be executed.

.LINK
https://cakebuild.net
#>

Param(
    [string]$Script = "build.cake",
    [string]$Target = "Default",
    [ValidateSet("Default", "MutationCore", "MutationLegacy")]
    [string]$Configuration = "Release",
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [string]$Verbosity = "Verbose",
    [Alias("DryRun","Noop")]
    [switch]$WhatIf,
    [switch]$SkipToolPackageRestore,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

Write-Output "Preparing to run build script..."

# Should we show verbose messages?
if ($Verbose.IsPresent)
{
    $VerbosePreference = "continue"
}

$TOOLS_DIR = Join-Path $PSScriptRoot "tools"

# Make sure tools folder exists
if ((Test-Path $PSScriptRoot) -and !(Test-Path $TOOLS_DIR)) {
    New-Item -Path $TOOLS_DIR -Type directory | out-null
}

# Restore tools from NuGet?
if (-Not $SkipToolPackageRestore.IsPresent)
{
    # Restore tools from NuGet.
    Push-Location
    Set-Location $TOOLS_DIR

    Write-Verbose -Message "Restoring tools from NuGet..."

    & dotnet tool restore | Write-Verbose

    Pop-Location
    if ($LASTEXITCODE -ne 0)
    {
        exit $LASTEXITCODE
    }
}

# Is this a dry run?
$additionalArgs = @();
if ($WhatIf.IsPresent) {
    $additionalArgs += "-dryrun"
}

# Start Cake
Write-Output "Running build script..."

& dotnet cake $Script "--target=$Target" "--configuration=$Configuration" "--verbosity=$Verbosity" $additionalArgs

exit $LASTEXITCODE
