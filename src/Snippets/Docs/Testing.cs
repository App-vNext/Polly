using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Polly.Registry;
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

    public static void PipelineProviderProviderMocking()
    {
        #region testing-resilience-pipeline-provider-mocking

        ResiliencePipelineProvider<string> pipelineProvider = Substitute.For<ResiliencePipelineProvider<string>>();

        // Mock the pipeline provider to return an empty pipeline for testing
        pipelineProvider
            .GetPipeline("my-pipeline")
            .Returns(ResiliencePipeline.Empty);

        // Use the mocked pipeline provider in your code
        var api = new MyApi(pipelineProvider);

        // You can now test the api

        #endregion
    }
}

#region testing-resilience-pipeline-provider-usage

// Represents an arbitrary API that needs resilience support
public class MyApi
{
    private readonly ResiliencePipeline _pipeline;

    // The value of pipelineProvider is injected via dependency injection
    public MyApi(ResiliencePipelineProvider<string> pipelineProvider)
    {
        if (pipelineProvider is null)
        {
            throw new ArgumentNullException(nameof(pipelineProvider));
        }

        _pipeline = pipelineProvider.GetPipeline("my-pipeline");
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken) =>
        await _pipeline.ExecuteAsync(
            static async token =>
            {
                // Add your code here
            },
            cancellationToken);
}

// Extensions to incorporate MyApi into dependency injection
public static class MyApiExtensions
{
    public static IServiceCollection AddMyApi(this IServiceCollection services) =>
        services
            .AddResiliencePipeline("my-pipeline", builder =>
            {
                builder.AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 4
                });
            })
            .AddSingleton<MyApi>();
}

#endregion
