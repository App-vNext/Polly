using Polly.Utilities.Wrappers;

namespace Polly;

/// <summary>
/// Extensions for conversion of resilience strategies to policies.
/// </summary>
public static class ResiliencePipelineConversionExtensions
{
    /// <summary>
    /// Converts a <see cref="ResiliencePipeline"/> to an <see cref="IAsyncPolicy"/>.
    /// </summary>
    /// <param name="strategy">The strategy instance.</param>
    /// <returns>An instance of <see cref="IAsyncPolicy"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is <see langword="null"/>.</exception>
    public static IAsyncPolicy AsAsyncPolicy(this ResiliencePipeline strategy)
        => new ResiliencePipelineAsyncPolicy(strategy ?? throw new ArgumentNullException(nameof(strategy)));

    /// <summary>
    /// Converts a <see cref="ResiliencePipeline{TResult}"/> to an <see cref="IAsyncPolicy{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="strategy">The strategy instance.</param>
    /// <returns>An instance of <see cref="IAsyncPolicy{TResult}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is <see langword="null"/>.</exception>
    public static IAsyncPolicy<TResult> AsAsyncPolicy<TResult>(this ResiliencePipeline<TResult> strategy)
        => new ResiliencePipelineAsyncPolicy<TResult>(strategy ?? throw new ArgumentNullException(nameof(strategy)));

    /// <summary>
    /// Converts a <see cref="ResiliencePipeline"/> to an <see cref="ISyncPolicy"/>.
    /// </summary>
    /// <param name="strategy">The strategy instance.</param>
    /// <returns>An instance of <see cref="ISyncPolicy"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is <see langword="null"/>.</exception>
    public static ISyncPolicy AsSyncPolicy(this ResiliencePipeline strategy)
        => new ResiliencePipelineSyncPolicy(strategy ?? throw new ArgumentNullException(nameof(strategy)));

    /// <summary>
    /// Converts a <see cref="ResiliencePipeline{TResult}"/> to an <see cref="ISyncPolicy{TResult}"/>.
    /// </summary>
    /// <param name="strategy">The strategy instance.</param>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <returns>An instance of <see cref="ISyncPolicy{TResult}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is <see langword="null"/>.</exception>
    public static ISyncPolicy<TResult> AsSyncPolicy<TResult>(this ResiliencePipeline<TResult> strategy)
        => new ResiliencePipelineSyncPolicy<TResult>(strategy ?? throw new ArgumentNullException(nameof(strategy)));
}
