using System.Diagnostics.Metrics;

namespace Polly.Telemetry;

internal sealed class TelemetrySource
{
    internal const string Name = "Polly";

    public static readonly TelemetrySource Instance = new();

    private TelemetrySource()
    {
        var version = GetVersion();

        // In .NET 10+ we could use ActivitySourceOptions and
        // MeterOptions to create these and provide a schema URL.
        ActivitySource = new(Name, version);
        Meter = new(Name, version);
    }

    public ActivitySource ActivitySource { get; }

    public Meter Meter { get; }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static string GetVersion()
    {
        var informationalVersion = typeof(TelemetrySource).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

#if NET
        var index = informationalVersion!.IndexOf('+', StringComparison.Ordinal);
#else
        var index = informationalVersion!.IndexOf('+');
#endif

        if (index > 0)
        {
            // Trim off any Git metadata
            informationalVersion = informationalVersion.Substring(0, index);
        }

#if NET
        index = informationalVersion!.IndexOf('-', StringComparison.Ordinal);
#else
        index = informationalVersion!.IndexOf('-');
#endif

        if (index > 0)
        {
            // Trim off any prerelease version
            informationalVersion = informationalVersion.Substring(0, index);
        }

        return informationalVersion;
    }
}
