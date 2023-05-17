using Polly.Strategy;

namespace Polly.Fallback;

/// <summary>
/// Represents arguments used in fallback handling scenarios.
/// </summary>
/// <param name="Context">The context associated with the execution of a user-provided callback.</param>
public readonly record struct OnFallbackArguments(ResilienceContext Context) : IResilienceArguments;
