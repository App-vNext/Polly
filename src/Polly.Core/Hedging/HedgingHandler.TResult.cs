using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

namespace Polly.Hedging;

/// <summary>
/// Encompasses logic for managing hedging operations for a single result type.
/// </summary>
/// <typeparam name="TResult">The result type.</typeparam>
/// <remarks>
/// Every hedging handler requires a predicate that determines whether a hedging should be performed for a given result and also
/// the hedging generator that creates a hedged action to execute.
/// </remarks>
public sealed class HedgingHandler<TResult>
{
    /// <summary>
    /// Gets or sets the predicate that determines whether a hedging should be performed for a given result.
    /// </summary>
    [Required]
    public OutcomePredicate<HandleHedgingArguments, TResult> ShouldHandle { get; set; } = new();

    /// <summary>
    /// Gets or sets the hedging action generator that creates hedged actions.
    /// </summary>
    /// <remarks>
    /// This property is required. Defaults to <c>null</c>.
    /// </remarks>
    [Required]
    public HedgingActionGenerator<TResult>? HedgingActionGenerator { get; set; } = null;
}
