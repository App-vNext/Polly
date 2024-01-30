namespace Polly;

public abstract partial class Policy<TResult>
{
    /// <summary>
    /// Defines the implementation of a policy for synchronous executions returning <typeparamref name="TResult"/>.
    /// </summary>
    /// <param name="action">The action passed by calling code to execute through the policy.</param>
    /// <param name="context">The policy execution context.</param>
    /// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
    /// <returns>A <typeparamref name="TResult"/> result of the execution.</returns>
    protected abstract TResult Implementation(
        Func<Context, CancellationToken, TResult> action,
        Context context,
        CancellationToken cancellationToken);
}
