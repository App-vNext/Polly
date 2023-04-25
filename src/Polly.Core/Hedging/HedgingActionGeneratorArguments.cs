using Polly.Strategy;

namespace Polly.Hedging;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents arguments used in <see cref="HedgingActionGenerator"/>.
/// </summary>
public readonly struct HedgingActionGeneratorArguments : IResilienceArguments
{
    internal HedgingActionGeneratorArguments(ResilienceContext context) => Context = context;

    /// <inheritdoc/>
    public ResilienceContext Context { get; }
}
