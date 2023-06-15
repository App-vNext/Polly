// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly;

public readonly struct OutcomeArguments<TResult, TArgs>
{
    public Outcome<TResult> Outcome { get; }
    public ResilienceContext Context { get; }
    public TArgs Arguments { get; }
    public Exception? Exception { get; }
    public TResult? Result { get; }
    public OutcomeArguments(ResilienceContext context, Outcome<TResult> outcome, TArgs arguments);
}
