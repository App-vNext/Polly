// Assembly 'Polly.Extensions'

using System;
using Microsoft.Extensions.DependencyInjection;
using Polly.DependencyInjection;
using Polly.Registry;

namespace Polly;

public static class PollyServiceCollectionExtensions
{
    public static IServiceCollection AddResiliencePipeline<TKey, TResult>(this IServiceCollection services, TKey key, Action<ResiliencePipelineBuilder<TResult>> configure) where TKey : notnull;
    public static IServiceCollection AddResiliencePipeline<TKey, TResult>(this IServiceCollection services, TKey key, Action<ResiliencePipelineBuilder<TResult>, AddResiliencePipelineContext<TKey>> configure) where TKey : notnull;
    public static IServiceCollection AddResiliencePipeline<TKey>(this IServiceCollection services, TKey key, Action<ResiliencePipelineBuilder> configure) where TKey : notnull;
    public static IServiceCollection AddResiliencePipeline<TKey>(this IServiceCollection services, TKey key, Action<ResiliencePipelineBuilder, AddResiliencePipelineContext<TKey>> configure) where TKey : notnull;
    public static IServiceCollection AddResiliencePipelineRegistry<TKey>(this IServiceCollection services, Action<ResiliencePipelineRegistryOptions<TKey>> configure) where TKey : notnull;
    public static IServiceCollection AddResiliencePipelineRegistry<TKey>(this IServiceCollection services) where TKey : notnull;
}
