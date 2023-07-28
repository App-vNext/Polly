// Assembly 'Polly.Core'

using System.Diagnostics.CodeAnalysis;
using Polly.Hedging;

namespace Polly;

public static class HedgingCompositeStrategyBuilderExtensions
{
    public static CompositeStrategyBuilder<TResult> AddHedging<TResult>(this CompositeStrategyBuilder<TResult> builder, HedgingStrategyOptions<TResult> options);
}
