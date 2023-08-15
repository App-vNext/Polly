using Polly.Registry;

namespace Polly.DependencyInjection;

internal sealed class ConfigureResiliencePipelineRegistryOptions<TKey>
    where TKey : notnull
{
    public List<Action<ResiliencePipelineRegistry<TKey>>> Actions { get; } = new();
}
