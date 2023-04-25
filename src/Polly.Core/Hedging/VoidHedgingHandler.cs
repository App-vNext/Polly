using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

namespace Polly.Hedging;

/// <summary>
/// Encompasses logic for managing hedging operations for a void-based result type.
/// </summary>
/// <remarks>
/// Every hedging handler requires a predicate that determines whether a hedging should be performed for a given void result and also
/// the hedging generator that creates a hedged action to execute.
/// </remarks>
public sealed class VoidHedgingHandler
{
    /// <summary>
    /// Gets or sets the predicate that determines whether a hedging should be performed for a given void-based result.
    /// </summary>
    [Required]
    public VoidOutcomePredicate<HandleHedgingArguments> ShouldHandle { get; set; } = new();

    /// <summary>
    /// Gets or sets the hedging action generator that creates hedged actions.
    /// </summary>
    /// <remarks>
    /// This property is required. Defaults to <c>null</c>.
    /// </remarks>
    [Required]
    public HedgingActionGenerator? HedgingActionGenerator { get; set; } = null;
}
