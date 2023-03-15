using Polly;

namespace Polly;

public static partial class ResilienceStrategyExtensions
{
    /// <summary>
    /// Executes the <paramref name="execution"/> callback.
    /// </summary>
    /// <typeparam name="TState">The type of state associated with the execution.</typeparam>
    /// <param name="strategy">The instance of <see cref="IResilienceStrategy"/>.</param>
    /// <param name="execution">The execution callback.</param>
    /// <param name="context">The context associated with the execution.</param>
    /// <param name="state">The state associated with the execution.</param>
    /// <remarks>This method should not be used directly. Instead, use the dedicated extensions to execute the user provided callback.</remarks>
    public static void Execute<TState>(
        this IResilienceStrategy strategy,
        Action<ResilienceContext, TState> execution,
        ResilienceContext context,
        TState state)
    {
        Guard.NotNull(strategy);
        Guard.NotNull(execution);
        Guard.NotNull(context);

        InitializeSyncContext(context);

        strategy.ExecuteInternalAsync(
            static (c, s) =>
            {
                s.execution(c, s.state);
                return new ValueTask<VoidResult>(VoidResult.Instance);
            },
            context,
            (execution, state)).GetResult();
    }

    /// <summary>
    /// Executes the <paramref name="execution"/> callback.
    /// </summary>
    /// <param name="strategy">The instance of <see cref="IResilienceStrategy"/>.</param>
    /// <param name="execution">The execution callback.</param>
    /// <remarks>This method should not be used directly. Instead, use the dedicated extensions to execute the user provided callback.</remarks>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the execution.</param>
    public static void Execute(
        this IResilienceStrategy strategy,
        Action<CancellationToken> execution,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(strategy);
        Guard.NotNull(execution);

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
                execution).GetResult();
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }

    private static ResilienceContext GetSyncContext(CancellationToken cancellationToken) => GetSyncContext<VoidResult>(cancellationToken);

    private static void InitializeSyncContext(ResilienceContext context) => InitializeSyncContext<VoidResult>(context);
}
