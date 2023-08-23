// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.CircuitBreaker;

public readonly struct OnCircuitOpenedArguments
{
    public TimeSpan BreakDuration { get; }
    public bool IsManual { get; }
    public OnCircuitOpenedArguments(TimeSpan breakDuration, bool isManual);
}
