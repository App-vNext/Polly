// Assembly 'Polly.Core'

using System;

namespace Polly.CircuitBreaker;

public class BrokenCircuitException : ExecutionRejectedException
{
    public BrokenCircuitException();
    public BrokenCircuitException(string message);
    public BrokenCircuitException(string message, Exception inner);
}
