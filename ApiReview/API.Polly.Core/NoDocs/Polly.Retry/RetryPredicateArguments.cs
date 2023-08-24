// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;
using Polly.Utils;

namespace Polly.Retry;

public readonly struct RetryPredicateArguments<TResult>
{
    public Outcome<TResult> Outcome { get; }
    public ResilienceContext Context { get; }
    public int AttemptNumber { get; }
    public RetryPredicateArguments(ResilienceContext context, Outcome<TResult> outcome, int attemptNumber);
}
