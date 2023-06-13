using Polly.Utilities.Wrappers;

namespace Polly;

/// <summary>
/// Extensions for conversion of resilience strategies to policies.
/// </summary>
public static class ResilienceStrategyConversionExtensions
{
    /// <summary>
    /// Converts a <see cref="ResilienceStrategy"/> to an <see cref="IAsyncPolicy"/>.
    /// </summary>
    /// <param name="strategy">The strategy instance.</param>
    /// <returns>An instance of <see cref="IAsyncPolicy"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is <see langword="null"/>.</exception>
    public static IAsyncPolicy AsAsyncPolicy(this ResilienceStrategy strategy)
        => new ResilienceStrategyAsyncPolicy(strategy ?? throw new ArgumentNullException(nameof(strategy)));

    /// <summary>
    /// Converts a <see cref="ResilienceStrategy{TResult}"/> to an <see cref="IAsyncPolicy{TResult}"/>.
    /// </summary>
    /// <param name="strategy">The strategy instance.</param>
    /// <returns>An instance of <see cref="IAsyncPolicy{TResult}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is <see langword="null"/>.</exception>
    public static IAsyncPolicy<TResult> AsAsyncPolicy<TResult>(this ResilienceStrategy<TResult> strategy)
        => new ResilienceStrategyAsyncPolicy<TResult>(strategy ?? throw new ArgumentNullException(nameof(strategy)));

    /// <summary>
    /// Converts a <see cref="ResilienceStrategy"/> to an <see cref="ISyncPolicy"/>.
    /// </summary>
    /// <param name="strategy">The strategy instance.</param>
    /// <returns>An instance of <see cref="ISyncPolicy"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is <see langword="null"/>.</exception>
    public static ISyncPolicy AsSyncPolicy(this ResilienceStrategy strategy)
        => new ResilienceStrategySyncPolicy(strategy ?? throw new ArgumentNullException(nameof(strategy)));

    /// <summary>
    /// Converts a <see cref="ResilienceStrategy{TResult}"/> to an <see cref="ISyncPolicy{TResult}"/>.
    /// </summary>
    /// <param name="strategy">The strategy instance.</param>
    /// <returns>An instance of <see cref="ISyncPolicy{TResult}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is <see langword="null"/>.</exception>
    public static ISyncPolicy<TResult> AsSyncPolicy<TResult>(this ResilienceStrategy<TResult> strategy)
        => new ResilienceStrategySyncPolicy<TResult>(strategy ?? throw new ArgumentNullException(nameof(strategy)));
}
