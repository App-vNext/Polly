// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Retry;

public readonly struct RetryDelayArguments
{
    public int Attempt { get; }
    public TimeSpan DelayHint { get; }
    public RetryDelayArguments(int attempt, TimeSpan delayHint);
}
