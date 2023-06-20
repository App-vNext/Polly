// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Timeout;

public record OnTimeoutArguments(ResilienceContext Context, Exception Exception, TimeSpan Timeout)
{
    [CompilerGenerated]
    protected virtual Type EqualityContract { get; }
}
