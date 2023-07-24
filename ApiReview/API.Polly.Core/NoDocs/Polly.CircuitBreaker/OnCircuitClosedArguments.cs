// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.CircuitBreaker;

public sealed class OnCircuitClosedArguments
{
    public bool IsManual { get; }
    public OnCircuitClosedArguments(bool isManual);
}
