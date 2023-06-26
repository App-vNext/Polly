// Assembly 'Polly.Core'

using Polly.Fallback;

namespace Polly;

public static class FallbackResilienceStrategyBuilderExtensions
{
    public static ResilienceStrategyBuilder<TResult> AddFallback<TResult>(this ResilienceStrategyBuilder<TResult> builder, FallbackStrategyOptions<TResult> options);
}
