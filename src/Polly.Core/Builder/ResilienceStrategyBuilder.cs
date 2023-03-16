using System.ComponentModel.DataAnnotations;

namespace Polly.Builder;

/// <inheritdoc/>
public class ResilienceStrategyBuilder : IResilienceStrategyBuilder
{
    private readonly List<Entry> _entries = new();
    private ResilienceStrategyBuilderOptions _options = new();

    /// <inheritdoc/>
    public ResilienceStrategyBuilderOptions Options
    {
        get => _options;
        set => _options = Guard.NotNull(value);
    }

    /// <inheritdoc/>
    public IResilienceStrategyBuilder AddStrategy(Func<ResilienceStrategyBuilderContext, IResilienceStrategy> factory, ResilienceStrategyOptions? options = null)
    {
        Guard.NotNull(factory);

        if (options is not null)
        {
            Validator.ValidateObject(options, new ValidationContext(options), validateAllProperties: true);
        }

        _entries.Add(new Entry(factory, options ?? new ResilienceStrategyOptions()));

        return this;
    }

    /// <inheritdoc/>
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
