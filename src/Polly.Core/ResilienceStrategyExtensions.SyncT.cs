using Polly;

namespace Polly;

public static partial class ResilienceStrategyExtensions
{
    /// <summary>
    /// Executes the <paramref name="execution"/> callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the execution callback.</typeparam>
    /// <typeparam name="TState">The type of state associated with the execution.</typeparam>
    /// <param name="strategy">The instance of <see cref="IResilienceStrategy"/>.</param>
    /// <param name="execution">The execution callback.</param>
    /// <param name="context">The context associated with the execution.</param>
    /// <param name="state">The state associated with the execution.</param>
    /// <returns>An instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    public static TResult Execute<TResult, TState>(
        this IResilienceStrategy strategy,
        Func<ResilienceContext, TState, TResult> execution,
        ResilienceContext context,
        TState state)
    {
        Guard.NotNull(strategy);
        Guard.NotNull(execution);
        Guard.NotNull(context);

        InitializeSyncContext<TResult>(context);

        return strategy.ExecuteInternalAsync((c, s) => new ValueTask<TResult>(s.execution(c, s.state)), context, (execution, state))
                       .GetResult();
    }

    /// <summary>
    /// Executes the <paramref name="execution"/> callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the execution callback.</typeparam>
    /// <param name="strategy">The instance of <see cref="IResilienceStrategy"/>.</param>
    /// <param name="execution">The execution callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the execution.</param>
    /// <returns>An instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    public static TResult Execute<TResult>(
        this IResilienceStrategy strategy,
        Func<CancellationToken, TResult> execution,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(strategy);
        Guard.NotNull(execution);

        var context = GetSyncContext<TResult>(cancellationToken);

        try
        {
            return strategy.ExecuteInternalAsync((c, state) => new ValueTask<TResult>(state(c.CancellationToken)), context, execution)
                           .GetResult();
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }

    private static ResilienceContext GetSyncContext<TResult>(CancellationToken cancellationToken)
    {
        var context = ResilienceContext.Get();
        context.CancellationToken = cancellationToken;

        InitializeSyncContext<TResult>(context);

        return context;
    }

    private static void InitializeSyncContext<TResult>(ResilienceContext context) => context.Initialize<TResult>(isSynchronous: true);
}
