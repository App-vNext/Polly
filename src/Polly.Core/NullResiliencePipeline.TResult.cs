namespace Polly;

/// <summary>
/// A resilience pipeline that executes an user-provided callback without any additional logic.
/// </summary>
/// <typeparam name="TResult">The type of result this pipeline handles.</typeparam>
public sealed class NullResiliencePipeline<TResult> : ResiliencePipeline<TResult>
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="NullResiliencePipeline"/>.
    /// </summary>
    public static readonly NullResiliencePipeline<TResult> Instance = new();

    private NullResiliencePipeline()
        : base(NullResiliencePipeline.Instance)
    {
    }
}
