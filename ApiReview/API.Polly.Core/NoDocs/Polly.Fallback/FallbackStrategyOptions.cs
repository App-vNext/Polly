// Assembly 'Polly.Core'

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Polly.Fallback;

public class FallbackStrategyOptions<TResult> : ResilienceStrategyOptions
{
    [Required]
    public Func<FallbackPredicateArguments<TResult>, ValueTask<bool>> ShouldHandle { get; set; }
    [Required]
    public Func<FallbackActionArguments<TResult>, ValueTask<Outcome<TResult>>>? FallbackAction { get; set; }
    public Func<OnFallbackArguments<TResult>, ValueTask>? OnFallback { get; set; }
    public FallbackStrategyOptions();
}
