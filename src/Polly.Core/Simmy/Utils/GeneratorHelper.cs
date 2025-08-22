namespace Polly.Simmy.Utils;

internal sealed class GeneratorHelper<TResult>(Func<int, int> weightGenerator)
{
    private readonly Func<int, int> _weightGenerator = weightGenerator;

    private readonly List<int> _weights = [];
    private readonly List<Func<ResilienceContext, Outcome<TResult>>> _factories = [];
    private int _totalWeight;

    public void AddOutcome(Func<ResilienceContext, Outcome<TResult>> generator, int weight)
    {
        Guard.NotNull(generator);

        _totalWeight += weight;
        _factories.Add(generator);
        _weights.Add(weight);
    }

    internal Func<ResilienceContext, Outcome<TResult>?> CreateGenerator()
    {
        if (_factories.Count == 0)
        {
            return _ => null;
        }

        var totalWeight = _totalWeight;
        var factories = _factories.ToArray();
        var weights = _weights.ToArray();
        var generator = _weightGenerator;

        return context =>
        {
            var generatedWeight = generator(totalWeight);
            var weight = 0;

            for (var i = 0; i < factories.Length; i++)
            {
                weight += weights[i];
                if (generatedWeight < weight)
                {
                    return factories[i](context);
                }
            }

            return null;
        };
    }
}

