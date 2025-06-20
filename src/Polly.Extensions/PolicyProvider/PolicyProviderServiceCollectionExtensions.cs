using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly.PolicyProvider;

namespace Polly;

/// <summary>
/// Extension methods for adding policy provider services to the service collection.
/// </summary>
public static class PolicyProviderServiceCollectionExtensions
{
    /// <summary>
    /// Adds the policy provider to the service collection with default configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddPolicyProvider(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IPolicyProvider, DefaultPolicyProvider>();

        return services;
    }

    /// <summary>
    /// Adds the policy provider to the service collection with configuration support.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">An action to configure the policy provider options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddPolicyProvider(
        this IServiceCollection services,
        Action<PolicyProviderOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        services.TryAddSingleton<IPolicyProvider, DefaultPolicyProvider>();

        return services;
    }
}