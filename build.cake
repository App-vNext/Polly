///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// EXTERNAL NUGET TOOLS
//////////////////////////////////////////////////////////////////////

#Tool "xunit.runner.console&version=2.7.0"
#Tool "dotnet-stryker&version=4.0.0"

//////////////////////////////////////////////////////////////////////
// EXTERNAL NUGET LIBRARIES
//////////////////////////////////////////////////////////////////////

#addin nuget:?package=Cake.FileHelpers&version=6.1.3
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

Task("__RunMutationTests")
    .Does((context) =>
{
    var runMutationTests = EnvironmentVariable("RUN_MUTATION_TESTS") switch
    {
        "false" => false,
        _ => true
    };

    if (!runMutationTests)
    {
        return;
    }

    var oldDirectory = context.Environment.WorkingDirectory;
    context.Environment.WorkingDirectory = MakeAbsolute(Directory("test"));

    TestProject(File("../src/Polly.Core/Polly.Core.csproj"), File("./Polly.Core.Tests/Polly.Core.Tests.csproj"), "Polly.Core.csproj");
    TestProject(File("../src/Polly.RateLimiting/Polly.RateLimiting.csproj"), File("./Polly.RateLimiting.Tests/Polly.RateLimiting.Tests.csproj"), "Polly.RateLimiting.csproj");
    TestProject(File("../src/Polly.Extensions/Polly.Extensions.csproj"), File("./Polly.Extensions.Tests/Polly.Extensions.Tests.csproj"), "Polly.Extensions.csproj");
    TestProject(File("../src/Polly.Testing/Polly.Testing.csproj"), File("./Polly.Testing.Tests/Polly.Testing.Tests.csproj"), "Polly.Testing.csproj");
    TestProject(File("../src/Polly/Polly.csproj"), File("./Polly.Specs/Polly.Specs.csproj"), "Polly.csproj");

    context.Environment.WorkingDirectory = oldDirectory;

    void TestProject(FilePath proj, FilePath testProj, string project)
    {
        var dotNetBuildSettings = new DotNetBuildSettings
        {
            Configuration = "Debug",
            Verbosity = DotNetVerbosity.Minimal,
            NoRestore = true
        };

        DotNetBuild(proj.ToString(), dotNetBuildSettings);

        var strykerPath = Context.Tools.Resolve("Stryker.CLI.dll");
        var mutationScore = XmlPeek(proj, "/Project/PropertyGroup/MutationScore/text()", new XmlPeekSettings { SuppressWarning = true });
        var score = int.Parse(mutationScore);

        Information($"Running mutation tests for '{proj}'. Test Project: '{testProj}'");

        var args = $"{strykerPath} --project {project} --test-project {testProj.FullPath} --break-at {score} --config-file {strykerConfig} --output {strykerOutput}/{project}";

        var result = StartProcess("dotnet", args);
        if (result != 0)
        {
            throw new InvalidOperationException($"The mutation testing of '{project}' project failed.");
        }
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

//////////////////////////////////////////////////////////////////////
// BUILD TASKS
//////////////////////////////////////////////////////////////////////

Task("Build")
    .IsDependentOn("__Clean")
    .IsDependentOn("__RestoreNuGetPackages")
    .IsDependentOn("__ValidateDocs")
    .IsDependentOn("__BuildSolutions")
    .IsDependentOn("__ValidateAot")
    .IsDependentOn("__RunTests")
    .IsDependentOn("__RunMutationTests")
    .IsDependentOn("__CreateNuGetPackages");

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
    var exePath = System.IO.Directory.GetFiles("./tools", exeFileName, SearchOption.AllDirectories).FirstOrDefault();
    return exePath;
}
