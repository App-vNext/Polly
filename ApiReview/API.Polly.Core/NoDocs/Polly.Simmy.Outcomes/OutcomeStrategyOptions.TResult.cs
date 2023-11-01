// Assembly 'Polly.Core'

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Polly.Simmy.Outcomes;

public class OutcomeStrategyOptions<TResult> : MonkeyStrategyOptions
{
    public Func<OnOutcomeInjectedArguments<TResult>, ValueTask>? OnOutcomeInjected { get; set; }
    [Required]
    public Func<OutcomeGeneratorArguments, ValueTask<Outcome<TResult>?>> OutcomeGenerator { get; set; }
    public OutcomeStrategyOptions();
}
