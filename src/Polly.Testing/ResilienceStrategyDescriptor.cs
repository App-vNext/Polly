namespace Polly.Testing;

/// <summary>
/// This class provides additional information about a <see cref="ResilienceStrategy"/>.
/// </summary>
public sealed class ResilienceStrategyDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResilienceStrategyDescriptor"/> class.
    /// </summary>
    /// <param name="options">The options used by the resilience strategy, if any.</param>
    /// <param name="strategyType">The type of the strategy.</param>
    public ResilienceStrategyDescriptor(ResilienceStrategyOptions? options, Type strategyType)
    {
        Options = options;
        StrategyType = strategyType;
    }

    /// <summary>
    /// Gets the options used by the resilience strategy, if any.
    /// </summary>
    public ResilienceStrategyOptions? Options { get; }

    /// <summary>
    /// Gets the type of the strategy.
    /// </summary>
    public Type StrategyType { get; }
}
