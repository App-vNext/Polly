namespace Polly;

public abstract partial class AsyncPolicy<TResult> : IAsyncPolicy<TResult>
{
    #region ExecuteAsync overloads

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The value returned by the action</returns>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync(Func<Task<TResult>> action) =>
        ExecuteInternalAsync((_, _) => action(), new Context(), CancellationToken.None, null);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <returns>The value returned by the action</returns>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync(Func<Context, Task<TResult>> action, IDictionary<string, object> contextData) =>
        ExecuteInternalAsync((ctx, _) => action(ctx), new Context(contextData), CancellationToken.None, null);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The value returned by the action</returns>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync(Func<Context, Task<TResult>> action, Context context) =>
        ExecuteInternalAsync((ctx, _) => action(ctx), context, CancellationToken.None, null);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
    /// <returns>The value returned by the action</returns>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken) =>
        ExecuteInternalAsync((_, ct) => action(ct), new Context(), cancellationToken, null);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
    /// <returns>The value returned by the action</returns>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        ExecuteInternalAsync((_, ct) => action(ct), new Context(), cancellationToken, continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The value returned by the action</returns>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken) =>
        ExecuteInternalAsync(action, new Context(contextData), cancellationToken, null);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
    /// <returns>The value returned by the action</returns>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken) =>
        ExecuteInternalAsync(action, context, cancellationToken, null);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The value returned by the action</returns>
    /// <exception cref="ArgumentNullException">contextData</exception>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        ExecuteInternalAsync(action, new Context(contextData), cancellationToken, continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
    /// <returns>The value returned by the action</returns>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        ExecuteInternalAsync(action, context, cancellationToken, continueOnCapturedContext);

    [DebuggerStepThrough]
    private async Task<TResult> ExecuteInternalAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool? continueOnCapturedContext)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        SetPolicyContext(context, out string priorPolicyWrapKey, out string priorPolicyKey);

        try
        {
            continueOnCapturedContext ??= DefaultContinueOnCapturedContext;
            return await ImplementationAsync(action, context, cancellationToken, continueOnCapturedContext.Value).ConfigureAwait(continueOnCapturedContext.Value);
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
    /// <returns>The captured result</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Task<TResult>> action) =>
        ExecuteAndCaptureInternalAsync((_, _) => action(), new Context(), CancellationToken.None, null);

    /// <summary>
    /// Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <exception cref="ArgumentNullException">contextData</exception>
    /// <returns>The captured result</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, Task<TResult>> action, IDictionary<string, object> contextData) =>
        ExecuteAndCaptureInternalAsync((ctx, _) => action(ctx), new Context(contextData), CancellationToken.None, null);

    /// <summary>
    /// Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The captured result</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, Task<TResult>> action, Context context) =>
        ExecuteAndCaptureInternalAsync((ctx, _) => action(ctx), context, CancellationToken.None, null);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The captured result</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken) =>
        ExecuteAndCaptureInternalAsync((_, ct) => action(ct), new Context(), cancellationToken, null);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The captured result</returns>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        ExecuteAndCaptureInternalAsync((_, ct) => action(ct), new Context(), cancellationToken, continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <exception cref="ArgumentNullException">contextData</exception>
    /// <returns>The captured result</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken) =>
        ExecuteAndCaptureInternalAsync(action, new Context(contextData), cancellationToken, null);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The captured result</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken) =>
        ExecuteAndCaptureInternalAsync(action, context, cancellationToken, null);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The captured result</returns>
    /// <exception cref="ArgumentNullException">contextData</exception>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        ExecuteAndCaptureInternalAsync(action, new Context(contextData), cancellationToken, continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The captured result</returns>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        ExecuteAndCaptureInternalAsync(action, context, cancellationToken, continueOnCapturedContext);

    [DebuggerStepThrough]
    private async Task<PolicyResult<TResult>> ExecuteAndCaptureInternalAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool? continueOnCapturedContext)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        try
        {
            continueOnCapturedContext ??= DefaultContinueOnCapturedContext;
            TResult result = await ExecuteAsync(action, context, cancellationToken, continueOnCapturedContext.Value).ConfigureAwait(continueOnCapturedContext.Value);

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
