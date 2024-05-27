namespace Polly;

#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable RS0027 // API with optional parameter(s) should have the most parameters amongst its public overloads

public partial class ResiliencePipeline
{
    /// <summary>
    /// Executes the specified outcome-based callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the callback.</typeparam>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method is for advanced and high performance scenarios. The caller must make sure that the <paramref name="callback"/>
    /// does not throw any exceptions. Instead, it converts them to <see cref="Outcome{TResult}"/>.
    /// </remarks>
    public ValueTask<Outcome<TResult>> ExecuteOutcomeAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        Guard.NotNull(callback);
        Guard.NotNull(context);

        InitializeAsyncContext<TResult>(context);

        return Component.ExecuteCore(callback, context, state);
    }

    /// <summary>
    /// Executes the specified outcome-based callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the callback.</typeparam>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method is for advanced and high performance scenarios. The <paramref name="callback"/> is converted to <see cref="Outcome{TResult}"/> to avoid
    /// rethrowing exceptions when the task is awaited.
    /// </remarks>
    public ValueTask<Outcome<TResult>> ExecuteOutcomeAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<TResult>> callback,
        ResilienceContext context,
        TState state) =>
        ExecuteOutcomeAsync(
            static (ResilienceContext rc, (Func<ResilienceContext, TState, ValueTask<TResult>> InternalCallback, TState InternalState) state) =>
                state.InternalCallback(rc, state.InternalState).ToAsyncOutcome(),
            context,
            (callback, state));

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
    public async ValueTask<TResult> ExecuteAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<TResult>> callback,
        ResilienceContext context,
        TState state)
    {
        Guard.NotNull(callback);
        Guard.NotNull(context);

        InitializeAsyncContext<TResult>(context);

        var outcome = await Component.ExecuteCore(
            static async (context, state) =>
            {
                try
                {
                    return Outcome.FromResult(await state.callback(context, state.state).ConfigureAwait(context.ContinueOnCapturedContext));
                }
                catch (Exception e)
                {
                    return Outcome.FromException<TResult>(e);
                }
            },
            context,
            (callback, state)).ConfigureAwait(context.ContinueOnCapturedContext);

        return outcome.GetResultOrRethrow();
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
    public async ValueTask<TResult> ExecuteAsync<TResult>(
        Func<ResilienceContext, ValueTask<TResult>> callback,
        ResilienceContext context)
    {
        Guard.NotNull(callback);
        Guard.NotNull(context);

        InitializeAsyncContext<TResult>(context);

        var outcome = await Component.ExecuteCore(
            static async (context, state) =>
            {
                try
                {
                    return Outcome.FromResult(await state(context).ConfigureAwait(context.ContinueOnCapturedContext));
                }
                catch (Exception e)
                {
                    return Outcome.FromException<TResult>(e);
                }
            },
            context,
            callback).ConfigureAwait(context.ContinueOnCapturedContext);

        return outcome.GetResultOrRethrow();
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
            var outcome = await Component.ExecuteCore(
                static async (context, state) =>
                {
                    try
                    {
                        return Outcome.FromResult(await state.callback(state.state, context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext));
                    }
                    catch (Exception e)
                    {
                        return Outcome.FromException<TResult>(e);
                    }
                },
                context,
                (callback, state)).ConfigureAwait(context.ContinueOnCapturedContext);

            return outcome.GetResultOrRethrow();
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
            var outcome = await Component.ExecuteCore(
                static async (context, state) =>
                {
                    try
                    {
                        return Outcome.FromResult(await state(context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext));
                    }
                    catch (Exception e)
                    {
                        return Outcome.FromException<TResult>(e);
                    }
                },
                context,
                callback).ConfigureAwait(context.ContinueOnCapturedContext);

            return outcome.GetResultOrRethrow();
        }
        finally
        {
            Pool.Return(context);
        }
    }

    private ResilienceContext GetAsyncContext<TResult>(CancellationToken cancellationToken)
    {
        var context = Pool.Get(cancellationToken);

        InitializeAsyncContext<TResult>(context);

        return context;
    }

    private void InitializeAsyncContext<TResult>(ResilienceContext context)
    {
        DisposeHelper.EnsureNotDisposed();

        context.Initialize<TResult>(isSynchronous: false);
    }
}
