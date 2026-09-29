#! /usr/bin/env pwsh

Param(
    [string]$Configuration = "Release",
    [string]$Framework = "net11.0",
    [Parameter(Mandatory = $false)][string] $Job = "",
    [Parameter(Mandatory = $false)][string[]] $Runtimes = @("net11.0"),
    [Parameter(Mandatory = $false)][string] $Affinity = "",
    [Parameter(Mandatory = $false)][switch] $EnableMemoryDiagnoser,
    [Parameter(Mandatory = $false)][switch] $EnableEventPipeProfiler,
    [switch]$Interactive
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

Write-Output "Running benchmarks..."

$additionalArgs = @()

$additionalArgs += "--runtimes"
$additionalArgs += $Runtimes

if (-Not [string]::IsNullOrEmpty($Job)) {
    $additionalArgs += "--job"
    $additionalArgs += $Job
}

if (-Not [string]::IsNullOrEmpty($Affinity)) {
    $additionalArgs += "--affinity"
    $additionalArgs += $Affinity
}

if ($EnableMemoryDiagnoser) {
    $additionalArgs += "--memory"
}

if ($EnableEventPipeProfiler) {
    $additionalArgs += "--profiler"
    $additionalArgs += "EP"
}

if ($Interactive -ne $true) {
    $additionalArgs += "--filter"
    $additionalArgs += "*"
}

$project = (Join-Path $PSScriptRoot "Polly.Core.Benchmarks" "Polly.Core.Benchmarks.csproj")

$dotnetArgs = @(
    "run"
    "--configuration", $Configuration
    "--framework", $Framework
    "--project", $project
    "--"
) + $additionalArgs

$p = Start-Process -FilePath "dotnet" -ArgumentList $dotnetArgs -NoNewWindow -PassThru
$p.WaitForExit()

if ($p.ExitCode -ne 0) {
    throw "Benchmarks failed with exit code $($p.ExitCode)."
}
