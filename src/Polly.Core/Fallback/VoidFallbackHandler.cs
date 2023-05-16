using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

namespace Polly.Fallback;

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
    /// Gets or sets the fallback action to be executed when the <see cref="ShouldHandle"/> predicate evaluates as true.
    /// </summary>
    [Required]
    public Func<Outcome, HandleFallbackArguments, ValueTask>? FallbackAction { get; set; } = null;
}
