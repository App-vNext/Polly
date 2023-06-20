// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.CircuitBreaker;

public record OnCircuitClosedArguments(bool IsManual)
{
    [CompilerGenerated]
    protected virtual Type EqualityContract { get; }
}
