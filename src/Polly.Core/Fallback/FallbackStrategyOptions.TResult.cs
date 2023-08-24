using System.ComponentModel.DataAnnotations;

namespace Polly.Fallback;

/// <summary>
/// Represents the options for configuring a fallback resilience strategy with a specific result type.
/// </summary>
/// <typeparam name="TResult">The result type.</typeparam>
public class FallbackStrategyOptions<TResult> : ResilienceStrategyOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FallbackStrategyOptions{TResult}"/> class.
    /// </summary>
    public FallbackStrategyOptions() => Name = FallbackConstants.DefaultName;

    /// <summary>
    /// Gets or sets a predicate that determines whether the fallback should be executed for a given outcome.
    /// </summary>
    /// <value>
    /// The default value is a predicate that fallbacks on any exception except <see cref="OperationCanceledException"/>. This property is required.
    /// </value>
    [Required]
    public Func<FallbackPredicateArguments<TResult>, ValueTask<bool>> ShouldHandle { get; set; } = DefaultPredicates<FallbackPredicateArguments<TResult>, TResult>.HandleOutcome;

    /// <summary>
    /// Gets or sets the fallback action to be executed when the <see cref="ShouldHandle"/> predicate evaluates as true.
    /// </summary>
    /// <value>
    /// The default value is <see langword="null"/>. This property is required.
    /// </value>
    [Required]
    public Func<FallbackPredicateArguments<TResult>, ValueTask<Outcome<TResult>>>? FallbackAction { get; set; }

    /// <summary>
    /// Gets or sets event delegate that is raised when fallback happens.
    /// </summary>
    /// <value>
    /// The default value is <see langword="null"/> instance.
    /// </value>
    public Func<OnFallbackArguments<TResult>, ValueTask>? OnFallback { get; set; }
}

