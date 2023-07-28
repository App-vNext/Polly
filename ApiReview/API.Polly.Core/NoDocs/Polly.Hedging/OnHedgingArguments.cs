// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Hedging;

public sealed class OnHedgingArguments
{
    public int AttemptNumber { get; }
    public bool HasOutcome { get; }
    public TimeSpan ExecutionTime { get; }
    public OnHedgingArguments(int attemptNumber, bool hasOutcome, TimeSpan executionTime);
}
