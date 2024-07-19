#nullable enable

namespace Polly.RateLimit;

/// <summary>
/// A rate-limit policy that can be applied to synchronous delegates.
/// </summary>
#pragma warning disable CA1062 // Validate arguments of public methods
public class RateLimitPolicy : Policy, IRateLimitPolicy
{
    private readonly IRateLimiter _rateLimiter;

    internal RateLimitPolicy(IRateLimiter rateLimiter) =>
        _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override TResult Implementation<TResult>(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken) =>
        RateLimitEngine.Implementation(_rateLimiter, null, action, context, cancellationToken);
}

/// <summary>
/// A rate-limit policy that can be applied to synchronous delegates returning a value of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public class RateLimitPolicy<TResult> : Policy<TResult>, IRateLimitPolicy<TResult>
{
    private readonly IRateLimiter _rateLimiter;
    private readonly Func<TimeSpan, Context, TResult>? _retryAfterFactory;

    internal RateLimitPolicy(
        IRateLimiter rateLimiter,
        Func<TimeSpan, Context, TResult>? retryAfterFactory)
    {
        _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
        _retryAfterFactory = retryAfterFactory;
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    protected override TResult Implementation(Func<Context, CancellationToken, TResult> action, Context context, CancellationToken cancellationToken) =>
        RateLimitEngine.Implementation(_rateLimiter, _retryAfterFactory, action, context, cancellationToken);
}
