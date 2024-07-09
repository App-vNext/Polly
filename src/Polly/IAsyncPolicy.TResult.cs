namespace Polly;

/// <summary>
/// An interface defining all executions available on an asynchronous policy generic-typed for executions returning results of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result of funcs executed through the Policy.</typeparam>
public interface IAsyncPolicy<TResult> : IsPolicy
{
    /// <summary>
    /// Sets the PolicyKey for this <see cref="IAsyncPolicy{TResult}"/> instance.
    /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
    /// </summary>
    /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="IAsyncPolicy{TResult}"/> instance.</param>
    /// <returns>An instance of <see cref="IAsyncPolicy{TResult}"/>.</returns>
    IAsyncPolicy<TResult> WithPolicyKey(string policyKey);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The value returned by the action.</returns>
    Task<TResult> ExecuteAsync(Func<Task<TResult>> action);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The value returned by the action.</returns>
    Task<TResult> ExecuteAsync(Func<Context, Task<TResult>> action, Context context);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <returns>The value returned by the action.</returns>
    Task<TResult> ExecuteAsync(Func<Context, Task<TResult>> action, IDictionary<string, object> contextData);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
    /// <returns>The value returned by the action.</returns>
    Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The value returned by the action.</returns>
    Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
    /// <returns>The value returned by the action.</returns>
    Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken);

#pragma warning disable CA1068
    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    Task<TResult> ExecuteAsync(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken, bool continueOnCapturedContext);
#pragma warning restore CA1068

#pragma warning disable CA1068
    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext);
#pragma warning restore CA1068

#pragma warning disable CA1068
    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy is in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    Task<TResult> ExecuteAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext);
#pragma warning restore CA1068

    /// <summary>
    /// Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The captured result.</returns>
    Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Task<TResult>> action);

    /// <summary>
    /// Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    /// <returns>The captured result.</returns>
    Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, Task<TResult>> action, IDictionary<string, object> contextData);

    /// <summary>
    /// Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The captured result.</returns>
    Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, Task<TResult>> action, Context context);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The captured result.</returns>
    Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    /// <returns>The captured result.</returns>
    Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken);

    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <returns>The captured result.</returns>
    Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken);

#pragma warning disable CA1068
    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken, bool continueOnCapturedContext);
#pragma warning restore CA1068

#pragma warning disable CA1068
    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> action, IDictionary<string, object> contextData, CancellationToken cancellationToken, bool continueOnCapturedContext);
#pragma warning restore CA1068

#pragma warning disable CA1068
    /// <summary>
    ///     Executes the specified asynchronous action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">A cancellation token which can be used to cancel the action.  When a retry policy in use, also cancels any further retries.</param>
    /// <param name="continueOnCapturedContext">Whether to continue on a captured synchronization context.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="InvalidOperationException">Please use asynchronous-defined policies when calling asynchronous ExecuteAsync (and similar) methods.</exception>
    Task<PolicyResult<TResult>> ExecuteAndCaptureAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken, bool continueOnCapturedContext);
#pragma warning restore CA1068
}
