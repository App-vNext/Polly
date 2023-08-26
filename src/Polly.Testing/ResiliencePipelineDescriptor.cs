namespace Polly.Testing;

/// <summary>
/// Describes the resilience pipeline.
/// </summary>
public sealed class ResiliencePipelineDescriptor
{
    internal ResiliencePipelineDescriptor(IReadOnlyList<ResilienceStrategyDescriptor> strategies, bool isReloadable)
    {
        Strategies = strategies;
        IsReloadable = isReloadable;
    }

    /// <summary>
    /// Gets the strategies the pipeline is composed of.
    /// </summary>
    public IReadOnlyList<ResilienceStrategyDescriptor> Strategies { get; }

    /// <summary>
    /// Gets the first strategy of the pipeline.
    /// </summary>
    public ResilienceStrategyDescriptor FirstStrategy => Strategies[0];

    /// <summary>
    /// Gets a value indicating whether the resilience pipeline is reloadable.
    /// </summary>
    public bool IsReloadable { get; }
}
