using System.Diagnostics.CodeAnalysis;
using Polly.Caching;

namespace Polly;

/// <summary>
/// Extensions for adding caching to <see cref="ResiliencePipelineBuilder"/>.
/// </summary>
public static class CacheResiliencePipelineBuilderExtensions
{
	/// <summary>
	/// Adds a caching strategy.
	/// </summary>
	/// <param name="builder">The builder instance.</param>
	/// <param name="options">The caching options.</param>
	/// <returns>The builder instance.</returns>
	[UnconditionalSuppressMessage(
		"Trimming",
		"IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
		Justification = "All options members preserved.")]
	[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CacheStrategyOptions))]
	public static ResiliencePipelineBuilder AddCache(this ResiliencePipelineBuilder builder, CacheStrategyOptions options)
	{
		Guard.NotNull(builder);
		Guard.NotNull(options);

		return builder.AddStrategy(
			_ => new CacheResilienceStrategy<object>(options),
			options);
	}

	/// <summary>
	/// Adds a caching strategy for typed pipelines.
	/// </summary>
	/// <typeparam name="TResult">The result type handled by the pipeline.</typeparam>
	/// <param name="builder">The typed builder instance.</param>
	/// <param name="options">The caching options.</param>
	/// <returns>The typed builder instance.</returns>
	[UnconditionalSuppressMessage(
		"Trimming",
		"IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
		Justification = "All options members preserved.")]
	public static ResiliencePipelineBuilder<TResult> AddCache<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResult>(
		this ResiliencePipelineBuilder<TResult> builder,
		CacheStrategyOptions<TResult> options)
	{
		Guard.NotNull(builder);
		Guard.NotNull(options);

		return builder.AddStrategy(
			_ => new CacheResilienceStrategy<TResult>(options),
			options);
	}
}
