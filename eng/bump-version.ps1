#! /usr/bin/env pwsh
param(
    [Parameter(Mandatory = $true)][string] $ReleaseVersion
)

$ErrorActionPreference = "Stop"

$repo = Join-Path $PSScriptRoot ".."
$properties = Join-Path $repo "Directory.Packages.props"

$xml = [xml](Get-Content $properties)
$pollyVersion = $xml.SelectSingleNode('Project/PropertyGroup/PollyVersion')

if ($ReleaseVersion.StartsWith("v")) {
    $ReleaseVersion = $ReleaseVersion.Substring(1)
}

$version = [System.Version]::new($ReleaseVersion)
$releasedVersion = $version.ToString()

Write-Output "Bumping version from $($pollyVersion.InnerText) to $releasedVersion"

$pollyVersion.InnerText = $releasedVersion

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
