﻿using System.Threading.RateLimiting;
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
        pipeline3.Invoking(p => p.Execute(() => string.Empty)).Should().Throw<ObjectDisposedException>();

        pipeline1.Execute(() => { });
        pipeline2.Execute(() => string.Empty);

        await registry1.DisposeAsync();

        pipeline1.Invoking(p => p.Execute(() => { })).Should().Throw<ObjectDisposedException>();
        pipeline2.Invoking(p => p.Execute(() => string.Empty)).Should().Throw<ObjectDisposedException>();
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
