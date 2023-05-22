using System.Runtime.ExceptionServices;
using Polly.Strategy;

namespace Polly;

#pragma warning disable CA1031 // Do not catch general exception types

public abstract partial class ResilienceStrategy
{
    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the callback.</typeparam>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
    public ValueTask<TResult> ExecuteAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<TResult>> callback,
        ResilienceContext context,
        TState state)
    {
        Guard.NotNull(callback);
        Guard.NotNull(context);

        InitializeAsyncContext<TResult>(context);

        return ExecuteCoreAndUnwrapAsync(
            static async (context, state) =>
            {
                try
                {
                    return new Outcome<TResult>(await state.callback(context, state.state).ConfigureAwait(context.ContinueOnCapturedContext));
                }
                catch (Exception e)
                {
                    return new Outcome<TResult>(e, ExceptionDispatchInfo.Capture(e));
                }
            },
            context,
            (callback, state));
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
    public ValueTask<TResult> ExecuteAsync<TResult>(
        Func<ResilienceContext, ValueTask<TResult>> callback,
        ResilienceContext context)
    {
        Guard.NotNull(callback);
        Guard.NotNull(context);

        InitializeAsyncContext<TResult>(context);

        return ExecuteCoreAndUnwrapAsync(
            static async (context, state) =>
            {
                try
                {
                    return new Outcome<TResult>(await state(context).ConfigureAwait(context.ContinueOnCapturedContext));
                }
                catch (Exception e)
                {
                    return new Outcome<TResult>(e, ExceptionDispatchInfo.Capture(e));
                }
            },
            context,
            callback);
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the callback.</typeparam>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the callback.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public async ValueTask<TResult> ExecuteAsync<TResult, TState>(
        Func<TState, CancellationToken, ValueTask<TResult>> callback,
        TState state,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(callback);

        var context = GetAsyncContext<TResult>(cancellationToken);

        try
        {
            return await ExecuteCoreAndUnwrapAsync(
                static async (context, state) =>
                {
                    try
                    {
                        return new Outcome<TResult>(await state.callback(state.state, context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext));
                    }
                    catch (Exception e)
                    {
                        return new Outcome<TResult>(e, ExceptionDispatchInfo.Capture(e));
                    }
                },
                context,
                (callback, state)).ConfigureAwait(context.ContinueOnCapturedContext);
        }
        finally
        {
            ResilienceContext.Return(context);
        }
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the callback.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public async ValueTask<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, ValueTask<TResult>> callback,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(callback);

        var context = GetAsyncContext<TResult>(cancellationToken);

        try
        {
            return await ExecuteCoreAndUnwrapAsync(
                static async (context, state) =>
                {
                    try
                    {
                        return new Outcome<TResult>(await state(context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext));
                    }
                    catch (Exception e)
                    {
                        return new Outcome<TResult>(e, ExceptionDispatchInfo.Capture(e));
                    }
                },
                context,
                callback).ConfigureAwait(context.ContinueOnCapturedContext);
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
