// Assembly 'Polly.RateLimiting'

using System.Threading.RateLimiting;
using Polly.RateLimiting;

namespace Polly;

public static class RateLimiterResilienceStrategyBuilderExtensions
{
    public static TBuilder AddConcurrencyLimiter<TBuilder>(this TBuilder builder, int permitLimit, int queueLimit = 0) where TBuilder : ResilienceStrategyBuilderBase;
    public static TBuilder AddConcurrencyLimiter<TBuilder>(this TBuilder builder, ConcurrencyLimiterOptions options) where TBuilder : ResilienceStrategyBuilderBase;
    public static TBuilder AddRateLimiter<TBuilder>(this TBuilder builder, RateLimiter limiter) where TBuilder : ResilienceStrategyBuilderBase;
    public static TBuilder AddRateLimiter<TBuilder>(this TBuilder builder, RateLimiterStrategyOptions options) where TBuilder : ResilienceStrategyBuilderBase;
}
