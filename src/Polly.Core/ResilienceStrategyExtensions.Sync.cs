using Polly;

namespace Polly;

public static partial class ResilienceStrategyExtensions
{
    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="strategy">The instance of <see cref="IResilienceStrategy"/>.</param>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    public static void Execute<TState>(
        this IResilienceStrategy strategy,
        Action<ResilienceContext, TState> callback,
        ResilienceContext context,
        TState state)
    {
        Guard.NotNull(strategy);
        Guard.NotNull(callback);
        Guard.NotNull(context);

        InitializeSyncContext(context);

        strategy.ExecuteInternalAsync(
            static (context, state) =>
            {
                state.callback(context, state.state);
                return new ValueTask<VoidResult>(VoidResult.Instance);
            },
            context,
            (callback, state)).GetResult();
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <param name="strategy">The instance of <see cref="IResilienceStrategy"/>.</param>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the callback.</param>
    public static void Execute(
        this IResilienceStrategy strategy,
        Action<CancellationToken> callback,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(strategy);
        Guard.NotNull(callback);

        var context = GetSyncContext(cancellationToken);

        try
        {
            strategy.ExecuteInternalAsync(
                static (context, state) =>
                {
                    state(context.CancellationToken);
                    return new ValueTask<VoidResult>(VoidResult.Instance);
                },
                context,
                callback).GetResult();
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }

    private static ResilienceContext GetSyncContext(CancellationToken cancellationToken) => GetSyncContext<VoidResult>(cancellationToken);

    private static void InitializeSyncContext(ResilienceContext context) => InitializeSyncContext<VoidResult>(context);
}
