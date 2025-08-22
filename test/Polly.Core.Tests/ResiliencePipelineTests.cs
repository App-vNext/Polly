using NSubstitute;
using Polly.Utils;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests;

public partial class ResiliencePipelineTests
{
    public static readonly CancellationToken CancellationToken = new CancellationTokenSource().Token;

#pragma warning disable IDE0028
    public static TheoryData<ResilienceContextPool?> ResilienceContextPools = new()
    {
        null,
        ResilienceContextPool.Shared,
    };
#pragma warning restore IDE0028

    [Fact]
    public async Task DisposeAsync_NullPipeline_OK()
    {
        await ResiliencePipeline.Empty.DisposeHelper.DisposeAsync();
        await ResiliencePipeline.Empty.DisposeHelper.DisposeAsync();

        ResiliencePipeline.Empty.Execute(() => 1).ShouldBe(1);
    }

    [Fact]
    public async Task DisposeAsync_NullGenericPipeline_OK()
    {
        await ResiliencePipeline<int>.Empty.DisposeHelper.DisposeAsync();
        await ResiliencePipeline<int>.Empty.DisposeHelper.DisposeAsync();

        ResiliencePipeline.Empty.Execute(() => 1).ShouldBe(1);
    }

    [Fact]
    public async Task DisposeAsync_Reject_Throws()
    {
        var component = Substitute.For<PipelineComponent>();
        var pipeline = new ResiliencePipeline(component, DisposeBehavior.Reject, null);

        var exception = await Should.ThrowAsync<InvalidOperationException>(() => pipeline.DisposeHelper.DisposeAsync().AsTask());
        exception.Message.ShouldBe("Disposing this resilience pipeline is not allowed because it is owned by the pipeline registry.");
    }

    [Fact]
    public async Task DisposeAsync_Allowed_Disposed()
    {
        var component = Substitute.For<PipelineComponent>();
        var pipeline = new ResiliencePipeline(component, DisposeBehavior.Allow, null);
        await pipeline.DisposeHelper.DisposeAsync();
        await pipeline.DisposeHelper.DisposeAsync();

        Should.Throw<ObjectDisposedException>(() => pipeline.Execute(() => { }));

        await component.Received(1).DisposeAsync();
    }

    [Fact]
    public void Null_Ok()
    {
        ResiliencePipeline.Empty.ShouldNotBeNull();
        ResiliencePipeline<string>.Empty.ShouldNotBeNull();
    }

    [Fact]
    public async Task DebuggerProxy_Ok()
    {
        await using var pipeline = (CompositeComponent)PipelineComponentFactory.CreateComposite(
        [
            Substitute.For<PipelineComponent>(),
            Substitute.For<PipelineComponent>(),
        ], null!, null!);

        new CompositeComponentDebuggerProxy(pipeline).Strategies.Count().ShouldBe(2);
    }

    [Fact]
    public void Pool_IsSharedPool()
    {
        var component = Substitute.For<PipelineComponent>();
        var disposeBehavior = DisposeBehavior.Ignore;
        ResilienceContextPool? pool = null;

        var pipeline = new ResiliencePipeline(component, disposeBehavior, pool);
        pipeline.Pool.ShouldBe(ResilienceContextPool.Shared);
    }

    [Fact]
    public void Pool_IsPool()
    {
        var component = Substitute.For<PipelineComponent>();
        var disposeBehavior = DisposeBehavior.Ignore;
        var pool = Substitute.For<ResilienceContextPool>();

        var pipeline = new ResiliencePipeline(component, disposeBehavior, pool);
        pipeline.Pool.ShouldBe(pool);
    }

    public class ExecuteParameters<T> : ExecuteParameters
    {
        public ExecuteParameters(Func<ResiliencePipeline, Task<T>> execute, T resultValue)
        {
            Execute = async strategy =>
            {
                var result = await execute(strategy);
                return result!;
            };

            AssertResult = result => result.ShouldBeOfType<T>().ShouldBe(resultValue);
        }

        public ExecuteParameters(Func<ResiliencePipeline, ValueTask<T>> execute, T resultValue)
        {
            Execute = async strategy =>
            {
                var result = await execute(strategy);
                return result!;
            };

            AssertResult = result => result.ShouldBeOfType<T>().ShouldBe(resultValue);
        }

        public ExecuteParameters(Func<ResiliencePipeline, T> execute, T resultValue)
        {
            Execute = strategy => new ValueTask<object>(execute(strategy)!);
            AssertResult = result => result.ShouldBeOfType<T>().ShouldBe(resultValue);
        }
    }

    public class ExecuteParameters
    {
        public ExecuteParameters()
        {
        }

        public ExecuteParameters(Func<ResiliencePipeline, ValueTask> execute)
        {
            Execute = async r =>
            {
                await execute(r);
                return VoidResult.Instance;
            };

            AssertResult = r => r.ShouldBe(VoidResult.Instance);
        }

        public ExecuteParameters(Func<ResiliencePipeline, Task> execute)
        {
            Execute = async r =>
            {
                await execute(r);
                return VoidResult.Instance;
            };

            AssertResult = r => r.ShouldBe(VoidResult.Instance);
        }

        public ExecuteParameters(Action<ResiliencePipeline> execute)
        {
            Execute = r =>
            {
                execute(r);
                return new ValueTask<object>(VoidResult.Instance);
            };

            AssertResult = r => r.ShouldBe(VoidResult.Instance);
        }

        public Func<ResiliencePipeline, ValueTask<object>> Execute { get; set; } = r => new ValueTask<object>(VoidResult.Instance);

        public Action<ResilienceContext> AssertContext { get; set; } = _ => { };

        public Action<object> AssertResult { get; set; } = _ => { };

        public Action<ResilienceContext> AssertContextAfter { get; set; } = _ => { };

        public string Caption { get; set; } = "unknown";

        public override string ToString() => Caption;
    }

    public static IEnumerable<object[]> ConvertExecuteParameters(Func<IEnumerable<ExecuteParameters>> parameters) => [.. parameters().Select(p => new object[] { p })];
}
