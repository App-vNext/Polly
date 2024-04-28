namespace Polly;

public abstract partial class AsyncPolicy<TResult> : IAsyncPolicy<TResult>
{
    #region ExecuteAsync overloads

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The value returned by the action.</returns>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync(Func<Task<TResult>> action) =>
        ExecuteAsync((_, _) => action(), [], CancellationToken.None, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <returns>The value returned by the action.</returns>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync(Func<Context, Task<TResult>> action, IDictionary<string, object> contextData) =>
        ExecuteAsync((ctx, _) => action(ctx), new Context(contextData), CancellationToken.None, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The value returned by the action.</returns>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync(Func<Context, Task<TResult>> action, Context context) =>
        ExecuteAsync((ctx, _) => action(ctx), context, CancellationToken.None, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
    /// <returns>The value returned by the action.</returns>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken) =>
        ExecuteAsync((_, ct) => action(ct), [], cancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        ExecuteAsync((_, ct) => action(ct), [], cancellationToken, continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The value returned by the action.</returns>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken) =>
        ExecuteAsync(action, new Context(contextData), cancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
    /// <returns>The value returned by the action.</returns>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken) =>
        ExecuteAsync(action, context, cancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        ExecuteAsync(action, new Context(contextData), cancellationToken, continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    [DebuggerStepThrough]
    public async Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        SetPolicyContext(context, out string priorPolicyWrapKey, out string priorPolicyKey);

        try
        {
            return await ImplementationAsync(action, context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }
        finally
        {
            PolicyBase.RestorePolicyContext(context, priorPolicyWrapKey, priorPolicyKey);
        }
    }

    #endregion

    #region ExecuteAndCaptureAsync overloads

    /// <summary>
    /// Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Task<TResult>> action) =>
        ExecuteAndCaptureAsync((_, _) => action(), [], CancellationToken.None, DefaultContinueOnCapturedContext);

    /// <summary>
    /// Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, Task<TResult>> action, IDictionary<string, object> contextData) =>
        ExecuteAndCaptureAsync((ctx, _) => action(ctx), new Context(contextData), CancellationToken.None, DefaultContinueOnCapturedContext);

    /// <summary>
    /// Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, Task<TResult>> action, Context context) =>
        ExecuteAndCaptureAsync((ctx, _) => action(ctx), context, CancellationToken.None, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken) =>
        ExecuteAndCaptureAsync((_, ct) => action(ct), [], cancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        ExecuteAndCaptureAsync((_, ct) => action(ct), [], cancellationToken, continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken) =>
        ExecuteAndCaptureAsync(action, new Context(contextData), cancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken) =>
        ExecuteAndCaptureAsync(action, context, cancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        ExecuteAndCaptureAsync(action, new Context(contextData), cancellationToken, continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    [DebuggerStepThrough]
    public async Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        try
        {
            TResult result = await ExecuteAsync(action, context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);

            if (ResultPredicates.AnyMatch(result))
            {
                return PolicyResult<TResult>.Failure(result, context);
            }

            return PolicyResult<TResult>.Successful(result, context);
        }
        catch (Exception exception)
        {
            return PolicyResult<TResult>.Failure(exception, GetExceptionType(ExceptionPredicates, exception), context);
        }
    }

    #endregion
}
