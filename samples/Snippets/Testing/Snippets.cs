using Microsoft.Extensions.Logging.Abstractions;
using Polly.Retry;
using Polly.Timeout;
using Polly;
using Xunit;
using Polly.Testing;

namespace Snippets.Testing;

internal static class Snippets
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
            .ConfigureTelemetry(NullLoggerFactory.Instance)
            .Build();

        // Retrieve inner strategies.
        ResiliencePipelineDescriptor descriptor = pipeline.GetPipelineDescriptor();

        // Assert the composition.
        Assert.Equal(2, descriptor.Strategies.Count);

        var retryOptions = Assert.IsType<RetryStrategyOptions>(descriptor.Strategies[0].Options);
        Assert.Equal(4, retryOptions.MaxRetryAttempts);

        var timeoutOptions = Assert.IsType<TimeoutStrategyOptions>(descriptor.Strategies[0].Options);
        Assert.Equal(TimeSpan.FromSeconds(1), timeoutOptions.Timeout);

        #endregion
    }
}
