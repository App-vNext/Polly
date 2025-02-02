namespace Polly;

public abstract partial class Policy<TResult> : ISyncPolicy<TResult>
{
    #region Execute overloads

    /// <summary>
    /// Executes the specified action within the policy and returns the Result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The value returned by the action.</returns>
    [DebuggerStepThrough]
    public TResult Execute(Func<TResult> action) =>
        Execute((_, _) => action(), [], DefaultCancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public TResult Execute(Func<Context, TResult> action, IDictionary<string, object> contextData) =>
        Execute((ctx, _) => action(ctx), new Context(contextData), DefaultCancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public TResult Execute(Func<Context, TResult> action, Context context) =>
        Execute((ctx, _) => action(ctx), context, DefaultCancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The value returned by the action.</returns>
    [DebuggerStepThrough]
    public TResult Execute(Func<CancellationToken, TResult> action, CancellationToken cancellationToken) =>
        Execute((_, ct) => action(ct), [], cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The value returned by the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public TResult Execute(Func<Context, CancellationToken, TResult> action, IDictionary<string, object> contextData, CancellationToken cancellationToken) =>
        Execute(action, new Context(contextData), cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The value returned by the action.</returns>
    [DebuggerStepThrough]
    public TResult Execute(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
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
            RestorePolicyContext(context, priorPolicyWrapKey, priorPolicyKey);
        }
    }

    #endregion

    #region ExecuteAndCapture overloads

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public PolicyResult<TResult> ExecuteAndCapture(Func<TResult> action) =>
        ExecuteAndCapture((_, _) => action(), [], DefaultCancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="contextData">Arbitrary data that is passed to the exception policy.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="contextData"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public PolicyResult<TResult> ExecuteAndCapture(Func<Context, TResult> action, IDictionary<string, object> contextData) =>
        ExecuteAndCapture((ctx, _) => action(ctx), new Context(contextData), DefaultCancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <returns>The captured result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
    [DebuggerStepThrough]
    public PolicyResult<TResult> ExecuteAndCapture(Func<Context, TResult> action, Context context) =>
        ExecuteAndCapture((ctx, _) => action(ctx), context, DefaultCancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public PolicyResult<TResult> ExecuteAndCapture(Func<CancellationToken, TResult> action, CancellationToken cancellationToken) =>
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
    public PolicyResult<TResult> ExecuteAndCapture(Func<Context, CancellationToken, TResult> action, IDictionary<string, object> contextData, CancellationToken cancellationToken) =>
        ExecuteAndCapture(action, new Context(contextData), cancellationToken);

    /// <summary>
    /// Executes the specified action within the policy and returns the captured result.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <param name="context">Context data that is passed to the exception policy.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The captured result.</returns>
    [DebuggerStepThrough]
    public PolicyResult<TResult> ExecuteAndCapture(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        try
        {
            TResult result = Execute(action, context, cancellationToken);

            if (ResultPredicates.AnyMatch(result))
            {
                return PolicyResult<TResult>.Failure(result, context);
            }

            return PolicyResult<TResult>.Successful(result, context);
        }
#pragma warning disable CA1031
        catch (Exception exception)
#pragma warning restore CA1031
        {
            return PolicyResult<TResult>.Failure(exception, GetExceptionType(ExceptionPredicates, exception), context);
        }
    }

    #endregion
}
