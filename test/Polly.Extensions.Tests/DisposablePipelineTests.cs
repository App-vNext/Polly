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

        limiters.Count.ShouldBe(0);
        provider.GetRequiredService<ResiliencePipelineProvider<string>>().GetPipeline("my-pipeline");
        provider.GetRequiredService<ResiliencePipelineProvider<string>>().GetPipeline("my-pipeline");
        limiters.Count.ShouldBe(1);
        IsDisposed(limiters[0]).ShouldBeFalse();

        provider.Dispose();
        limiters.Count.ShouldBe(1);
        IsDisposed(limiters[0]).ShouldBeTrue();
    }

    [Fact]
    public async Task DisposePipeline_EnsureExternalPipelineNotDisposed()
    {
        var registry1 = new ResiliencePipelineRegistry<string>();
        var pipeline1 = registry1.GetOrAddPipeline("my-pipeline", builder => builder.AddConcurrencyLimiter(1));
        var pipeline2 = registry1.GetOrAddPipeline<string>("my-pipeline", builder => builder.AddConcurrencyLimiter(1));

        var registry2 = new ResiliencePipelineRegistry<string>();
        var pipeline3 = registry2.GetOrAddPipeline<string>("my-pipeline", builder => builder
            .AddPipeline(pipeline1)
            .AddPipeline(pipeline2));

        pipeline3.Execute(() => string.Empty);
        await registry2.DisposeAsync();
        Should.Throw<ObjectDisposedException>(() => pipeline3.Execute(() => string.Empty));

        pipeline1.Execute(() => { });
        pipeline2.Execute(() => string.Empty);

        await registry1.DisposeAsync();

        Should.Throw<ObjectDisposedException>(() => pipeline1.Execute(() => { }));
        Should.Throw<ObjectDisposedException>(() => pipeline2.Execute(() => string.Empty));
    }

    [Fact]
    public void DisposePipeline_OnPipelineDisposed_Throws_NullArg()
    {
        var asserted = false;

        var provider = new ServiceCollection()
            .AddResiliencePipeline("my-pipeline", (_, context) =>
            {
                Assert.Throws<ArgumentNullException>("callback", () => context.OnPipelineDisposed(null!));
                asserted = true;
            })
            .BuildServiceProvider();

        provider.GetRequiredService<ResiliencePipelineProvider<string>>().GetPipeline("my-pipeline");
        asserted.ShouldBeTrue();
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
