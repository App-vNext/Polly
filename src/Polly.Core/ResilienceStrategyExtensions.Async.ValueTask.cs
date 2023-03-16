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
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="strategy">Instance of <see cref="IResilienceStrategy"/>.</param>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    public static async ValueTask ExecuteAsync<TState>(
        this IResilienceStrategy strategy,
        Func<ResilienceContext, TState, ValueTask> callback,
        ResilienceContext context,
        TState state)
    {
        Guard.NotNull(strategy);
        Guard.NotNull(callback);
        Guard.NotNull(context);

        InitializeAsyncContext(context);

        await strategy.ExecuteInternalAsync(
            static async (context, state) =>
            {
                await state.callback(context, state.state).ConfigureAwait(context.ContinueOnCapturedContext);
                return VoidResult.Instance;
            },
            context,
            (callback, state)).ConfigureAwait(context.ContinueOnCapturedContext);
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <param name="strategy">Instance of <see cref="IResilienceStrategy"/>.</param>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the callback.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents an asynchronous callback.</returns>
    public static async ValueTask ExecuteAsync(
        this IResilienceStrategy strategy,
        Func<CancellationToken, ValueTask> callback,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(strategy);
        Guard.NotNull(callback);

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
                callback).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }

    private static ResilienceContext GetAsyncContext(CancellationToken cancellationToken) => GetAsyncContext<VoidResult>(cancellationToken);

    private static void InitializeAsyncContext(ResilienceContext context) => InitializeAsyncContext<VoidResult>(context);
}
