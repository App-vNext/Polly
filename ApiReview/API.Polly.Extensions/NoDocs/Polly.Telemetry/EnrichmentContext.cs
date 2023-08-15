// Assembly 'Polly.Extensions'

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Polly.Telemetry;

public readonly struct EnrichmentContext<TResult, TArgs>
{
    public TelemetryEventArguments<TResult, TArgs> TelemetryEvent { get; }
    public IList<KeyValuePair<string, object?>> Tags { get; }
    public EnrichmentContext(in TelemetryEventArguments<TResult, TArgs> telemetryEvent, IList<KeyValuePair<string, object?>> tags);
}
