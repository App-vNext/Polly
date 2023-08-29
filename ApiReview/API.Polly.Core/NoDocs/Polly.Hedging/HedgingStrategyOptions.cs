// Assembly 'Polly.Core'

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Polly.Hedging;

public class HedgingStrategyOptions<TResult> : ResilienceStrategyOptions
{
    public TimeSpan Delay { get; set; }
    [Range(1, 10)]
    public int MaxHedgedAttempts { get; set; }
    [Required]
    public Func<HedgingPredicateArguments<TResult>, ValueTask<bool>> ShouldHandle { get; set; }
    [Required]
    public Func<HedgingActionGeneratorArguments<TResult>, Func<ValueTask<Outcome<TResult>>>?> ActionGenerator { get; set; }
    public Func<HedgingDelayGeneratorArguments, ValueTask<TimeSpan>>? DelayGenerator { get; set; }
    public Func<OnHedgingArguments<TResult>, ValueTask>? OnHedging { get; set; }
    public HedgingStrategyOptions();
}
