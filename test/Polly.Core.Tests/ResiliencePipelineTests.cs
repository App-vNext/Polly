using NSubstitute;
using Polly.Utils;

namespace Polly.Core.Tests;

public partial class ResiliencePipelineTests
{
    public static readonly CancellationToken CancellationToken = new CancellationTokenSource().Token;

    [Fact]
    public void Null_Ok()
    {
        ResiliencePipeline.Null.Should().NotBeNull();
        ResiliencePipeline<string>.Null.Should().NotBeNull();
    }

    [Fact]
    public void DebuggerProxy_Ok()
    {
        var pipeline = (PipelineComponent.CompositeComponent)PipelineComponent.CreateComposite(new[]
        {
            Substitute.For<PipelineComponent>(),
            Substitute.For<PipelineComponent>(),
        }, null!, null!);

        new PipelineComponent.CompositeDebuggerProxy(pipeline).Strategies.Should().HaveCount(2);
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
