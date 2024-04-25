namespace Polly;

/// <summary>
/// An interface defining all executions available on a non-generic, asynchronous policy.
/// </summary>
public interface IAsyncPolicy : IsPolicy
{
    /// <summary>
    /// Sets the PolicyKey for this <see cref="IAsyncPolicy"/> instance.
    /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
    /// </summary>
    /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="IAsyncPolicy"/> instance.</param>
    IAsyncPolicy WithPolicyKey(string policyKey);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    Task ExecuteAsync(Func<Task> action);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    Task ExecuteAsync(Func<Context, Task> action, IDictionary<string, object> contextData);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    Task ExecuteAsync(Func<Context, Task> action, Context context);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    Task ExecuteAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    Task ExecuteAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken, bool continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    Task ExecuteAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    Task ExecuteAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <returns>The value returned by the action.</returns>
    Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The value returned by the action.</returns>
    Task<TResult> ExecuteAsync<TResult>(Func<Context, Task<TResult>> action, Context context);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <returns>The value returned by the action.</returns>
    Task<TResult> ExecuteAsync<TResult>(Func<Context, Task<TResult>> action, IDictionary<string, object> contextData);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
    /// <returns>The value returned by the action.</returns>
    Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The value returned by the action.</returns>
    Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
    /// <returns>The value returned by the action.</returns>
    Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken, bool continueOnCapturedContext);

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
    Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext);

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
    Task<TResult> ExecuteAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The captured result.</returns>
    Task<PolicyResult> ExecuteAndCaptureAsync(Func<Task> action);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    /// <returns>The captured result.</returns>
    Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, Task> action, IDictionary<string, object> contextData);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The captured result.</returns>
    Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, Task> action, Context context);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    Task<PolicyResult> ExecuteAndCaptureAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    /// <returns>The captured result.</returns>
    Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    Task<PolicyResult> ExecuteAndCaptureAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken, bool continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    /// <returns>The captured result.</returns>
    Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    Task<PolicyResult> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext);

    /// <summary>
    /// Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <returns>The captured result.</returns>
    Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Task<TResult>> action);

    /// <summary>
    /// Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    /// <returns>The captured result.</returns>
    Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, Task<TResult>> action, IDictionary<string, object> contextData);

    /// <summary>
    /// Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The captured result.</returns>
    Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, Task<TResult>> action, Context context);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The captured result.</returns>
    Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    /// <returns>The captured result.</returns>
    Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The captured result.</returns>
    Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken, bool continueOnCapturedContext);

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
    Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext);

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
    Task<PolicyResult<TResult>> ExecuteAndCaptureAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext);
}
