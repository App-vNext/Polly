// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;
using Polly.Utils;

namespace Polly.Retry;

public readonly struct OnRetryArguments<TResult>
{
    public Outcome<TResult> Outcome { get; }
    public ResilienceContext Context { get; }
    public int AttemptNumber { get; }
    public TimeSpan RetryDelay { get; }
    public TimeSpan Duration { get; }
    public OnRetryArguments(ResilienceContext context, Outcome<TResult> outcome, int attemptNumber, TimeSpan retryDelay, TimeSpan duration);
}
