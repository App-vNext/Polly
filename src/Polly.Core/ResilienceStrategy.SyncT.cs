using System.Runtime.ExceptionServices;

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
    /// <returns>An instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
    public TResult Execute<TResult, TState>(
        Func<ResilienceContext, TState, TResult> callback,
        ResilienceContext context,
        TState state)
    {
        Guard.NotNull(callback);
        Guard.NotNull(context);

        InitializeSyncContext<TResult>(context);

        return ExecuteCoreSync(
           static (context, state) =>
           {
               try
               {
                   var result = state.callback(context, state.state);
                   return new Outcome<TResult>(result);
               }
               catch (Exception e)
               {
                   return new Outcome<TResult>(ExceptionDispatchInfo.Capture(e));
               }
           },
           context,
           (callback, state)).GetResultOrRethrow();
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <returns>An instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
    public TResult Execute<TResult>(
        Func<ResilienceContext, TResult> callback,
        ResilienceContext context)
    {
        Guard.NotNull(callback);
        Guard.NotNull(context);

        InitializeSyncContext<TResult>(context);

        return ExecuteCoreSync(
            static (context, state) =>
            {
                try
                {
                    var result = state(context);
                    return new Outcome<TResult>(result);
                }
                catch (Exception e)
                {
                    return new Outcome<TResult>(ExceptionDispatchInfo.Capture(e));
                }
            },
            context,
            callback).GetResultOrRethrow();
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the callback.</param>
    /// <returns>An instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public TResult Execute<TResult>(
        Func<CancellationToken, TResult> callback,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(callback);

        var context = GetSyncContext<TResult>(cancellationToken);

        try
        {
            return ExecuteCoreSync(
                static (context, state) =>
                {
                    try
                    {
                        var result = state(context.CancellationToken);
                        return new Outcome<TResult>(result);
                    }
                    catch (Exception e)
                    {
                        return new Outcome<TResult>(ExceptionDispatchInfo.Capture(e));
                    }
                },
                context,
                callback).GetResultOrRethrow();
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
    /// <returns>An instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public TResult Execute<TResult>(Func<TResult> callback)
    {
        Guard.NotNull(callback);

        var context = GetSyncContext<TResult>(CancellationToken.None);

        try
        {
            return ExecuteCoreSync(
                static (_, state) =>
                {
                    try
                    {
                        var result = state();
                        return new Outcome<TResult>(result);
                    }
                    catch (Exception e)
                    {
                        return new Outcome<TResult>(ExceptionDispatchInfo.Capture(e));
                    }
                },
                context,
                callback).GetResultOrRethrow();
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
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <returns>An instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public TResult Execute<TResult, TState>(Func<TState, TResult> callback, TState state)
    {
        Guard.NotNull(callback);

        var context = GetSyncContext<TResult>(CancellationToken.None);

        try
        {
            return ExecuteCoreSync(
                static (_, state) =>
                {
                    try
                    {
                        var result = state.callback(state.state);
                        return new Outcome<TResult>(result);
                    }
                    catch (Exception e)
                    {
                        return new Outcome<TResult>(ExceptionDispatchInfo.Capture(e));
                    }
                },
                context,
                (callback, state)).GetResultOrRethrow();
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
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the callback.</param>
    /// <returns>An instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public TResult Execute<TResult, TState>(
        Func<TState, CancellationToken, TResult> callback,
        TState state,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(callback);

        var context = GetSyncContext<TResult>(cancellationToken);

        try
        {
            return ExecuteCoreSync(
                static (context, state) =>
                {
                    try
                    {
                        var result = state.callback(state.state, context.CancellationToken);
                        return new Outcome<TResult>(result);
                    }
                    catch (Exception e)
                    {
                        return new Outcome<TResult>(ExceptionDispatchInfo.Capture(e));
                    }
                },
                context,
                (callback, state)).GetResultOrRethrow();
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
