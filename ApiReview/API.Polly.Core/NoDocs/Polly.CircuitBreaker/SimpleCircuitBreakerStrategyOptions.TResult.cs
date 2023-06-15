// Assembly 'Polly.Core'

using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Polly.CircuitBreaker;

public class SimpleCircuitBreakerStrategyOptions<TResult> : CircuitBreakerStrategyOptions<TResult>
{
    [Range(1, int.MaxValue)]
    public int FailureThreshold { get; set; }
    public SimpleCircuitBreakerStrategyOptions();
}
