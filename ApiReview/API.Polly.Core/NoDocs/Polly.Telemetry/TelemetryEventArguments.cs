// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.Telemetry;

public readonly struct TelemetryEventArguments<TResult, TArgs>
{
    public ResilienceTelemetrySource Source { get; }
    public ResilienceEvent Event { get; }
    public ResilienceContext Context { get; }
    public TArgs Arguments { get; }
    public Outcome<TResult>? Outcome { get; }
    public TelemetryEventArguments(ResilienceTelemetrySource source, ResilienceEvent resilienceEvent, ResilienceContext context, TArgs args, Outcome<TResult>? outcome);
}
