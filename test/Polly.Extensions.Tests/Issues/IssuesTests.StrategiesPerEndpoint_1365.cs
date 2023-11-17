using System.Diagnostics.CodeAnalysis;
using System.Threading.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly.DependencyInjection;
using Polly.Registry;
using Polly.Timeout;

namespace Polly.Extensions.Tests.Issues;

public partial class IssuesTests
{
    [Fact]
    public void StrategiesPerEndpoint_1365()
    {
        var events = new List<MeteringEvent>(1024);
        using var listener = TestUtilities.EnablePollyMetering(events);
        var services = new ServiceCollection();

        services.AddResiliencePipelineRegistry<string>();
        services.AddOptions<EndpointsOptions>();

        // add resilience pipeline, keyed by EndpointKey that only defines the builder name
        services.AddResiliencePipeline(new EndpointKey("endpoint-pipeline", string.Empty, string.Empty), (builder, context) =>
        {
            var serviceProvider = context.ServiceProvider;
            var endpointOptions = context.GetOptions<EndpointsOptions>().Endpoints[context.PipelineKey.EndpointName];
            var registry = context.ServiceProvider.GetRequiredService<ResiliencePipelineRegistry<string>>();

            // we want this pipeline to react to changes to the options
            context.EnableReloads<EndpointsOptions>();

            // we want to limit the number of concurrent requests per endpoint and not include the resource.
            // using a registry we can create and cache the shared resilience pipeline
            var rateLimiterStrategy = registry.GetOrAddPipeline($"rate-limiter/{context.PipelineKey.EndpointName}", (builder, context) =>
            {
                // let's also enable reloads for the rate limiter
                context.EnableReloads(serviceProvider.GetRequiredService<IOptionsMonitor<EndpointsOptions>>());

                builder.AddConcurrencyLimiter(new ConcurrencyLimiterOptions { PermitLimit = endpointOptions.MaxParallelization });
            });

            builder.AddPipeline(rateLimiterStrategy);

            // apply retries optionally per-endpoint
            if (endpointOptions.Retries > 0)
            {
                builder.AddRetry(new()
                {
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    MaxRetryAttempts = endpointOptions.Retries,
                    Name = $"{context.PipelineKey.EndpointName}-Retry",
                });
            }

            // apply circuit breaker
            builder.AddCircuitBreaker(new()
            {
                BreakDuration = endpointOptions.BreakDuration,
                Name = $"{context.PipelineKey.EndpointName}-{context.PipelineKey.Resource}-CircuitBreaker"
            });

            // apply timeout
            builder.AddTimeout(new TimeoutStrategyOptions
            {
                Name = $"{context.PipelineKey.EndpointName}-Timeout",
                Timeout = endpointOptions.Timeout.Add(TimeSpan.FromSeconds(1)),
            });
        });

        // configure the registry to allow multi-dimensional keys
        services.AddResiliencePipelineRegistry<EndpointKey>(options =>
        {
            options.BuilderComparer = new EndpointKey.BuilderComparer();
            options.PipelineComparer = new EndpointKey.StrategyComparer();

            // format the key for telemetry
            options.InstanceNameFormatter = key => $"{key.EndpointName}/{key.Resource}";

            // format the builder name for telemetry
            options.BuilderNameFormatter = key => key.BuilderName;
        });

        // create the pipeline provider
        var provider = services.BuildServiceProvider().GetRequiredService<ResiliencePipelineProvider<EndpointKey>>();

        // define a key for each resource/endpoint combination
        var resource1Key = new EndpointKey("endpoint-pipeline", "Endpoint 1", "Resource 1");
        var resource2Key = new EndpointKey("endpoint-pipeline", "Endpoint 1", "Resource 2");

        var pipeline1 = provider.GetPipeline(resource1Key);
        var pipeline2 = provider.GetPipeline(resource2Key);

        pipeline1.Should().NotBe(pipeline2);
        provider.GetPipeline(resource1Key).Should().BeSameAs(pipeline1);
        provider.GetPipeline(resource2Key).Should().BeSameAs(pipeline2);

        pipeline1.Execute(() => { });
        events.Should().HaveCount(5);
        events[0].Tags["pipeline.name"].Should().Be("endpoint-pipeline");
        events[0].Tags["pipeline.instance"].Should().Be("Endpoint 1/Resource 1");
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
            ["Endpoint 1"] = new EndpointOptions { Retries = 2 },
            ["Endpoint 2"] = new EndpointOptions { Retries = 3 },
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
