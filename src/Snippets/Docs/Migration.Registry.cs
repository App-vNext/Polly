using Polly.Registry;

namespace Snippets.Docs;

internal static partial class Migration
{
    private const string PolicyKey = nameof(PolicyKey);
    public static void Registry_V7()
    {
        #region migration-registry-v7

        // Create a registry
        var registry = new PolicyRegistry();

        // Add a policy
        registry.Add(PolicyKey, Policy.Timeout(TimeSpan.FromSeconds(10)));

        // Try get a policy
        registry.TryGet<IAsyncPolicy>(PolicyKey, out IAsyncPolicy? policy);

        // Try get a generic policy
        registry.TryGet<IAsyncPolicy<string>>(PolicyKey, out IAsyncPolicy<string>? genericPolicy);

        // Update a policy
        registry.AddOrUpdate(
            PolicyKey,
            Policy.Timeout(TimeSpan.FromSeconds(10)),
            (key, previous) => Policy.Timeout(TimeSpan.FromSeconds(10)));

        #endregion
    }

    private const string PipelineKey = nameof(PipelineKey);
    public static void Registry_V8()
    {
        #region migration-registry-v8

        // Create a registry
        var registry = new ResiliencePipelineRegistry<string>();

        // Add a pipeline using a builder, when the pipeline is retrieved it will be dynamically built and cached
        registry.TryAddBuilder(PipelineKey, (builder, context) => builder.AddTimeout(TimeSpan.FromSeconds(10)));

        // Try get a pipeline
        registry.TryGetPipeline(PipelineKey, out ResiliencePipeline? pipeline);

        // Try get a generic pipeline
        registry.TryGetPipeline(PipelineKey, out ResiliencePipeline<string>? genericPipeline);

        // Get or add pipeline
        registry.GetOrAddPipeline(PipelineKey, builder => builder.AddTimeout(TimeSpan.FromSeconds(10)));

        #endregion
    }
}
