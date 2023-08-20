using System.ComponentModel.DataAnnotations;

namespace Polly.Registry;

/// <summary>
/// An options class used by <see cref="ResiliencePipelineRegistry{TKey}"/>.
/// </summary>
/// <typeparam name="TKey">The type of the key used by the registry.</typeparam>
public class ResiliencePipelineRegistryOptions<TKey>
{
    /// <summary>
    /// Gets or sets the factory method that creates instances of <see cref="ResiliencePipelineBuilder"/>.
    /// </summary>
    /// <value>
    /// The default value is a function that creates a new instance of <see cref="ResiliencePipelineBuilder"/> using the default constructor.
    /// </value>
    [Required]
    public Func<ResiliencePipelineBuilder> BuilderFactory { get; set; } = static () => new ResiliencePipelineBuilder();

    /// <summary>
    /// Gets or sets the comparer that is used by the registry to retrieve the resilience pipelines.
    /// </summary>
    /// <value>
    /// The default value is <see cref="EqualityComparer{T}.Default"/>.
    /// </value>
    [Required]
    public IEqualityComparer<TKey> PipelineComparer { get; set; } = EqualityComparer<TKey>.Default;

    /// <summary>
    /// Gets or sets the comparer that is used by the registry to retrieve the resilience pipeline builders.
    /// </summary>
    /// <value>
    /// The default value is <see cref="EqualityComparer{T}.Default"/>.
    /// </value>
    [Required]
    public IEqualityComparer<TKey> BuilderComparer { get; set; } = EqualityComparer<TKey>.Default;

    /// <summary>
    /// Gets or sets the formatter that is used by the registry to format the <typeparamref name="TKey"/> to a string that
    /// represents the instance name of the builder.
    /// </summary>
    /// <remarks>
    /// Use custom formatter for composite keys in case you want to have different metric values for a builder and instance key.
    /// In general, pipelines can have the same builder name and different instance names.
    /// </remarks>
    /// <value>
    /// The default value is <see langword="null"/>.
    /// </value>
    public Func<TKey, string>? InstanceNameFormatter { get; set; }

    /// <summary>
    /// Gets or sets the formatter that is used by the registry to format the <typeparamref name="TKey"/> to a string that
    /// represents the builder name.
    /// </summary>
    /// <remarks>
    /// Use custom formatter for composite keys in case you want to have different metric values for a builder and strategy key.
    /// In general, pipelines can have the same builder name and different pipeline keys.
    /// </remarks>
    /// <value>
    /// The default value is a formatter that formats the keys using the <see cref="object.ToString"/> method.
    /// </value>
    [Required]
    public Func<TKey, string> BuilderNameFormatter { get; set; } = (key) => key?.ToString() ?? string.Empty;
}
