// Assembly 'Polly.Core'

using System;

namespace Polly.CircuitBreaker;

public class IsolatedCircuitException : BrokenCircuitException
{
    public IsolatedCircuitException();
    public IsolatedCircuitException(string message);
    public IsolatedCircuitException(string message, Exception innerException);
}
