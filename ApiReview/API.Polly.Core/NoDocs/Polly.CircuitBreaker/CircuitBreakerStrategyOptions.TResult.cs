// Assembly 'Polly.Core'

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Polly.CircuitBreaker;

public class CircuitBreakerStrategyOptions<TResult> : ResilienceStrategyOptions
{
    [Range(0.0, 1.0)]
    public double FailureRatio { get; set; }
    [Range(2, int.MaxValue)]
    public int MinimumThroughput { get; set; }
    [Range(typeof(TimeSpan), "00:00:00.500", "1.00:00:00")]
    public TimeSpan SamplingDuration { get; set; }
    [Range(typeof(TimeSpan), "00:00:00.500", "1.00:00:00")]
    public TimeSpan BreakDuration { get; set; }
    [Required]
    public Func<CircuitBreakerPredicateArguments<TResult>, ValueTask<bool>> ShouldHandle { get; set; }
    public Func<OnCircuitClosedArguments<TResult>, ValueTask>? OnClosed { get; set; }
    public Func<OnCircuitOpenedArguments<TResult>, ValueTask>? OnOpened { get; set; }
    public Func<OnCircuitHalfOpenedArguments, ValueTask>? OnHalfOpened { get; set; }
    public CircuitBreakerManualControl? ManualControl { get; set; }
    public CircuitBreakerStateProvider? StateProvider { get; set; }
    public CircuitBreakerStrategyOptions();
}
