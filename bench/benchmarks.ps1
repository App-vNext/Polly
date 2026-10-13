#! /usr/bin/env pwsh

Param(
    [string]$Configuration = "Release",
    [string]$Framework = "net11.0",
    [Parameter(Mandatory = $false)][string] $Job = "",
    [Parameter(Mandatory = $false)][string[]] $Runtimes = @("net11.0"),
    [Parameter(Mandatory = $false)][string] $Affinity = "",
    [Parameter(Mandatory = $false)][string] $Filter = "*",
    [Parameter(Mandatory = $false)][switch] $EnableMemoryDiagnoser,
    [Parameter(Mandatory = $false)][switch] $EnableEventPipeProfiler
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

if (-Not [string]::IsNullOrEmpty($Filter)) {
    $additionalArgs += "--filter"
    $additionalArgs += $Filter
}

if ($EnableMemoryDiagnoser) {
    $additionalArgs += "--memory"
}

if ($EnableEventPipeProfiler) {
    $additionalArgs += "--profiler"
    $additionalArgs += "EP"
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
