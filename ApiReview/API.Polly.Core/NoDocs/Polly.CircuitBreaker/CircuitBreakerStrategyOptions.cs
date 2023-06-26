// Assembly 'Polly.Core'

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Polly.CircuitBreaker;

public abstract class CircuitBreakerStrategyOptions<TResult> : ResilienceStrategyOptions
{
    public sealed override string StrategyType { get; }
    [Range(typeof(TimeSpan), "00:00:00.500", "1.00:00:00")]
    public TimeSpan BreakDuration { get; set; }
    [Required]
    public Func<OutcomeArguments<TResult, CircuitBreakerPredicateArguments>, ValueTask<bool>> ShouldHandle { get; set; }
    public Func<OutcomeArguments<TResult, OnCircuitClosedArguments>, ValueTask>? OnClosed { get; set; }
    public Func<OutcomeArguments<TResult, OnCircuitOpenedArguments>, ValueTask>? OnOpened { get; set; }
    public Func<OnCircuitHalfOpenedArguments, ValueTask>? OnHalfOpened { get; set; }
    public CircuitBreakerManualControl? ManualControl { get; set; }
    public CircuitBreakerStateProvider? StateProvider { get; set; }
    protected CircuitBreakerStrategyOptions();
}
