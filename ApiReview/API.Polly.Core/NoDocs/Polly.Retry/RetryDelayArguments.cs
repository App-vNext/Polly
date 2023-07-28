// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Retry;

public readonly struct RetryDelayArguments
{
    public int AttemptNumber { get; }
    public TimeSpan DelayHint { get; }
    public RetryDelayArguments(int attemptNumber, TimeSpan delayHint);
}
