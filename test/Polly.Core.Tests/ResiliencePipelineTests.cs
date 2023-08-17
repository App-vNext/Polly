using NSubstitute;
using Polly.Utils;
using Polly.Utils.Pipeline;

namespace Polly.Core.Tests;

#pragma warning disable S3966 // Objects should not be disposed more than once

public partial class ResiliencePipelineTests
{
    public static readonly CancellationToken CancellationToken = new CancellationTokenSource().Token;

    [Fact]
    public async Task Dispose_NullPipeline_OK()
    {
        ResiliencePipeline.Null.DisposeHelper.Dispose();
        ResiliencePipeline.Null.DisposeHelper.Dispose();
        await ResiliencePipeline.Null.DisposeHelper.DisposeAsync();
        await ResiliencePipeline.Null.DisposeHelper.DisposeAsync();

        ResiliencePipeline.Null.Execute(() => 1).Should().Be(1);
    }

    [Fact]
    public async Task Dispose_NullGenericPipeline_OK()
    {
        ResiliencePipeline<int>.Null.DisposeHelper.Dispose();
        ResiliencePipeline<int>.Null.DisposeHelper.Dispose();
        await ResiliencePipeline<int>.Null.DisposeHelper.DisposeAsync();
        await ResiliencePipeline<int>.Null.DisposeHelper.DisposeAsync();

        ResiliencePipeline.Null.Execute(() => 1).Should().Be(1);
    }

    [Fact]
    public async Task Dispose_Reject_Throws()
    {
        var component = Substitute.For<PipelineComponent>();
        var pipeline = new ResiliencePipeline(component, DisposeBehavior.Reject);

        pipeline.Invoking(p => p.DisposeHelper.Dispose())
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Disposing this resilience pipeline is not allowed because it is owned by the pipeline registry.");

        (await pipeline.Invoking(p => p.DisposeHelper.DisposeAsync().AsTask())
            .Should()
            .ThrowAsync<InvalidOperationException>())
            .WithMessage("Disposing this resilience pipeline is not allowed because it is owned by the pipeline registry.");
    }

    [Fact]
    public void Dispose_Allowed_Disposed()
    {
        var component = Substitute.For<PipelineComponent>();
        var pipeline = new ResiliencePipeline(component, DisposeBehavior.Allow);
        pipeline.DisposeHelper.Dispose();
        pipeline.DisposeHelper.Dispose();

        pipeline.Invoking(p => p.Execute(() => { })).Should().Throw<ObjectDisposedException>();

        component.Received(1).Dispose();
    }

    [Fact]
    public async Task DisposeAsync_Allowed_Disposed()
    {
        var component = Substitute.For<PipelineComponent>();
        var pipeline = new ResiliencePipeline(component, DisposeBehavior.Allow);
        await pipeline.DisposeHelper.DisposeAsync();
        await pipeline.DisposeHelper.DisposeAsync();

        pipeline.Invoking(p => p.Execute(() => { })).Should().Throw<ObjectDisposedException>();

        await component.Received(1).DisposeAsync();
    }

    [Fact]
    public void Null_Ok()
    {
        ResiliencePipeline.Null.Should().NotBeNull();
        ResiliencePipeline<string>.Null.Should().NotBeNull();
    }

    [Fact]
    public void DebuggerProxy_Ok()
    {
        using var pipeline = (CompositeComponent)PipelineComponentFactory.CreateComposite(new[]
        {
            Substitute.For<PipelineComponent>(),
            Substitute.For<PipelineComponent>(),
        }, null!, null!);

        new CompositeComponentDebuggerProxy(pipeline).Strategies.Should().HaveCount(2);
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
