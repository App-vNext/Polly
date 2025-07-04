using System.Threading.RateLimiting;
using Polly.Telemetry;

namespace Polly.RateLimiting;

internal sealed class RateLimiterResilienceStrategy : ResilienceStrategy, IDisposable, IAsyncDisposable
{
    private readonly ResilienceStrategyTelemetry _telemetry;

    public RateLimiterResilienceStrategy(
        Func<RateLimiterArguments, ValueTask<RateLimitLease>> limiter,
        Func<OnRateLimiterRejectedArguments, ValueTask>? onRejected,
        ResilienceStrategyTelemetry telemetry,
        RateLimiter? wrapper)
    {
        Limiter = limiter;
        OnLeaseRejected = onRejected;

        _telemetry = telemetry;
        Wrapper = wrapper;
    }

    public Func<RateLimiterArguments, ValueTask<RateLimitLease>> Limiter { get; }

    public Func<OnRateLimiterRejectedArguments, ValueTask>? OnLeaseRejected { get; }

    public RateLimiter? Wrapper { get; }

    public void Dispose() => Wrapper?.Dispose();

    public ValueTask DisposeAsync()
    {
        if (Wrapper is not null)
        {
            return Wrapper.DisposeAsync();
        }

        return default;
    }

    protected override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        using var lease = await Limiter(new RateLimiterArguments(context)).ConfigureAwait(context.ContinueOnCapturedContext);

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

        var exception = retryAfter is not null
            ? new RateLimiterRejectedException(retryAfterValue)
            : new RateLimiterRejectedException();

        _telemetry.SetTelemetrySource(exception);

        return Outcome.FromException<TResult>(exception.TrySetStackTrace());
    }
}
