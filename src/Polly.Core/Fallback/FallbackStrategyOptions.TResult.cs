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
    /// <remarks>
    /// Defaults to a delegate that hedges on any exception except <see cref="OperationCanceledException"/>.
    /// This property is required.
    /// </remarks>
    [Required]
    public Func<OutcomeArguments<TResult, FallbackPredicateArguments>, ValueTask<bool>> ShouldHandle { get; set; } = DefaultPredicates<FallbackPredicateArguments, TResult>.HandleOutcome;

    /// <summary>
    /// Gets or sets the fallback action to be executed when the <see cref="ShouldHandle"/> predicate evaluates as true.
    /// </summary>
    /// <remarks>
    /// This property is required. Defaults to <see langword="null"/>.
    /// </remarks>
    [Required]
    public Func<OutcomeArguments<TResult, FallbackPredicateArguments>, ValueTask<Outcome<TResult>>>? FallbackAction { get; set; }

    /// <summary>
    /// Gets or sets the outcome event instance responsible for triggering fallback events.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/> instance.
    /// </remarks>
    public Func<OutcomeArguments<TResult, OnFallbackArguments>, ValueTask>? OnFallback { get; set; }
}

