// Assembly 'Polly.Extensions'

using System;
using Microsoft.Extensions.DependencyInjection;
using Polly.Extensions.DependencyInjection;
using Polly.Registry;

namespace Polly;

public static class PollyServiceCollectionExtensions
{
    public static IServiceCollection AddResilienceStrategy<TKey, TResult>(this IServiceCollection services, TKey key, Action<ResilienceStrategyBuilder<TResult>> configure) where TKey : notnull;
    public static IServiceCollection AddResilienceStrategy<TKey, TResult>(this IServiceCollection services, TKey key, Action<ResilienceStrategyBuilder<TResult>, AddResilienceStrategyContext<TKey>> configure) where TKey : notnull;
    public static IServiceCollection AddResilienceStrategy<TKey>(this IServiceCollection services, TKey key, Action<ResilienceStrategyBuilder> configure) where TKey : notnull;
    public static IServiceCollection AddResilienceStrategy<TKey>(this IServiceCollection services, TKey key, Action<ResilienceStrategyBuilder, AddResilienceStrategyContext<TKey>> configure) where TKey : notnull;
    public static IServiceCollection AddResilienceStrategyRegistry<TKey>(this IServiceCollection services, Action<ResilienceStrategyRegistryOptions<TKey>> configure) where TKey : notnull;
    public static IServiceCollection AddResilienceStrategyRegistry<TKey>(this IServiceCollection services) where TKey : notnull;
}
