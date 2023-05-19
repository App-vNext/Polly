using System.ComponentModel.DataAnnotations;
using Polly.Strategy;

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
    /// This property is required.
    /// </remarks>
    [Required]
    public OutcomePredicate<HandleFallbackArguments, TResult> ShouldHandle { get; set; } = new();

    /// <summary>
    /// Gets or sets the fallback action to be executed when the <see cref="ShouldHandle"/> predicate evaluates as true.
    /// </summary>
    /// <remarks>
    /// This property is required. Defaults to <see langword="null"/>.
    /// </remarks>
    [Required]
    public Func<Outcome<TResult>, HandleFallbackArguments, ValueTask<TResult>>? FallbackAction { get; set; }

    /// <summary>
    /// Gets or sets the outcome event instance responsible for triggering fallback events.
    /// </summary>
    /// <remarks>
    /// This property is required.
    /// </remarks>
    [Required]
    public OutcomeEvent<OnFallbackArguments, TResult> OnFallback { get; set; } = new();

    internal FallbackStrategyOptions AsNonGenericOptions()
    {
        return new FallbackStrategyOptions
        {
            StrategyName = StrategyName,
            OnFallback = new OutcomeEvent<OnFallbackArguments>().SetCallbacks(OnFallback),
            Handler = new FallbackHandler().SetFallback<TResult>(handler =>
            {
                handler.ShouldHandle = ShouldHandle;
                handler.FallbackAction = FallbackAction;
            })
        };
    }
}

