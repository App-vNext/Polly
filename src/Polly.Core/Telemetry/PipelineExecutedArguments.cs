namespace Polly.Telemetry;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments that indicate the pipeline execution started.
/// </summary>
public readonly struct PipelineExecutedArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineExecutedArguments"/> struct.
    /// </summary>
    /// <param name="duration">The pipeline execution duration.</param>
    public PipelineExecutedArguments(TimeSpan duration) => Duration = duration;

    /// <summary>
    /// Gets the pipeline execution duration.
    /// </summary>
    public TimeSpan Duration { get; }
}
