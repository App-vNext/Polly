namespace Polly;

public static partial class ResilienceStrategyExtensions
{
    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the execution callback.</typeparam>
    /// <typeparam name="TState">The type of s associated with the execution.</typeparam>
    /// <param name="strategy">The instance of <see cref="IResilienceStrategy"/>.</param>
    /// <param name="execution">The execution callback.</param>
    /// <param name="context">The context associated with the execution.</param>
    /// <param name="state">The state associated with the execution.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    public static ValueTask<TResult> ExecuteAsync<TResult, TState>(
        this IResilienceStrategy strategy,
        Func<ResilienceContext, TState, ValueTask<TResult>> execution,
        ResilienceContext context,
        TState state)
    {
        Guard.NotNull(strategy);
        Guard.NotNull(execution);
        Guard.NotNull(context);

        InitializeAsyncContext<TResult>(context);

        return strategy.ExecuteInternalAsync(execution, context, state);
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the execution callback.</typeparam>
    /// <param name="strategy">The instance of <see cref="IResilienceStrategy"/>.</param>
    /// <param name="execution">The execution callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the execution.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    public static async ValueTask<TResult> ExecuteAsync<TResult>(
        this IResilienceStrategy strategy,
        Func<CancellationToken, ValueTask<TResult>> execution,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(strategy);
        Guard.NotNull(execution);

        var context = GetAsyncContext<TResult>(cancellationToken);

        try
        {
            return await strategy.ExecuteInternalAsync(static (c, s) => s(c.CancellationToken), context, execution)
                                 .ConfigureAwait(context.ContinueOnCapturedContext);
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }

    private static ResilienceContext GetAsyncContext<TResult>(CancellationToken cancellationToken)
    {
        var context = ResilienceContext.Get();
        context.CancellationToken = cancellationToken;

        InitializeAsyncContext<TResult>(context);

        return context;
    }

    private static void InitializeAsyncContext<TResult>(ResilienceContext context) => context.Initialize<TResult>(isSynchronous: false);
}
