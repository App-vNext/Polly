# find StrongNaming.psd1 in the packages directory and import it
Import-Module @(Get-ChildItem $packages_dir -filter "StrongNaming.psd1" -recurse)[0].FullName

Properties {
  $solution_file_name = "Polly.sln"
  $solution_dir = Split-Path $psake.build_script_file
  $version_file_name = "SolutionVersion.cs"

  $nuget_spec_name = "Polly.nuspec"
  $nuget_package_name = "Polly"
  $nuget_signed_package_name = "Polly-Signed"

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

  $snk_name = "Polly.snk"  
  
  $solution_file_path = "$solution_dir\$solution_file_name"

  $xunit_runner_exe = "xunit.console.clr4.exe"
  $xunit_runner_full_path = @(Get-ChildItem $packages_dir -filter $xunit_runner_exe -recurse)[0].FullName

  $nuget_full_path = "$solution_dir\.nuget\nuget.exe"

  $framework_version = @("net35", "net40", "net45")

  $project_to_nuget_folder_map = @{
    "net35" = "net35"; 
    "net40" = "net40"; 
    "net45" = "net45";
    "pcl"   = "portable-net45+netcore45+wpa81+wp8+MonoAndroid1+MonoTouch1";
  }

  $test_project_names = @("Polly.Specs")
}

FormatTaskName {
   param($taskName)
   write-host "`n $(('-'*25)) [$taskName] $(('-'*25))" -ForegroundColor Yellow
}

task Default -depends PrintVariables, Clean, CreateOutputDirectories, Compile, RunTests, CreateNugetPackage, CreateSignedNugetPackage

task CreateNugetPackage {
  Create-NuGet-Structure-For-Package $nuget_package_name
	
  $version = Get-VersionNumber-From-SolutionVersionFile
  Write-Host "creating NuGet package $nuget_package_name v$version" -ForegroundColor Green

  Exec { & $nuget_full_path pack $nuget_dir\$nuget_package_name\$nuget_package_name.nuspec -o $nuget_dir\ -version $version }
}

task CreateSignedNugetPackage {
  Create-NuGet-Structure-For-Package $nuget_signed_package_name
  
  $version = Get-VersionNumber-From-SolutionVersionFile
  Write-Host "creating NuGet package $nuget_signed_package_name v$version" -ForegroundColor Green

  $key = Import-StrongNameKeyPair -KeyFile $snk_name
  Get-ChildItem -Recurse $nuget_dir\$nuget_signed_package_name\Polly.dll| Set-Strongname -keypair $key -verbose -NoBackup

  Exec { & $nuget_full_path pack $nuget_dir\$nuget_signed_package_name\$nuget_signed_package_name.nuspec -o $nuget_dir\ -version $version }
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

function global:Create-Directory($directory_path) {
  New-Item $directory_path -itemType directory -ErrorAction SilentlyContinue | Out-Null
}

function global:Get-TestProject-Output-Dir($test_project_name) {
    return "$projects_dir\$test_project_name\bin\$configuration"
}

function global:Get-Project-Output-Dir($project_name, $framework_version) {
    return "$projects_dir\$project_name.$framework_version\bin\$configuration"
}

function global:Get-VersionNumber-From-SolutionVersionFile {
  $assemblyVersionPattern = 'AssemblyVersion\("([0-9]+(\.([0-9]+|\*)){1,3})"\)'
  $versionNumberGroup = get-content $version_file_name | select-string -pattern $assemblyVersionPattern | select -first 1 | % { $_.Matches }            
  $versionNumber = $versionNumberGroup.Groups[1].Value

  return $versionNumber
}

function global:Create-NuGet-Structure-For-Package($package_name) {
  $nuget_package_folder = "$nuget_dir\$package_name"
  write-host $nuget_package_folder
  Create-Directory $nuget_package_folder

  Copy-Item "$solution_dir\$nuget_spec_name" "$nuget_package_folder\$package_name.nuspec"
  Replace-Text $nuget_package_folder\$package_name.nuspec "{{PACKAGE_NAME}}" $package_name

  $project_to_nuget_folder_map.GetEnumerator() | % { 
    $framework_dir = "$nuget_package_folder\lib\$($_.value)"
    $framework_dll_dir = Get-Project-Output-Dir "Polly" $($_.key)

    write-host "Copying $($_.key) version to $framework_dir"
    
    Create-Directory $framework_dir
    Copy-Item "$framework_dll_dir\Polly.dll" $framework_dir
    Copy-Item "$framework_dll_dir\Polly.xml" $framework_dir
  }

}

function global:Replace-Text($file, $key, $value) {
  (Get-Content $file) | Foreach-Object {$_ -replace $key, $value } | Set-Content $file
}