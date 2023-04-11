using System.Threading.RateLimiting;
using Polly.Telemetry;

namespace Polly.RateLimiting;

internal class RateLimiterResilienceStrategy : ResilienceStrategy
{
    private readonly ResilienceTelemetry _telemetry;

    public RateLimiterResilienceStrategy(RateLimiter limiter, OnRateLimiterRejectedEvent @event, ResilienceTelemetry telemetry)
    {
        Limiter = limiter;
        OnLeaseRejected = @event.CreateHandlerInternal();

        _telemetry = telemetry;
    }

    public RateLimiter Limiter { get; }

    public Func<OnRateLimiterRejectedArguments, ValueTask>? OnLeaseRejected { get; }

    protected override async ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
    {
        using var lease = await Limiter.AcquireAsync(
            permitCount: 1,
            context.CancellationToken).ConfigureAwait(context.ContinueOnCapturedContext);

        if (lease.IsAcquired)
        {
            return await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        TimeSpan? retryAfter = null;

        if (lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfterValue))
        {
            retryAfter = retryAfterValue;
        }

        _telemetry.Report(RateLimiterConstants.OnRateLimiterRejectedEvent, context);

        if (OnLeaseRejected != null)
        {
            await OnLeaseRejected(new OnRateLimiterRejectedArguments(context, lease, retryAfter)).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        throw retryAfter.HasValue ? new RateLimiterRejectedException(retryAfter.Value) : new RateLimiterRejectedException();
    }
}
