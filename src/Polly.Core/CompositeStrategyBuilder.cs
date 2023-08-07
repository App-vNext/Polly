using System.ComponentModel.DataAnnotations;

namespace Polly;

/// <summary>
/// A builder that is used to create an instance of <see cref="ResilienceStrategy"/>.
/// </summary>
/// <remarks>
/// The builder supports combining multiple strategies into a composite resilience strategy.
/// The resulting instance of <see cref="ResilienceStrategy"/> created by the <see cref="Build"/> call will execute the strategies in the same order they were added to the builder.
/// The order of the strategies is important.
/// </remarks>
public sealed class CompositeStrategyBuilder : CompositeStrategyBuilderBase<object>
{
    /// <summary>
    /// Builds the resilience strategy.
    /// </summary>
    /// <returns>An instance of <see cref="ResilienceStrategy"/>.</returns>
    /// <exception cref="ValidationException">Thrown when this builder has invalid configuration.</exception>
    public ResilienceStrategy Build() => new(BuildStrategy());
}
