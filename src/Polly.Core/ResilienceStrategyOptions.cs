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
    /// This name uniquely identifies a particular instance of a specific strategy.
    /// This property is also included in the telemetry that is produced by the individual resilience strategies.
    /// </remarks>
    /// <value>The default value is <see langword="null"/>.</value>
    public string? Name { get; set; }
}
