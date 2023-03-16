using System.ComponentModel.DataAnnotations;

namespace Polly.Builder;

/// <summary>
/// A builder that is used to create an instance of <see cref="IResilienceStrategy"/>.
/// </summary>
/// <remarks>
/// The builder supports chaining multiple strategies into a pipeline of strategies.
/// The resulting instance of <see cref="IResilienceStrategy"/> created by the <see cref="Build"/> call will execute the strategies in the same order they were added to the builder.
/// The order of the strategies is important.
/// </remarks>
public class ResilienceStrategyBuilder
{
    private readonly List<Entry> _entries = new();
    private ResilienceStrategyBuilderOptions _options = new();

    /// <summary>
    /// Gets or sets the builder options.
    /// </summary>
    public ResilienceStrategyBuilderOptions Options
    {
        get => _options;
        set => _options = Guard.NotNull(value);
    }

    /// <summary>
    /// Adds an already created strategy instance to the builder.
    /// </summary>
    /// <param name="strategy">The strategy instance.</param>
    /// <param name="options">The options associated with the strategy. If none are provided the default instance of <see cref="ResilienceStrategyOptions"/> is created.</param>
    /// <returns>The same builder instance.</returns>
    public ResilienceStrategyBuilder AddStrategy(IResilienceStrategy strategy, ResilienceStrategyOptions? options = null)
    {
        Guard.NotNull(strategy);

        return AddStrategy(_ => strategy, options);
    }

    /// <summary>
    /// Adds a strategy to the builder.
    /// </summary>
    /// <param name="factory">The factory that creates a resilience strategy.</param>
    /// <param name="options">The options associated with the strategy. If none are provided the default instance of <see cref="ResilienceStrategyOptions"/> is created.</param>
    /// <returns>The same builder instance.</returns>
    public ResilienceStrategyBuilder AddStrategy(Func<ResilienceStrategyBuilderContext, IResilienceStrategy> factory, ResilienceStrategyOptions? options = null)
    {
        Guard.NotNull(factory);

        if (options is not null)
        {
            Validator.ValidateObject(options, new ValidationContext(options), validateAllProperties: true);
        }

        _entries.Add(new Entry(factory, options ?? new ResilienceStrategyOptions()));

        return this;
    }

    /// <summary>
    /// Builds the resilience strategy.
    /// </summary>
    /// <returns>An instance of <see cref="IResilienceStrategy"/>.</returns>
    public IResilienceStrategy Build()
    {
        Validator.ValidateObject(Options, new ValidationContext(Options), validateAllProperties: true);

        if (_entries.Count == 0)
        {
            return NullResilienceStrategy.Instance;
        }

        var strategies = new List<DelegatingResilienceStrategy>(_entries.Count);

        foreach (var entry in _entries)
        {
            var context = new ResilienceStrategyBuilderContext(
                builderName: Options.BuilderName,
                strategyName: entry.Properties.StrategyName,
                strategyType: entry.Properties.StrategyType);

            var strategy = entry.Factory(context);

            if (strategy is DelegatingResilienceStrategy delegatingStrategy)
            {
                strategies.Add(delegatingStrategy);
            }
            else
            {
                strategies.Add(new DelegatingStrategyWrapper(strategy));
            }
        }

        for (var i = 0; i < strategies.Count - 1; i++)
        {
            strategies[i].Next = strategies[i + 1];
        }

        return new DelegatingStrategyWrapper(strategies[0]);
    }

    private sealed class Entry
    {
        public Entry(Func<ResilienceStrategyBuilderContext, IResilienceStrategy> factory, ResilienceStrategyOptions properties)
        {
            Factory = factory;
            Properties = properties;
        }

        public Func<ResilienceStrategyBuilderContext, IResilienceStrategy> Factory { get; }

        public ResilienceStrategyOptions Properties { get; }
    }
}
