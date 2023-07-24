// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.Retry;

public readonly struct RetryPredicateArguments
{
    public int Attempt { get; }
    public RetryPredicateArguments(int attempt);
}
