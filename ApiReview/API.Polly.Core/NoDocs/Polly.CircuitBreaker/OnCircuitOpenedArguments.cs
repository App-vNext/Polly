// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.CircuitBreaker;

public record OnCircuitOpenedArguments(TimeSpan BreakDuration, bool IsManual)
{
    [CompilerGenerated]
    protected virtual Type EqualityContract { get; }
}
