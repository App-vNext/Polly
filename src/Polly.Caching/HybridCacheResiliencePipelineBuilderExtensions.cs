using System.Diagnostics.CodeAnalysis;
using Polly.Caching;

namespace Polly;

/// <summary>
/// Extensions for integrating HybridCache with typed <see cref="ResiliencePipelineBuilder{TResult}"/>.
/// </summary>
/// <remarks>
/// This caching strategy is designed for use with typed resilience pipelines only.
/// HybridCache requires concrete types for proper serialization and deserialization.
/// </remarks>
public static class HybridCacheResiliencePipelineBuilderExtensions
{
    /// <summary>
    /// Adds a HybridCache-based caching strategy to a typed resilience pipeline.
    /// </summary>
    /// <typeparam name="TResult">The result type of the pipeline.</typeparam>
    /// <param name="builder">The typed pipeline builder.</param>
    /// <param name="options">The HybridCache strategy options.</param>
    /// <returns>The same typed builder instance.</returns>
    /// <remarks>
    /// This extension method only supports typed pipelines (<see cref="ResiliencePipelineBuilder{TResult}"/>).
    /// For untyped pipelines, consider using a typed pipeline or a different caching strategy.
    /// </remarks>
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise break functionality when trimming application code",
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
