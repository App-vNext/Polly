// Assembly 'Polly.Core'

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Polly.Utils;

namespace Polly.Telemetry;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed record TelemetryEventArguments
{
    public ResilienceTelemetrySource Source { get; }
    public string EventName { get; }
    public ResilienceContext Context { get; }
    public Outcome<object>? Outcome { get; }
    public object Arguments { get; }
}
