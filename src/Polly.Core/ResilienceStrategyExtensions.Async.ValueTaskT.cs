namespace Polly;

public static partial class ResilienceStrategyExtensions
{
    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the callback.</typeparam>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="strategy">The instance of <see cref="IResilienceStrategy"/>.</param>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    public static ValueTask<TResult> ExecuteAsync<TResult, TState>(
        this IResilienceStrategy strategy,
        Func<ResilienceContext, TState, ValueTask<TResult>> callback,
        ResilienceContext context,
        TState state)
    {
        Guard.NotNull(strategy);
        Guard.NotNull(callback);
        Guard.NotNull(context);

        InitializeAsyncContext<TResult>(context);

        return strategy.ExecuteInternalAsync(callback, context, state);
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the callback.</typeparam>
    /// <param name="strategy">The instance of <see cref="IResilienceStrategy"/>.</param>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the callback.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    public static async ValueTask<TResult> ExecuteAsync<TResult>(
        this IResilienceStrategy strategy,
        Func<CancellationToken, ValueTask<TResult>> callback,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(strategy);
        Guard.NotNull(callback);

        var context = GetAsyncContext<TResult>(cancellationToken);

        try
        {
            return await strategy.ExecuteInternalAsync(static (context, state) => state(context.CancellationToken), context, callback)
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
