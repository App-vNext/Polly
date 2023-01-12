using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


namespace Polly.Internals;

internal class ResilienceStrategyFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ResilienceStrategyFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public IResilienceStrategy CreateResilienceStrategy(string name)
    {
        var actions = _serviceProvider.GetRequiredService<IOptionsMonitor<ResilienceStrategyFactoryOptions>>().Get(name).ConfigureActions;
        var builder = _serviceProvider.GetRequiredService<IResilienceStrategyBuilder>();
        var context = new ResilienceStrategyContext(_serviceProvider, builder);

        foreach (var action in actions)
        {
            action.Invoke(context);
        }

        return builder.Create();
    }
}
