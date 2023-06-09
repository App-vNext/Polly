using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Polly.Extensions.Utils;

internal class OptionsReloadHelperRegistry<TKey> : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<(Type optionsType, TKey key, string? name), object> _helpers = new();

    public OptionsReloadHelperRegistry(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public int Count => _helpers.Count;

    public OptionsReloadHelper<TOptions> Get<TOptions>(TKey key, string? name)
    {
        return (OptionsReloadHelper<TOptions>)_helpers.GetOrAdd((typeof(TOptions), key, name), _ =>
        {
            return new OptionsReloadHelper<TOptions>(_serviceProvider.GetRequiredService<IOptionsMonitor<TOptions>>(), name);
        });
    }

    public void Dispose()
    {
        foreach (var helper in _helpers.Values.OfType<IDisposable>())
        {
            helper.Dispose();
        }

        _helpers.Clear();
    }
}
