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
    /// <para> If you want a greater control over hedging delay customization use <see cref="HedgingDelayGenerator"/>.</para>
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
    /// Gets or sets the predicate that determines whether a hedging should be performed for a given result.
    /// </summary>
    /// <remarks>
    /// This property is required. Defaults to <see langword="null"/>.
    /// </remarks>
    [Required]
    public Func<Outcome<TResult>, HandleHedgingArguments, ValueTask<bool>>? ShouldHandle { get; set; }

    /// <summary>
    /// Gets or sets the hedging action generator that creates hedged actions.
    /// </summary>
    /// <remarks>
    /// This property is required. Defaults to <see langword="null"/>.
    /// </remarks>
    [Required]
    public Func<HedgingActionGeneratorArguments<TResult>, Func<Task<TResult>>?>? HedgingActionGenerator { get; set; } = null;

    /// <summary>
    /// Gets or sets the generator that generates hedging delays for each hedging attempt.
    /// </summary>
    /// <remarks>
    /// The <see cref="HedgingDelayGenerator"/> takes precedence over <see cref="HedgingDelay"/>. If specified, the <see cref="HedgingDelay"/> is ignored.
    /// <para>Defaults to <see langword="null"/>.</para>
    /// </remarks>
    public Func<HedgingDelayArguments, ValueTask<TimeSpan>>? HedgingDelayGenerator { get; set; }

    /// <summary>
    /// Gets or sets the event that is raised when a hedging is performed.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    public Func<Outcome<TResult>, OnHedgingArguments, ValueTask>? OnHedging { get; set; }

    internal HedgingStrategyOptions AsNonGenericOptions()
    {
        var options = new HedgingStrategyOptions
        {
            StrategyName = StrategyName,
            HedgingDelay = HedgingDelay,
            Handler = new HedgingHandler().SetHedging<TResult>(handler =>
            {
                handler.ShouldHandle = ShouldHandle;
                handler.HedgingActionGenerator = HedgingActionGenerator;
            }),
            MaxHedgedAttempts = MaxHedgedAttempts,
            HedgingDelayGenerator = HedgingDelayGenerator
        };

        if (OnHedging is var onHedging)
        {
            options.OnHedging = (outcome, args) =>
            {
                if (args.Context.ResultType != typeof(TResult))
                {
                    return default;
                }

                return onHedging!(outcome.AsOutcome<TResult>(), args);
            };
        }

        return options;
    }
}
