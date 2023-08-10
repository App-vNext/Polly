namespace Polly;

/// <summary>
/// A resilience strategy that executes an user-provided callback without any additional logic.
/// </summary>
public sealed class NullResilienceStrategy : ResilienceStrategy
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="NullResilienceStrategy"/>.
    /// </summary>
    public static readonly NullResilienceStrategy Instance = new();

    private NullResilienceStrategy()
    {
    }

    /// <inheritdoc/>
    internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
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
