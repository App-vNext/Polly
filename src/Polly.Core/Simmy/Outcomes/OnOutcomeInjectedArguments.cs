namespace Polly.Simmy.Outcomes;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by the outcome chaos strategy to notify that an outcome was injected.
/// </summary>
/// <typeparam name="TResult">The type of the outcome that was injected.</typeparam>
internal readonly struct OnOutcomeInjectedArguments<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnOutcomeInjectedArguments{TResult}"/> struct.
    /// </summary>
    /// <param name="context">The context associated with the execution of a user-provided callback.</param>
    /// <param name="outcome">The outcome that was injected.</param>
    public OnOutcomeInjectedArguments(ResilienceContext context, Outcome<TResult> outcome)
    {
        Context = context;
        Outcome = outcome;
    }

    /// <summary>
    /// Gets the context of this event.
    /// </summary>
    public ResilienceContext Context { get; }

    /// <summary>
    /// Gets the Outcome that was injected.
    /// </summary>
    public Outcome<TResult> Outcome { get; }
}
