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

#addin "System.Text.Json"
using System.Text.Json;

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var sourceProjectName = "Polly";
var targetProjectName = "Polly.Net40Async";
var keyName = "Polly.snk"; // Do we need a different strong-name key for Net40Async?

var solutions = GetFiles("./**/*.sln");
var solutionPaths = solutions.Select(solution => solution.GetDirectory());

var srcDir = Directory("./src");
var buildDir = Directory("./build");
var artifactsDir = Directory("./artifacts");
var testResultsDir = artifactsDir + Directory("test-results");

// NuGet
var nuspecFilename = targetProjectName + ".nuspec";
var nuspecSrcFile = srcDir + File(nuspecFilename);
var nuspecDestFile = buildDir + File(nuspecFilename);
var nupkgDestDir = artifactsDir + Directory("nuget-package");
var snkFile = srcDir + File(keyName);

var projectToNugetFolderMap = new Dictionary<string, string[]>() {
    { "Net40Async", new [] {"net40"} },
};

// Gitversion
var gitVersionPath = ToolsExePath("GitVersion.exe");
Dictionary<string, object> gitVersionOutput;

// StrongNameSigner
var strongNameSignerPath = ToolsExePath("StrongNameSigner.Console.exe");


///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(() =>
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

Teardown(() =>
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
                .UseToolVersion(MSBuildToolVersion.NET46)
                .SetVerbosity(Verbosity.Minimal)
                .SetNodeReuse(false));
    }
});

Task("__RunTests")
    .Does(() =>
{
    XUnit2("./src/**/bin/" + configuration + "/*.Specs.dll", new XUnit2Settings {
        OutputDirectory = testResultsDir,
        XmlReportV1 = true
    });
});

Task("__CopyOutputToNugetFolder")
    .Does(() =>
{
    foreach(var project in projectToNugetFolderMap.Keys) {
        var sourceDir = srcDir + Directory(sourceProjectName + "." + project) + Directory("bin") + Directory(configuration);

        foreach(var targetFolder in projectToNugetFolderMap[project]) {
            var destDir = buildDir + Directory("lib") + Directory(targetFolder);

            Information("Copying {0} -> {1}.", sourceDir, destDir);
            CopyDirectory(sourceDir, destDir);
       }
    }

    CopyFile(nuspecSrcFile, nuspecDestFile);
});

Task("__CreateNugetPackage")
    .Does(() =>
{
    var nugetVersion = gitVersionOutput["NuGetVersion"].ToString();
    var packageName = targetProjectName;

    Information("Building {0}.{1}.nupkg", packageName, nugetVersion);

    var nuGetPackSettings = new NuGetPackSettings {
        Id = packageName,
        Title = packageName,
        Version = nugetVersion,
        OutputDirectory = nupkgDestDir
    };

    NuGetPack(nuspecDestFile, nuGetPackSettings);
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
    var packageName = targetProjectName + "-Signed";

    Information("Building {0}.{1}.nupkg", packageName, nugetVersion);

    var nuGetPackSettings = new NuGetPackSettings {
        Id = packageName,
        Title = packageName,
        Version = nugetVersion,
        OutputDirectory = nupkgDestDir
    };

    NuGetPack(nuspecDestFile, nuGetPackSettings);
});

//////////////////////////////////////////////////////////////////////
// BUILD TASKS
//////////////////////////////////////////////////////////////////////

Task("Build")
    .IsDependentOn("__Clean")
    .IsDependentOn("__RestoreNugetPackages")
    .IsDependentOn("__UpdateAssemblyVersionInformation")
    .IsDependentOn("__UpdateAppVeyorBuildNumber")
    .IsDependentOn("__BuildSolutions")
    .IsDependentOn("__RunTests")
    .IsDependentOn("__CopyOutputToNugetFolder")    
    .IsDependentOn("__CreateNugetPackage")
    .IsDependentOn("__StronglySignAssemblies")
    .IsDependentOn("__CreateSignedNugetPackage");

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