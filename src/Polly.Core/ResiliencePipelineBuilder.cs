using System.ComponentModel.DataAnnotations;

namespace Polly;

/// <summary>
/// A builder that is used to create an instance of <see cref="ResiliencePipeline"/>.
/// </summary>
/// <remarks>
/// The builder supports combining multiple strategies into a pipeline of resilience strategies.
/// The resulting instance of <see cref="ResiliencePipeline"/> created by the <see cref="Build"/> call executes the strategies in the same order they were added to the builder.
/// The order of the strategies is important.
/// </remarks>
public sealed class ResiliencePipelineBuilder : ResiliencePipelineBuilderBase
{
    /// <summary>
    /// Builds the resilience pipeline.
    /// </summary>
    /// <returns>An instance of <see cref="ResiliencePipeline"/>.</returns>
    /// <exception cref="ValidationException">Thrown when this builder has invalid configuration.</exception>
    public ResiliencePipeline Build() => new(BuildPipelineComponent(), DisposeBehavior.Allow, ResilienceContextPool);
}
