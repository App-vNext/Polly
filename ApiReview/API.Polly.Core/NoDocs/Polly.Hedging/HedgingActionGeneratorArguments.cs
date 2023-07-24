// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Polly.Hedging;

public readonly struct HedgingActionGeneratorArguments<TResult>
{
    public ResilienceContext PrimaryContext { get; }
    public ResilienceContext ActionContext { get; }
    public int Attempt { get; }
    public Func<ResilienceContext, ValueTask<Outcome<TResult>>> Callback { get; }
    public HedgingActionGeneratorArguments(ResilienceContext primaryContext, ResilienceContext actionContext, int attempt, Func<ResilienceContext, ValueTask<Outcome<TResult>>> callback);
}
