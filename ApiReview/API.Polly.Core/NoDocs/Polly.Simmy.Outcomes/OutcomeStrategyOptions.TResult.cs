// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Polly.Simmy.Outcomes;

public class OutcomeStrategyOptions<TResult> : MonkeyStrategyOptions
{
    public Func<OnOutcomeInjectedArguments<TResult>, ValueTask>? OnOutcomeInjected { get; set; }
    public Func<OutcomeGeneratorArguments, ValueTask<Outcome<TResult>?>>? OutcomeGenerator { get; set; }
    public Outcome<TResult>? Outcome { get; set; }
    public OutcomeStrategyOptions();
}
