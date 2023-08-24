// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;
using Polly.Utils;

namespace Polly.CircuitBreaker;

public readonly struct OnCircuitClosedArguments<TResult>
{
    public Outcome<TResult> Outcome { get; }
    public ResilienceContext Context { get; }
    public bool IsManual { get; }
    public OnCircuitClosedArguments(ResilienceContext context, Outcome<TResult> outcome, bool isManual);
}
