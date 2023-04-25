using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

namespace Polly.Fallback;

/// <summary>
/// Represents an asynchronous delegate for handling fallback actions.
/// </summary>
/// <typeparam name="TResult">The result type.</typeparam>
/// <param name="outcome">The <see cref="Outcome{TResult}"/> of the operation.</param>
/// <param name="arguments">Supplementary <see cref="HandleFallbackArguments"/> for the fallback action.</param>
/// <returns>A <see cref="ValueTask{TResult}"/> containing the TResult.</returns>
public delegate ValueTask<TResult> FallbackAction<TResult>(Outcome<TResult> outcome, HandleFallbackArguments arguments);

/// <summary>
/// Encompasses logic for managing fallback operations for a single result type.
/// </summary>
/// <typeparam name="TResult">The result type.</typeparam>
/// <remarks>
/// Every fallback handler requires a predicate that determines whether a fallback should be performed for a given result and also
/// the fallback action to execute.
/// </remarks>
public class FallbackHandler<TResult>
{
    /// <summary>
    /// Gets or sets the predicate that determines whether a fallback should be performed for a given result.
    /// </summary>
    [Required]
    public OutcomePredicate<HandleFallbackArguments, TResult> ShouldHandle { get; set; } = new();

    /// <summary>
    /// Gets or sets the fallback action to be executed if <see cref="ShouldHandle"/> predicate evaluates as true.
    /// </summary>
    [Required]
    public FallbackAction<TResult>? FallbackAction { get; set; } = null;
}
