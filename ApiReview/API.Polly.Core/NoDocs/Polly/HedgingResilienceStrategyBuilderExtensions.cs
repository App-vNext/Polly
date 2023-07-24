// Assembly 'Polly.Core'

using System.Diagnostics.CodeAnalysis;
using Polly.Hedging;

namespace Polly;

public static class HedgingResilienceStrategyBuilderExtensions
{
    public static ResilienceStrategyBuilder<TResult> AddHedging<TResult>(this ResilienceStrategyBuilder<TResult> builder, HedgingStrategyOptions<TResult> options);
}
