namespace Polly.Telemetry;

/// <summary>
/// The source of resilience telemetry events.
/// </summary>
/// <remarks>
/// This class is used by the telemetry infrastructure and should not be used directly by user code.
/// </remarks>
public sealed class ResilienceTelemetrySource
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResilienceTelemetrySource"/> class.
    /// </summary>
    /// <param name="pipelineName">The pipeline name.</param>
    /// <param name="pipelineInstanceName">The pipeline instance name.</param>
    /// <param name="strategyName">The strategy name.</param>
    public ResilienceTelemetrySource(
        string? pipelineName,
        string? pipelineInstanceName,
        string? strategyName)
    {
        PipelineName = pipelineName;
        PipelineInstanceName = pipelineInstanceName;
        StrategyName = strategyName;
    }

    /// <summary>
    /// Gets the pipeline name.
    /// </summary>
    public string? PipelineName { get; }

    /// <summary>
    /// Gets the pipeline instance name.
    /// </summary>
    public string? PipelineInstanceName { get; }

    /// <summary>
    /// Gets the strategy name.
    /// </summary>
    public string? StrategyName { get; }
}

