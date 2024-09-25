namespace Polly;

public abstract partial class AsyncPolicy : PolicyBase, IAsyncPolicy
{
    #region ExecuteAsync overloads

    /// <summary>
    ///     Executes the specified asynchronous action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>A <see cref="Task" /> which completes when <see cref="AsyncPolicy"/> is registered.</returns>
    [DebuggerStepThrough]
    public Task ExecuteAsync(Func<Task> action) =>
        ExecuteAsync((_, _) => action(), [], DefaultCancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <returns>A <see cref="Task" /> which completes when <see cref="AsyncPolicy"/> is registered.</returns>
    [DebuggerStepThrough]
    public Task ExecuteAsync(Func<Context, Task> action, IDictionary<string, object> contextData) =>
        ExecuteAsync((ctx, _) => action(ctx), new Context(contextData), DefaultCancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>A <see cref="Task" /> which completes when <see cref="AsyncPolicy"/> is registered.</returns>
    [DebuggerStepThrough]
    public Task ExecuteAsync(Func<Context, Task> action, Context context) =>
        ExecuteAsync((ctx, _) => action(ctx), context, DefaultCancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>A <see cref="Task" /> which completes when <see cref="AsyncPolicy"/> is registered.</returns>
    [DebuggerStepThrough]
    public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken) =>
        ExecuteAsync((_, ct) => action(ct), [], cancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>A <see cref="Task" /> which completes when <see cref="AsyncPolicy"/> is registered.</returns>
    [DebuggerStepThrough]
    public Task ExecuteAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken) =>
        ExecuteAsync(action, new Context(contextData), cancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>A <see cref="Task" /> which completes when <see cref="AsyncPolicy"/> is registered.</returns>
    [DebuggerStepThrough]
    public Task ExecuteAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken) =>
        ExecuteAsync(action, context, cancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    /// <returns>A <see cref="Task" /> which completes when <see cref="AsyncPolicy"/> is registered.</returns>
    [DebuggerStepThrough]
    public Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        ExecuteAsync((_, ct) => action(ct), [], cancellationToken, continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    /// <returns>A <see cref="Task" /> which completes when <see cref="AsyncPolicy"/> is registered.</returns>
    [DebuggerStepThrough]
    public Task ExecuteAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        ExecuteAsync(action, new Context(contextData), cancellationToken, continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    /// <returns>A <see cref="Task" /> which completes when <see cref="AsyncPolicy"/> is registered.</returns>
    [DebuggerStepThrough]
    public Task ExecuteAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return ExecuteInternalAsync(action, context, continueOnCapturedContext, cancellationToken);
    }

    #region Overloads method-generic in TResult

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <returns>The value returned by the action.</returns>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action) =>
        ExecuteAsync((_, _) => action(), [], DefaultCancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <returns>The value returned by the action.</returns>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync<TResult>(Func<Context, Task<TResult>> action, IDictionary<string, object> contextData) =>
        ExecuteAsync((ctx, _) => action(ctx), new Context(contextData), DefaultCancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The value returned by the action.</returns>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync<TResult>(Func<Context, Task<TResult>> action, Context context) =>
        ExecuteAsync((ctx, _) => action(ctx), context, DefaultCancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
    /// <returns>The value returned by the action.</returns>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken) =>
        ExecuteAsync((_, ct) => action(ct), [], cancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The value returned by the action.</returns>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken) =>
        ExecuteAsync(action, new Context(contextData), cancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
    /// <returns>The value returned by the action.</returns>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken) =>
        ExecuteAsync(action, context, cancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        ExecuteAsync((_, ct) => action(ct), [], cancellationToken, continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        ExecuteAsync(action, new Context(contextData), cancellationToken, continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    [DebuggerStepThrough]
    public Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return ExecuteInternalAsync(action, context, continueOnCapturedContext, cancellationToken);
    }

    #endregion

    #endregion

    #region ExecuteAndCaptureAsync overloads

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Task> action) =>
        ExecuteAndCaptureAsync((_, _) => action(), [], DefaultCancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, Task> action, IDictionary<string, object> contextData) =>
        ExecuteAndCaptureAsync((ctx, _) => action(ctx), new Context(contextData), DefaultCancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, Task> action, Context context) =>
        ExecuteAndCaptureAsync((ctx, _) => action(ctx), context, DefaultCancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult> ExecuteAndCaptureAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken) =>
        ExecuteAndCaptureAsync((_, ct) => action(ct), [], cancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken) =>
        ExecuteAndCaptureAsync(action, new Context(contextData), cancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken) =>
        ExecuteAndCaptureAsync(action, context, cancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult> ExecuteAndCaptureAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        ExecuteAndCaptureAsync((_, ct) => action(ct), [], cancellationToken, continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        ExecuteAndCaptureAsync(action, new Context(contextData), cancellationToken, continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return ExecuteAndCaptureInternalAsync(action, context, continueOnCapturedContext, cancellationToken);
    }

    #region Overloads method-generic in TResult

    /// <summary>
    /// Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Task<TResult>> action) =>
        ExecuteAndCaptureAsync((_, _) => action(), [], DefaultCancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    /// Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, Task<TResult>> action, IDictionary<string, object> contextData) =>
        ExecuteAndCaptureAsync((ctx, _) => action(ctx), new Context(contextData), DefaultCancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    /// Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, Task<TResult>> action, Context context) =>
        ExecuteAndCaptureAsync((ctx, _) => action(ctx), context, DefaultCancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken) =>
        ExecuteAndCaptureAsync((_, ct) => action(ct), [], cancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken) =>
        ExecuteAndCaptureAsync(action, new Context(contextData), cancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken) =>
        ExecuteAndCaptureAsync(action, context, cancellationToken, DefaultContinueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        ExecuteAndCaptureAsync((_, ct) => action(ct), [], cancellationToken, continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext) =>
        ExecuteAndCaptureAsync(action, new Context(contextData), cancellationToken, continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    [DebuggerStepThrough]
    public Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(
        Func<Context, CancellationToken, Task<TResult>> action,
        Context context,
        CancellationToken cancellationToken,
        bool continueOnCapturedContext)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return ExecuteAndCaptureInternalAsync(action, context, continueOnCapturedContext, cancellationToken);
    }

    #endregion

    #endregion

    private async Task ExecuteInternalAsync(Func<Context, CancellationToken, Task> action, Context context, bool continueOnCapturedContext, CancellationToken cancellationToken)
    {
        SetPolicyContext(context, out string priorPolicyWrapKey, out string priorPolicyKey);

        try
        {
            await ImplementationAsync(action, context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }
        finally
        {
            RestorePolicyContext(context, priorPolicyWrapKey, priorPolicyKey);
        }
    }

    private async Task<TResult> ExecuteInternalAsync<TResult>(
        Func<Context, CancellationToken, Task<TResult>> action,
        Context context,
        bool continueOnCapturedContext,
        CancellationToken cancellationToken)
    {
        SetPolicyContext(context, out string priorPolicyWrapKey, out string priorPolicyKey);

        try
        {
            return await ImplementationAsync(action, context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
        }
        finally
        {
            RestorePolicyContext(context, priorPolicyWrapKey, priorPolicyKey);
        }
    }

    private async Task<PolicyResult> ExecuteAndCaptureInternalAsync(
        Func<Context, CancellationToken, Task> action,
        Context context,
        bool continueOnCapturedContext,
        CancellationToken cancellationToken)
    {
        try
        {
            await ExecuteAsync(action, context, cancellationToken, continueOnCapturedContext).ConfigureAwait(continueOnCapturedContext);
            return PolicyResult.Successful(context);
        }
        catch (Exception exception)
        {
            return PolicyResult.Failure(exception, GetExceptionType(ExceptionPredicates, exception), context);
        }
    }

    private async Task<PolicyResult<TResult>> ExecuteAndCaptureInternalAsync<TResult>(
        Func<Context, CancellationToken, Task<TResult>> action,
        Context context,
        bool continueOnCapturedContext,
        CancellationToken cancellationToken)
    {
        try
        {
            return PolicyResult<TResult>.Successful(
                await ExecuteAsync(action, context, cancellationToken, continueOnCapturedContext)
                    .ConfigureAwait(continueOnCapturedContext), context);
        }
        catch (Exception exception)
        {
            return PolicyResult<TResult>.Failure(exception, GetExceptionType(ExceptionPredicates, exception), context);
        }
    }
}
