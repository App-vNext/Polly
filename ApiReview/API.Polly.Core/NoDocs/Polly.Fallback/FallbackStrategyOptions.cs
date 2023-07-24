// Assembly 'Polly.Core'

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Polly.Fallback;

public class FallbackStrategyOptions<TResult> : ResilienceStrategyOptions
{
    [Required]
    public Func<OutcomeArguments<TResult, FallbackPredicateArguments>, ValueTask<bool>> ShouldHandle { get; set; }
    [Required]
    public Func<OutcomeArguments<TResult, FallbackPredicateArguments>, ValueTask<Outcome<TResult>>>? FallbackAction { get; set; }
    public Func<OutcomeArguments<TResult, OnFallbackArguments>, ValueTask>? OnFallback { get; set; }
    public FallbackStrategyOptions();
}
