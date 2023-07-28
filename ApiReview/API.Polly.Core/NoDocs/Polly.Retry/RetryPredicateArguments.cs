// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.Retry;

public readonly struct RetryPredicateArguments
{
    public int AttemptNumber { get; }
    public RetryPredicateArguments(int attemptNumber);
}
