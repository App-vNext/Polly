using System.Threading.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Polly.RateLimiting;
using Polly.Registry;

namespace Polly.Extensions.Tests.Issues;

#pragma warning disable xUnit1031

public partial class IssuesTests
{
    //// This test demonstrates how to use PartitionedRateLimiter to rate-limit the individual users.
    //// Additionally, it also shows how to disable rate limiting for admin users.
    [Fact]
    public async Task PartitionedRateLimiter_EnsureUserLimited_1365()
    {
        // arrange
        var userKey = new ResiliencePropertyKey<string>("user");
        var services = new ServiceCollection();
        services
            .AddResiliencePipeline("shared-limiter", (builder, context) =>
            {
                var partitionedLimiter = PartitionedRateLimiter.Create<ResilienceContext, string>(context =>
                {
                    // you can use `context.ServiceProvider` to resolve whatever service you
                    // want form DI such as `IHttpContextAccessor` and use it here to resolve
                    // any information from HttpContext

                    if (!context.Properties.TryGetValue(userKey, out var user) || user == "admin")
                    {
                        return RateLimitPartition.GetNoLimiter("none");
                    }

                    return RateLimitPartition.GetConcurrencyLimiter(user, key => new ConcurrencyLimiterOptions { PermitLimit = 2, QueueLimit = 0 });
                });

                builder.AddRateLimiter(new RateLimiterStrategyOptions
                {
                    RateLimiter = args => partitionedLimiter.AcquireAsync(args.Context, 1, args.Context.CancellationToken)
                });
            });

        var serviceProvider = services.BuildServiceProvider();
        var pipeline = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>().GetPipeline("shared-limiter");

        // assert user is limited
        using var asserted = new ManualResetEvent(false);
        await Assert.ThrowsAsync<RateLimiterRejectedException>(() => ExecuteBatch("guest-user", asserted));
        asserted.Set();

        // assert admin is not limited
        using var adminAsserted = new ManualResetEvent(false);
        var task = ExecuteBatch("admin", adminAsserted);
        task.Wait(100).Should().BeFalse();
        adminAsserted.Set();
        await task;

        async Task ExecuteBatch(string user, ManualResetEvent waitAsserted)
        {
            var tasks = Enumerable.Repeat(0, 10).Select(_ =>
            {
                return Task.Run(async () =>
                {
                    var context = ResilienceContextPool.Shared.Get();
                    context.Properties.Set(userKey, user);

                    await pipeline.ExecuteAsync(async _ =>
                    {
                        await Task.Yield();
                        waitAsserted.WaitOne();
                    },
                    context);
                });
            }).ToArray();

            var finished = await Task.WhenAny(tasks);
            await finished;
        }
    }
}
