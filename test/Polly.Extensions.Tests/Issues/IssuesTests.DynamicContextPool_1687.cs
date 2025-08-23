using Microsoft.Extensions.DependencyInjection;
using Polly.Registry;

namespace Polly.Extensions.Tests.Issues;

public partial class IssuesTests
{
    private class CustomResilienceContextPool : ResilienceContextPool
    {
        public override ResilienceContext Get(ResilienceContextCreationArguments arguments)
        {
            if (arguments.ContinueOnCapturedContext is null)
            {
                arguments = new ResilienceContextCreationArguments(arguments.OperationKey, continueOnCapturedContext: true, arguments.CancellationToken);
            }

            return Shared.Get(arguments);
        }

        public override void Return(ResilienceContext context) => Shared.Return(context);
    }

    private class ContextCreationTestStrategy : ResilienceStrategy
    {
        public int HitCount { get; private set; }

        protected override async ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state)
        {
            context.ContinueOnCapturedContext.ShouldBeTrue();

            HitCount++;

            return await callback(context, state);
        }
    }

    [Fact]
    public async Task DynamicContextPool_1687()
    {
        var pool = new CustomResilienceContextPool();
        var strategy = new ContextCreationTestStrategy();
        var services = new ServiceCollection();
        string key = "my-key";

        services.AddResiliencePipelineRegistry<string>(options => options.BuilderFactory = () => new ResiliencePipelineBuilder
        {
            ContextPool = pool,
        });

        services.AddResiliencePipeline(key, builder =>
        {
            builder.ContextPool.ShouldBe(pool);
            builder.AddStrategy(strategy);
        });

        // create the pipeline provider
        var provider = services.BuildServiceProvider().GetRequiredService<ResiliencePipelineProvider<string>>();

        var pipeline = provider.GetPipeline(key);

        await pipeline.ExecuteAsync(async ct => await default(ValueTask), TestCancellation.Token);

        strategy.HitCount.ShouldBeGreaterThan(0);
    }
}
