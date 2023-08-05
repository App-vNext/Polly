using Polly.Telemetry;

namespace Polly.Simmy.Outcomes;

internal sealed class OutcomeChaosStrategy : OutcomeChaosStrategy<object>
{
    public OutcomeChaosStrategy(OutcomeStrategyOptions<Exception> options, ResilienceStrategyTelemetry telemetry)
        : base(options, telemetry)
    {
    }
}
