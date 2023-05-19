using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

namespace Polly.Hedging;

/// <summary>
/// Hedging strategy options.
/// </summary>
public class HedgingStrategyOptions : ResilienceStrategyOptions
{
    /// <summary>
    /// Gets the strategy type.
    /// </summary>
    /// <remarks>Returns <c>Hedging</c> value.</remarks>
    public sealed override string StrategyType => HedgingConstants.StrategyType;

    /// <summary>
    /// Gets or sets the minimal time of waiting before spawning a new hedged call.
    /// </summary>
    /// <remarks>
    /// Defaults to 2 seconds.
    /// <para>
    /// You can also use <see cref="TimeSpan.Zero"/> to create all hedged tasks (value of <see cref="MaxHedgedAttempts"/>) at once
    /// or <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> to force the hedging strategy to never create new task before the old one is finished.
    /// </para>
    /// <para>
    /// If you want a greater control over hedging delay customization use <see cref="HedgingDelayGenerator"/>.
    /// </para>
    /// </remarks>
    public TimeSpan HedgingDelay { get; set; } = HedgingConstants.DefaultHedgingDelay;

    /// <summary>
    /// Gets or sets the maximum hedged attempts to perform the desired task.
    /// </summary>
    /// <remarks>
    /// Defaults to 2. The value must be bigger or equal to 2, and lower or equal to 10.
    /// <para>
    /// The value defines how many concurrent hedged tasks will be triggered by the strategy.
    /// This includes the primary hedged task that is initially performed, and the further tasks that will
    /// be fetched from the provider and spawned in parallel.
    /// </para>
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
    /// <para>By default, this generator is empty and does not have any custom hedging delays.</para>
    /// </remarks>
    [Required]
    public NoOutcomeGenerator<HedgingDelayArguments, TimeSpan> HedgingDelayGenerator { get; set; } = new();

    /// <summary>
    /// Gets or sets the event that is triggered when a hedging is performed.
    /// </summary>
    /// <remarks>
    /// This property is required. By default, this event is empty.
    /// </remarks>
    [Required]
    public OutcomeEvent<OnHedgingArguments> OnHedging { get; set; } = new();
}
