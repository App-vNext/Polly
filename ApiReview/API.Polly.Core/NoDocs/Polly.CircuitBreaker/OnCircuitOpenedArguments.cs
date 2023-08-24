// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;
using Polly.Utils;

namespace Polly.CircuitBreaker;

public readonly struct OnCircuitOpenedArguments<TResult>
{
    public Outcome<TResult> Outcome { get; }
    public ResilienceContext Context { get; }
    public TimeSpan BreakDuration { get; }
    public bool IsManual { get; }
    public OnCircuitOpenedArguments(ResilienceContext context, Outcome<TResult> outcome, TimeSpan breakDuration, bool isManual);
}
