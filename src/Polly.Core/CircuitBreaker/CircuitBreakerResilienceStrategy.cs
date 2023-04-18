using Polly.Strategy;

namespace Polly;

internal class CircuitBreakerResilienceStrategy : ResilienceStrategy
{
#pragma warning disable IDE0052 // Remove unread private members
    private readonly TimeProvider _timeProvider;
    private readonly ResilienceStrategyTelemetry _telemetry;
#pragma warning restore IDE0052 // Remove unread private members

    public CircuitBreakerResilienceStrategy(TimeProvider timeProvider, ResilienceStrategyTelemetry telemetry)
    {
        _timeProvider = timeProvider;
        _telemetry = telemetry;
    }

    protected internal override ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
    {
        return callback(context, state);
    }
}

