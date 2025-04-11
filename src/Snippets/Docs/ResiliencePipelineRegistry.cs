using System.Net.Http;
using System.Threading.RateLimiting;
using Polly.RateLimiting;
using Polly.Registry;
using Polly.Retry;

namespace Snippets.Docs;

internal static class ResiliencePipelineRegistry
{
    public static async Task Usage()
    {
        #region registry-usage

        var registry = new ResiliencePipelineRegistry<string>();

        // Register builder for pipeline "A"
        registry.TryAddBuilder("A", (builder, context) =>
        {
            // Define your pipeline
            builder.AddRetry(new RetryStrategyOptions());
        });

        // Register generic builder for pipeline "A"; you can use the same key
        // because generic and non-generic pipelines are stored separately
        registry.TryAddBuilder<HttpResponseMessage>("A", (builder, context) =>
        {
            // Define your pipeline
            builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>());
        });

        // Fetch pipeline "A"
        ResiliencePipeline pipelineA = registry.GetPipeline("A");

        // Fetch generic pipeline "A"
        ResiliencePipeline<HttpResponseMessage> genericPipelineA = registry.GetPipeline<HttpResponseMessage>("A");

        // Returns false since pipeline "unknown" isn't registered
        var doesPipelineExist = registry.TryGetPipeline("unknown", out var pipeline);

        // Throws KeyNotFoundException because pipeline "unknown" isn't registered
        try
        {
            registry.GetPipeline("unknown");
        }
        catch (KeyNotFoundException)
        {
            // Handle the exception
        }

        #endregion
    }

    public static async Task UsageWithoutBuilders()
    {
        #region registry-usage-no-builder

        var registry = new ResiliencePipelineRegistry<string>();

        // Dynamically retrieve or create pipeline "A"
        ResiliencePipeline pipeline = registry.GetOrAddPipeline("A", (builder, context) =>
        {
            // Define your pipeline
            builder.AddRetry(new RetryStrategyOptions());
        });

        // Dynamically retrieve or create generic pipeline "A"
        ResiliencePipeline<HttpResponseMessage> genericPipeline = registry.GetOrAddPipeline<HttpResponseMessage>("A", (builder, context) =>
        {
            // Define your pipeline
            builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>());
        });

        #endregion
    }

    public static async Task RegistryOptions()
    {
        #region registry-options

        var options = new ResiliencePipelineRegistryOptions<string>
        {
            BuilderComparer = StringComparer.OrdinalIgnoreCase,
            PipelineComparer = StringComparer.OrdinalIgnoreCase,
            BuilderFactory = () => new ResiliencePipelineBuilder
            {
                InstanceName = "lets change the default of InstanceName",
                Name = "lets change the default of Name",
            },
            BuilderNameFormatter = key => $"key:{key}",
            InstanceNameFormatter = key => $"instance-key:{key}",
        };

        var registry = new ResiliencePipelineRegistry<string>();

        #endregion
    }

    public static async Task DynamicReloads()
    {
        var cancellationToken = CancellationToken.None;

        #region registry-reloads

        var registry = new ResiliencePipelineRegistry<string>();

        registry.TryAddBuilder("A", (builder, context) =>
        {
            // Add the reload token. Tokens that are already canceled are ignored.
            context.AddReloadToken(cancellationToken);

            // Define the pipeline.
            builder.AddRetry(new RetryStrategyOptions());
        });

        // This instance remains valid even after a reload.
        ResiliencePipeline pipeline = registry.GetPipeline("A");

        #endregion
    }

    public static void RegistryDisposed()
    {
        #region registry-disposed

        var registry = new ResiliencePipelineRegistry<string>();

        // This instance is valid even after reload.
        ResiliencePipeline pipeline = registry
            .GetOrAddPipeline("A", (builder, context) => builder.AddTimeout(TimeSpan.FromSeconds(10)));

        // Dispose the registry
        registry.Dispose();

        try
        {
            pipeline.Execute(() => { });
        }
        catch (ObjectDisposedException)
        {
            // Using a pipeline that was disposed by the registry
        }

        #endregion
    }

    public static async Task DynamicReloadsWithDispose()
    {
        #region registry-reloads-and-dispose

        var registry = new ResiliencePipelineRegistry<string>();

        registry.TryAddBuilder("A", (builder, context) =>
        {
            var cancellation = new CancellationTokenSource();

            // Register the source for potential external triggering
            RegisterCancellationSource(cancellation);

            // Add the reload token; note that an already cancelled token is disregarded
            context.AddReloadToken(cancellation.Token);

            // Configure your pipeline
            builder.AddRetry(new RetryStrategyOptions());

            context.OnPipelineDisposed(() => cancellation.Dispose());
        });

        #endregion
    }

    private static void RegisterCancellationSource(CancellationTokenSource cancellation)
    {
        // Register the source
    }

    #region registry-ratelimiter-dispose
    public static class Program
    {
        public static void Main()
        {
            using var pra = new PipelineRegistryAdapter();
            pra.GetOrCreateResiliencePipeline("Pipeline foo", 1, 10, 100, 1000);
            pra.GetOrCreateResiliencePipeline("Pipeline bar", 2, 20, 200, 2000);
        }
    }

    public sealed class PipelineRegistryAdapter : IDisposable
    {
        private readonly ResiliencePipelineRegistry<string> _resiliencePipelineRegistry = new();
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                _resiliencePipelineRegistry.Dispose();
                _disposed = true;
            }
        }

        private static PartitionedRateLimiter<ResilienceContext> CreateConcurrencyLimiter(string partitionKey, int permitLimit) =>
            PartitionedRateLimiter.Create<ResilienceContext, string>(context =>
            RateLimitPartition.GetConcurrencyLimiter(
                partitionKey: partitionKey,
                factory: partitionKey => new ConcurrencyLimiterOptions { PermitLimit = permitLimit, QueueLimit = 0 }));

        private static PartitionedRateLimiter<ResilienceContext> CreateFixedWindowLimiter(string partitionKey, int permitLimit, TimeSpan window) =>
            PartitionedRateLimiter.Create<ResilienceContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: partitionKey,
                factory: partitionKey => new FixedWindowRateLimiterOptions { PermitLimit = permitLimit, QueueLimit = 0, Window = window }));

        public ResiliencePipeline GetOrCreateResiliencePipeline(string partitionKey, int maximumConcurrentThreads, int sendLimitPerSecond, int sendLimitPerHour, int sendLimitPerDay)
        {
            return _resiliencePipelineRegistry.GetOrAddPipeline(partitionKey, (builder, context) =>
            {
                PartitionedRateLimiter<ResilienceContext>? threadLimiter = null;
                PartitionedRateLimiter<ResilienceContext>? requestLimiter = null;

                // outer strategy: limit threads
                builder.AddRateLimiter(new RateLimiterStrategyOptions
                {
                    RateLimiter = args =>
                    {
                        threadLimiter = CreateConcurrencyLimiter(partitionKey, maximumConcurrentThreads);
                        return threadLimiter.AcquireAsync(args.Context, permitCount: 1, args.Context.CancellationToken);
                    }
                });

                // inner strategy: limit requests (by second, hour, day)
                builder.AddRateLimiter(new RateLimiterStrategyOptions
                {
                    RateLimiter = args =>
                    {
                        PartitionedRateLimiter<ResilienceContext>[] limiters = [
                            CreateFixedWindowLimiter(partitionKey, sendLimitPerSecond, TimeSpan.FromSeconds(1)),
                            CreateFixedWindowLimiter(partitionKey, sendLimitPerHour,   TimeSpan.FromHours(1)),
                            CreateFixedWindowLimiter(partitionKey, sendLimitPerDay,    TimeSpan.FromDays(1)),
                        ];
                        requestLimiter = PartitionedRateLimiter.CreateChained(limiters);
                        return requestLimiter.AcquireAsync(args.Context, permitCount: 1, args.Context.CancellationToken);
                    }
                });

                // unlike other strategies, rate limiters disposed manually
                context.OnPipelineDisposed(() =>
                {
                    threadLimiter?.Dispose();
                    requestLimiter?.Dispose();
                });
            });
        }
    }
    #endregion
}

