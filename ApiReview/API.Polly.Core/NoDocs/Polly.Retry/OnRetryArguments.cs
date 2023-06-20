// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Retry;

public record OnRetryArguments(int Attempt, TimeSpan RetryDelay, TimeSpan ExecutionTime)
{
    [CompilerGenerated]
    protected virtual Type EqualityContract { get; }
}
