// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.CircuitBreaker;

public sealed class OnCircuitHalfOpenedArguments
{
    public ResilienceContext Context { get; }
    public OnCircuitHalfOpenedArguments(ResilienceContext context);
}
