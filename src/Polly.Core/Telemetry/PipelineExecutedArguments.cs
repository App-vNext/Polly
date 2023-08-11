namespace Polly.Telemetry;

/// <summary>
/// Arguments that indicate the pipeline execution started.
/// </summary>
public sealed partial class PipelineExecutedArguments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineExecutedArguments"/> class.
    /// </summary>
    /// <param name="duration">The pipeline execution duration.</param>
    public PipelineExecutedArguments(TimeSpan duration) => Duration = duration;

    internal PipelineExecutedArguments()
    {
    }

    /// <summary>
    /// Gets the pipeline execution duration.
    /// </summary>
    public TimeSpan Duration { get; internal set; }
}
