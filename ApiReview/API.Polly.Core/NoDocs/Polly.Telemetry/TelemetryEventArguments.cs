// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;
using Polly.Utils;

namespace Polly.Telemetry;

public sealed record TelemetryEventArguments
{
    public ResilienceTelemetrySource Source { get; }
    public ResilienceEvent Event { get; }
    public ResilienceContext Context { get; }
    public Outcome<object>? Outcome { get; }
    public object Arguments { get; }
}
