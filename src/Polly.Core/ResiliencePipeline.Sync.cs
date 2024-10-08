namespace Polly;

#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable RS0027 // API with optional parameter(s) should have the most parameters amongst its public overloads

public partial class ResiliencePipeline
{
    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
    public void Execute<TState>(
        Action<ResilienceContext, TState> callback,
        ResilienceContext context,
        TState state)
    {
        Guard.NotNull(callback);
        Guard.NotNull(context);

        InitializeSyncContext(context);

        Component.ExecuteCoreSync(
            [DebuggerDisableUserUnhandledExceptions] static (context, state) =>
            {
                try
                {
                    state.callback(context, state.state);
                    return Outcome.Void;
                }
                catch (Exception e)
                {
                    return Outcome.FromException(e);
                }
            },
            context,
            (callback, state)).GetResultOrRethrow();
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
    public void Execute(
        Action<ResilienceContext> callback,
        ResilienceContext context)
    {
        Guard.NotNull(callback);
        Guard.NotNull(context);

        InitializeSyncContext(context);

        Component.ExecuteCoreSync(
            [DebuggerDisableUserUnhandledExceptions] static (context, state) =>
            {
                try
                {
                    state(context);
                    return Outcome.Void;
                }
                catch (Exception e)
                {
                    return Outcome.FromException(e);
                }
            },
            context,
            callback).GetResultOrRethrow();
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the callback.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public void Execute<TState>(
        Action<TState, CancellationToken> callback,
        TState state,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(callback);

        var context = GetSyncContext(cancellationToken);

        try
        {
            Component.ExecuteCoreSync(
                [DebuggerDisableUserUnhandledExceptions] static (context, state) =>
                {
                    try
                    {
                        state.callback(state.state, context.CancellationToken);
                        return Outcome.Void;
                    }
                    catch (Exception e)
                    {
                        return Outcome.FromException(e);
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
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the callback.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public void Execute(
        Action<CancellationToken> callback,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(callback);

        var context = GetSyncContext(cancellationToken);

        try
        {
            Component.ExecuteCoreSync(
                [DebuggerDisableUserUnhandledExceptions] static (context, state) =>
                {
                    try
                    {
                        state(context.CancellationToken);
                        return Outcome.Void;
                    }
                    catch (Exception e)
                    {
                        return Outcome.FromException(e);
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
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public void Execute<TState>(
        Action<TState> callback,
        TState state)
    {
        Guard.NotNull(callback);

        var context = GetSyncContext(CancellationToken.None);

        try
        {
            Component.ExecuteCoreSync(
                [DebuggerDisableUserUnhandledExceptions] static (_, state) =>
                {
                    try
                    {
                        state.callback(state.state);
                        return Outcome.Void;
                    }
                    catch (Exception e)
                    {
                        return Outcome.FromException(e);
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
    /// <param name="callback">The user-provided callback.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public void Execute(Action callback)
    {
        Guard.NotNull(callback);

        var context = GetSyncContext(CancellationToken.None);

        try
        {
            Component.ExecuteCoreSync(
                [DebuggerDisableUserUnhandledExceptions] static (_, state) =>
                {
                    try
                    {
                        state();
                        return Outcome.Void;
                    }
                    catch (Exception e)
                    {
                        return Outcome.FromException(e);
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

    private ResilienceContext GetSyncContext(CancellationToken cancellationToken) => GetSyncContext<VoidResult>(cancellationToken);

    private void InitializeSyncContext(ResilienceContext context) => InitializeSyncContext<VoidResult>(context);
}
