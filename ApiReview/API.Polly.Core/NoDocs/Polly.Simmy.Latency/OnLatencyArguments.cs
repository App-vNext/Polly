// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Simmy.Latency;

public readonly struct OnLatencyArguments
{
    public ResilienceContext Context { get; }
    public TimeSpan Latency { get; }
    public OnLatencyArguments(ResilienceContext context, TimeSpan latency);
}
