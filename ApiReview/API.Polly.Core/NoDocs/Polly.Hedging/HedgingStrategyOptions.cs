// Assembly 'Polly.Core'

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Polly.Hedging;

public class HedgingStrategyOptions<TResult> : ResilienceStrategyOptions
{
    public sealed override string StrategyType { get; }
    public TimeSpan HedgingDelay { get; set; }
    [Range(2, 10)]
    public int MaxHedgedAttempts { get; set; }
    [Required]
    public Func<OutcomeArguments<TResult, HandleHedgingArguments>, ValueTask<bool>>? ShouldHandle { get; set; }
    [Required]
    public Func<HedgingActionGeneratorArguments<TResult>, Func<ValueTask<Outcome<TResult>>>?> HedgingActionGenerator { get; set; }
    public Func<HedgingDelayArguments, ValueTask<TimeSpan>>? HedgingDelayGenerator { get; set; }
    public Func<OutcomeArguments<TResult, OnHedgingArguments>, ValueTask>? OnHedging { get; set; }
    public HedgingStrategyOptions();
}
