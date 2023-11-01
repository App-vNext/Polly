// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Simmy.Fault;

public readonly struct OnFaultInjectedArguments
{
    public ResilienceContext Context { get; }
    public Exception Fault { get; }
    public OnFaultInjectedArguments(ResilienceContext context, Exception fault);
}
