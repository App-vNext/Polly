using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly.Internals;

namespace Polly;

public static class ResilienceServiceCollectionExtensions
{
    public static IServiceCollection AddResilienceStrategy(this IServiceCollection services, string name, Action<ResilienceStrategyContext> configure)
    {
        services.TryAddTransient<IResilienceStrategyBuilder>(serviceProvider =>
        {
            var builder = new ResilienceStrategyBuilder();

            builder.Options.Name = name;
            builder.Options.LoggerFactory = serviceProvider.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;

            return builder;
        });
        services.TryAddSingleton<IResilienceStrategyProvider, ResilienceStrategyProvider>();
        services.TryAddSingleton<ResilienceStrategyFactory>();
        services.Configure<ResilienceStrategyFactoryOptions>(name, options => options.ConfigureActions.Add(configure));

        return services;
    }
}
