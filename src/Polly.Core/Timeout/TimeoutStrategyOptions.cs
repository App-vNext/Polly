using System;
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
