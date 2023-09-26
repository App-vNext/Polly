using System.Net.Http;
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
        registry.TryGetPipeline("unknown", out var pipeline);

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
                InstanceName = "lets change the defaults",
                Name = "lets change the defaults",
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
}

