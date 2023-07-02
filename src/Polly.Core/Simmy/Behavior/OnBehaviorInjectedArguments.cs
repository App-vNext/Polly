namespace Polly.Simmy.Behavior;

/// <summary>
/// Arguments used by the latency strategy to notify that a delayed occurred.
/// </summary>
/// <param name="Context">The context associated with the execution of a user-provided callback.</param>
public readonly record struct OnBehaviorInjectedArguments(ResilienceContext Context);
