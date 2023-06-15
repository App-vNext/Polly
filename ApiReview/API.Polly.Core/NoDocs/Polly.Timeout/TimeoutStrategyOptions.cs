// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Polly.Timeout;

public class TimeoutStrategyOptions : ResilienceStrategyOptions
{
    public sealed override string StrategyType { get; }
    [Timeout]
    public TimeSpan Timeout { get; set; }
    public Func<TimeoutGeneratorArguments, ValueTask<TimeSpan>>? TimeoutGenerator { get; set; }
    public Func<OnTimeoutArguments, ValueTask>? OnTimeout { get; set; }
    public TimeoutStrategyOptions();
}
