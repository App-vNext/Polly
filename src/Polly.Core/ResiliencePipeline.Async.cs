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
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
    public async ValueTask ExecuteAsync<TState>(
        Func<ResilienceContext, TState, ValueTask> callback,
        ResilienceContext context,
        TState state)
    {
        Guard.NotNull(callback);
        Guard.NotNull(context);

        InitializeAsyncContext(context);

        var outcome = await Component.ExecuteCore(
            [DebuggerDisableUserUnhandledExceptions] static async (context, state) =>
            {
                try
                {
                    await state.callback(context, state.state).ConfigureAwait(context.ContinueOnCapturedContext);
                    return Outcome.Void;
                }
                catch (Exception e)
                {
                    return Outcome.FromException(e);
                }
            },
            context,
            (callback, state)).ConfigureAwait(context.ContinueOnCapturedContext);

        outcome.GetResultOrRethrow();
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
    public async ValueTask ExecuteAsync(
        Func<ResilienceContext, ValueTask> callback,
        ResilienceContext context)
    {
        Guard.NotNull(callback);
        Guard.NotNull(context);

        InitializeAsyncContext(context);

        var outcome = await Component.ExecuteCore(
            [DebuggerDisableUserUnhandledExceptions] static async (context, state) =>
            {
                try
                {
                    await state(context).ConfigureAwait(context.ContinueOnCapturedContext);
                    return Outcome.Void;
                }
                catch (Exception e)
                {
                    return Outcome.FromException(e);
                }
            },
            context,
            callback).ConfigureAwait(context.ContinueOnCapturedContext);

        outcome.GetResultOrRethrow();
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the callback.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents an asynchronous callback.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public async ValueTask ExecuteAsync<TState>(
        Func<TState, CancellationToken, ValueTask> callback,
        TState state,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(callback);

        var context = GetAsyncContext(cancellationToken);

        try
        {
            var outcome = await Component.ExecuteCore(
                [DebuggerDisableUserUnhandledExceptions] static async (context, state) =>
                {
                    try
                    {
                        await state.callback(state.state, context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext);
                        return Outcome.Void;
                    }
                    catch (Exception e)
                    {
                        return Outcome.FromException(e);
                    }
                },
                context,
                (callback, state)).ConfigureAwait(context.ContinueOnCapturedContext);

            outcome.GetResultOrRethrow();
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
    /// <returns>The instance of <see cref="ValueTask"/> that represents an asynchronous callback.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public async ValueTask ExecuteAsync(
        Func<CancellationToken, ValueTask> callback,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(callback);

        var context = GetAsyncContext(cancellationToken);

        try
        {
            var outcome = await Component.ExecuteCore(
                [DebuggerDisableUserUnhandledExceptions] static async (context, state) =>
                {
                    try
                    {
                        await state(context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext);
                        return Outcome.Void;
                    }
                    catch (Exception e)
                    {
                        return Outcome.FromException(e);
                    }

                },
                context,
                callback).ConfigureAwait(context.ContinueOnCapturedContext);

            outcome.GetResultOrRethrow();
        }
        finally
        {
            Pool.Return(context);
        }
    }

    private ResilienceContext GetAsyncContext(CancellationToken cancellationToken) => GetAsyncContext<VoidResult>(cancellationToken);

    private void InitializeAsyncContext(ResilienceContext context) => InitializeAsyncContext<VoidResult>(context);
}
