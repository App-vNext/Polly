// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly.Simmy.Outcomes;

public readonly struct OnOutcomeInjectedArguments<TResult>
{
    public ResilienceContext Context { get; }
    public Outcome<TResult> Outcome { get; }
    public OnOutcomeInjectedArguments(ResilienceContext context, Outcome<TResult> outcome);
}
