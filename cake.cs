#:sdk Cake.Sdk
#:package Cake.FileHelpers
#:package Newtonsoft.Json
#:property ProjectType=Test
#:property SignAssembly=false

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

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
    var projects = GetFiles("./test/**/*.csproj");

    foreach (var proj in projects)
    {
        var projectName = proj.GetFilenameWithoutExtension().ToString();
        var configLower = configuration.ToLowerInvariant();
        var outputBase = MakeAbsolute(Directory($"./artifacts/bin/{projectName}"));

        foreach (var tfmDir in GetDirectories($"{outputBase}/{configLower}_*"))
        {
            var dll = tfmDir.CombineWithFilePath($"{projectName}.dll");
            var runtimeConfig = tfmDir.CombineWithFilePath($"{projectName}.runtimeconfig.json");
            if (!FileExists(dll) || !FileExists(runtimeConfig))
                continue;

            var tfmName = tfmDir.GetDirectoryName().Substring(configLower.Length + 1);
            Information($"Testing {projectName} ({tfmName})");

            var args = new ProcessArgumentBuilder();
            FilePath executable;

            if (tfmName.StartsWith("net4"))
            {
                executable = tfmDir.CombineWithFilePath($"{projectName}.exe");
            }
            else
            {
                executable = Context.Tools.Resolve("dotnet") ?? new FilePath("dotnet");
                args.Append("exec");
                args.AppendQuoted(dll.FullPath);
            }

            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GITHUB_SHA")))
            {
                args.Append("-jUnit");
                args.AppendQuoted($"{projectName}-{tfmName}.junit.xml");
            }

            var result = StartProcess(executable, new ProcessSettings { Arguments = args });
            if (result != 0)
            {
                throw new InvalidOperationException($"Tests failed for '{projectName}' ({tfmName}).");
            }
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

Task("PatchStryker")
    .Does((_) =>
{
    PatchStrykerMtpRunner();
});

Task("MutationTestsCore")
    .IsDependentOn("__Setup")
    .IsDependentOn("PatchStryker")
    .Does((_) =>
{
    RunMutationTests(File("./src/Polly.Core/Polly.Core.csproj"), File("./test/Polly.Core.Tests/Polly.Core.Tests.csproj"));
});

Task("MutationTestsRateLimiting")
    .IsDependentOn("__Setup")
    .IsDependentOn("PatchStryker")
    .Does((_) =>
{
    RunMutationTests(File("./src/Polly.RateLimiting/Polly.RateLimiting.csproj"), File("./test/Polly.RateLimiting.Tests/Polly.RateLimiting.Tests.csproj"));
});

Task("MutationTestsExtensions")
    .IsDependentOn("__Setup")
    .IsDependentOn("PatchStryker")
    .Does((_) =>
{
    RunMutationTests(File("./src/Polly.Extensions/Polly.Extensions.csproj"), File("./test/Polly.Extensions.Tests/Polly.Extensions.Tests.csproj"));
});

Task("MutationTestsTesting")
    .IsDependentOn("__Setup")
    .IsDependentOn("PatchStryker")
    .Does((_) =>
{
    RunMutationTests(File("./src/Polly.Testing/Polly.Testing.csproj"), File("./test/Polly.Testing.Tests/Polly.Testing.Tests.csproj"));
});

Task("MutationTestsLegacy")
    .IsDependentOn("__Setup")
    .IsDependentOn("PatchStryker")
    .Does((_) =>
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

await RunTargetAsync(target);

//////////////////////////////////////////////////////////////////////
// HELPER FUNCTIONS
//////////////////////////////////////////////////////////////////////

string PatchStrykerConfig(string path, Action<Newtonsoft.Json.Linq.JObject> patch)
{
    var json = System.IO.File.ReadAllText(path);
    var config = Newtonsoft.Json.Linq.JObject.Parse(json);

    patch(config.Value<Newtonsoft.Json.Linq.JObject>("stryker-config")!);

    // Create a new file to avoid polluting the Git tree
    var tempPath = System.IO.Path.GetTempFileName();
    tempPath = System.IO.Path.ChangeExtension(tempPath, "json");

    System.IO.File.WriteAllText(tempPath, config.ToString());

    return tempPath;
}

void PatchStrykerMtpRunner()
{
    // Patches Stryker's MTP test runner to fix three bugs:
    // 1. "error" execution state not counted as test failure (only "failed" was checked)
    // 2. EveryTest() sentinel not properly accumulated when server crashes
    // 3. Static field initializer mutations not killed due to MTP process reuse
    // See: https://github.com/stryker-mutator/stryker-net/issues/3117
    // This patch can be removed once Stryker fixes these issues upstream.

    var strykerVersion = "4.14.1";
    var strykerTag = $"dotnet-stryker@{strykerVersion}";
    // Resolve relative to the directory containing cake.cs, not the process working directory
    var scriptDir = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath("cake.cs")) ?? ".";
    var patchFile = System.IO.Path.Combine(scriptDir, "eng", "stryker-mtp-runner.patch");
    var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"stryker-patch-{strykerVersion}");
    var targetDll = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".nuget", "packages", "dotnet-stryker", strykerVersion, "tools", "net8.0", "any",
        "Stryker.TestRunner.MicrosoftTestPlatform.dll");

    // Check if already patched (by presence of a marker file)
    var markerFile = targetDll + ".patched";
    if (System.IO.File.Exists(markerFile))
    {
        Information("Stryker MTP runner already patched.");
        return;
    }

    Information("Patching Stryker MTP runner...");

    // Clone stryker-net at the correct tag
    if (!System.IO.Directory.Exists(tempDir))
    {
        var cloneResult = StartProcess("git", new ProcessSettings
        {
            Arguments = $"clone --depth 1 --branch {strykerTag} https://github.com/stryker-mutator/stryker-net.git {tempDir}",
        });
        if (cloneResult != 0)
        {
            throw new InvalidOperationException("Failed to clone stryker-net repository.");
        }
    }
    else
    {
        // Reset any leftover changes from a previous failed run
        StartProcess("git", new ProcessSettings
        {
            Arguments = "checkout -- .",
            WorkingDirectory = tempDir,
        });
    }

    // Apply the patch
    var applyResult = StartProcess("git", new ProcessSettings
    {
        Arguments = $"apply {patchFile}",
        WorkingDirectory = tempDir,
    });
    if (applyResult != 0)
    {
        throw new InvalidOperationException("Failed to apply Stryker MTP runner patch.");
    }

    // Build the patched project (must run from tempDir so NuGet packages resolve correctly)
    var projectPath = System.IO.Path.Combine("src", "Stryker.TestRunner.MicrosoftTestPlatform",
        "Stryker.TestRunner.MicrosoftTestPlatform.csproj");
    var buildResult = StartProcess("dotnet", new ProcessSettings
    {
        Arguments = $"build \"{projectPath}\" -c Release",
        WorkingDirectory = tempDir,
    });
    if (buildResult != 0)
    {
        throw new InvalidOperationException("Failed to build patched Stryker MTP runner.");
    }

    // Copy the patched DLL
    var builtDll = System.IO.Path.Combine(tempDir, "src", "Stryker.TestRunner.MicrosoftTestPlatform",
        "bin", "Release", "net8.0", "Stryker.TestRunner.MicrosoftTestPlatform.dll");

    System.IO.File.Copy(builtDll, targetDll, overwrite: true);
    System.IO.File.WriteAllText(markerFile, $"Patched from {System.IO.Path.GetFileName(patchFile)} at {DateTime.UtcNow:O}");

    Information("Stryker MTP runner patched successfully.");
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
            var reporters = config.Value<Newtonsoft.Json.Linq.JArray>("reporters")!;
            reporters.Add("dashboard");

            config["reporters"] = reporters;
            config["project-info"] = new Newtonsoft.Json.Linq.JObject
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

    var extraArgs = "";
    if (moduleName == "Polly")
    {
        // Enable debug logging for legacy project to diagnose MTP runner deadlock on CI
        extraArgs = " --verbosity debug --log-to-file";
    }

    var args = $"stryker --project {targetFileName} --test-project {testProject.GetFilename()} --break-at {score} --config-file {strykerConfigPath} --output {strykerOutput}/{targetFileName}{extraArgs}";

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
