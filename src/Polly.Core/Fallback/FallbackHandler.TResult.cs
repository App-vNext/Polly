using System;
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
internal sealed class FallbackHandler<TResult>
{
    /// <summary>
    /// Gets or sets the predicate that determines whether a fallback should be performed for a given result.
    /// </summary>
    /// <remarks>
    /// This property is required. Defaults to <see langword="null"/>.
    /// </remarks>
    [Required]
    public Func<Outcome<TResult>, HandleFallbackArguments, ValueTask<bool>>? ShouldHandle { get; set; }

    /// <summary>
    /// Gets or sets the fallback action to be executed if <see cref="ShouldHandle"/> predicate evaluates as true.
    /// </summary>
    /// <remarks>
    /// This property is required. Defaults to <see langword="null"/>.
    /// </remarks>
    [Required]
    public Func<Outcome<TResult>, HandleFallbackArguments, ValueTask<TResult>>? FallbackAction { get; set; } = null;

    internal async ValueTask<Func<Outcome<TResult>, HandleFallbackArguments, ValueTask<TResult>>?> ShouldHandleAsync(
        Outcome<TResult> outcome,
        HandleFallbackArguments arguments)
    {
        if (!await ShouldHandle!(outcome, arguments).ConfigureAwait(arguments.Context.ContinueOnCapturedContext))
        {
            return null;
        }

        return FallbackAction;
    }
}
