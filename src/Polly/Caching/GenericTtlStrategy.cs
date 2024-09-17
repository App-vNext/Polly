#nullable enable
namespace Polly.Caching;

/// <summary>
/// Represents a strongly-typed <see cref="ITtlStrategy"/> wrapper of a non-generic strategy.
/// </summary>
internal sealed class GenericTtlStrategy<TResult> : ITtlStrategy<TResult>
{
    private readonly ITtlStrategy _wrappedTtlStrategy;

    internal GenericTtlStrategy(ITtlStrategy ttlStrategy) =>
        _wrappedTtlStrategy = ttlStrategy ?? throw new ArgumentNullException(nameof(ttlStrategy));

    /// <summary>
    /// Gets a TTL for a cacheable item, given the current execution context and result.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="result">The execution result.</param>
    /// <returns>A <see cref="Ttl"/> representing the remaining Ttl of the cached item.</returns>
    public Ttl GetTtl(Context context, TResult? result) =>
        _wrappedTtlStrategy.GetTtl(context, result);
}
