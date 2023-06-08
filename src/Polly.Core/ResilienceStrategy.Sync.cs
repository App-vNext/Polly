using System.Runtime.ExceptionServices;

namespace Polly;

#pragma warning disable CA1031 // Do not catch general exception types

public abstract partial class ResilienceStrategy
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

        ExecuteCoreSync(
            static (context, state) =>
            {
                try
                {
                    state.callback(context, state.state);
                    return VoidResult.Outcome;
                }
                catch (Exception e)
                {
                    return new Outcome<VoidResult>(ExceptionDispatchInfo.Capture(e));
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

        ExecuteCoreSync(
            static (context, state) =>
            {
                try
                {
                    state(context);
                    return VoidResult.Outcome;
                }
                catch (Exception e)
                {
                    return new Outcome<VoidResult>(ExceptionDispatchInfo.Capture(e));
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
            ExecuteCoreSync(
                static (context, state) =>
                {
                    try
                    {
                        state.callback(state.state, context.CancellationToken);
                        return VoidResult.Outcome;
                    }
                    catch (Exception e)
                    {
                        return new Outcome<VoidResult>(ExceptionDispatchInfo.Capture(e));
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
            ExecuteCoreSync(
                static (context, state) =>
                {
                    try
                    {
                        state(context.CancellationToken);
                        return VoidResult.Outcome;
                    }
                    catch (Exception e)
                    {
                        return new Outcome<VoidResult>(ExceptionDispatchInfo.Capture(e));
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
            ExecuteCoreSync(
                static (_, state) =>
                {
                    try
                    {
                        state.callback(state.state);
                        return VoidResult.Outcome;
                    }
                    catch (Exception e)
                    {
                        return new Outcome<VoidResult>(ExceptionDispatchInfo.Capture(e));
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
    /// <param name="callback">The user-provided callback.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public void Execute(Action callback)
    {
        Guard.NotNull(callback);

        var context = GetSyncContext(CancellationToken.None);

        try
        {
            ExecuteCoreSync(
                static (_, state) =>
                {
                    try
                    {
                        state();
                        return VoidResult.Outcome;
                    }
                    catch (Exception e)
                    {
                        return new Outcome<VoidResult>(ExceptionDispatchInfo.Capture(e));
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

    private static ResilienceContext GetSyncContext(CancellationToken cancellationToken) => GetSyncContext<VoidResult>(cancellationToken);

    private static void InitializeSyncContext(ResilienceContext context) => InitializeSyncContext<VoidResult>(context);
}
