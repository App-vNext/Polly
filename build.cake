///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// EXTERNAL NUGET TOOLS
//////////////////////////////////////////////////////////////////////

#Tool "xunit.runner.console&version=2.9.3"
#Tool "dotnet-stryker&version=4.5.1"

//////////////////////////////////////////////////////////////////////
// EXTERNAL NUGET LIBRARIES
//////////////////////////////////////////////////////////////////////

#addin nuget:?package=Cake.FileHelpers&version=7.0.0
#addin nuget:?package=Newtonsoft.Json&version=13.0.3

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var projectName = "Polly";

var solutions = GetFiles("./**/*.sln");
var solutionPaths = solutions.Select(solution => solution.GetDirectory());

var srcDir = Directory("./src");
var artifactsDir = Directory("./artifacts");
var testResultsDir = System.IO.Path.Combine(artifactsDir, Directory("test-results"));

// NuGet
var nupkgDestDir = System.IO.Path.Combine(artifactsDir, Directory("package"), Directory("release"));

// Stryker / Mutation Testing
var strykerConfig = MakeAbsolute(File("./eng/stryker-config.json"));
var strykerOutput = MakeAbsolute(Directory(System.IO.Path.Combine(artifactsDir, Directory("mutation-report"))));

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(_ =>
{
    Information("");
    Information("----------------------------------------");
    Information("Starting the cake build script");
    Information("Building: " + projectName);
    Information("----------------------------------------");
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
    CleanDirectories(
    [
        testResultsDir,
        nupkgDestDir,
        artifactsDir,
        strykerOutput
    ]);

    foreach (var path in solutionPaths)
    {
        Information("Cleaning {0}", path);

        var dotNetCleanSettings = new DotNetCleanSettings
        {
            Verbosity = DotNetVerbosity.Minimal,
        };

        DotNetClean(path.ToString(), dotNetCleanSettings);
    }
});

Task("__RestoreNuGetPackages")
    .Does(() =>
{
    foreach (var solution in solutions)
    {
        Information("Restoring NuGet Packages for {0}", solution);
        DotNetRestore(solution.ToString());
    }
});

Task("__BuildSolutions")
    .Does(() =>
{
    foreach (var solution in solutions)
    {
        Information("Building {0}", solution);

        var dotNetBuildSettings = new DotNetBuildSettings
        {
            Configuration = configuration,
            Verbosity = DotNetVerbosity.Minimal,
            NoRestore = true,
            MSBuildSettings = new DotNetMSBuildSettings
            {
                TreatAllWarningsAs = MSBuildTreatAllWarningsAs.Error,
            },
        };

        DotNetBuild(solution.ToString(), dotNetBuildSettings);
    }
});

Task("__ValidateAot")
    .Does(() =>
{
    var aotProject = MakeAbsolute(File("./test/Polly.AotTest/Polly.AotTest.csproj"));
    var settings = new DotNetPublishSettings
    {
        Configuration = configuration,
        Verbosity = DotNetVerbosity.Minimal,
        MSBuildSettings = new DotNetMSBuildSettings
        {
            TreatAllWarningsAs = MSBuildTreatAllWarningsAs.Error,
        },
    };

    DotNetPublish(aotProject.ToString(), settings);
});

Task("__RunTests")
    .Does(() =>
{
    var loggers = Array.Empty<string>();

    if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GITHUB_SHA")))
    {
        loggers = ["GitHubActions;report-warnings=false"];
    }

    var projects = GetFiles("./test/**/*.csproj");

    foreach (var proj in projects)
    {
        DotNetTest(proj.FullPath, new DotNetTestSettings
        {
            Configuration = configuration,
            Loggers = loggers,
            NoBuild = true,
            ToolTimeout = System.TimeSpan.FromMinutes(10),
        });
    }
});

Task("__CreateNuGetPackages")
    .Does(() =>
{
    var dotNetPackSettings = new DotNetPackSettings
    {
        Configuration = configuration,
        NoBuild = true,
        OutputDirectory = nupkgDestDir,
        MSBuildSettings = new DotNetMSBuildSettings
        {
            TreatAllWarningsAs = MSBuildTreatAllWarningsAs.Error,
        },
    };

    string[] packages =
    [
        System.IO.Path.Combine(srcDir, "Polly.Core", "Polly.Core.csproj"),
        System.IO.Path.Combine(srcDir, "Polly", "Polly.csproj"),
        System.IO.Path.Combine(srcDir, "Polly.RateLimiting", "Polly.RateLimiting.csproj"),
        System.IO.Path.Combine(srcDir, "Polly.Extensions", "Polly.Extensions.csproj"),
        System.IO.Path.Combine(srcDir, "Polly.Testing", "Polly.Testing.csproj"),
    ];

    Information("Building NuGet packages");

    foreach (string project in packages)
    {
        DotNetPack(project, dotNetPackSettings);
    }
});

Task("__ValidateDocs")
    .Does(() =>
{
    var result = StartProcess("dotnet", "mdsnippets --validate-content");
    if (result != 0)
    {
        throw new InvalidOperationException($"Failed to validate the documentation snippets. Are the links correct?");
    }
});

Task("__Setup")
    .IsDependentOn("__Clean")
    .IsDependentOn("__RestoreNuGetPackages");

Task("__CommonBuild")
    .IsDependentOn("__Setup")
    .IsDependentOn("__ValidateDocs")
    .IsDependentOn("__BuildSolutions");

//////////////////////////////////////////////////////////////////////
// BUILD TASKS
//////////////////////////////////////////////////////////////////////

Task("Build")
    .IsDependentOn("__CommonBuild")
    .IsDependentOn("__ValidateAot")
    .IsDependentOn("__RunTests")
    .IsDependentOn("__CreateNuGetPackages");

///////////////////////////////////////////////////////////////////////////////
// PRIMARY TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build");

///////////////////////////////////////////////////////////////////////////////
// MUTATION TESTING TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("MutationTestsCore")
    .IsDependentOn("__Setup")
    .Does((context) =>
{
    RunMutationTests(File("./src/Polly.Core/Polly.Core.csproj"), File("./test/Polly.Core.Tests/Polly.Core.Tests.csproj"));
});

Task("MutationTestsRateLimiting")
    .IsDependentOn("__Setup")
    .Does((context) =>
{
    RunMutationTests(File("./src/Polly.RateLimiting/Polly.RateLimiting.csproj"), File("./test/Polly.RateLimiting.Tests/Polly.RateLimiting.Tests.csproj"));
});

Task("MutationTestsExtensions")
    .IsDependentOn("__Setup")
    .Does((context) =>
{
    RunMutationTests(File("./src/Polly.Extensions/Polly.Extensions.csproj"), File("./test/Polly.Extensions.Tests/Polly.Extensions.Tests.csproj"));
});

Task("MutationTestsTesting")
    .IsDependentOn("__Setup")
    .Does((context) =>
{
    RunMutationTests(File("./src/Polly.Testing/Polly.Testing.csproj"), File("./test/Polly.Testing.Tests/Polly.Testing.Tests.csproj"));
});

Task("MutationTestsLegacy")
    .IsDependentOn("__Setup")
    .Does((context) =>
{
    RunMutationTests(File("./src/Polly/Polly.csproj"), File("./test/Polly.Specs/Polly.Specs.csproj"));
});

Task("MutationTests")
    .IsDependentOn("MutationTestsCore")
    .IsDependentOn("MutationTestsRateLimiting")
    .IsDependentOn("MutationTestsExtensions")
    .IsDependentOn("MutationTestsTesting")
    .IsDependentOn("MutationTestsLegacy");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);

//////////////////////////////////////////////////////////////////////
// HELPER FUNCTIONS
//////////////////////////////////////////////////////////////////////

string ToolsExePath(string exeFileName) {
    var exePath = System.IO.Directory.GetFiles("./tools", exeFileName, SearchOption.AllDirectories).FirstOrDefault();
    return exePath;
}

void RunMutationTests(FilePath target, FilePath testProject)
{
    var strykerPath = Context.Tools.Resolve("Stryker.CLI.dll");
    var mutationScore = XmlPeek(target, "/Project/PropertyGroup/MutationScore/text()", new XmlPeekSettings { SuppressWarning = true });
    var score = int.Parse(mutationScore);
    var targetFileName = target.GetFilename();

    Information($"Running mutation tests for '{targetFileName}'. Test Project: '{testProject}'");

    var args = $"{strykerPath} --project {targetFileName} --test-project {testProject.FullPath} --break-at {score} --config-file {strykerConfig} --output {strykerOutput}/{targetFileName}";

    var result = StartProcess("dotnet", args);
    if (result != 0)
    {
        throw new InvalidOperationException($"The mutation testing of '{targetFileName}' project failed.");
    }
}
