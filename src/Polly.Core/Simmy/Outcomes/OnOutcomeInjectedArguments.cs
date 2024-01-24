namespace Polly.Simmy.Outcomes;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Arguments used by the outcome chaos strategy to notify that an outcome was injected.
/// </summary>
/// <typeparam name="TResult">The type of the outcome that was injected.</typeparam>
public readonly struct OnOutcomeInjectedArguments<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnOutcomeInjectedArguments{TResult}"/> struct.
    /// </summary>
    /// <param name="context">The resilience context instance.</param>
    /// <param name="outcome">The outcome that was injected.</param>
    public OnOutcomeInjectedArguments(ResilienceContext context, Outcome<TResult> outcome)
    {
        Context = context;
        Outcome = outcome;
    }

    /// <summary>
    /// Gets the resilience context instance.
    /// </summary>
    public ResilienceContext Context { get; }

    /// <summary>
    /// Gets the outcome that was injected.
    /// </summary>
    public Outcome<TResult> Outcome { get; }
}
