using System.Threading.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Polly.RateLimiting;
using Polly.Registry;

namespace Polly.Extensions.Tests;

public class DisposablePipelineTests
{
    [Fact]
    public void DisposePipeline_EnsureLinkedResourcesDisposedToo()
    {
        var limiters = new List<RateLimiter>();

        var provider = new ServiceCollection()
            .AddResiliencePipeline("my-pipeline", (builder, context) =>
            {
                var limiter = new ConcurrencyLimiter(new ConcurrencyLimiterOptions
                {
                    PermitLimit = 1,
                    QueueLimit = 1
                });
                limiters.Add(limiter);

                builder.AddRateLimiter(new RateLimiterStrategyOptions
                {
                    RateLimiter = args => limiter.AcquireAsync(1, args.Context.CancellationToken)
                });

                // when the pipeline instance is disposed, limiter is disposed too
                context.OnPipelineDisposed(() => limiter.Dispose());
            })
            .BuildServiceProvider();

        limiters.Should().HaveCount(0);
        provider.GetRequiredService<ResiliencePipelineProvider<string>>().GetPipeline("my-pipeline");
        provider.GetRequiredService<ResiliencePipelineProvider<string>>().GetPipeline("my-pipeline");
        limiters.Should().HaveCount(1);
        IsDisposed(limiters[0]).Should().BeFalse();

        provider.Dispose();
        limiters.Should().HaveCount(1);
        IsDisposed(limiters[0]).Should().BeTrue();
    }

    private static bool IsDisposed(RateLimiter limiter)
    {
        try
        {
            limiter.AcquireAsync(1).AsTask().GetAwaiter().GetResult();
            return false;
        }
        catch (ObjectDisposedException)
        {
            return true;
        }
    }
}
