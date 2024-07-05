#nullable enable

namespace Polly.Caching;

/// <summary>
/// Class that provides helper methods for configuring TtlStrategies.
/// </summary>
internal static class TtlStrategyExtensions
{
    /// <summary>
    /// Provides a strongly <typeparamref name="TResult"/>-typed version of the supplied <see cref="ITtlStrategy"/>.
    /// </summary>
    /// <typeparam name="TResult">The type the returned <see cref="ITtlStrategy{TResult}"/> will handle.</typeparam>
    /// <param name="ttlStrategy">The non-generic ttl strategy to wrap.</param>
    /// <returns>ITtlStrategy{TCacheFormat}.</returns>
    internal static ITtlStrategy<TResult> For<TResult>(this ITtlStrategy ttlStrategy) =>
        new GenericTtlStrategy<TResult>(ttlStrategy);
}
