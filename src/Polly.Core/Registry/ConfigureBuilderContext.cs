namespace Polly.Registry;

/// <summary>
/// The context used by <see cref="ResilienceStrategyRegistry{TKey}"/>.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public class ConfigureBuilderContext<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureBuilderContext{TKey}"/> class.
    /// </summary>
    /// <param name="strategyKey">The strategy key.</param>
    /// <param name="builderName">The builder name.</param>
    /// <param name="strategyKeyString">The strategy key as string.</param>
    public ConfigureBuilderContext(TKey strategyKey, string builderName, string strategyKeyString)
    {
        StrategyKey = strategyKey;
        BuilderName = builderName;
        StrategyKeyString = strategyKeyString;
    }

    /// <summary>
    /// Gets the strategy key for the strategy being created.
    /// </summary>
    public TKey StrategyKey { get; }

    /// <summary>
    /// Gets the builder name for the builder being used to create the strategy.
    /// </summary>
    public string BuilderName { get; }

    /// <summary>
    /// Gets the string representation of strategy key for the strategy being created.
    /// </summary>
    public string StrategyKeyString { get; }
}
