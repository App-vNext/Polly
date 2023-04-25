using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

namespace Polly.Hedging;

/// <summary>
/// Hedging strategy options.
/// </summary>
public class HedgingStrategyOptions : ResilienceStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HedgingStrategyOptions"/> class.
    /// </summary>
    public HedgingStrategyOptions() => StrategyType = HedgingConstants.StrategyType;

    /// <summary>
    /// A <see cref="TimeSpan"/> that represents the infinite hedging delay.
    /// </summary>
    public static readonly TimeSpan InfiniteHedgingDelay = TimeSpan.FromMilliseconds(-1);

    /// <summary>
    /// Gets or sets the minimal time of waiting before spawning a new hedged call.
    /// </summary>
    /// <remarks>
    /// Default is set to 2 seconds.
    ///
    /// You can also use <see cref="TimeSpan.Zero"/> to create all hedged tasks (value of <see cref="MaxHedgedAttempts"/>) at once
    /// or <see cref="InfiniteHedgingDelay"/> to force the hedging strategy to never create new task before the old one is finished.
    ///
    /// If you want a greater control over hedging delay customization use <see cref="HedgingDelayGenerator"/>.
    /// </remarks>
    public TimeSpan HedgingDelay { get; set; } = HedgingConstants.DefaultHedgingDelay;

    /// <summary>
    /// Gets or sets the maximum hedged attempts to perform the desired task.
    /// </summary>
    /// <remarks>
    /// Default set to 2.
    /// The value defines how many concurrent hedged tasks will be triggered by the strategy.
    /// This includes the primary hedged task that is initially performed, and the further tasks that will
    /// be fetched from the provider and spawned in parallel.
    /// The value must be bigger or equal to 2, and lower or equal to 10.
    /// </remarks>
    [Range(HedgingConstants.MinimumHedgedAttempts, HedgingConstants.MaximumHedgedAttempts)]
    public int MaxHedgedAttempts { get; set; } = HedgingConstants.DefaultMaxHedgedAttempts;

    /// <summary>
    /// Gets or sets the hedging handler that manages hedging operations for multiple result types.
    /// </summary>
    [Required]
    public HedgingHandler Handler { get; set; } = new();

    /// <summary>
    /// Gets or sets the generator that generates hedging delays for each hedging attempt.
    /// </summary>
    /// <remarks>
    /// The <see cref="HedgingDelayGenerator"/> takes precedence over <see cref="HedgingDelay"/>. If specified, the <see cref="HedgingDelay"/> is ignored.
    ///
    /// By default, this generator is empty and does not have on hedging delays.
    /// </remarks>
    [Required]
    public NoOutcomeGenerator<HedgingDelayArguments, TimeSpan> HedgingDelayGenerator { get; set; } = new();
}
