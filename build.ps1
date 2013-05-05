Properties {
  $solution_file_name = "Polly.sln"
  $solution_dir = Split-Path $psake.build_script_file
  $version_file_name = "SolutionVersion.cs"

  $nuget_package_name = "Polly.nuspec"

  $configuration = "release"

  $build_dir_name = "build_artifacts"
  $test_results_dir_name = "test_results"
  $nuget_dir_name = "nuget"
  $projects_dir_name = "src"
  $packages_dir_name = "packages"

  $build_dir = "$solution_dir\$build_dir_name"
  $test_results_dir = "$build_dir\$test_results_dir_name"
  $nuget_dir = "$build_dir\$nuget_dir_name"
  $projects_dir = "$solution_dir\$projects_dir_name"
  $packages_dir = "$solution_dir\$packages_dir_name"  
  
  $solution_file_path = "$solution_dir\$solution_file_name"

  $xunit_runner_exe = "xunit.console.clr4.exe"
  $xunit_runner_full_path = @(gci $packages_dir -filter $xunit_runner_exe -recurse)[0].FullName

  $nuget_full_path = "$solution_dir\.nuget\nuget.exe"

  $framework_version = @("net35", "net40", "net45")
  $test_project_names = @("Polly.Specs")
}

FormatTaskName {
   param($taskName)
   write-host "`n $(('-'*25)) [$taskName] $(('-'*25))" -ForegroundColor Yellow
}

task Default -depends PrintVariables, Clean, CreateOutputDirectories, Compile, RunTests, CreateNugetPackage

task CreateNugetPackage {
	$version = Get-VersionNumber-From-SolutionVersionFile

  Write-Host "creating NuGet package v$version" -ForegroundColor Green

  Copy-Item $solution_dir\$nuget_package_name $nuget_dir

  $framework_version | ForEach-Object { 
    $framework_dir = "$nuget_dir\lib\$_"
    $framework_dll_dir = Get-Project-Output-Dir "Polly" $_

    write-host "Copying $_ version to $framework_dir"
    
    Create-Directory $framework_dir
    Copy-Item $framework_dll_dir\Polly.dll $framework_dir
    Copy-Item $framework_dll_dir\Polly.xml $framework_dir
  }

  Exec { & $nuget_full_path pack $nuget_dir\$nuget_package_name -o $nuget_dir -version $version }
}

task RunTests { 
  $test_project_names | ForEach-Object { 
    Write-Host "testing $_" -ForegroundColor Green

    $test_results_filename = "$test_results_dir\$_.Results.html"
    $test_dll = "$(Get-TestProject-Output-Dir $_)\$_.dll"

    Write-Host "$xunit_runner_full_path ""$test_dll"" /html ""$test_results_filename""" -ForegroundColor yellow
    Exec { & $xunit_runner_full_path "$test_dll" /html "$test_results_filename"}

    Write-Host ""
  }
}

task Compile { 
  Write-Host "building $solution_file_name in $configuration mode" -ForegroundColor Green

  Write-Host "msbuild $solution_file_name /t:Build /p:Configuration=$configuration /v:quiet" -ForegroundColor yellow
  Exec { msbuild $solution_file_name /t:Build /p:Configuration=$configuration /v:quiet } 
}

task CreateOutputDirectories  {
  Write-Host "creating build_dir"
  Create-Directory $build_dir

  Write-Host "creating test_results_dir"
  Create-Directory $test_results_dir

  Write-Host "creating nuget_dir"
  Create-Directory $nuget_dir
}

task Clean { 
  Write-Host "removing $build_dir directory" -ForegroundColor Green
  Remove-Item -force -recurse $build_dir -ErrorAction SilentlyContinue

  Write-Host "msbuild $solution_file_name /t:Clean /p:Configuration=$configuration /v:quiet" -ForegroundColor yellow
  Exec { msbuild $solution_file_name /t:Clean /p:Configuration=$configuration /v:quiet }
} 

task PrintVariables { 
  Write-Host "configuration: $configuration"
  Write-Host "solution_dir: $solution_dir"
  Write-Host "solution_file_path: $solution_file_path"
  Write-Host "nuget_package_name: $nuget_package_name"
  Write-Host "build_dir: $build_dir"
  Write-Host "test_results_dir: $test_results_dir"
  Write-Host "nuget_dir_name: $nuget_dir_name"
  Write-Host "projects_dir_name: $projects_dir_name"
  Write-Host "packages_dir: $packages_dir"
  Write-Host "xunit_runner_full_path: $xunit_runner_full_path"
  Write-Host "test_project_names: $test_project_names"
} 

function global:Create-Directory([string]$directory_path) {
  New-Item $directory_path -itemType directory -ErrorAction SilentlyContinue | Out-Null
}

function global:Get-TestProject-Output-Dir($test_project_name) {
    return "$projects_dir\$test_project_name\bin\$configuration"
}

function global:Get-Project-Output-Dir($project_name, $framework_version) {
    return "$projects_dir\$project_name.$framework_version\bin\$configuration"
}

function global:Get-VersionNumber-From-SolutionVersionFile
{
  $assemblyVersionPattern = 'AssemblyVersion\("([0-9]+(\.([0-9]+|\*)){1,3})"\)'
  $versionNumberGroup = get-content $version_file_name | select-string -pattern $assemblyVersionPattern | select -first 1 | % { $_.Matches }            
  $versionNumber = $versionNumberGroup.Groups[1].Value

  return $versionNumber
}