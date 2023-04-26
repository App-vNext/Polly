using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

namespace Polly.Fallback;

/// <summary>
/// Represents an asynchronous delegate for handling void-based fallback actions.
/// </summary>
/// <param name="outcome">The <see cref="Outcome"/> of the operation.</param>
/// <param name="arguments">Supplementary <see cref="HandleFallbackArguments"/> for the fallback action.</param>
/// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
public delegate ValueTask FallbackAction(Outcome outcome, HandleFallbackArguments arguments);

/// <summary>
/// Encompasses logic for managing fallback operations with void-based results.
/// </summary>
/// <remarks>
/// Every fallback handler requires a predicate that determines whether a fallback should be performed for a given
/// void-based result and also the fallback action to execute.
/// </remarks>
public sealed class VoidFallbackHandler
{
    /// <summary>
    /// Gets or sets the predicate that determines whether a fallback should be handled.
    /// </summary>
    [Required]
    public VoidOutcomePredicate<HandleFallbackArguments> ShouldHandle { get; set; } = new();

    /// <summary>
    /// Gets or sets the fallback action to be executed if the <see cref="ShouldHandle"/> predicate evaluates as true.
    /// </summary>
    [Required]
    public FallbackAction? FallbackAction { get; set; } = null;
}
