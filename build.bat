@echo off

set nuget_dir=%~dp0.nuget
set packages_dir=%~dp0packages

%nuget_dir%\Nuget.exe install %nuget_dir%\packages.config -OutputDirectory %packages_dir%

set configuration=release
set task=default

if not [%1] == [] set task=%1
if not [%2] == [] set configuration=%2

powershell -NoProfile -ExecutionPolicy Bypass -Command "$psake_script_path = @(gci '%packages_dir%' -filter psake.ps1 -recurse)[0].FullName;& $psake_script_path -properties @{configuration='%configuration%'} build.ps1 %task%; if ($psake.build_success -eq $false) { exit 1 } else { exit 0 }"