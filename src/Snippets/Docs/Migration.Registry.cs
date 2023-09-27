using Polly.Registry;

namespace Snippets.Docs;

internal static partial class Migration
{
    public static void Registry_V7()
    {
        #region migration-registry-v7

        // Create a registry
        var registry = new PolicyRegistry();

        // Try get a policy
        registry.TryGet<IAsyncPolicy>("my-key", out IAsyncPolicy? policy);

        // Try get a generic policy
        registry.TryGet<IAsyncPolicy<string>>("my-key", out IAsyncPolicy<string>? genericPolicy);

        // Add a policy
        registry.Add("my-key", Policy.Timeout(TimeSpan.FromSeconds(10)));

        // Update a policy
        registry.AddOrUpdate(
            "my-key",
            Policy.Timeout(TimeSpan.FromSeconds(10)),
            (key, previous) => Policy.Timeout(TimeSpan.FromSeconds(10)));

        #endregion
    }

    public static void Registry_V8()
    {
        #region migration-registry-v8

        // Create a registry
        var registry = new ResiliencePipelineRegistry<string>();

        // Try get a pipeline
        registry.TryGetPipeline("my-key", out ResiliencePipeline? pipeline);

        // Try get a generic pipeline
        registry.TryGetPipeline<string>("my-key", out ResiliencePipeline<string>? genericPipeline);

        // Add a pipeline using a builder, when "my-key" pipeline is retrieved it will be dynamically built and cached
        registry.TryAddBuilder("my-key", (builder, context) => builder.AddTimeout(TimeSpan.FromSeconds(10)));

        // Get or add pipeline
        registry.GetOrAddPipeline("my-key", builder => builder.AddTimeout(TimeSpan.FromSeconds(10)));

        #endregion
    }
}
