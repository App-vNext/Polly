// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Hedging;

public readonly struct OnHedgingArguments
{
    public int AttemptNumber { get; }
    public bool HasOutcome { get; }
    public TimeSpan Duration { get; }
    public OnHedgingArguments(int attemptNumber, bool hasOutcome, TimeSpan duration);
}