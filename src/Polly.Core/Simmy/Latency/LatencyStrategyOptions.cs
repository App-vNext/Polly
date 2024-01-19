namespace Polly.Simmy.Latency;

#pragma warning disable CS8618 // Required members are not initialized in constructor since this is a DTO, default value is null

/// <summary>
/// Represents the options for the Latency chaos strategy.
/// </summary>
public class LatencyStrategyOptions : MonkeyStrategyOptions
{
    /// <summary>
    /// Gets or sets the delegate that's raised when a delay occurs.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    public Func<OnLatencyInjectedArguments, ValueTask>? OnLatencyInjected { get; set; }

    /// <summary>
    /// Gets or sets the latency generator that generates the delay for a given execution.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>. Either <see cref="Latency"/> or this property is required.
    /// When this property is <see langword="null"/> the <see cref="Latency"/> is used.
    /// </remarks>
    public Func<LatencyGeneratorArguments, ValueTask<TimeSpan>>? LatencyGenerator { get; set; }

    /// <summary>
    /// Gets or sets the delay for a given execution.
    /// </summary>
    /// <remarks>
    /// Defaults to 30 seconds. Either <see cref="LatencyGenerator"/> or this property is required.
    /// </remarks>
    public TimeSpan Latency { get; set; } = LatencyConstants.DefaultLatency;
}
