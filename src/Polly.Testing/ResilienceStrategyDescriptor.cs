namespace Polly.Testing;

/// <summary>
/// This class provides additional information about a <see cref="ResiliencePipeline"/>.
/// </summary>
public sealed class ResilienceStrategyDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResilienceStrategyDescriptor"/> class.
    /// </summary>
    /// <param name="options">The options used by the resilience strategy, if any.</param>
    /// <param name="strategyInstance">The strategy instance.</param>
    public ResilienceStrategyDescriptor(ResilienceStrategyOptions? options, object strategyInstance)
    {
        Options = options;
        StrategyInstance = strategyInstance;
    }

    /// <summary>
    /// Gets the options used by the resilience strategy, if any.
    /// </summary>
    public ResilienceStrategyOptions? Options { get; }

    /// <summary>
    /// Gets the strategy instance.
    /// </summary>
    public object StrategyInstance { get; }
}
