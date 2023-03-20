namespace Polly.Builder;

/// <summary>
/// The context used for building an individual resilience strategy.
/// </summary>
public class ResilienceStrategyBuilderContext
{
    internal ResilienceStrategyBuilderContext(
        string builderName,
        ResilienceProperties builderProperties,
        string strategyName,
        string strategyType)
    {
        BuilderName = Guard.NotNull(builderName);
        BuilderProperties = Guard.NotNull(builderProperties);
        StrategyName = Guard.NotNull(strategyName);
        StrategyType = Guard.NotNull(strategyType);
    }

    /// <summary>
    /// Gets the name of the builder.
    /// </summary>
    public string BuilderName { get; }

    /// <summary>
    /// Gets the custom properties attached to the builder.
    /// </summary>
    public ResilienceProperties BuilderProperties { get; }

    /// <summary>
    /// Gets the name of the strategy.
    /// </summary>
    public string StrategyName { get; }

    /// <summary>
    /// Gets the type of the strategy.
    /// </summary>
    public string StrategyType { get; }
}
