using System.Threading;
using Polly;

namespace Polly;

public abstract partial class ResilienceStrategy
{
    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <returns>The instance of <see cref="Task"/> that represents the asynchronous execution.</returns>
    public async Task ExecuteAsync<TState>(
        Func<ResilienceContext, TState, Task> callback,
        ResilienceContext context,
        TState state)
    {
        Guard.NotNull(callback);
        Guard.NotNull(context);

        InitializeAsyncContext(context);

        await ExecuteCoreAsync(
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
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the callback.</param>
    /// <returns>The instance of <see cref="Task"/> that represents an asynchronous callback.</returns>
    public async Task ExecuteAsync(
        Func<CancellationToken, Task> callback,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(callback);

        var context = GetAsyncContext(cancellationToken);

        try
        {
            await ExecuteCoreAsync(
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
}
