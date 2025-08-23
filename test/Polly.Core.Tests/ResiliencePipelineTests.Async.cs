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

        yield return new ExecuteParameters(r => r.ExecuteAsync(async t => { t.ShouldBe(CancellationToken); }, CancellationToken))
        {
            Caption = "ExecuteAsync_Cancellation",
            AssertContext = AssertResilienceContextAndToken,
        };

        yield return new ExecuteParameters(r => r.ExecuteAsync(async (state, t) => { state.ShouldBe("state"); t.ShouldBe(CancellationToken); }, "state", CancellationToken))
        {
            Caption = "ExecuteAsync_StateAndCancellation",
            AssertContext = AssertResilienceContextAndToken,
        };

        yield return new ExecuteParameters(r => r.ExecuteAsync(async (state, t) => { state.ShouldBe("state"); t.ShouldBe(CancellationToken); }, "state", CancellationToken))
        {
            Caption = "ExecuteAsync_StateAndCancellation",
            AssertContext = AssertResilienceContextAndToken,
        };

        yield return new ExecuteParameters(r => r.ExecuteAsync(async (_, s) => { s.ShouldBe("dummy-state"); }, ResilienceContextPool.Shared.Get(TestCancellation.Token), "dummy-state"))
        {
            Caption = "ExecuteAsync_ResilienceContextAndState",
            AssertContext = AssertResilienceContext,
            AssertContextAfter = AssertContextInitialized,
        };

        yield return new ExecuteParameters(r => r.ExecuteAsync(context => default, ResilienceContextPool.Shared.Get(TestCancellation.Token)))
        {
            Caption = "ExecuteAsync_ResilienceContext",
            AssertContext = AssertResilienceContext,
            AssertContextAfter = AssertContextInitialized,
        };

        static void AssertResilienceContext(ResilienceContext context)
        {
            context.IsSynchronous.ShouldBeFalse();
            context.IsVoid.ShouldBeTrue();
            context.ContinueOnCapturedContext.ShouldBeFalse();
        }

        static void AssertResilienceContextAndToken(ResilienceContext context)
        {
            AssertResilienceContext(context);
            context.CancellationToken.ShouldBe(CancellationToken);
        }

        static void AssertContextInitialized(ResilienceContext context) => context.IsInitialized.ShouldBeTrue();
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
        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);

        await AssertStackTrace(s => s.ExecuteAsync(_ => MyThrowingMethod()));
        await AssertStackTrace(s => s.ExecuteAsync(_ => MyThrowingMethod(), context));
        await AssertStackTrace(s => s.ExecuteAsync((_, _) => MyThrowingMethod(), context, "state"));
        await AssertStackTrace(s => s.ExecuteAsync((_, _) => MyThrowingMethod(), "state"));

        static async ValueTask AssertStackTrace(Func<ResiliencePipeline, ValueTask> execute)
        {
            var strategy = new TestResilienceStrategy().AsPipeline();

            var error = await Should.ThrowAsync<FormatException>(() => execute(strategy).AsTask());

            error.StackTrace.ShouldNotBeNull();
            error.StackTrace.ShouldContain(nameof(MyThrowingMethod));
        }

        static ValueTask MyThrowingMethod() => throw new FormatException();
    }
}
