///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// EXTERNAL NUGET LIBRARIES
//////////////////////////////////////////////////////////////////////

#addin nuget:?package=Cake.FileHelpers&version=7.0.0
#addin nuget:?package=Newtonsoft.Json&version=13.0.3

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var projectName = "Polly";

var solutions = GetFiles("./**/*.slnx");
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
    if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
    {
        // Nothing to clean in CI
        return;
    }

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
        loggers =
        [
            "junit;LogFilePath=junit.xml",
            "GitHubActions;report-warnings=false",
        ];
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
        System.IO.Path.Combine(srcDir, "Polly.Caching", "Polly.Caching.csproj"),
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

Task("MutationTestsCaching")
  .IsDependentOn("__Setup")
  .Does((context) =>
{
  RunMutationTests(
    File("./src/Polly.Caching/Polly.Caching.csproj"),
    File("./test/Polly.Caching.Tests/Polly.Caching.Tests.csproj"));
});

Task("MutationTests")
    .IsDependentOn("MutationTestsCore")
    .IsDependentOn("MutationTestsRateLimiting")
    .IsDependentOn("MutationTestsExtensions")
    .IsDependentOn("MutationTestsTesting")
    .IsDependentOn("MutationTestsLegacy")
    .IsDependentOn("MutationTestsCaching");

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

string PatchStrykerConfig(string path, Action<Newtonsoft.Json.Linq.JObject> patch)
{
    var json = System.IO.File.ReadAllText(path);
    var config = Newtonsoft.Json.Linq.JObject.Parse(json);

    patch(config.Value<Newtonsoft.Json.Linq.JObject>("stryker-config"));

    // Create a new file to avoid polluting the Git tree
    var tempPath = System.IO.Path.GetTempFileName();
    tempPath = System.IO.Path.ChangeExtension(tempPath, "json");

    System.IO.File.WriteAllText(tempPath, config.ToString());

    return tempPath;
}

void RunMutationTests(FilePath target, FilePath testProject)
{
    var mutationScore = XmlPeek(target, "/Project/PropertyGroup/MutationScore/text()", new XmlPeekSettings { SuppressWarning = true });
    var score = int.Parse(mutationScore);
    var targetFileName = target.GetFilename();
    var isGitHubActions = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";
    var dashboardUrl = string.Empty;
    var moduleName = target.GetFilenameWithoutExtension().ToString();
    var strykerConfigPath = strykerConfig.FullPath;

    if (moduleName == "Polly.Testing")
    {
        strykerConfigPath = PatchStrykerConfig(strykerConfigPath, (config) => config.Remove("ignore-mutations"));
    }

    if (isGitHubActions &&
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("STRYKER_DASHBOARD_API_KEY")))
    {
        var projectName = $"github.com/{Environment.GetEnvironmentVariable("GITHUB_REPOSITORY")}";
        var version = Environment.GetEnvironmentVariable("GITHUB_REF_NAME");

        dashboardUrl = $"https://dashboard.stryker-mutator.io/reports/{projectName}/{version}#mutant/{moduleName}";

        strykerConfigPath = PatchStrykerConfig(strykerConfigPath, (config) =>
        {
            var reporters = config.Value<Newtonsoft.Json.Linq.JArray>("reporters");
            reporters.Add("dashboard");

            config["reporters"] = reporters;
            config["project-info"] = new Newtonsoft.Json.Linq.JObject()
            {
                ["module"] = moduleName,
                ["name"] = projectName,
                ["version"] = version
            };
        });

        Information("Configured Stryker dashboard.");
        Information($"Mutation report will be available at {dashboardUrl}");
    }

    Information($"Running mutation tests for '{targetFileName}'. Test Project: '{testProject}'");

    var args = $"stryker --project {targetFileName} --test-project {testProject.GetFilename()} --break-at {score} --config-file {strykerConfigPath} --output {strykerOutput}/{targetFileName}";

    var testProjectDir = testProject.GetDirectory();
    var result = StartProcess("dotnet", new ProcessSettings
    {
        Arguments = args,
        WorkingDirectory = testProjectDir.FullPath,
    });

    if (result != 0)
    {
        throw new InvalidOperationException($"The mutation testing of '{targetFileName}' project failed.");
    }

    var stepSummary = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");
    var markdownSummary = System.IO.Path.Combine(
        strykerOutput.FullPath,
        targetFileName.ToString(),
        "reports",
        "mutation-report.md");

    if (!string.IsNullOrWhiteSpace(stepSummary) && System.IO.File.Exists(markdownSummary))
    {
        var markdown = System.IO.File.ReadAllText(markdownSummary);

        if (!string.IsNullOrWhiteSpace(dashboardUrl))
        {
            markdown += $"\n\n## Mutation Dashboard\n\n[View Mutation Report :notebook:]({dashboardUrl})";
        }

        System.IO.File.WriteAllText(stepSummary, markdown);
    }
}
