namespace Polly;

/// <summary>
/// The options associated with the <see cref="ResilienceStrategy"/>.
/// </summary>
public abstract class ResilienceStrategyOptions
{
    /// <summary>
    /// Gets or sets the name of the strategy.
    /// </summary>
    /// <remarks>
    /// This name uniquely identifies particular instance of specific strategy.
    /// This property is also included in the telemetry that is produced by the individual resilience strategies.
    /// </remarks>
    /// <value>The default value is <see langword="null"/>.</value>
    public string? StrategyName { get; set; }

    /// <summary>
    /// Gets the strategy type.
    /// </summary>
    /// <remarks>This property is also included in the telemetry that is produced by the individual resilience strategies.
    /// The strategy type uniquely identifies the strategy in the telemetry. The name should be in PascalCase (i.e. Retry, CircuitBreaker, Timeout).</remarks>
    public abstract string StrategyType { get; }
}
