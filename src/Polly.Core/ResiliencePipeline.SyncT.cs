namespace Polly;

#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable RS0027 // API with optional parameter(s) should have the most parameters amongst its public overloads

public partial class ResiliencePipeline
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

        return Component.ExecuteCoreSync(
           [DebuggerDisableUserUnhandledExceptions] static (context, state) =>
           {
               try
               {
                   var result = state.callback(context, state.state);
                   return Outcome.FromResult(result);
               }
               catch (Exception e)
               {
                   return Outcome.FromException<TResult>(e);
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

        return Component.ExecuteCoreSync(
            [DebuggerDisableUserUnhandledExceptions] static (context, state) =>
            {
                try
                {
                    var result = state(context);
                    return Outcome.FromResult(result);
                }
                catch (Exception e)
                {
                    return Outcome.FromException<TResult>(e);
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
            return Component.ExecuteCoreSync(
                [DebuggerDisableUserUnhandledExceptions] static (context, state) =>
                {
                    try
                    {
                        return Outcome.FromResult(state(context.CancellationToken));
                    }
                    catch (Exception e)
                    {
                        return Outcome.FromException<TResult>(e);
                    }
                },
                context,
                callback).GetResultOrRethrow();
        }
        finally
        {
            Pool.Return(context);
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
            return Component.ExecuteCoreSync(
                [DebuggerDisableUserUnhandledExceptions] static (_, state) =>
                {
                    try
                    {
                        return Outcome.FromResult(state());
                    }
                    catch (Exception e)
                    {
                        return Outcome.FromException<TResult>(e);
                    }
                },
                context,
                callback).GetResultOrRethrow();
        }
        finally
        {
            Pool.Return(context);
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
            return Component.ExecuteCoreSync(
                [DebuggerDisableUserUnhandledExceptions] static (_, state) =>
                {
                    try
                    {
                        return Outcome.FromResult(state.callback(state.state));
                    }
                    catch (Exception e)
                    {
                        return Outcome.FromException<TResult>(e);
                    }
                },
                context,
                (callback, state)).GetResultOrRethrow();
        }
        finally
        {
            Pool.Return(context);
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
            return Component.ExecuteCoreSync(
                [DebuggerDisableUserUnhandledExceptions] static (context, state) =>
                {
                    try
                    {
                        return Outcome.FromResult(state.callback(state.state, context.CancellationToken));
                    }
                    catch (Exception e)
                    {
                        return Outcome.FromException<TResult>(e);
                    }
                },
                context,
                (callback, state)).GetResultOrRethrow();
        }
        finally
        {
            Pool.Return(context);
        }
    }

    private ResilienceContext GetSyncContext<TResult>(CancellationToken cancellationToken)
    {
        var context = Pool.Get(cancellationToken);

        InitializeSyncContext<TResult>(context);

        return context;
    }

    private void InitializeSyncContext<TResult>(ResilienceContext context)
    {
        DisposeHelper.EnsureNotDisposed();

        context.Initialize<TResult>(isSynchronous: true);
    }
}
