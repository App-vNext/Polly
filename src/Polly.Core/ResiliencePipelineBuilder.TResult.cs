using System.ComponentModel.DataAnnotations;

namespace Polly;

/// <summary>
/// A builder that is used to create an instance of <see cref="ResiliencePipeline{TResult}"/>.
/// </summary>
/// <typeparam name="TResult">The type of result to handle.</typeparam>
/// <remarks>
/// The builder supports combining multiple strategies into a pipeline of resilience strategies.
/// The resulting instance of <see cref="ResiliencePipeline{TResult}"/> created by the <see cref="Build"/> call will execute the strategies in the same order they were added to the builder.
/// The order of the strategies is important.
/// </remarks>
public sealed class ResiliencePipelineBuilder<TResult> : ResiliencePipelineBuilderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResiliencePipelineBuilder{TResult}"/> class.
    /// </summary>
    public ResiliencePipelineBuilder()
    {
    }

    internal ResiliencePipelineBuilder(ResiliencePipelineBuilderBase other)
        : base(other)
    {
    }

    /// <summary>
    /// Builds the resilience pipeline.
    /// </summary>
    /// <returns>An instance of <see cref="ResiliencePipeline{TResult}"/>.</returns>
    /// <exception cref="ValidationException">Thrown when this builder has invalid configuration.</exception>
    public ResiliencePipeline<TResult> Build() => new(BuildPipelineComponent(), DisposeBehavior.Allow);
}
