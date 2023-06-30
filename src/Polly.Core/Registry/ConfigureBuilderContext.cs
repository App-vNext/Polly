using System.ComponentModel;

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

    internal Func<Func<CancellationToken>>? ReloadTokenProducer { get; private set; }

    /// <summary>
    /// Enables dynamic reloading of the strategy retrieved from <see cref="ResilienceStrategyRegistry{TKey}"/>.
    /// </summary>
    /// <param name="tokenProducerFactory">The producer of <see cref="CancellationToken"/> that is triggered when change occurs.</param>
    /// <remarks>
    /// The <paramref name="tokenProducerFactory"/> should always return function that returns a new <see cref="CancellationToken"/> instance when invoked otherwise
    /// the reload infrastructure will stop listening for changes. The <paramref name="tokenProducerFactory"/> is called only once for each streategy.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void EnableReloads(Func<Func<CancellationToken>> tokenProducerFactory)
    {
        Guard.NotNull(tokenProducerFactory);

        ReloadTokenProducer = tokenProducerFactory;
    }
}
