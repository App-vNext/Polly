using System.ComponentModel.DataAnnotations;

namespace Polly.Timeout;

/// <summary>
/// Represents the options for the timeout strategy.
/// </summary>
public class TimeoutStrategyOptions : ResilienceStrategyOptions
{
    /// <summary>
    /// Gets the strategy type.
    /// </summary>
    /// <remarks>Returns <c>Timeout</c> value.</remarks>
    public sealed override string StrategyType => TimeoutConstants.StrategyType;

    /// <summary>
    /// Gets or sets the default timeout.
    /// </summary>
    /// <remarks>
    /// Defaults to 30 seconds. This value must be greater than 1 second and less than 24 hours.
    /// </remarks>
    [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00")]
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the timeout generator that generates the timeout for a given execution.
    /// </summary>
    /// <remarks>
    /// If generator returns a <see cref="TimeSpan"/> value that is less or equal to <see cref="TimeSpan.Zero"/>
    /// its value is ignored and <see cref="Timeout"/> is used instead. When generator is <see langword="null"/> the <see cref="Timeout"/> is used.
    /// <para>
    /// Return <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> to disable the timeout for the given execution.
    /// </para>
    /// <para>
    /// Defaults to <see langword="null"/>.
    /// </para>
    /// </remarks>
    public Func<TimeoutGeneratorArguments, ValueTask<TimeSpan>>? TimeoutGenerator { get; set; }

    /// <summary>
    /// Gets or sets the timeout that's raised when timeout occurs.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    public Func<OnTimeoutArguments, ValueTask>? OnTimeout { get; set; }
}
