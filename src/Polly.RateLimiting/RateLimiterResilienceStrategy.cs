using System.Threading.RateLimiting;
using Polly.Telemetry;

namespace Polly.RateLimiting;

internal sealed class RateLimiterResilienceStrategy : ResilienceStrategy
{
    private readonly ResilienceStrategyTelemetry _telemetry;

    public RateLimiterResilienceStrategy(
        RateLimiter limiter,
        Func<OnRateLimiterRejectedArguments, ValueTask>? onRejected,
        ResilienceStrategyTelemetry telemetry)
    {
        Limiter = limiter;
        OnLeaseRejected = onRejected;

        _telemetry = telemetry;
    }

    public RateLimiter Limiter { get; }

    public Func<OnRateLimiterRejectedArguments, ValueTask>? OnLeaseRejected { get; }

    protected override async ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
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
        _telemetry.Report(RateLimiterConstants.OnRateLimiterRejectedEvent, context, args);

        if (OnLeaseRejected != null)
        {
            await OnLeaseRejected(new OnRateLimiterRejectedArguments(context, lease, retryAfter)).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        var exception = retryAfter.HasValue ? new RateLimiterRejectedException(retryAfter.Value) : new RateLimiterRejectedException();

        return new Outcome<TResult>(exception.TrySetStackTrace());
    }
}
