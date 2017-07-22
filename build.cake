///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// EXTERNAL NUGET TOOLS
//////////////////////////////////////////////////////////////////////

#Tool "xunit.runner.console"
#Tool "GitVersion.CommandLine"
#Tool "Brutal.Dev.StrongNameSigner"

//////////////////////////////////////////////////////////////////////
// EXTERNAL NUGET LIBRARIES
//////////////////////////////////////////////////////////////////////

#addin "Cake.FileHelpers"
#addin "System.Text.Json"
using System.Text.Json;

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var projectName = "Polly";
var net40AsyncProjectName = "Polly.Net40Async";
var keyName = "Polly.snk";

var solutions = GetFiles("./**/*.sln");
var solutionPaths = solutions.Select(solution => solution.GetDirectory());

var srcDir = Directory("./src");
var buildDir = Directory("./build");
var artifactsDir = Directory("./artifacts");
var testResultsDir = artifactsDir + Directory("test-results");

// NuGet
var nuspecFilename = projectName + ".nuspec";
var net40AsyncNuspecFilename = net40AsyncProjectName + ".nuspec";
var nuspecSrcFile = srcDir + File(nuspecFilename);
var nuspecDestFile = buildDir + File(nuspecFilename);
var net40AsyncNuspecSrcFile = srcDir + File(net40AsyncNuspecFilename);
var net40AsyncNuspecDestFile = buildDir + Directory(net40AsyncProjectName) + File(net40AsyncNuspecFilename);
var nupkgDestDir = artifactsDir + Directory("nuget-package");
var snkFile = srcDir + File(keyName);

var projectToNugetFolderMap = new Dictionary<string, string[]>() {
    { "Net45"        , new [] {"net45"} },
    { "NetStandard11", new [] {"netstandard1.1"} },
};

var net40AsyncProjectToNugetFolderMap = new Dictionary<string, string[]>() {
    { "Net40Async"   , new [] {"net40"} },
};

// Gitversion
var gitVersionPath = ToolsExePath("GitVersion.exe");
Dictionary<string, object> gitVersionOutput;

// StrongNameSigner
var strongNameSignerPath = ToolsExePath("StrongNameSigner.Console.exe");


///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(_ =>
{
    Information("");
    Information(" ██████╗  ██████╗ ██╗     ██╗  ██╗   ██╗");
    Information(" ██╔══██╗██╔═══██╗██║     ██║  ╚██╗ ██╔╝");
    Information(" ██████╔╝██║   ██║██║     ██║   ╚████╔╝ ");
    Information(" ██╔═══╝ ██║   ██║██║     ██║    ╚██╔╝  ");
    Information(" ██║     ╚██████╔╝███████╗███████╗██║   ");
    Information(" ╚═╝      ╚═════╝ ╚══════╝╚══════╝╚═╝   ");
    Information("");
});

Teardown(_ =>
{
    Information("Finished running tasks.");
});

//////////////////////////////////////////////////////////////////////
// PRIVATE TASKS
//////////////////////////////////////////////////////////////////////

Task("__Clean")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] {
        buildDir,
        artifactsDir,
        testResultsDir,
        nupkgDestDir
  	});

    foreach(var path in solutionPaths)
    {
        Information("Cleaning {0}", path);
        CleanDirectories(path + "/**/bin/" + configuration);
        CleanDirectories(path + "/**/obj/" + configuration);
    }
});

Task("__RestoreNugetPackages")
    .Does(() =>
{
    foreach(var solution in solutions)
    {
        Information("Restoring NuGet Packages for {0}", solution);
        NuGetRestore(solution);
    }
});

Task("__UpdateAssemblyVersionInformation")
    .Does(() =>
{
    var gitVersionSettings = new ProcessSettings()
        .SetRedirectStandardOutput(true);

    IEnumerable<string> outputLines;
    StartProcess(gitVersionPath, gitVersionSettings, out outputLines);

    var output = string.Join("\n", outputLines);
    gitVersionOutput = new JsonParser().Parse<Dictionary<string, object>>(output);

    Information("Updated GlobalAssemblyInfo");
    Information("AssemblyVersion -> {0}", gitVersionOutput["AssemblySemVer"]);
    Information("AssemblyFileVersion -> {0}", gitVersionOutput["MajorMinorPatch"]);
    Information("AssemblyInformationalVersion -> {0}", gitVersionOutput["InformationalVersion"]);
});

Task("__UpdateDotNetStandardAssemblyVersionNumber")
    .Does(() =>
{
    // NOTE: TEMPORARY fix only, while GitVersionTask does not support .Net Standard assemblies.  See https://github.com/App-vNext/Polly/issues/176.  
    // This build Task can be removed when GitVersionTask supports .Net Standard assemblies.
    var assemblySemVer = gitVersionOutput["AssemblySemVer"].ToString();
    Information("Updating NetStandard1.1 AssemblyVersion to {0}", assemblySemVer);
    var replacedFiles = ReplaceRegexInFiles("./src/Polly.NetStandard11/Properties/AssemblyInfo.cs", "AssemblyVersion[(]\".*\"[)]", "AssemblyVersion(\"" + assemblySemVer +"\")");
    if (!replacedFiles.Any())
    {
        Information("NetStandard1.1 AssemblyVersion could not be updated.");
    }
});

Task("__UpdateAppVeyorBuildNumber")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() =>
{
    var fullSemVer = gitVersionOutput["FullSemVer"].ToString();
    AppVeyor.UpdateBuildVersion(fullSemVer);
});

Task("__BuildSolutions")
    .Does(() =>
{
    foreach(var solution in solutions)
    {
        Information("Building {0}", solution);

        MSBuild(solution, settings =>
            settings
                .SetConfiguration(configuration)
                .WithProperty("TreatWarningsAsErrors", "true")
                .UseToolVersion(MSBuildToolVersion.VS2017)
                .SetVerbosity(Verbosity.Minimal)
                .SetNodeReuse(false));
    }
});

Task("__RunTests")
    .Does(() =>
{
    XUnit2("./src/**/bin/" + configuration + "/**/*.Net4*.Specs.dll", new XUnit2Settings {
        OutputDirectory = testResultsDir,
        XmlReportV1 = true
    });
});

Task("__RunDotnetTests")
    .Does(() =>
{
    DotNetCoreTest("./src/Polly.NetStandard11.Specs/Polly.NetStandard11.Specs.csproj", new DotNetCoreTestSettings {
        Configuration = configuration,
        NoBuild = true
    });
});

Task("__CopyOutputToNugetFolder")
    .Does(() =>
{
    foreach(var project in projectToNugetFolderMap.Keys) {
        var sourceDir = srcDir + Directory(projectName + "." + project) + Directory("bin") + Directory(configuration);

        foreach(var targetFolder in projectToNugetFolderMap[project]) {
            var destDir = buildDir + Directory("lib");

            Information("Copying {0} -> {1}.", sourceDir, destDir);
            CopyDirectory(sourceDir, destDir);
       }
    }

    CopyFile(nuspecSrcFile, nuspecDestFile);
});

Task("__CopyNet40AsyncOutputToNugetFolder")
    .Does(() =>
{
    foreach(var project in net40AsyncProjectToNugetFolderMap.Keys) {
        var sourceDir = srcDir + Directory(projectName + "." + project) + Directory("bin") + Directory(configuration);

        foreach(var targetFolder in net40AsyncProjectToNugetFolderMap[project]) {
            var destDir = buildDir + Directory(net40AsyncProjectName) + Directory("lib");

            Information("Copying {0} -> {1}.", sourceDir, destDir);
            CopyDirectory(sourceDir, destDir);
       }
    }

    CopyFile(net40AsyncNuspecSrcFile, net40AsyncNuspecDestFile);
});

Task("__CreateNugetPackage")
    .Does(() =>
{
    var nugetVersion = gitVersionOutput["NuGetVersion"].ToString();
    var packageName = projectName;

    Information("Building {0}.{1}.nupkg", packageName, nugetVersion);

    var nuGetPackSettings = new NuGetPackSettings {
        Id = packageName,
        Title = packageName,
        Version = nugetVersion,
        OutputDirectory = nupkgDestDir
    };

    NuGetPack(nuspecDestFile, nuGetPackSettings);
});

Task("__CreateNet40AsyncNugetPackage")
    .Does(() =>
{
    var nugetVersion = gitVersionOutput["NuGetVersion"].ToString();
    var packageName = net40AsyncProjectName;

    Information("Building {0}.{1}.nupkg", packageName, nugetVersion);

    var nuGetPackSettings = new NuGetPackSettings {
        Id = packageName,
        Title = packageName,
        Version = nugetVersion,
        OutputDirectory = nupkgDestDir
    };

    NuGetPack(net40AsyncNuspecDestFile, nuGetPackSettings);
});

Task("__StronglySignAssemblies")
    .Does(() =>
{
    //see: https://github.com/brutaldev/StrongNameSigner
    var strongNameSignerSettings = new ProcessSettings()
        .WithArguments(args => args
            .Append("-in")
            .AppendQuoted(buildDir)
            .Append("-k")
            .AppendQuoted(snkFile)
            .Append("-l")
            .AppendQuoted("Changes"));

    StartProcess(strongNameSignerPath, strongNameSignerSettings);
});

Task("__CreateSignedNugetPackage")
    .Does(() =>
{
    var nugetVersion = gitVersionOutput["NuGetVersion"].ToString();
    var packageName = projectName + "-Signed";

    Information("Building {0}.{1}.nupkg", packageName, nugetVersion);

    var nuGetPackSettings = new NuGetPackSettings {
        Id = packageName,
        Title = packageName,
        Version = nugetVersion,
        OutputDirectory = nupkgDestDir
    };

    NuGetPack(nuspecDestFile, nuGetPackSettings);
});

Task("__CreateSignedNet40AsyncNugetPackage")
    .Does(() =>
{
    var nugetVersion = gitVersionOutput["NuGetVersion"].ToString();
    var packageName = net40AsyncProjectName + "-Signed";

    Information("Building {0}.{1}.nupkg", packageName, nugetVersion);

    var nuGetPackSettings = new NuGetPackSettings {
        Id = packageName,
        Title = packageName,
        Version = nugetVersion,
        OutputDirectory = nupkgDestDir
    };

    NuGetPack(net40AsyncNuspecDestFile, nuGetPackSettings);
});

//////////////////////////////////////////////////////////////////////
// BUILD TASKS
//////////////////////////////////////////////////////////////////////

Task("Build")
    .IsDependentOn("__Clean")
    .IsDependentOn("__RestoreNugetPackages")
    .IsDependentOn("__UpdateAssemblyVersionInformation")
    .IsDependentOn("__UpdateDotNetStandardAssemblyVersionNumber")
    .IsDependentOn("__UpdateAppVeyorBuildNumber")
    .IsDependentOn("__BuildSolutions")
    .IsDependentOn("__RunTests")
    .IsDependentOn("__RunDotnetTests")
    .IsDependentOn("__CopyOutputToNugetFolder")
	.IsDependentOn("__CopyNet40AsyncOutputToNugetFolder")  
    .IsDependentOn("__CreateNugetPackage")
	.IsDependentOn("__CreateNet40AsyncNugetPackage")
    .IsDependentOn("__StronglySignAssemblies")
    .IsDependentOn("__CreateSignedNugetPackage")
	.IsDependentOn("__CreateSignedNet40AsyncNugetPackage");

///////////////////////////////////////////////////////////////////////////////
// PRIMARY TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);

//////////////////////////////////////////////////////////////////////
// HELPER FUNCTIONS
//////////////////////////////////////////////////////////////////////

string ToolsExePath(string exeFileName) {
    var exePath = System.IO.Directory.GetFiles(@".\Tools", exeFileName, SearchOption.AllDirectories).FirstOrDefault();
    return exePath;
}
