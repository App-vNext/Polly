#nullable enable

namespace Polly.RateLimit;

internal static class RateLimitEngine
{
    internal static TResult Implementation<TResult>(
        IRateLimiter rateLimiter,
        Func<TimeSpan, Context, TResult>? retryAfterFactory,
        Func<Context, CancellationToken, TResult> action,
        Context context,
        CancellationToken cancellationToken)
    {
        (bool permit, TimeSpan retryAfter) = rateLimiter.PermitExecution();

        if (permit)
        {
            return action(context, cancellationToken);
        }

        if (retryAfterFactory != null)
        {
            return retryAfterFactory(retryAfter, context);
        }

        throw new RateLimitRejectedException(retryAfter);
    }
}
