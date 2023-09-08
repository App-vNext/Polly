// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.Simmy.Latency;

public readonly struct LatencyGeneratorArguments
{
    public ResilienceContext Context { get; }
    public LatencyGeneratorArguments(ResilienceContext context);
}
