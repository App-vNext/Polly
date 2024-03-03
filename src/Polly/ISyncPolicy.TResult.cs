namespace Polly;

/// <summary>
/// An interface defining all executions available on a synchronous policy generic-typed for executions returning results of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result of funcs executed through the Policy.</typeparam>
public interface ISyncPolicy<TResult> : IsPolicy
{
    /// <summary>
    /// Sets the PolicyKey for this <see cref="Policy"/> instance.
    /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
    /// </summary>
    /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="Policy"/> instance.</param>
    ISyncPolicy<TResult> WithPolicyKey(string policyKey);

    /// <summary>
    /// Executes the specified action within the policy and returns the Result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The value returned by the action.</returns>
    TResult Execute(Func<TResult> action);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    TResult Execute(Func<Context, TResult> action, IDictionary<string, object> contextData);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    TResult Execute(Func<Context, TResult> action, Context context);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The value returned by the action.</returns>
    TResult Execute(Func<CancellationToken, TResult> action, CancellationToken cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    TResult Execute(Func<Context, CancellationToken, TResult> action, IDictionary<string, object> contextData, CancellationToken cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The value returned by the action.</returns>
    TResult Execute(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The captured result.</returns>
    PolicyResult<TResult> ExecuteAndCapture(Func<TResult> action);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    /// <returns>The captured result.</returns>
    PolicyResult<TResult> ExecuteAndCapture(Func<Context, TResult> action, IDictionary<string, object> contextData);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    /// <returns>The captured result.</returns>
    PolicyResult<TResult> ExecuteAndCapture(Func<Context, TResult> action, Context context);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The captured result.</returns>
    PolicyResult<TResult> ExecuteAndCapture(Func<CancellationToken, TResult> action, CancellationToken cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    PolicyResult<TResult> ExecuteAndCapture(Func<Context, CancellationToken, TResult> action, IDictionary<string, object> contextData, CancellationToken cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The captured result.</returns>
    PolicyResult<TResult> ExecuteAndCapture(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken);
}
