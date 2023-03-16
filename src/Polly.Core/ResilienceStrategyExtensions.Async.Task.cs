using System.Threading;
using Polly;

namespace Polly;

public static partial class ResilienceStrategyExtensions
{
    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TState">The type of state associated with the execution.</typeparam>
    /// <param name="strategy">Instance of <see cref="IResilienceStrategy"/>.</param>
    /// <param name="execution">The execution callback.</param>
    /// <param name="context">The context associated with the execution.</param>
    /// <param name="state">The state associated with the execution.</param>
    /// <returns>The instance of <see cref="Task"/> that represents the asynchronous execution.</returns>
    public static async Task ExecuteAsTaskAsync<TState>(
        this IResilienceStrategy strategy,
        Func<ResilienceContext, TState, Task> execution,
        ResilienceContext context,
        TState state)
    {
        Guard.NotNull(strategy);
        Guard.NotNull(execution);
        Guard.NotNull(context);

        InitializeAsyncContext(context);

        await strategy.ExecuteInternalAsync(
            static async (c, s) =>
            {
                await s.execution(c, s.state).ConfigureAwait(c.ContinueOnCapturedContext);
                return VoidResult.Instance;
            },
            context,
            (execution, state)).ConfigureAwait(context.ContinueOnCapturedContext);
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <param name="strategy">Instance of <see cref="IResilienceStrategy"/>.</param>
    /// <param name="execution">The execution callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the execution.</param>
    /// <returns>The instance of <see cref="Task"/> that represents an asynchronous execution.</returns>
    public static async Task ExecuteAsTaskAsync(
        this IResilienceStrategy strategy,
        Func<CancellationToken, Task> execution,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(strategy);
        Guard.NotNull(execution);

        var context = GetAsyncContext(cancellationToken);

        try
        {
            await strategy.ExecuteInternalAsync(
                static async (context, state) =>
                {
                    await state(context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext);
                    return VoidResult.Instance;
                },
                context,
                execution).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }
}
