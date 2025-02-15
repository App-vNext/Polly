#! /usr/bin/env pwsh

Param(
    [string]$Configuration = "Release",
    [switch]$Interactive
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

Write-Output "Running benchmarks..."

$additionalArgs = @()

if ($Interactive -ne $true) {
    $additionalArgs += "--"
    $additionalArgs += "--filter"
    $additionalArgs += "*"
}

$project = Join-Path "Polly.Core.Benchmarks" "Polly.Core.Benchmarks.csproj"

dotnet run --configuration $Configuration --framework net9.0 --project $project $additionalArgs

exit $LASTEXITCODE
