namespace Polly;

/// <summary>
/// An interface defining all executions available on a non-generic, synchronous policy.
/// </summary>
public interface ISyncPolicy : IsPolicy
{
    /// <summary>
    /// Sets the PolicyKey for this <see cref="Policy"/> instance.
    /// <remarks>Must be called before the policy is first used.  Can only be set once.</remarks>
    /// </summary>
    /// <param name="policyKey">The unique, used-definable key to assign to this <see cref="Policy"/> instance.</param>
    /// <returns>An instance of <see cref="ISyncPolicy"/>.</returns>
    ISyncPolicy WithPolicyKey(string policyKey);

    /// <summary>
    /// Executes the specified action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    void Execute(Action action);

    /// <summary>
    /// Executes the specified action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    void Execute(Action<Context> action, IDictionary<string, object> contextData);

    /// <summary>
    /// Executes the specified action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    void Execute(Action<Context> action, Context context);

    /// <summary>
    /// Executes the specified action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    void Execute(Action<CancellationToken> action, CancellationToken cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    void Execute(Action<Context, CancellationToken> action, IDictionary<string, object> contextData, CancellationToken cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    void Execute(Action<Context, CancellationToken> action, Context context, CancellationToken cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the Result.
    /// </summary>
    /// <typeparam name="TResult">The type of the Result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <returns>The value returned by the action.</returns>
    TResult Execute<TResult>(Func<TResult> action);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    TResult Execute<TResult>(Func<Context, TResult> action, IDictionary<string, object> contextData);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
    TResult Execute<TResult>(Func<Context, TResult> action, Context context);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The value returned by the action.</returns>
    TResult Execute<TResult>(Func<CancellationToken, TResult> action, CancellationToken cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    TResult Execute<TResult>(Func<Context, CancellationToken, TResult> action, IDictionary<string, object> contextData, CancellationToken cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The value returned by the action.</returns>
    TResult Execute<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The captured result.</returns>
    PolicyResult ExecuteAndCapture(Action action);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    PolicyResult ExecuteAndCapture(Action<Context> action, IDictionary<string, object> contextData);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The captured result.</returns>
    PolicyResult ExecuteAndCapture(Action<Context> action, Context context);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The captured result.</returns>
    PolicyResult ExecuteAndCapture(Action<CancellationToken> action, CancellationToken cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    PolicyResult ExecuteAndCapture(Action<Context, CancellationToken> action, IDictionary<string, object> contextData, CancellationToken cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The captured result.</returns>
    PolicyResult ExecuteAndCapture(Action<Context, CancellationToken> action, Context context, CancellationToken cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <returns>The captured result.</returns>
    PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<TResult> action);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, TResult> action, IDictionary<string, object> contextData);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
    PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, TResult> action, Context context);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <typeparam name="TResult">The type of the t result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The captured result.</returns>
    PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<CancellationToken, TResult> action, CancellationToken cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, CancellationToken, TResult> action, IDictionary<string, object> contextData, CancellationToken cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The captured result.</returns>
    PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken);
}
