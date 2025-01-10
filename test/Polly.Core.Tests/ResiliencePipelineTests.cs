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

        ResiliencePipeline.Empty.Execute(() => 1).Should().Be(1);
    }

    [Fact]
    public async Task DisposeAsync_NullGenericPipeline_OK()
    {
        await ResiliencePipeline<int>.Empty.DisposeHelper.DisposeAsync();
        await ResiliencePipeline<int>.Empty.DisposeHelper.DisposeAsync();

        ResiliencePipeline.Empty.Execute(() => 1).Should().Be(1);
    }

    [Fact]
    public async Task DisposeAsync_Reject_Throws()
    {
        var component = Substitute.For<PipelineComponent>();
        var pipeline = new ResiliencePipeline(component, DisposeBehavior.Reject, null);

        (await pipeline.Invoking(p => p.DisposeHelper.DisposeAsync().AsTask())
            .Should()
            .ThrowAsync<InvalidOperationException>())
            .WithMessage("Disposing this resilience pipeline is not allowed because it is owned by the pipeline registry.");
    }

    [Fact]
    public async Task DisposeAsync_Allowed_Disposed()
    {
        var component = Substitute.For<PipelineComponent>();
        var pipeline = new ResiliencePipeline(component, DisposeBehavior.Allow, null);
        await pipeline.DisposeHelper.DisposeAsync();
        await pipeline.DisposeHelper.DisposeAsync();

        pipeline.Invoking(p => p.Execute(() => { })).Should().Throw<ObjectDisposedException>();

        await component.Received(1).DisposeAsync();
    }

    [Fact]
    public void Null_Ok()
    {
        ResiliencePipeline.Empty.Should().NotBeNull();
        ResiliencePipeline<string>.Empty.Should().NotBeNull();
    }

    [Fact]
    public async Task DebuggerProxy_Ok()
    {
        await using var pipeline = (CompositeComponent)PipelineComponentFactory.CreateComposite(
        [
            Substitute.For<PipelineComponent>(),
            Substitute.For<PipelineComponent>(),
        ], null!, null!);

        new CompositeComponentDebuggerProxy(pipeline).Strategies.Should().HaveCount(2);
    }

    [Fact]
    public void Pool_IsSharedPool()
    {
        var component = Substitute.For<PipelineComponent>();
        var disposeBehavior = DisposeBehavior.Ignore;
        ResilienceContextPool? pool = null;

        var pipeline = new ResiliencePipeline(component, disposeBehavior, pool);
        pipeline.Pool.Should().Be(ResilienceContextPool.Shared);
    }

    [Fact]
    public void Pool_IsPool()
    {
        var component = Substitute.For<PipelineComponent>();
        var disposeBehavior = DisposeBehavior.Ignore;
        var pool = Substitute.For<ResilienceContextPool>();

        var pipeline = new ResiliencePipeline(component, disposeBehavior, pool);
        pipeline.Pool.Should().Be(pool);
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

            AssertResult = result => result.Should().BeOfType<T>().And.Be(resultValue);
        }

        public ExecuteParameters(Func<ResiliencePipeline, ValueTask<T>> execute, T resultValue)
        {
            Execute = async strategy =>
            {
                var result = await execute(strategy);
                return result!;
            };

            AssertResult = result => result.Should().BeOfType<T>().And.Be(resultValue);
        }

        public ExecuteParameters(Func<ResiliencePipeline, T> execute, T resultValue)
        {
            Execute = strategy => new ValueTask<object>(execute(strategy)!);
            AssertResult = result => result.Should().BeOfType<T>().And.Be(resultValue);
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

            AssertResult = r => r.Should().Be(VoidResult.Instance);
        }

        public ExecuteParameters(Func<ResiliencePipeline, Task> execute)
        {
            Execute = async r =>
            {
                await execute(r);
                return VoidResult.Instance;
            };

            AssertResult = r => r.Should().Be(VoidResult.Instance);
        }

        public ExecuteParameters(Action<ResiliencePipeline> execute)
        {
            Execute = r =>
            {
                execute(r);
                return new ValueTask<object>(VoidResult.Instance);
            };

            AssertResult = r => r.Should().Be(VoidResult.Instance);
        }

        public Func<ResiliencePipeline, ValueTask<object>> Execute { get; set; } = r => new ValueTask<object>(VoidResult.Instance);

        public Action<ResilienceContext> AssertContext { get; set; } = _ => { };

        public Action<object> AssertResult { get; set; } = _ => { };

        public Action<ResilienceContext> AssertContextAfter { get; set; } = _ => { };

        public string Caption { get; set; } = "unknown";

        public override string ToString() => Caption;
    }

    public static IEnumerable<object[]> ConvertExecuteParameters(Func<IEnumerable<ExecuteParameters>> parameters) => parameters().Select(p => new object[] { p }).ToArray();
}
