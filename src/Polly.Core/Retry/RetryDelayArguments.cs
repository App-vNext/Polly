using Polly.Strategy;

namespace Polly.Retry;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents the arguments used in <see cref="RetryDelayGenerator"/> for generating the next retry delay.
/// </summary>
public readonly struct RetryDelayArguments : IResilienceArguments
{
    internal RetryDelayArguments(ResilienceContext context, int attempt)
    {
        Attempt = attempt;
        Context = context;
    }

    /// <summary>
    /// Gets the zero-based attempt number.
    /// </summary>
    /// <remarks>
    /// The first attempt is 0, the second attempt is 1, and so on.
    /// </remarks>
    public int Attempt { get; }

    /// <inheritdoc/>
    public ResilienceContext Context { get; }
}
