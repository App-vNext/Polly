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
    /// This property is required. Defaults to <see langword="null"/>.
    /// </remarks>
    [Required]
    public Func<Outcome<TResult>, HandleFallbackArguments, ValueTask<bool>>? ShouldHandle { get; set; }

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
    /// Defaults to <see langword="null"/> instance.
    /// </remarks>
    public Func<Outcome<TResult>, OnFallbackArguments, ValueTask>? OnFallback { get; set; }

    internal FallbackStrategyOptions AsNonGenericOptions()
    {
        var options = new FallbackStrategyOptions
        {
            StrategyName = StrategyName,
            Handler = new FallbackHandler().SetFallback<TResult>(handler =>
            {
                handler.ShouldHandle = ShouldHandle;
                handler.FallbackAction = FallbackAction;
            })
        };

        if (OnFallback is var fallback)
        {
            options.OnFallback = (outcome, args) =>
            {
                if (args.Context.ResultType != typeof(TResult))
                {
                    return default;
                }

                return fallback!(outcome.AsOutcome<TResult>(), args);
            };
        }

        return options;
    }
}

