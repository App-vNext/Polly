#nullable enable
namespace Polly.RateLimit;

/// <summary>
/// A rate-limit policy that can be applied to asynchronous delegates.
/// </summary>
public class AsyncRateLimitPolicy : AsyncPolicy, IRateLimitPolicy
{
    private readonly IRateLimiter _rateLimiter;

    internal AsyncRateLimitPolicy(IRateLimiter rateLimiter) =>
        _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
        bool continueOnCapturedContext) =>
        AsyncRateLimitEngine.ImplementationAsync(_rateLimiter, null, action, context, continueOnCapturedContext, cancellationToken);
}

/// <summary>
/// A rate-limit policy that can be applied to asynchronous delegates returning a value of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class AsyncRateLimitPolicy<TResult> : AsyncPolicy<TResult>, IRateLimitPolicy<TResult>
{
    private readonly IRateLimiter _rateLimiter;
    private readonly Func<TimeSpan, Context, TResult>? _retryAfterFactory;

    internal AsyncRateLimitPolicy(
        IRateLimiter rateLimiter,
        Func<TimeSpan, Context, TResult>? retryAfterFactory)
    {
        _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
        _retryAfterFactory = retryAfterFactory;
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override Task<TResult> ImplementationAsync(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
        bool continueOnCapturedContext) =>
        AsyncRateLimitEngine.ImplementationAsync(_rateLimiter, _retryAfterFactory, action, context, continueOnCapturedContext, cancellationToken);
}
