using System.Threading.RateLimiting;
using Polly.Telemetry;

namespace Polly.RateLimiting;

internal sealed class RateLimiterResilienceStrategy : ResilienceStrategy, IDisposable, IAsyncDisposable
{
    private readonly ResilienceStrategyTelemetry _telemetry;

    public RateLimiterResilienceStrategy(
        ResilienceRateLimiter limiter,
        Func<OnRateLimiterRejectedArguments, ValueTask>? onRejected,
        ResilienceStrategyTelemetry telemetry)
    {
        Limiter = limiter;
        OnLeaseRejected = onRejected;

        _telemetry = telemetry;
    }

    public ResilienceRateLimiter Limiter { get; }

    public Func<OnRateLimiterRejectedArguments, ValueTask>? OnLeaseRejected { get; }

    public void Dispose() => Limiter.Dispose();

    public ValueTask DisposeAsync() => Limiter.DisposeAsync();

    protected override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        using var lease = await Limiter.AcquireAsync(context).ConfigureAwait(context.ContinueOnCapturedContext);

        if (lease.IsAcquired)
        {
            return await callback(context, state).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        TimeSpan? retryAfter = null;

        if (lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfterValue))
        {
            retryAfter = retryAfterValue;
        }

        var args = new OnRateLimiterRejectedArguments(context, lease);
        _telemetry.Report(new(ResilienceEventSeverity.Error, RateLimiterConstants.OnRateLimiterRejectedEvent), context, args);

        if (OnLeaseRejected != null)
        {
            await OnLeaseRejected(new OnRateLimiterRejectedArguments(context, lease)).ConfigureAwait(context.ContinueOnCapturedContext);
        }

        var exception = retryAfter.HasValue ? new RateLimiterRejectedException(retryAfter.Value) : new RateLimiterRejectedException();

        return Outcome.FromException<TResult>(exception.TrySetStackTrace());
    }
}
