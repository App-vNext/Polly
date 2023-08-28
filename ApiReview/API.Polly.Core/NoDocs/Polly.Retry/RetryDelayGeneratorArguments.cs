// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly.Retry;

public readonly struct RetryDelayGeneratorArguments<TResult>
{
    public Outcome<TResult> Outcome { get; }
    public ResilienceContext Context { get; }
    public int AttemptNumber { get; }
    public TimeSpan DelayHint { get; }
    public RetryDelayGeneratorArguments(ResilienceContext context, Outcome<TResult> outcome, int attemptNumber, TimeSpan delayHint);
}
