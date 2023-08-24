// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;
using Polly.Utils;

namespace Polly.CircuitBreaker;

public readonly struct CircuitBreakerPredicateArguments<TResult>
{
    public Outcome<TResult> Outcome { get; }
    public ResilienceContext Context { get; }
    public CircuitBreakerPredicateArguments(ResilienceContext context, Outcome<TResult> outcome);
}
