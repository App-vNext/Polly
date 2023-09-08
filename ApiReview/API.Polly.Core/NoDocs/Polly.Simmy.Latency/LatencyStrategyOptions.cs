// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Polly.Simmy.Latency;

public class LatencyStrategyOptions : MonkeyStrategyOptions
{
    public Func<OnLatencyArguments, ValueTask>? OnLatency { get; set; }
    public Func<LatencyGeneratorArguments, ValueTask<TimeSpan>>? LatencyGenerator { get; set; }
    public TimeSpan Latency { get; set; }
    public LatencyStrategyOptions();
}
