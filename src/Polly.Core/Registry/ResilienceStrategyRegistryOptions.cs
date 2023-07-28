using System.ComponentModel.DataAnnotations;

namespace Polly.Registry;

/// <summary>
/// An options class used by <see cref="ResilienceStrategyRegistry{TKey}"/>.
/// </summary>
/// <typeparam name="TKey">The type of the key used by the registry.</typeparam>
public class ResilienceStrategyRegistryOptions<TKey>
{
    /// <summary>
    /// Gets or sets the factory method that creates instances of <see cref="CompositeStrategyBuilder"/>.
    /// </summary>
    /// <value>
    /// The default value is a function that creates a new instance of <see cref="CompositeStrategyBuilder"/> using the default constructor.
    /// </value>
    [Required]
    public Func<CompositeStrategyBuilder> BuilderFactory { get; set; } = static () => new CompositeStrategyBuilder();

    /// <summary>
    /// Gets or sets the comparer that is used by the registry to retrieve the resilience strategies.
    /// </summary>
    /// <value>
    /// The default value is <see cref="EqualityComparer{T}.Default"/>.
    /// </value>
    [Required]
    public IEqualityComparer<TKey> StrategyComparer { get; set; } = EqualityComparer<TKey>.Default;

    /// <summary>
    /// Gets or sets the comparer that is used by the registry to retrieve the resilience strategy builders.
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
    /// In general, strategies can have the same builder name and different instance names.
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
    /// In general, strategies can have the same builder name and different strategy keys.
    /// </remarks>
    /// <value>
    /// The default value is a formatter that formats the keys using the <see cref="object.ToString"/> method.
    /// </value>
    [Required]
    public Func<TKey, string> BuilderNameFormatter { get; set; } = (key) => key?.ToString() ?? string.Empty;
}
