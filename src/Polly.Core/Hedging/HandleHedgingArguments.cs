using Polly.Strategy;

namespace Polly.Hedging;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents arguments used in hedging handling scenarios.
/// </summary>
public readonly struct HandleHedgingArguments : IResilienceArguments
{
    internal HandleHedgingArguments(ResilienceContext context) => Context = context;

    /// <inheritdoc/>
    public ResilienceContext Context { get; }
}
