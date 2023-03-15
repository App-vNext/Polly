namespace Polly;

/// <summary>
/// A resilience strategy that just executes the execution callback without any additional logic.
/// </summary>
public sealed class NullResilienceStrategy : IResilienceStrategy
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="NullResilienceStrategy"/>.
    /// </summary>
    public static readonly NullResilienceStrategy Instance = new();

    private NullResilienceStrategy()
    {
    }

    /// <inheritdoc/>
    ValueTask<TResult> IResilienceStrategy.ExecuteInternalAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> execution, ResilienceContext context, TState state)
    {
        Guard.NotNull(execution);
        Guard.NotNull(context);

        context.AssertInitialized();

        return execution(context, state);
    }
}
