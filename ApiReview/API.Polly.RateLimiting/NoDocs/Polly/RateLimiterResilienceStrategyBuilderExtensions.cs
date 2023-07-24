// Assembly 'Polly.RateLimiting'

using System.Diagnostics.CodeAnalysis;
using System.Threading.RateLimiting;
using Polly.RateLimiting;

namespace Polly;

public static class RateLimiterResilienceStrategyBuilderExtensions
{
    public static TBuilder AddConcurrencyLimiter<TBuilder>(this TBuilder builder, int permitLimit, int queueLimit = 0) where TBuilder : ResilienceStrategyBuilderBase;
    public static TBuilder AddConcurrencyLimiter<TBuilder>(this TBuilder builder, ConcurrencyLimiterOptions options) where TBuilder : ResilienceStrategyBuilderBase;
    public static TBuilder AddRateLimiter<TBuilder>(this TBuilder builder, RateLimiter limiter) where TBuilder : ResilienceStrategyBuilderBase;
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(RateLimiterStrategyOptions))]
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "All options members are preserved.")]
    public static TBuilder AddRateLimiter<TBuilder>(this TBuilder builder, RateLimiterStrategyOptions options) where TBuilder : ResilienceStrategyBuilderBase;
}
