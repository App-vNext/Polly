namespace Polly.Simmy.Behavior;

/// <summary>
/// Arguments used by the behavior chaos strategy to notify that a custom behavior was injected.
/// </summary>
/// <param name="Context">The context associated with the execution of a user-provided callback.</param>
public readonly record struct OnBehaviorInjectedArguments(ResilienceContext Context);
