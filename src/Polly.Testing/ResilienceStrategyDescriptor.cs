namespace Polly.Testing;

/// <summary>
/// This class provides additional information about <see cref="ResilienceStrategy"/>.
/// </summary>
/// <param name="Options">The options used by the resilience strategy, if any.</param>
/// <param name="Type">The type of strategy as an enum.</param>
/// <param name="StrategyType">The type of the strategy.</param>
public record ResilienceStrategyDescriptor(ResilienceStrategyOptions? Options, ResilienceStrategyType Type, Type StrategyType)
{
}

