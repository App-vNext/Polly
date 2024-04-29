#nullable enable

namespace Polly.NoOp;

/// <summary>
/// A noop policy that can be applied to asynchronous delegates.
/// </summary>
public class AsyncNoOpPolicy : AsyncPolicy, INoOpPolicy
{
    internal AsyncNoOpPolicy()
    {
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
        bool continueOnCapturedContext) =>
        NoOpEngine.ImplementationAsync(action, context, cancellationToken);
}

/// <summary>
/// A noop policy that can be applied to asynchronous delegates returning a value of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class AsyncNoOpPolicy<TResult> : AsyncPolicy<TResult>, INoOpPolicy<TResult>
{
    internal AsyncNoOpPolicy()
    {
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override Task<TResult> ImplementationAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
        bool continueOnCapturedContext) =>
        NoOpEngine.ImplementationAsync(action, context, cancellationToken);
}
