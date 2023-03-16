using System.Threading;
using Polly;

namespace Polly;

/// <summary>
/// Extensions for <see cref="IResilienceStrategy"/>. These extension methods are used to execute the user callbacks in various execution modes.
/// For example, you can use the same <see cref="IResilienceStrategy"/> instance to execute a synchronous callback,
/// an asynchronous callback, or a callback that returns a void result.
/// </summary>
public static partial class ResilienceStrategyExtensions
{
    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TState">The type of s associated with the execution.</typeparam>
    /// <param name="strategy">Instance of <see cref="IResilienceStrategy"/>.</param>
    /// <param name="execution">The execution callback.</param>
    /// <param name="context">The context associated with the execution.</param>
    /// <param name="state">The state associated with the execution.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    public static async ValueTask ExecuteAsync<TState>(
        this IResilienceStrategy strategy,
        Func<ResilienceContext, TState, ValueTask> execution,
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
    /// <returns>The instance of <see cref="ValueTask"/> that represents an asynchronous execution.</returns>
    public static async ValueTask ExecuteAsync(
        this IResilienceStrategy strategy,
        Func<CancellationToken, ValueTask> execution,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(strategy);
        Guard.NotNull(execution);

        var context = GetAsyncContext(cancellationToken);

        try
        {
            await strategy.ExecuteInternalAsync(
                static async (c, s) =>
                {
                    await s(c.CancellationToken).ConfigureAwait(c.ContinueOnCapturedContext);
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

    private static ResilienceContext GetAsyncContext(CancellationToken cancellationToken) => GetAsyncContext<VoidResult>(cancellationToken);

    private static void InitializeAsyncContext(ResilienceContext context) => InitializeAsyncContext<VoidResult>(context);
}
