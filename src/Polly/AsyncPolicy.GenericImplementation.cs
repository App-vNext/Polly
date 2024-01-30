namespace Polly;

public abstract partial class AsyncPolicy<TResult>
{
    /// <summary>
    /// Defines the implementation of a policy for async executions returning <typeparamref name="TResult"/>.
    /// </summary>
    /// <param name="action">The action passed by calling code to execute through the policy.</param>
    /// <param name="context">The policy execution context.</param>
    /// <param name="cancellationToken">A token to signal that execution should be cancelled.</param>
    /// <param name="continueOnCapturedContext">Whether async continuations should continue on a captured context.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the execution.</returns>
    protected abstract Task<TResult> ImplementationAsync(
        Func<Context, CancellationToken, Task<TResult>> action,
        Context context,
        CancellationToken cancellationToken,
        bool continueOnCapturedContext);
}
