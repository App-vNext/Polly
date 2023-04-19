using Polly.Strategy;

namespace Polly.CircuitBreaker;

internal sealed class CircuitBreakerResilienceStrategy : ResilienceStrategy
{
#pragma warning disable IDE0052 // Remove unread private members
    private readonly TimeProvider _timeProvider;
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly BaseCircuitBreakerStrategyOptions _options;
#pragma warning restore IDE0052 // Remove unread private members

    public CircuitBreakerResilienceStrategy(TimeProvider timeProvider, ResilienceStrategyTelemetry telemetry, BaseCircuitBreakerStrategyOptions options)
    {
        _timeProvider = timeProvider;
        _telemetry = telemetry;
        _options = options;
    }

    protected internal override ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
    {
        return callback(context, state);
    }
}

