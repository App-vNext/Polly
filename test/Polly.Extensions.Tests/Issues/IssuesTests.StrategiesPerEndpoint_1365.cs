using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly.Extensions.Registry;
using Polly.Registry;
using Polly.Retry;
using Polly.Timeout;

namespace Polly.Extensions.Tests.Issues;

public partial class IssuesTests
{
    [Fact]
    public void StrategiesPerEndpoint_1365()
    {
        var services = new ServiceCollection();

        services.AddResilienceStrategy<string>();
        services.AddOptions<EndpointsOptions>();

        // add resilience strategy, keyed by EndpointKey that only defines the builder name
        services.AddResilienceStrategy(new EndpointKey("endpoint-pipeline", string.Empty, string.Empty), (builder, context) =>
        {
            // we want this pipeline to react to changes in options
            context.EnableReloads<EndpointsOptions>();

            var endpointOptions = context.GetOptions<EndpointsOptions>().Endpoints[context.StrategyKey.EndpointName];
            var registry = context.ServiceProvider.GetRequiredService<ResilienceStrategyRegistry<string>>();

            // we want to limit the number of concurrent requests per endpoint and not include the resource.
            // using a registry we can create and cache the shared resilience strategy
            var rateLimiterStrategy = registry.GetOrAddStrategy($"rate-limiter/{context.StrategyKey.EndpointName}", (b, c) =>
            {
                // let's also enable reloads for rate limiter
                c.EnableReloads(context.ServiceProvider.GetRequiredService<IOptionsMonitor<EndpointsOptions>>());

                b.AddConcurrencyLimiter(new ConcurrencyLimiterOptions { PermitLimit = endpointOptions.MaxParallelization });
            });

            builder.AddStrategy(rateLimiterStrategy);

            // apply retries optionally per-endpoint
            if (endpointOptions.Retries > 0)
            {
                builder.AddRetry(new()
                {
                    BackoffType = RetryBackoffType.ExponentialWithJitter,
                    RetryCount = endpointOptions.Retries,
                    StrategyName = $"{context.StrategyKey.EndpointName}-Retry",
                });
            }

            // apply circuit breaker
            builder.AddAdvancedCircuitBreaker(new()
            {
                BreakDuration = endpointOptions.BreakDuration,
                StrategyName = $"{context.StrategyKey.EndpointName}-{context.StrategyKey.Resource}-CircuitBreaker"
            });

            // apply timeout
            builder.AddTimeout(new TimeoutStrategyOptions
            {
                StrategyName = $"{context.StrategyKey.EndpointName}-Timeout",
                Timeout = endpointOptions.Timeout.Add(TimeSpan.FromSeconds(1)),
            });
        });

        // configure the registry to allow multi-dimensional keys
        services.Configure<ResilienceStrategyRegistryOptions<EndpointKey>>(options =>
        {
            options.BuilderComparer = new EndpointKey.BuilderComparer();
            options.StrategyComparer = new EndpointKey.StrategyComparer();

            // format the key for telemetry
            options.StrategyKeyFormatter = key => $"{key.BuilderName}-{key.EndpointName}-{key.Resource}";

            // format the builder name for telemetry
            options.BuilderNameFormatter = key => key.BuilderName;
        });

        // create the strategy provider
        var provider = services.BuildServiceProvider().GetRequiredService<ResilienceStrategyProvider<EndpointKey>>();

        // define a key for each resource/endpoint combination
        var resource1Key = new EndpointKey("endpoint-pipeline", "Endpoint 1", "Resource 1");
        var resource2Key = new EndpointKey("endpoint-pipeline", "Endpoint 1", "Resource 2");

        var strategy1 = provider.GetStrategy(resource1Key);
        var strategy2 = provider.GetStrategy(resource2Key);

        strategy1.Should().NotBe(strategy2);
        provider.GetStrategy(resource1Key).Should().BeSameAs(strategy1);
        provider.GetStrategy(resource2Key).Should().BeSameAs(strategy2);
    }

    public class EndpointOptions
    {
        public int Retries { get; set; } = 3;

        public TimeSpan BreakDuration { get; set; } = TimeSpan.FromSeconds(10);

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(4);

        public int MaxParallelization { get; set; } = 10;
    }

    public class EndpointsOptions
    {
        public Dictionary<string, EndpointOptions> Endpoints { get; set; } = new()
        {
            {  "Endpoint 1", new EndpointOptions { Retries = 2 } },
            {  "Endpoint 2", new EndpointOptions { Retries = 3 } },
        };
    }

    public record EndpointKey(string BuilderName, string EndpointName, string Resource)
    {
        public class BuilderComparer : IEqualityComparer<EndpointKey>
        {
            public bool Equals(EndpointKey? x, EndpointKey? y) => StringComparer.Ordinal.Equals(x?.BuilderName, y?.BuilderName);

            public int GetHashCode([DisallowNull] EndpointKey obj) => StringComparer.Ordinal.GetHashCode(obj.BuilderName);
        }

        public class StrategyComparer : IEqualityComparer<EndpointKey>
        {
            public bool Equals(EndpointKey? x, EndpointKey? y) => (x?.BuilderName, x?.EndpointName, x?.Resource) == (y?.BuilderName, y?.EndpointName, y?.Resource);

            public int GetHashCode([DisallowNull] EndpointKey obj) => (obj.BuilderName, obj.EndpointName, obj.Resource).GetHashCode();
        }
    }
}
