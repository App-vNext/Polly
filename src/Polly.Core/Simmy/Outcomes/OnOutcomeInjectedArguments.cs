namespace Polly.Simmy.Outcomes;

/// <summary>
/// Arguments used by the latency chaos strategy to notify that an outcome was injected.
/// </summary>
/// <typeparam name="TResult">The type of the outcome that was injected.</typeparam>
/// <param name="Context">The context associated with the execution of a user-provided callback.</param>
/// <param name="Outcome">The outcome that was injected.</param>
/// <summary>
public readonly record struct OnOutcomeInjectedArguments<TResult>(ResilienceContext Context, Outcome<TResult> Outcome);
