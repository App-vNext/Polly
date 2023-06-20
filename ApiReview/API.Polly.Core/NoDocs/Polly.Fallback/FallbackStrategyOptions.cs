// Assembly 'Polly.Core'

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Polly.Fallback;

public class FallbackStrategyOptions<TResult> : ResilienceStrategyOptions
{
    public sealed override string StrategyType { get; }
    [Required]
    public Func<OutcomeArguments<TResult, FallbackPredicateArguments>, ValueTask<bool>> ShouldHandle { get; set; }
    [Required]
    public Func<OutcomeArguments<TResult, FallbackPredicateArguments>, ValueTask<Outcome<TResult>>>? FallbackAction { get; set; }
    public Func<OutcomeArguments<TResult, OnFallbackArguments>, ValueTask>? OnFallback { get; set; }
    public FallbackStrategyOptions();
}
