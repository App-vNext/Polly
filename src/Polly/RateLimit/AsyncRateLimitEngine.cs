#nullable enable
namespace Polly.RateLimit;

internal static class AsyncRateLimitEngine
{
    internal static async Task<TResult> ImplementationAsync<TResult>(
        IRateLimiter rateLimiter,
        Func<TimeSpan, Context, TResult>? retryAfterFactory,
        Func<Context, CancellationToken, Task<TResult>> action,
        Context context,
        bool continueOnCapturedContext,
        CancellationToken cancellationToken)
    {
        (bool permit, TimeSpan retryAfter) = rateLimiter.PermitExecution();

        if (permit)
        {
            return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }

        if (retryAfterFactory != null)
        {
            return retryAfterFactory(retryAfter, context);
        }

        throw new RateLimitRejectedException(retryAfter);
    }
}
