// Assembly 'Polly.Extensions'

using System;

namespace Polly.Extensions.DependencyInjection;

public static class PollyDependencyInjectionKeys
{
    public static readonly ResiliencePropertyKey<IServiceProvider> ServiceProvider;
}
