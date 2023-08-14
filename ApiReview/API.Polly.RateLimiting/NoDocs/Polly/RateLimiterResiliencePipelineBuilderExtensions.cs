// Assembly 'Polly.RateLimiting'

using System.Diagnostics.CodeAnalysis;
using System.Threading.RateLimiting;
using Polly.RateLimiting;

namespace Polly;

public static class RateLimiterResiliencePipelineBuilderExtensions
{
    public static TBuilder AddConcurrencyLimiter<TBuilder>(this TBuilder builder, int permitLimit, int queueLimit = 0) where TBuilder : ResiliencePipelineBuilderBase;
    public static TBuilder AddConcurrencyLimiter<TBuilder>(this TBuilder builder, ConcurrencyLimiterOptions options) where TBuilder : ResiliencePipelineBuilderBase;
    public static TBuilder AddRateLimiter<TBuilder>(this TBuilder builder, RateLimiter limiter) where TBuilder : ResiliencePipelineBuilderBase;
    public static TBuilder AddRateLimiter<TBuilder>(this TBuilder builder, RateLimiterStrategyOptions options) where TBuilder : ResiliencePipelineBuilderBase;
}
