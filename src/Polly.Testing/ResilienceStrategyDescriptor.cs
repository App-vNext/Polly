namespace Polly.Testing;

/// <summary>
/// This class provides additional information about a <see cref="ResilienceStrategy"/>.
/// </summary>
/// <param name="Options">The options used by the resilience strategy, if any.</param>
/// <param name="StrategyType">The type of the strategy.</param>
public record ResilienceStrategyDescriptor(ResilienceStrategyOptions? Options, Type StrategyType)
{
}
