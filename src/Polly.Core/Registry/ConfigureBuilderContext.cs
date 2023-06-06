namespace Polly.Registry;

/// <summary>
/// The context used by <see cref="ResilienceStrategyRegistry{TKey}"/>.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public class ConfigureBuilderContext<TKey>
    where TKey : notnull
{
    internal ConfigureBuilderContext(TKey strategyKey, string builderName, string strategyKeyString)
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

    internal Func<CancellationToken>? ReloadTokenProducer { get; private set; }

    /// <summary>
    /// Enables dynamic reloading of strategy retrieved from <see cref="ResilienceStrategyRegistry{TKey}"/>.
    /// </summary>
    /// <param name="reloadTokenProducer">The producer of <see cref="CancellationToken"/> that is triggered when change occurs.</param>
    /// <remarks>
    /// The <paramref name="reloadTokenProducer"/> should always return a new <see cref="CancellationToken"/> instance when invoked otherwise
    /// the reload infrastructure will stop listening for changes.
    /// </remarks>
    public void EnableReloads(Func<CancellationToken> reloadTokenProducer)
    {
        Guard.NotNull(reloadTokenProducer);

        ReloadTokenProducer = reloadTokenProducer;
    }
}
