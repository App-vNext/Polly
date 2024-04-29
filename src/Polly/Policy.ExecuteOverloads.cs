namespace Polly;

public abstract partial class Policy : ISyncPolicy
{
    #region Execute overloads

    /// <summary>
    /// Executes the specified action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    [DebuggerStepThrough]
    public void Execute(Action action) =>
        Execute((_, _) => action(), [], DefaultCancellationToken);

    /// <summary>
    /// Executes the specified action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    [DebuggerStepThrough]
    public void Execute(Action<Context> action, IDictionary<string, object> contextData) =>
        Execute((ctx, _) => action(ctx), new Context(contextData), DefaultCancellationToken);

    /// <summary>
    /// Executes the specified action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    [DebuggerStepThrough]
    public void Execute(Action<Context> action, Context context) =>
        Execute((ctx, _) => action(ctx), context, DefaultCancellationToken);

    /// <summary>
    /// Executes the specified action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [DebuggerStepThrough]
    public void Execute(Action<CancellationToken> action, CancellationToken cancellationToken) =>
        Execute((_, ct) => action(ct), [], cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public void Execute(Action<Context, CancellationToken> action, IDictionary<string, object> contextData, CancellationToken cancellationToken) =>
        Execute(action, new Context(contextData), cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [DebuggerStepThrough]
    public void Execute(Action<Context, CancellationToken> action, Context context, CancellationToken cancellationToken)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        SetPolicyContext(context, out string priorPolicyWrapKey, out string priorPolicyKey);

        try
        {
            Implementation(action, context, cancellationToken);
        }
        finally
        {
            PolicyBase.RestorePolicyContext(context, priorPolicyWrapKey, priorPolicyKey);
        }
    }

    #region Overloads method-generic in TResult

    /// <summary>
    /// Executes the specified action within the policy and returns the Result.
    /// </summary>
    /// <typeparam name="TResult">The type of the Result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <returns>The value returned by the action.</returns>
    [DebuggerStepThrough]
    public TResult Execute<TResult>(Func<TResult> action) =>
        Execute((_, _) => action(), [], DefaultCancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public TResult Execute<TResult>(Func<Context, TResult> action, IDictionary<string, object> contextData) =>
        Execute((ctx, _) => action(ctx), new Context(contextData), DefaultCancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public TResult Execute<TResult>(Func<Context, TResult> action, Context context) =>
        Execute((ctx, _) => action(ctx), context, DefaultCancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The value returned by the action.</returns>
    [DebuggerStepThrough]
    public TResult Execute<TResult>(Func<CancellationToken, TResult> action, CancellationToken cancellationToken) =>
        Execute((_, ct) => action(ct), [], cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public TResult Execute<TResult>(Func<Context, CancellationToken, TResult> action, IDictionary<string, object> contextData, CancellationToken cancellationToken) =>
        Execute(action, new Context(contextData), cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The value returned by the action.</returns>
    [DebuggerStepThrough]
    public TResult Execute<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        SetPolicyContext(context, out string priorPolicyWrapKey, out string priorPolicyKey);

        try
        {
            return Implementation(action, context, cancellationToken);
        }
        finally
        {
            PolicyBase.RestorePolicyContext(context, priorPolicyWrapKey, priorPolicyKey);
        }
    }

    #endregion

    #endregion

    #region ExecuteAndCapture overloads

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public PolicyResult ExecuteAndCapture(Action action) =>
        ExecuteAndCapture((_, _) => action(), [], DefaultCancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public PolicyResult ExecuteAndCapture(Action<Context> action, IDictionary<string, object> contextData) =>
        ExecuteAndCapture((ctx, _) => action(ctx), new Context(contextData), DefaultCancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public PolicyResult ExecuteAndCapture(Action<Context> action, Context context) =>
        ExecuteAndCapture((ctx, _) => action(ctx), context, DefaultCancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public PolicyResult ExecuteAndCapture(Action<CancellationToken> action, CancellationToken cancellationToken) =>
        ExecuteAndCapture((_, ct) => action(ct), [], cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public PolicyResult ExecuteAndCapture(Action<Context, CancellationToken> action, IDictionary<string, object> contextData, CancellationToken cancellationToken) =>
        ExecuteAndCapture(action, new Context(contextData), cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public PolicyResult ExecuteAndCapture(Action<Context, CancellationToken> action, Context context, CancellationToken cancellationToken)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        try
        {
            Execute(action, context, cancellationToken);
            return PolicyResult.Successful(context);
        }
        catch (Exception exception)
        {
            return PolicyResult.Failure(exception, GetExceptionType(ExceptionPredicates, exception), context);
        }
    }

    #region Overloads method-generic in TResult

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<TResult> action) =>
        ExecuteAndCapture((_, _) => action(), [], DefaultCancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, TResult> action, IDictionary<string, object> contextData) =>
        ExecuteAndCapture((ctx, _) => action(ctx), new Context(contextData), DefaultCancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, TResult> action, Context context) =>
        ExecuteAndCapture((ctx, _) => action(ctx), context, DefaultCancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <typeparam name="TResult">The type of the t result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The captured result.</returns>
    public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<CancellationToken, TResult> action, CancellationToken cancellationToken) =>
        ExecuteAndCapture((_, ct) => action(ct), [], cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, CancellationToken, TResult> action, IDictionary<string, object> contextData, CancellationToken cancellationToken) =>
        ExecuteAndCapture(action, new Context(contextData), cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public PolicyResult<TResult> ExecuteAndCapture<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        try
        {
            return PolicyResult<TResult>.Successful(Execute(action, context, cancellationToken), context);
        }
        catch (Exception exception)
        {
            return PolicyResult<TResult>.Failure(exception, GetExceptionType(ExceptionPredicates, exception), context);
        }
    }
    #endregion

    #endregion
}
