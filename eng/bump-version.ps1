#! /usr/bin/env pwsh
param(
    [Parameter(Mandatory = $true)][string] $ReleaseVersion
)

$ErrorActionPreference = "Stop"

$repo = Join-Path $PSScriptRoot ".."
$properties = Join-Path $repo "Directory.Packages.props"

$xml = [xml](Get-Content $properties)

if ($ReleaseVersion.StartsWith("v")) {
    $ReleaseVersion = $ReleaseVersion.Substring(1)
}

$version = [System.Version]::new($ReleaseVersion)
$releasedVersion = $version.ToString()

$packages = @(
    "Polly",
    "Polly.Core",
    "Polly.Extensions",
    "Polly.RateLimiting",
    "Polly.Testing"
)

foreach ($package in $packages) {
    $packageVersion = $xml.SelectSingleNode("//PackageVersion[@Include='$package']/@Version")
    Write-Output "Bumping $package version from $($packageVersion.Value) to $releasedVersion"
    $packageVersion.InnerText = $releasedVersion
}

$settings = New-Object System.Xml.XmlWriterSettings
$settings.Encoding = New-Object System.Text.UTF8Encoding($false)
$settings.Indent = $true
$settings.OmitXmlDeclaration = $true

try {
    $writer = [System.Xml.XmlWriter]::Create($properties, $settings)
    $xml.Save($writer)
} finally {
    if ($writer) {
        $writer.Flush()
        $writer.Dispose()
        "" >> $properties
    }
    $writer = $null
}

$targets = Join-Path $repo "eng" "Library.targets"
$pattern = '(<PackageValidationBaselineVersion\b[^>]*>)[^<]+(</PackageValidationBaselineVersion>)'
$content = Get-Content $targets -Raw

if ($content -notmatch $pattern) {
    throw "Unable to find PackageValidationBaselineVersion in $targets."
}

Write-Output "Bumping PackageValidationBaselineVersion to $releasedVersion"

$content = $content -replace $pattern, "`${1}$releasedVersion`${2}"
[System.IO.File]::WriteAllText($targets, $content, (New-Object System.Text.UTF8Encoding($false)))
