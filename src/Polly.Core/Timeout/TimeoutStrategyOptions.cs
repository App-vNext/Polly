using System;
using System.ComponentModel.DataAnnotations;
using Polly.Builder;

namespace Polly.Timeout;

/// <summary>
/// The options for the timeout strategy.
/// </summary>
public class TimeoutStrategyOptions : ResilienceStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutStrategyOptions"/> class.
    /// </summary>
    public TimeoutStrategyOptions() => StrategyType = TimeoutConstants.StrategyType;

    /// <summary>
    /// Gets or sets the default timeout.
    /// </summary>
    /// <remarks>
    /// By default, the value is set to <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> thus making the timeout strategy disabled.
    /// </remarks>
    [Timeout]
    public TimeSpan Timeout { get; set; } = TimeoutConstants.InfiniteTimeout;

    /// <summary>
    /// Gets or sets the timeout generator that generates the timeout for a given execution.
    /// </summary>
    /// <remarks>
    /// By default, the generator is empty and the <see cref="Timeout"/> is used by default.
    /// If generator returns a <see cref="TimeSpan"/> value that is less or equal to <see cref="TimeSpan.Zero"/>
    /// its value is ignored and <see cref="TimeSpan"/> is used instead.
    ///
    /// Return <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> to disable the timeout for the given execution.
    /// </remarks>
    [Required]
    public TimeoutGenerator TimeoutGenerator { get; set; } = new();

    /// <summary>
    /// Gets or sets the timeout event that notifies the timeout occurred.
    /// </summary>
    [Required]
    public OnTimeoutEvent OnTimeout { get; set; } = new();
}
