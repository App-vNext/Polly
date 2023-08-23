// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.CircuitBreaker;

public class BrokenCircuitException<TResult> : BrokenCircuitException
{
    public TResult Result { get; }
    public BrokenCircuitException(TResult result);
    public BrokenCircuitException(string message, TResult result);
    public BrokenCircuitException(string message, Exception inner, TResult result);
}
