using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

namespace Polly.Fallback;

/// <summary>
/// Encompasses logic for managing fallback operations for a single result type.
/// </summary>
/// <typeparam name="TResult">The result type.</typeparam>
/// <remarks>
/// Every fallback handler requires a predicate that determines whether a fallback should be performed for a given result and also
/// the fallback action to execute.
/// </remarks>
public sealed class FallbackHandler<TResult>
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
    public Func<Outcome<TResult>, HandleFallbackArguments, ValueTask<TResult>>? FallbackAction { get; set; } = null;
}
