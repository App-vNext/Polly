using Polly.Telemetry;

namespace Polly.Simmy.Outcomes;

internal sealed class OutcomeChaosStrategy : OutcomeChaosStrategy<object>
{
    public OutcomeChaosStrategy(OutcomeStrategyOptions<Exception> options, ResilienceStrategyTelemetry telemetry, bool isGeneric)
        : base(options, telemetry, isGeneric)
    {
    }
}
