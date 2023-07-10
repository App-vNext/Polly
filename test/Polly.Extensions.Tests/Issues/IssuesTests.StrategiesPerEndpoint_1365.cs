using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
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

        // add resilience strategy, keyed by EndpointKey that only defines the builder name
        services.AddResilienceStrategy(new EndpointKey("endpoint-pipeline", string.Empty, string.Empty, new()), (builder, context) =>
        {
            var endpointOptions = context.StrategyKey.EndpointOptions;

            var registry = context.ServiceProvider.GetRequiredService<ResilienceStrategyRegistry<string>>();

            // we want to limit the number of concurrent requests per endpoint and not include the resource.
            // using a registry we can create and cache the shared resilience strategy
            var rateLimiterStrategy = registry.GetOrAddStrategy($"rate-limiter/{context.StrategyKey.EndpointName}", b =>
            {
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

        // Endpoint 1
        var endpoint1Options = new EndpointOptions
        {
            Retries = 3,
            BreakDuration = TimeSpan.FromSeconds(30),
            Timeout = TimeSpan.FromSeconds(10),
        };

        // define a key for each resource/endpoint combination
        var resource1Key = new EndpointKey("endpoint-pipeline", "Endpoint 1", "Resource 1", endpoint1Options);
        var resource2Key = new EndpointKey("endpoint-pipeline", "Endpoint 1", "Resource 2", endpoint1Options);

        var strategy1 = provider.GetStrategy(resource1Key);
        var strategy2 = provider.GetStrategy(resource2Key);

        strategy1.Should().NotBe(strategy2);
        provider.GetStrategy(resource1Key).Should().BeSameAs(strategy1);
        provider.GetStrategy(resource2Key).Should().BeSameAs(strategy2);
    }

    public class EndpointOptions
    {
        public int Retries { get; set; }

        public TimeSpan BreakDuration { get; set; }

        public TimeSpan Timeout { get; set; }

        public int MaxParallelization { get; set; } = 10;
    }

    public record EndpointKey(string BuilderName, string EndpointName, string Resource, EndpointOptions EndpointOptions)
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
