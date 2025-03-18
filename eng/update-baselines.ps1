#! /usr/bin/env pwsh
param()

$ErrorActionPreference = "Stop"

$encoding = New-Object System.Text.UTF8Encoding($true)
$releasedName = "PublicAPI.Shipped.txt"
$unshippedName = "PublicAPI.Unshipped.txt"

$repo = Join-Path $PSScriptRoot ".."
$src = Join-Path $repo "src"
$files = Get-ChildItem -Path $src -Filter $unshippedName -Recurse

# See https://github.com/dotnet/roslyn-analyzers/blob/b07c100bfc66013a8444172d00cfa04c9ceb5a97/src/PublicApiAnalyzers/Core/CodeFixes/DeclarePublicApiFix.cs#L373-L390
$comparer = {
    param(
        [string]$x,
        [string]$y
    )
    $comparison = [System.StringComparer]::OrdinalIgnoreCase.Compare($x, $y)
    if ($comparison -eq 0) {
        $comparison = [System.StringComparer]::Ordinal.Compare($x, $y)
    }
    return $comparison;
}

foreach ($file in $files) {
    $directory = [System.IO.Path]::GetDirectoryName($file)
    $changesPath = Join-Path $directory $unshippedName
    $baselinePath = Join-Path $directory $releasedName

    $baseline = Get-Content $baselinePath
    $baseline = [System.Collections.Generic.List[string]]$baseline

    $additions = Get-Content $changesPath
    $additions = [System.Collections.Generic.List[string]]$additions

    $edited = $false

    # Skip any "#nullable enable" header
    $nullableHeader = "#nullable enable"
    $index = (($additions.Count -gt 0) -And ($additions[0] -eq $nullableHeader)) ? 1 : 0
    while ($additions.Count -gt $index) {
        $addition = $additions[$index]
        $additions.RemoveAt($index)
        $baseline.Add($addition)
        $edited = $true
    }

    if ($edited) {
        $additions.Sort($comparer)
        $baseline.Sort($comparer)

        if (($additions.Count -eq 1) -and ($additions[0] -eq $nullableHeader)) {
            $additions | Set-Content $changesPath -Encoding $encoding
        } else {
            $null | Set-Content $changesPath -Encoding $encoding
        }

        $baseline | Set-Content $baselinePath -Encoding $encoding
    }
}
