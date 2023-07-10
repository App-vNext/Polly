// Assembly 'Polly.Extensions'

using Microsoft.Extensions.Options;
using Polly.Registry;

namespace Polly.Extensions.Registry;

public static class ConfigureBuilderContextExtensions
{
    public static void EnableReloads<TKey, TOptions>(this ConfigureBuilderContext<TKey> context, IOptionsMonitor<TOptions> optionsMonitor, string? name = null) where TKey : notnull;
}
