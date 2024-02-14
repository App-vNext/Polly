namespace Polly.Core.Tests;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

public partial class ResiliencePipelineTests
{
    public static IEnumerable<object[]> ExecuteAsync_EnsureCorrectBehavior_Data() =>
        ConvertExecuteParameters(ExecuteAsync_EnsureCorrectBehavior_ExecuteParameters);

    private static IEnumerable<ExecuteParameters> ExecuteAsync_EnsureCorrectBehavior_ExecuteParameters()
    {
        yield return new ExecuteParameters(r => r.ExecuteAsync(async _ => { }))
        {
            Caption = "ExecuteAsync_NoCancellation",
            AssertContext = AssertResilienceContext,
        };

        yield return new ExecuteParameters(r => r.ExecuteAsync(async t => { t.Should().Be(CancellationToken); }, CancellationToken))
        {
            Caption = "ExecuteAsync_Cancellation",
            AssertContext = AssertResilienceContextAndToken,
        };

        yield return new ExecuteParameters(r => r.ExecuteAsync(async (state, t) => { state.Should().Be("state"); t.Should().Be(CancellationToken); }, "state", CancellationToken))
        {
            Caption = "ExecuteAsync_StateAndCancellation",
            AssertContext = AssertResilienceContextAndToken,
        };

        yield return new ExecuteParameters(r => r.ExecuteAsync(async (state, t) => { state.Should().Be("state"); t.Should().Be(CancellationToken); }, "state", CancellationToken))
        {
            Caption = "ExecuteAsync_StateAndCancellation",
            AssertContext = AssertResilienceContextAndToken,
        };

        yield return new ExecuteParameters(r => r.ExecuteAsync(async (_, s) => { s.Should().Be("dummy-state"); }, ResilienceContextPool.Shared.Get(), "dummy-state"))
        {
            Caption = "ExecuteAsync_ResilienceContextAndState",
            AssertContext = AssertResilienceContext,
            AssertContextAfter = AssertContextInitialized,
        };

        yield return new ExecuteParameters(r => r.ExecuteAsync(context => default, ResilienceContextPool.Shared.Get()))
        {
            Caption = "ExecuteAsync_ResilienceContext",
            AssertContext = AssertResilienceContext,
            AssertContextAfter = AssertContextInitialized,
        };

        static void AssertResilienceContext(ResilienceContext context)
        {
            context.IsSynchronous.Should().BeFalse();
            context.IsVoid.Should().BeTrue();
            context.ContinueOnCapturedContext.Should().BeFalse();
        }

        static void AssertResilienceContextAndToken(ResilienceContext context)
        {
            AssertResilienceContext(context);
            context.CancellationToken.Should().Be(CancellationToken);
        }

        static void AssertContextInitialized(ResilienceContext context) => context.IsInitialized.Should().BeTrue();
    }

    [MemberData(nameof(ExecuteAsync_EnsureCorrectBehavior_Data))]
    [Theory]
    public async Task ExecuteAsync_Ok(ExecuteParameters parameters)
    {
        ResilienceContext? context = null;

        var strategy = new TestResilienceStrategy
        {
            Before = (c, _) =>
            {
                context = c;
                parameters.AssertContext(c);
            },
        }.AsPipeline();

        var result = await parameters.Execute(strategy);

        parameters.AssertContextAfter(context!);
        parameters.AssertResult(result);
    }

    [Fact]
    public async Task ExecuteAsync_EnsureCallStackPreserved()
    {
        await AssertStackTrace(s => s.ExecuteAsync(_ => MyThrowingMethod()));
        await AssertStackTrace(s => s.ExecuteAsync(_ => MyThrowingMethod(), ResilienceContextPool.Shared.Get()));
        await AssertStackTrace(s => s.ExecuteAsync((_, _) => MyThrowingMethod(), ResilienceContextPool.Shared.Get(), "state"));
        await AssertStackTrace(s => s.ExecuteAsync((_, _) => MyThrowingMethod(), "state"));

        static async ValueTask AssertStackTrace(Func<ResiliencePipeline, ValueTask> execute)
        {
            var strategy = new TestResilienceStrategy().AsPipeline();

            var error = await strategy
                .Invoking(s =>
                {
                    return execute(s).AsTask();
                })
                .Should()
                .ThrowAsync<FormatException>();

            error.And.StackTrace.Should().Contain(nameof(MyThrowingMethod));
        }

        static ValueTask MyThrowingMethod() => throw new FormatException();
    }
}
