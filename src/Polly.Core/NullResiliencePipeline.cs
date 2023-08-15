namespace Polly;

/// <summary>
/// A resilience pipeline that executes an user-provided callback without any additional logic.
/// </summary>
public sealed class NullResiliencePipeline : ResiliencePipeline
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="NullResiliencePipeline"/>.
    /// </summary>
    public static readonly NullResiliencePipeline Instance = new();

    private NullResiliencePipeline()
        : base(PipelineComponent.Null)
    {
    }
}
