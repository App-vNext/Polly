// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Timeout;

public sealed class OnTimeoutArguments
{
    public ResilienceContext Context { get; }
    public Exception Exception { get; }
    public TimeSpan Timeout { get; }
    public OnTimeoutArguments(ResilienceContext context, Exception exception, TimeSpan timeout);
}
