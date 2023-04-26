using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

namespace Polly.Hedging;

/// <summary>
/// Hedging strategy options.
/// </summary>
/// <typeparam name="TResult">The type of result these hedging options handle.</typeparam>
public class HedgingStrategyOptions<TResult> : ResilienceStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HedgingStrategyOptions{TResult}"/> class.
    /// </summary>
    public HedgingStrategyOptions() => StrategyType = HedgingConstants.StrategyType;

    /// <summary>
    /// Gets or sets the minimal time of waiting before spawning a new hedged call.
    /// </summary>
    /// <remarks>
    /// Default is set to 2 seconds.
    /// <para>
    /// You can also use <see cref="TimeSpan.Zero"/> to create all hedged tasks (value of <see cref="MaxHedgedAttempts"/>) at once
    /// or <see cref="HedgingStrategyOptions.InfiniteHedgingDelay"/> to force the hedging strategy to never create new task before the old one is finished.
    /// </para>
    /// <para> If you want a greater control over hedging delay customization use <see cref="HedgingDelayGenerator"/>.</para>
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
    /// Gets or sets the predicate that determines whether a hedging should be performed for a given result.
    /// </summary>
    [Required]
    public OutcomePredicate<HandleHedgingArguments, TResult> ShouldHandle { get; set; } = new();

    /// <summary>
    /// Gets or sets the hedging action generator that creates hedged actions.
    /// </summary>
    [Required]
    public HedgingActionGenerator<TResult>? HedgingActionGenerator { get; set; } = null;

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
    public OutcomeEvent<OnHedgingArguments, TResult> OnHedging { get; set; } = new();

    internal HedgingStrategyOptions AsNonGenericOptions()
    {
        return new HedgingStrategyOptions
        {
            StrategyType = StrategyType,
            StrategyName = StrategyName,
            HedgingDelay = HedgingDelay,
            HedgingDelayGenerator = HedgingDelayGenerator,
            Handler = new HedgingHandler().SetHedging<TResult>(handler =>
            {
                handler.ShouldHandle = ShouldHandle;
                handler.HedgingActionGenerator = HedgingActionGenerator;
            }),
            MaxHedgedAttempts = MaxHedgedAttempts,
            OnHedging = new OutcomeEvent<OnHedgingArguments>().SetCallbacks(OnHedging)
        };
    }
}
