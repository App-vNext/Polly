namespace Polly.Simmy.Latency;

#pragma warning disable CS8618 // Required members are not initialized in constructor since this is a DTO, default value is null

/// <summary>
/// Represents the options for the latency chaos strategy.
/// </summary>
public class ChaosLatencyStrategyOptions : ChaosStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChaosLatencyStrategyOptions"/> class.
    /// </summary>
    public ChaosLatencyStrategyOptions() => Name = ChaosLatencyConstants.DefaultName;

    /// <summary>
    /// Gets or sets the delegate that's raised when a latency is injected.
    /// </summary>
    /// <value>
    /// Defaults to <see langword="null"/>.
    /// </value>
    public Func<OnLatencyInjectedArguments, ValueTask>? OnLatencyInjected { get; set; }

    /// <summary>
    /// Gets or sets the latency generator that generates the delay for a given execution.
    /// </summary>
    /// <value>
    /// Defaults to <see langword="null"/>. When this property is <see langword="null"/> the <see cref="Latency"/> is used.
    /// </value>
    public Func<LatencyGeneratorArguments, ValueTask<TimeSpan>>? LatencyGenerator { get; set; }

    /// <summary>
    /// Gets or sets the latency to be injected for a given execution.
    /// </summary>
    /// <value>
    /// Defaults to <c>30</c> seconds. When <see cref="LatencyGenerator"/> is specified, this property is ignored.
    /// </value>
    public TimeSpan Latency { get; set; } = ChaosLatencyConstants.DefaultLatency;
}
