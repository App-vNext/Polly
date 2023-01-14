using Polly.Internals;

namespace Polly;

public class ResilienceStrategyBuilder : IResilienceStrategyBuilder
{
    private readonly List<Entry> _entries = new();

    public ResilienceStrategyBuilderOptions Options { get; set; } = new();

    public IResilienceStrategyBuilder AddStrategy(IResilienceStrategy strategy, ResilienceStrategyOptions? options = null) => AddStrategy(_ => strategy, options);

    public IResilienceStrategyBuilder AddStrategy(Func<ResilienceStrategyBuilderContext, IResilienceStrategy> factory, ResilienceStrategyOptions? options = null)
    {
        _entries.Add(new Entry(factory, options ?? new ResilienceStrategyOptions()));

        return this;
    }

    public IResilienceStrategy Build()
    {
        var strategies = new List<DelegatingResilienceStrategy>(_entries.Count);

        foreach (var entry in _entries)
        {
            var context = new ResilienceStrategyBuilderContext(Options, entry.Properties);
            var strategy = entry.Factory(context);

            if (strategy is DelegatingResilienceStrategy delegatingResilienceStrategy)
            {
                strategies.Add(delegatingResilienceStrategy);
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

        return strategies[0];
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
