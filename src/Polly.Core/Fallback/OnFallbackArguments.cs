using Polly.Strategy;

namespace Polly.Fallback;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents arguments used when the fallback is about to be executed.
/// </summary>
public readonly struct OnFallbackArguments : IResilienceArguments
{
    internal OnFallbackArguments(ResilienceContext context) => Context = context;

    /// <inheritdoc/>
    public ResilienceContext Context { get; }
}
