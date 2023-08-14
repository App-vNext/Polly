// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Timeout;

public sealed class OnTimeoutArguments
{
    public ResilienceContext Context { get; }
    public TimeSpan Timeout { get; }
    public OnTimeoutArguments(ResilienceContext context, TimeSpan timeout);
}
