// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Hedging;

public record OnHedgingArguments(int Attempt, bool HasOutcome, TimeSpan ExecutionTime)
{
    [CompilerGenerated]
    protected virtual Type EqualityContract { get; }
}
