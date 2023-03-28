using System;

namespace Polly.DependencyInjection;

/// <summary>
/// The resilience keys used in the dependency injection scenarios.
/// </summary>
public static class PollyDependencyInjectionKeys
{
    /// <summary>
    /// The key used to store and access the <see cref="IServiceProvider"/> from <see cref="ResilienceProperties"/>.
    /// </summary>
    public static readonly ResiliencePropertyKey<IServiceProvider> ServiceProvider = new("Polly.DependencyInjection.ServiceProvider");
}
