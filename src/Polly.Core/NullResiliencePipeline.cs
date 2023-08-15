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

    internal class NullStrategy : ResilienceStrategy
    {
        protected internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state)
        {
            Guard.NotNull(callback);
            Guard.NotNull(context);

            context.AssertInitialized();

            return callback(context, state);
        }
    }
}
