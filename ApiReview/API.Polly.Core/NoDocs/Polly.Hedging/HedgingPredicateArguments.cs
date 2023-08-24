// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;
using Polly.Utils;

namespace Polly.Hedging;

public readonly struct HedgingPredicateArguments<TResult>
{
    public Outcome<TResult> Outcome { get; }
    public ResilienceContext Context { get; }
    public HedgingPredicateArguments(ResilienceContext context, Outcome<TResult> outcome);
}
