using System.Diagnostics.CodeAnalysis;

namespace Polly.Registry;

/// <summary>
/// Represents a provider for resilience pipelines that are accessible by <typeparamref name="TKey"/>.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
public abstract class ResiliencePipelineProvider<TKey>
    where TKey : notnull
{
    /// <summary>
    /// Retrieves a resilience pipeline from the provider using the specified key.
    /// </summary>
    /// <param name="key">The key used to identify the resilience pipeline.</param>
    /// <returns>The resilience pipeline associated with the specified key.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no resilience pipeline is found for the specified key.</exception>
    public virtual ResiliencePipeline GetPipeline(TKey key)
    {
        if (TryGetPipeline(key, out var pipeline))
        {
            return pipeline;
        }

        throw new KeyNotFoundException($"Unable to find a resilience pipeline associated with the key '{key}'. " +
            $"Please ensure that either the resilience pipeline or the builder is registered.");
    }

    /// <summary>
    /// Retrieves a generic resilience pipeline from the provider using the specified key.
    /// </summary>
    /// <typeparam name="TResult">The type of result that the resilience pipeline handles.</typeparam>
    /// <param name="key">The key used to identify the resilience pipeline.</param>
    /// <returns>The resilience pipeline associated with the specified key.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no resilience pipeline is found for the specified key.</exception>
    public virtual ResiliencePipeline<TResult> GetPipeline<TResult>(TKey key)
    {
        if (TryGetPipeline<TResult>(key, out var pipeline))
        {
            return pipeline;
        }

        throw new KeyNotFoundException($"Unable to find a generic resilience pipeline of '{typeof(TResult).Name}' associated with the key '{key}'. " +
            $"Please ensure that either the generic resilience pipeline or the generic builder is registered.");
    }

    /// <summary>
    /// Tries to get a resilience pipeline from the provider using the specified key.
    /// </summary>
    /// <param name="key">The key used to identify the resilience pipeline.</param>
    /// <param name="pipeline">The output resilience pipeline if found, <see langword="null"/> otherwise.</param>
    /// <returns><see langword="true"/> if the pipeline was found, <see langword="false"/> otherwise.</returns>
    public abstract bool TryGetPipeline(TKey key, [NotNullWhen(true)] out ResiliencePipeline? pipeline);

    /// <summary>
    /// Tries to get a generic resilience pipeline from the provider using the specified key.
    /// </summary>
    /// <typeparam name="TResult">The type of result that the resilience pipeline handles.</typeparam>
    /// <param name="key">The key used to identify the resilience pipeline.</param>
    /// <param name="pipeline">The output resilience pipeline if found, <see langword="null"/> otherwise.</param>
    /// <returns><see langword="true"/> if the pipeline was found, <see langword="false"/> otherwise.</returns>
    public abstract bool TryGetPipeline<TResult>(TKey key, [NotNullWhen(true)] out ResiliencePipeline<TResult>? pipeline);
}
