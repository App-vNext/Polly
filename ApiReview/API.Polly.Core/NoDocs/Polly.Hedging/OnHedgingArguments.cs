// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;
using Polly.Utils;

namespace Polly.Hedging;

public readonly struct OnHedgingArguments<TResult>
{
    public Outcome<TResult> Outcome { get; }
    public ResilienceContext Context { get; }
    public int AttemptNumber { get; }
    public bool HasOutcome { get; }
    public TimeSpan Duration { get; }
    public OnHedgingArguments(ResilienceContext context, Outcome<TResult> outcome, int attemptNumber, bool hasOutcome, TimeSpan duration);
}
