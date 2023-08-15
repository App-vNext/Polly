// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.CircuitBreaker;

public readonly struct OnCircuitHalfOpenedArguments
{
    public ResilienceContext Context { get; }
    public OnCircuitHalfOpenedArguments(ResilienceContext context);
}
