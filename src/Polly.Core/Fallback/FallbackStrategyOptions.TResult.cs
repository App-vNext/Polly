using System.ComponentModel.DataAnnotations;

namespace Polly.Fallback;

/// <summary>
/// Represents the options for configuring a fallback resilience strategy with a specific result type.
/// </summary>
/// <typeparam name="TResult">The result type.</typeparam>
public class FallbackStrategyOptions<TResult> : ResilienceStrategyOptions
{
    /// <summary>
    /// Gets the strategy type.
    /// </summary>
    /// <remarks>Returns <c>Fallback</c> value.</remarks>
    public sealed override string StrategyType => FallbackConstants.StrategyType;

    /// <summary>
    /// Gets or sets the outcome predicate for determining whether a fallback should be executed.
    /// </summary>
    /// <value>
    /// The default value is a predicate that fallbacks on any exception except <see cref="OperationCanceledException"/>. This property is required.
    /// </value>
    [Required]
    public Func<OutcomeArguments<TResult, FallbackPredicateArguments>, ValueTask<bool>> ShouldHandle { get; set; } = DefaultPredicates<FallbackPredicateArguments, TResult>.HandleOutcome;

    /// <summary>
    /// Gets or sets the fallback action to be executed when the <see cref="ShouldHandle"/> predicate evaluates as true.
    /// </summary>
    /// <value>
    /// The default value is <see langword="null"/>. This property is required.
    /// </value>
    [Required]
    public Func<OutcomeArguments<TResult, FallbackPredicateArguments>, ValueTask<Outcome<TResult>>>? FallbackAction { get; set; }

    /// <summary>
    /// Gets or sets the outcome event instance responsible for triggering fallback events.
    /// </summary>
    /// <value>
    /// The default value is <see langword="null"/> instance.
    /// </value>
    public Func<OutcomeArguments<TResult, OnFallbackArguments>, ValueTask>? OnFallback { get; set; }
}

