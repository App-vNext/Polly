namespace Polly.Builder;

/// <summary>
/// The context used for building an individual resilience strategy.
/// </summary>
public class ResilienceStrategyBuilderContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResilienceStrategyBuilderContext"/> class.
    /// </summary>
    /// <param name="builderName">The name of the builder.</param>
    /// <param name="strategyName">The strategy name.</param>
    /// <param name="strategyType">The strategy type.</param>
    public ResilienceStrategyBuilderContext(string builderName, string strategyName, string strategyType)
    {
        BuilderName = Guard.NotNull(builderName);
        StrategyName = Guard.NotNull(strategyName);
        StrategyType = Guard.NotNull(strategyType);
    }

    /// <summary>
    /// Gets the name of the builder.
    /// </summary>
    public string BuilderName { get; }

    /// <summary>
    /// Gets the name of the strategy.
    /// </summary>
    public string StrategyName { get; }

    /// <summary>
    /// Gets the type of the strategy.
    /// </summary>
    public string StrategyType { get; }
}
