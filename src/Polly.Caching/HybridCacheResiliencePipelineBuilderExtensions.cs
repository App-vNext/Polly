using System.Diagnostics.CodeAnalysis;
using Polly.Caching;

namespace Polly;

/// <summary>
/// Extensions for integrating HybridCache with <see cref="ResiliencePipelineBuilder"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public static class HybridCacheResiliencePipelineBuilderExtensions
{
    /// <summary>
    /// Adds a HybridCache-based caching strategy to an untyped resilience pipeline.
    /// </summary>
    /// <param name="builder">The pipeline builder.</param>
    /// <param name="options">The HybridCache strategy options.</param>
    /// <returns>The same builder instance.</returns>
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "Options are validated and all members preserved.")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(HybridCacheStrategyOptions))]
    public static ResiliencePipelineBuilder AddHybridCache(this ResiliencePipelineBuilder builder, HybridCacheStrategyOptions options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddStrategy(
            _ => new HybridCacheResilienceStrategy<object>(options),
            options);
    }

    /// <summary>
    /// Adds a HybridCache-based caching strategy to a typed resilience pipeline producing TResult.
    /// </summary>
    /// <typeparam name="TResult">The result type of the pipeline.</typeparam>
    /// <param name="builder">The typed pipeline builder.</param>
    /// <param name="options">The HybridCache strategy options.</param>
    /// <returns>The same typed builder instance.</returns>
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "Options are validated and all members preserved.")]
    public static ResiliencePipelineBuilder<TResult> AddHybridCache<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(
        this ResiliencePipelineBuilder<TResult> builder,
        HybridCacheStrategyOptions<TResult> options)
    {
        Guard.NotNull(builder);
        Guard.NotNull(options);

        return builder.AddStrategy(
            _ => new HybridCacheResilienceStrategy<TResult>(options),
            options);
    }
}
