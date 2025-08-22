using System.ComponentModel.DataAnnotations;

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
    public HedgingStrategyOptions() => Name = HedgingConstants.DefaultName;

    /// <summary>
    /// Gets or sets the maximum waiting time before spawning a new hedged action.
    /// </summary>
    /// <remarks>
    /// You can use <see cref="TimeSpan.Zero"/> to create all hedged actions (value of <see cref="MaxHedgedAttempts"/>) at once
    /// or <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> to force the hedging strategy to never create new action before the old one is finished.
    /// <para> If you want a greater control over hedging delay customization use <see cref="DelayGenerator"/>.</para>
    /// </remarks>
    /// <value>
    /// The default value is 2 seconds.
    /// </value>
    public TimeSpan Delay { get; set; } = HedgingConstants.DefaultHedgingDelay;

    /// <summary>
    /// Gets or sets the maximum number of hedged actions to use, in addition to the original action.
    /// </summary>
    /// <value>
    /// The default value is 1. The value must be bigger or equal to 1, and lower or equal to 10.
    /// </value>
    [Range(HedgingConstants.MinimumHedgedAttempts, HedgingConstants.MaximumHedgedAttempts)]
    public int MaxHedgedAttempts { get; set; } = HedgingConstants.DefaultMaxHedgedAttempts;

    /// <summary>
    /// Gets or sets a predicate that determines whether the hedging should be executed for a given outcome.
    /// </summary>
    /// <value>
    /// The default value is a predicate that hedges on any exception except <see cref="OperationCanceledException"/>.
    /// This property is required.
    /// </value>
    [Required]
    public Func<HedgingPredicateArguments<TResult>, ValueTask<bool>> ShouldHandle { get; set; } = DefaultPredicates<HedgingPredicateArguments<TResult>, TResult>.HandleOutcome;

    /// <summary>
    /// Gets or sets a generator that creates hedged actions.
    /// </summary>
    /// <value>
    /// The default generator executes the original callback that was passed to the hedging resilience strategy. This property is required.
    /// </value>
    [Required]
    public Func<HedgingActionGeneratorArguments<TResult>, Func<ValueTask<Outcome<TResult>>>?> ActionGenerator { get; set; } = DefaultActionGenerator;

    internal static readonly Func<HedgingActionGeneratorArguments<TResult>, Func<ValueTask<Outcome<TResult>>>?> DefaultActionGenerator = args =>
    {
        return () =>
        {
            if (args.PrimaryContext.IsSynchronous)
            {
                return new(Task.Run(() => args.Callback(args.ActionContext).AsTask()));
            }

            return args.Callback(args.ActionContext);
        };
    };

    /// <summary>
    /// Gets or sets a generator that generates hedging delays for each hedging action.
    /// </summary>
    /// <remarks>
    /// The <see cref="DelayGenerator"/> takes precedence over <see cref="Delay"/>. If specified, the <see cref="Delay"/> is ignored.
    /// </remarks>
    /// <value>
    /// The default value is <see langword="null"/>.
    /// </value>
    public Func<HedgingDelayGeneratorArguments, ValueTask<TimeSpan>>? DelayGenerator { get; set; }

    /// <summary>
    /// Gets or sets the event that is raised when a hedging is performed.
    /// </summary>
    /// <remarks>
    /// The hedging is executed when the current attempt outcome is not successful and the <see cref="ShouldHandle"/> predicate returns <see langword="true"/> or when
    /// the current attempt did not finish within the <see cref="Delay"/>.
    /// </remarks>
    /// <value>
    /// The default value is <see langword="null"/>.
    /// </value>
    public Func<OnHedgingArguments<TResult>, ValueTask>? OnHedging { get; set; }
}
