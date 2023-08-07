namespace Polly;

/// <summary>
/// A resilience strategy that executes an user-provided callback without any additional logic.
/// </summary>
/// <typeparam name="TResult">The type of result this strategy handles.</typeparam>
public sealed class NullResilienceStrategy<TResult> : ResilienceStrategy<TResult>
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="NullResilienceStrategy"/>.
    /// </summary>
    public static readonly NullResilienceStrategy<TResult> Instance = new();

    private NullResilienceStrategy()
    {
    }

    protected override ValueTask<Outcome<TResult>> ExecuteCore<TState>(
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
