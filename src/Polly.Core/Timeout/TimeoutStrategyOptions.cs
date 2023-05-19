using System;
using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

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
    /// By default, the value is set to <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> thus making the timeout strategy disabled.
    /// </remarks>
    [Timeout]
    public TimeSpan Timeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;

    /// <summary>
    /// Gets or sets the timeout generator that generates the timeout for a given execution.
    /// </summary>
    /// <remarks>
    /// By default, the generator is empty and the <see cref="Timeout"/> is used by default.
    /// If generator returns a <see cref="TimeSpan"/> value that is less or equal to <see cref="TimeSpan.Zero"/>
    /// its value is ignored and <see cref="Timeout"/> is used instead.
    /// <para>
    /// Return <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> to disable the timeout for the given execution.
    /// </para>
    /// </remarks>
    [Required]
    public NoOutcomeGenerator<TimeoutGeneratorArguments, TimeSpan> TimeoutGenerator { get; set; } = new();

    /// <summary>
    /// Gets or sets the timeout event that notifies the timeout occurred.
    /// </summary>
    [Required]
    public NoOutcomeEvent<OnTimeoutArguments> OnTimeout { get; set; } = new();
}
