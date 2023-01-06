///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// EXTERNAL NUGET TOOLS
//////////////////////////////////////////////////////////////////////

#Tool "GitVersion.CommandLine&version=5.8.1"
#Tool "xunit.runner.console&version=2.4.1"

//////////////////////////////////////////////////////////////////////
// EXTERNAL NUGET LIBRARIES
//////////////////////////////////////////////////////////////////////

#addin nuget:?package=Cake.FileHelpers&version=5.0.0
#addin nuget:?package=Cake.Yaml&version=4.0.0
#addin nuget:?package=Newtonsoft.Json&version=13.0.1
#addin nuget:?package=YamlDotNet&version=11.2.1

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
var nupkgDestDir = System.IO.Path.Combine(artifactsDir, Directory("nuget-package"));

// GitVersion
var gitVersionPath = ToolsExePath("GitVersion.exe");
var gitVersionConfigFilePath = "./GitVersionConfig.yaml";
Dictionary<string, object> gitVersionOutput;

// Versioning
string nugetVersion;
string assemblyVersion;
string assemblySemver;

///////////////////////////////////////////////////////////////////////////////
// INNER CLASSES
///////////////////////////////////////////////////////////////////////////////
class GitVersionConfigYaml
{
    public string NextVersion { get; set; }
}

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
    DirectoryPath[] cleanDirectories = new DirectoryPath[]
    {
        testResultsDir,
        nupkgDestDir,
        artifactsDir
  	};

    CleanDirectories(cleanDirectories);

    foreach(var path in cleanDirectories) { EnsureDirectoryExists(path); }

    foreach(var path in solutionPaths)
    {
        Information("Cleaning {0}", path);
        DotNetClean(path.ToString());
    }
});

Task("__RestoreNuGetPackages")
    .Does(() =>
{
    foreach(var solution in solutions)
    {
        Information("Restoring NuGet Packages for {0}", solution);
        DotNetRestore(solution.ToString());
    }
});

Task("__UpdateAssemblyVersionInformation")
    .Does(() =>
{
    var gitVersionSettings = new ProcessSettings()
        .SetRedirectStandardOutput(true);

    try
    {
        IEnumerable<string> outputLines;
        StartProcess(gitVersionPath, gitVersionSettings, out outputLines);

        var output = string.Join("\n", outputLines);
        gitVersionOutput = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(output);
    }
    catch
    {
        Information("Error reading git version information. Build may be running outside of a git repo. Falling back to version specified in " + gitVersionConfigFilePath);

        string gitVersionYamlString = System.IO.File.ReadAllText(gitVersionConfigFilePath);
        GitVersionConfigYaml deserialized = DeserializeYaml<GitVersionConfigYaml>(gitVersionYamlString.Replace("next-version", "NextVersion"));
        string gitVersionConfig = deserialized.NextVersion;

        gitVersionOutput = new Dictionary<string, object>
        {
            { "NuGetVersion", gitVersionConfig + "-NotFromGitRepo" },
            { "FullSemVer", gitVersionConfig },
            { "AssemblySemVer", gitVersionConfig },
            { "Major", gitVersionConfig.Split('.')[0] },
        };
    }

    Information("");
    Information("Obtained raw version info for package versioning:");
    Information("NuGetVersion -> {0}", gitVersionOutput["NuGetVersion"]);
    Information("FullSemVer -> {0}", gitVersionOutput["FullSemVer"]);
    Information("AssemblySemVer -> {0}", gitVersionOutput["AssemblySemVer"]);

    nugetVersion = gitVersionOutput["NuGetVersion"].ToString();
    assemblyVersion = gitVersionOutput["Major"].ToString() + ".0.0.0";
    assemblySemver = gitVersionOutput["AssemblySemVer"].ToString();

    Information("");
    Information("Mapping versioning information to:");
    Information("NuGet package version -> {0}", nugetVersion);
    Information("AssemblyVersion -> {0}", assemblyVersion);
    Information("AssemblyFileVersion -> {0}", assemblySemver);
    Information("AssemblyInformationalVersion -> {0}", assemblySemver);
});

Task("__UpdateDotNetStandardAssemblyVersionNumber")
    .WithCriteria(() => AppVeyor.IsRunningOnAppVeyor)
    .Does(() =>
{
    Information("Updating Assembly Version Information");

    var attributeToValueMap = new Dictionary<string, string>()
    {
        { "AssemblyVersion", assemblyVersion },
        { "FileVersion", assemblySemver },
        { "InformationalVersion", assemblySemver },
        { "Version", nugetVersion },
        { "PackageVersion", nugetVersion },
        { "ContinuousIntegrationBuild", "true" },
    };

    var csproj = File("./src/" + projectName + "/" + projectName + ".csproj");

    foreach(var attributeMap in attributeToValueMap)
    {
        var attribute = attributeMap.Key;
        var value = attributeMap.Value;

        var replacedFiles = ReplaceRegexInFiles(csproj, $@"\<{attribute}\>[^\<]*\</{attribute}\>", $@"<{attribute}>{value}</{attribute}>");
        if (!replacedFiles.Any())
        {
            throw new Exception($"{attribute} version could not be updated in {csproj}.");
        }
    }
});

Task("__BuildSolutions")
    .Does(() =>
{
    foreach(var solution in solutions)
    {
        Information("Building {0}", solution);

        var dotNetCoreBuildSettings = new DotNetBuildSettings
        {
            Configuration = configuration,
            Verbosity = DotNetCoreVerbosity.Minimal,
            NoRestore = true,
            MSBuildSettings = new DotNetMSBuildSettings { TreatAllWarningsAs = MSBuildTreatAllWarningsAs.Error },
        };

        DotNetBuild(solution.ToString(), dotNetCoreBuildSettings);
    }
});

Task("__RunTests")
    .Does(() =>
{
    var loggers = Array.Empty<string>();

    if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GITHUB_SHA")))
    {
        loggers = new[] { "GitHubActions;report-warnings=false" };
    }

    foreach(var specsProj in GetFiles("./src/**/*.Specs.csproj"))
    {
        DotNetTest(specsProj.FullPath, new DotNetTestSettings
        {
            Configuration = configuration,
            Loggers = loggers,
            NoBuild = true,
        });
    }
});

Task("__CreateSignedNuGetPackage")
    .Does(() =>
{
    var packageName = projectName;

    Information("Building {0}.{1}.nupkg", packageName, nugetVersion);

    var dotNetCorePackSettings = new DotNetPackSettings
    {
        Configuration = configuration,
        NoBuild = true,
        OutputDirectory = nupkgDestDir,
    };

    DotNetPack(System.IO.Path.Combine(srcDir, projectName + ".sln"), dotNetCorePackSettings);
});

//////////////////////////////////////////////////////////////////////
// BUILD TASKS
//////////////////////////////////////////////////////////////////////

Task("Build")
    .IsDependentOn("__Clean")
    .IsDependentOn("__RestoreNuGetPackages")
    .IsDependentOn("__UpdateAssemblyVersionInformation")
    .IsDependentOn("__UpdateDotNetStandardAssemblyVersionNumber")
    .IsDependentOn("__BuildSolutions")
    .IsDependentOn("__RunTests")
    .IsDependentOn("__CreateSignedNuGetPackage");

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
