// Assembly 'Polly.Core'

using System;
using System.Threading.Tasks;
using Polly.Fallback;

namespace Polly;

public static class FallbackResilienceStrategyBuilderExtensions
{
    public static ResilienceStrategyBuilder<TResult> AddFallback<TResult>(this ResilienceStrategyBuilder<TResult> builder, Action<PredicateBuilder<TResult>> shouldHandle, Func<OutcomeArguments<TResult, HandleFallbackArguments>, ValueTask<Outcome<TResult>>> fallbackAction);
    public static ResilienceStrategyBuilder<TResult> AddFallback<TResult>(this ResilienceStrategyBuilder<TResult> builder, FallbackStrategyOptions<TResult> options);
}
