namespace Polly.Telemetry;

/// <summary>
/// Arguments that indicate the pipeline execution started.
/// </summary>
public sealed class PipelineExecutingArguments
{
    internal static readonly PipelineExecutingArguments Instance = new();
}
