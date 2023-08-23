// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.CircuitBreaker;

public readonly struct OnCircuitClosedArguments
{
    public bool IsManual { get; }
    public OnCircuitClosedArguments(bool isManual);
}
