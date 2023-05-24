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
internal sealed class VoidHedgingHandler
{
    /// <summary>
    /// Gets or sets the predicate that determines whether a hedging should be performed for a given void-based result.
    /// </summary>
    /// <remarks>
    /// This property is required. Defaults to <see langword="null"/>.
    /// </remarks>
    [Required]
    public Func<Outcome, HandleHedgingArguments, ValueTask<bool>>? ShouldHandle { get; set; }

    /// <summary>
    /// Gets or sets the hedging action generator that creates hedged actions.
    /// </summary>
    /// <remarks>
    /// Make sure that the action returned by the generator represents a real asynchronous work, otherwise the hedging
    /// engine will be blocked and parallel hedged actions won't ever be executed. The generator can return a <see langword="null"/> function.
    /// In such a case the hedging is not executed for that attempt.
    /// <para>
    /// This property is required. Defaults to <see langword="null"/>.
    /// </para>
    /// </remarks>
    [Required]
    public Func<HedgingActionGeneratorArguments, Func<Task>?>? HedgingActionGenerator { get; set; } = null;
}
