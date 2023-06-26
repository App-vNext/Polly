// Assembly 'Polly.Core'

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Polly.CircuitBreaker;

public class AdvancedCircuitBreakerStrategyOptions<TResult> : CircuitBreakerStrategyOptions<TResult>
{
    [Range(0.0, 1.0)]
    public double FailureThreshold { get; set; }
    [Range(2, int.MaxValue)]
    public int MinimumThroughput { get; set; }
    [Range(typeof(TimeSpan), "00:00:00.500", "1.00:00:00")]
    public TimeSpan SamplingDuration { get; set; }
    public AdvancedCircuitBreakerStrategyOptions();
}
