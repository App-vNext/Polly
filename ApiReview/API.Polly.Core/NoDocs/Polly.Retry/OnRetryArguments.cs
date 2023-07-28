// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Retry;

public sealed class OnRetryArguments
{
    public int AttemptNumber { get; }
    public TimeSpan RetryDelay { get; }
    public TimeSpan ExecutionTime { get; }
    public OnRetryArguments(int attemptNumber, TimeSpan retryDelay, TimeSpan executionTime);
}
