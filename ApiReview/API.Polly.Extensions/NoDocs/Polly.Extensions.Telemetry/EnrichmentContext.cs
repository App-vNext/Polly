// Assembly 'Polly.Extensions'

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Polly.Utils;

namespace Polly.Extensions.Telemetry;

public sealed class EnrichmentContext
{
    public Outcome<object>? Outcome { get; }
    public object? Arguments { get; }
    public ResilienceContext Context { get; }
    public ICollection<KeyValuePair<string, object?>> Tags { get; }
}
