// Assembly 'Polly.Core'

using System.Diagnostics.CodeAnalysis;
using Polly.Fallback;

namespace Polly;

public static class FallbackCompositeStrategyBuilderExtensions
{
    public static CompositeStrategyBuilder<TResult> AddFallback<TResult>(this CompositeStrategyBuilder<TResult> builder, FallbackStrategyOptions<TResult> options);
}
