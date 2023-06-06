using System.ComponentModel.DataAnnotations;

namespace Polly.Fallback;

/// <summary>
/// Encompasses logic for managing fallback operations with void-based results.
/// </summary>
/// <remarks>
/// Every fallback handler requires a predicate that determines whether a fallback should be performed for a given
/// void-based result and also the fallback action to execute.
/// </remarks>
internal sealed class VoidFallbackHandler
{
    /// <summary>
    /// Gets or sets the predicate that determines whether a fallback should be handled.
    /// </summary>
    /// <remarks>
    /// This property is required. Defaults to <see langword="null"/>.
    /// </remarks>
    [Required]
    public Func<OutcomeArguments<object, HandleFallbackArguments>, ValueTask<bool>>? ShouldHandle { get; set; }

    /// <summary>
    /// Gets or sets the fallback action to be executed when the <see cref="ShouldHandle"/> predicate evaluates as true.
    /// </summary>
    /// <remarks>
    /// This property is required. Defaults to <see langword="null"/>.
    /// </remarks>
    [Required]
    public Func<OutcomeArguments<object, HandleFallbackArguments>, ValueTask>? FallbackAction { get; set; } = null;
}
