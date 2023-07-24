// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Retry;

public sealed class OnRetryArguments
{
    public int Attempt { get; }
    public TimeSpan RetryDelay { get; }
    public TimeSpan ExecutionTime { get; }
    public OnRetryArguments(int attempt, TimeSpan retryDelay, TimeSpan executionTime);
}
