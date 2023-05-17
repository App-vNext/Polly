using Polly.Strategy;

namespace Polly.Telemetry;

internal sealed record class TelemetryEventArguments(ResilienceTelemetrySource Source, string EventName, IResilienceArguments Arguments, Outcome? Outcome) : IResilienceArguments
{
    public ResilienceContext Context => Arguments.Context;
}
