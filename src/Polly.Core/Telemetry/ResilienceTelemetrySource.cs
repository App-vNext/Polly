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
    /// <param name="pipelineName">The builder name.</param>
    /// <param name="pipelineInstanceName">The builder instance name.</param>
    /// <param name="builderProperties">The builder properties.</param>
    /// <param name="strategyName">The strategy name.</param>
    public ResilienceTelemetrySource(
        string? pipelineName,
        string? pipelineInstanceName,
        ResilienceProperties builderProperties,
        string? strategyName)
    {
        PipelineName = pipelineName;
        PipelineInstanceName = pipelineInstanceName;
        BuilderProperties = builderProperties;
        StrategyName = strategyName;
    }

    /// <summary>
    /// Gets the builder name.
    /// </summary>
    public string? PipelineName { get; }

    /// <summary>
    /// Gets the builder instance name.
    /// </summary>
    public string? PipelineInstanceName { get; }

    /// <summary>
    /// Gets the builder properties.
    /// </summary>
    public ResilienceProperties BuilderProperties { get; }

    /// <summary>
    /// Gets the strategy name.
    /// </summary>
    public string? StrategyName { get; }
}

