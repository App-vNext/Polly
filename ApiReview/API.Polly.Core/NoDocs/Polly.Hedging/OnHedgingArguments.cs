// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Hedging;

public readonly struct OnHedgingArguments<TResult>
{
    public Outcome<TResult>? Outcome { get; }
    public ResilienceContext Context { get; }
    public int AttemptNumber { get; }
    public TimeSpan Duration { get; }
    public OnHedgingArguments(ResilienceContext context, Outcome<TResult>? outcome, int attemptNumber, TimeSpan duration);
}
