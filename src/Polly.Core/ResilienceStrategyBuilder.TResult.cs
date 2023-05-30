using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

namespace Polly;

/// <summary>
/// A builder that is used to create an instance of <see cref="ResilienceStrategy{TResult}"/>.
/// </summary>
/// <typeparam name="TResult">The type of result to handle.</typeparam>
/// <remarks>
/// The builder supports chaining multiple strategies into a pipeline of strategies.
/// The resulting instance of <see cref="ResilienceStrategy{TResult}"/> created by the <see cref="Build"/> call will execute the strategies in the same order they were added to the builder.
/// The order of the strategies is important.
/// </remarks>
public sealed class ResilienceStrategyBuilder<TResult> : ResilienceStrategyBuilderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResilienceStrategyBuilder{TResult}"/> class.
    /// </summary>
    public ResilienceStrategyBuilder()
    {
    }

    internal ResilienceStrategyBuilder(ResilienceStrategyBuilderBase other)
        : base(other)
    {
    }

    internal override bool IsGenericBuilder => true;

    /// <summary>
    /// Builds the resilience strategy.
    /// </summary>
    /// <returns>An instance of <see cref="ResilienceStrategy{TResult}"/>.</returns>
    /// <exception cref="ValidationException">Thrown when this builder has invalid configuration.</exception>
    public ResilienceStrategy<TResult> Build() => new(BuildStrategy());
}
