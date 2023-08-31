namespace Polly.Utils;

/// <summary>
/// Marker interface for outcome arguments.
/// </summary>
/// <typeparam name="TResult">The type of result.</typeparam>
internal interface IOutcomeArguments<TResult>
{
    /// <summary>
    /// Gets the resilience context.
    /// </summary>
    ResilienceContext Context { get; }

    /// <summary>
    /// Gets the outcome.
    /// </summary>
    Outcome<TResult> Outcome { get; }
}
