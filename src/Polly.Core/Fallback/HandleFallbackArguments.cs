using Polly.Strategy;

namespace Polly.Fallback;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents arguments used in fallback handling scenarios.
/// </summary>
public readonly struct HandleFallbackArguments : IResilienceArguments
{
    internal HandleFallbackArguments(ResilienceContext context) => Context = context;

    /// <inheritdoc/>
    public ResilienceContext Context { get; }
}
