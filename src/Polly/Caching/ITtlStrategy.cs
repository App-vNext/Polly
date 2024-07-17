#nullable enable
namespace Polly.Caching;

/// <summary>
/// Defines a strategy for providing time-to-live durations for cacheable results.
/// </summary>
public interface ITtlStrategy : ITtlStrategy<object>
{
}

/// <summary>
/// Defines a strategy for providing time-to-live durations for cacheable results.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface ITtlStrategy<in TResult>
{
    /// <summary>
    /// Gets a TTL for a cacheable item, given the current execution context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="result">The execution result.</param>
    /// <returns>A <see cref="Ttl"/> representing the remaining Ttl of the cached item.</returns>
    Ttl GetTtl(Context context, TResult? result);
}
