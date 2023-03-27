using System;
using System.ComponentModel.DataAnnotations;
using Polly.Builder;

namespace Polly.Timeout;

/// <summary>
/// Repreents the options for the timeout strategy.
/// </summary>
public class TimeoutStrategyOptions : ResilienceStrategyOptions
{
    /// <summary>
    /// Gets the value that represent the infinite timeout.
    /// </summary>
    /// <remarks>
    /// When the timeout is set to infinite the timeout resilience strategy is effectively disabled.
    /// This value is the same as <see cref="System.Threading.Timeout.InfiniteTimeSpan"/>.
    /// </remarks>
    public static readonly TimeSpan InfiniteTimeout = System.Threading.Timeout.InfiniteTimeSpan;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutStrategyOptions"/> class.
    /// </summary>
    public TimeoutStrategyOptions() => StrategyType = TimeoutConstants.StrategyType;

    /// <summary>
    /// Gets or sets the default timeout.
    /// </summary>
    /// <remarks>
    /// By default, the value is set to <see cref="InfiniteTimeout"/> thus making the timeout strategy disabled.
    /// </remarks>
    [Timeout]
    public TimeSpan Timeout { get; set; } = InfiniteTimeout;

    /// <summary>
    /// Gets or sets the timeout generator that generates the timeout for a given execution.
    /// </summary>
    /// <remarks>
    /// By default, the generator is empty and the <see cref="Timeout"/> is used by default.
    /// If generator returns a <see cref="TimeSpan"/> value that is less or equal to <see cref="TimeSpan.Zero"/>
    /// its value is ignored and <see cref="Timeout"/> is used instead.
    /// <para>
    /// Return <see cref="InfiniteTimeout"/> to disable the timeout for the given execution.
    /// </para>
    /// </remarks>
    [Required]
    public TimeoutGenerator TimeoutGenerator { get; set; } = new();

    /// <summary>
    /// Gets or sets the timeout event that notifies the timeout occurred.
    /// </summary>
    [Required]
    public OnTimeoutEvent OnTimeout { get; set; } = new();
}
