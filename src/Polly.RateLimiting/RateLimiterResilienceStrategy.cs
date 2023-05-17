using System.Threading.RateLimiting;
using Polly.Strategy;

namespace Polly.RateLimiting;

internal sealed class RateLimiterResilienceStrategy : ResilienceStrategy
{
    private readonly ResilienceStrategyTelemetry _telemetry;

    public RateLimiterResilienceStrategy(RateLimiter limiter, NoOutcomeEvent<OnRateLimiterRejectedArguments> @event, ResilienceStrategyTelemetry telemetry)
    {
        Limiter = limiter;
        OnLeaseRejected = @event.CreateHandler();

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

        var args = new OnRateLimiterRejectedArguments(context, lease, retryAfter);
        _telemetry.Report(RateLimiterConstants.OnRateLimiterRejectedEvent, args);

        if (OnLeaseRejected != null)
        {
            await OnLeaseRejected(new OnRateLimiterRejectedArguments(context, lease, retryAfter)).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        throw retryAfter.HasValue ? new RateLimiterRejectedException(retryAfter.Value) : new RateLimiterRejectedException();
    }
}
