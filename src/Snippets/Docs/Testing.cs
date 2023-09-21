using Polly.Retry;
using Polly.Testing;
using Polly.Timeout;
using Xunit;

namespace Snippets.Docs;

internal static class Testing
{
    public static void GetPipelineDescriptor()
    {
        #region get-pipeline-descriptor

        // Build your resilience pipeline.
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 4
            })
            .AddTimeout(TimeSpan.FromSeconds(1))
            .Build();

        // Retrieve the descriptor.
        ResiliencePipelineDescriptor descriptor = pipeline.GetPipelineDescriptor();

        // Check the pipeline's composition with the descriptor.
        Assert.Equal(2, descriptor.Strategies.Count);

        // Verify the retry settings.
        var retryOptions = Assert.IsType<RetryStrategyOptions>(descriptor.Strategies[0].Options);
        Assert.Equal(4, retryOptions.MaxRetryAttempts);

        // Confirm the timeout settings.
        var timeoutOptions = Assert.IsType<TimeoutStrategyOptions>(descriptor.Strategies[1].Options);
        Assert.Equal(TimeSpan.FromSeconds(1), timeoutOptions.Timeout);

        #endregion
    }

    public static void GetPipelineDescriptorGeneric()
    {
        #region get-pipeline-descriptor-generic

        // Construct your resilience pipeline.
        ResiliencePipeline<string> pipeline = new ResiliencePipelineBuilder<string>()
            .AddRetry(new RetryStrategyOptions<string>
            {
                MaxRetryAttempts = 4
            })
            .AddTimeout(TimeSpan.FromSeconds(1))
            .Build();

        // Obtain the descriptor.
        ResiliencePipelineDescriptor descriptor = pipeline.GetPipelineDescriptor();

        // Check the pipeline's composition with the descriptor.
        // ...

        #endregion
    }
}
