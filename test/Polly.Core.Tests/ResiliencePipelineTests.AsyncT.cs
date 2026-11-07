namespace Polly.Core.Tests;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

public partial class ResiliencePipelineTests
{
    public static IEnumerable<object[]> ExecuteAsyncT_EnsureCorrectBehavior_Data() =>
        ConvertExecuteParameters(ExecuteAsyncT_EnsureCorrectBehavior_ExecuteParameters);

    private static IEnumerable<ExecuteParameters> ExecuteAsyncT_EnsureCorrectBehavior_ExecuteParameters()
    {
        long result = 12345;

        yield return new ExecuteParameters<long>(r => r.ExecuteAsync(async t => result), result)
        {
            Caption = "ExecuteAsyncT_NoCancellation",
            AssertContext = AssertResilienceContext,
        };

        yield return new ExecuteParameters<long>(r => r.ExecuteAsync(async t => { t.ShouldBe(CancellationToken); return result; }, CancellationToken), result)
        {
            Caption = "ExecuteAsyncT_Cancellation",
            AssertContext = AssertResilienceContextAndToken,
        };

        yield return new ExecuteParameters<long>(r => r.ExecuteAsync(async (state, t) =>
        {
            state.ShouldBe("state");
            t.ShouldBe(CancellationToken);
            return result;
        }, "state", CancellationToken), result)
        {
            Caption = "ExecuteAsyncT_StateAndCancellation",
            AssertContext = AssertResilienceContextAndToken,
        };

        yield return new ExecuteParameters<long>(r => r.ExecuteAsync(async (_, s) => { s.ShouldBe("dummy-state"); return result; }, ResilienceContextPool.Shared.Get(TestCancellation.Token), "dummy-state"), result)
        {
            Caption = "ExecuteAsyncT_ResilienceContextAndState",
            AssertContext = AssertResilienceContext,
            AssertContextAfter = AssertContextInitialized,
        };

        yield return new ExecuteParameters<long>(r => r.ExecuteAsync(async _ => result, ResilienceContextPool.Shared.Get(TestCancellation.Token)), result)
        {
            Caption = "ExecuteAsyncT_ResilienceContext",
            AssertContext = AssertResilienceContext,
            AssertContextAfter = AssertContextInitialized,
        };

        static void AssertResilienceContext(ResilienceContext context)
        {
            context.IsSynchronous.ShouldBeFalse();
            context.IsVoid.ShouldBeFalse();
            context.ResultType.ShouldBe(typeof(long));
            context.ContinueOnCapturedContext.ShouldBeFalse();
        }

        static void AssertResilienceContextAndToken(ResilienceContext context)
        {
            AssertResilienceContext(context);
            context.CancellationToken.ShouldBe(CancellationToken);
        }

        static void AssertContextInitialized(ResilienceContext context) => context.IsInitialized.ShouldBeTrue();
    }

    [Theory]
#pragma warning disable xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
    [MemberData(nameof(ExecuteAsyncT_EnsureCorrectBehavior_Data))]
#pragma warning restore xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
    public async Task ExecuteAsyncT_Ok(ExecuteParameters parameters)
    {
        ResilienceContext? context = null;

        var strategy = new TestResilienceStrategy
        {
            Before = (c, _) =>
            {
                context = c;
                parameters.AssertContext(c);
            },
        };

        var result = await parameters.Execute(strategy.AsPipeline());

        parameters.AssertContextAfter(context!);
        parameters.AssertResult(result);
    }

    [Fact]
    public async Task ExecuteAsync_T_EnsureCallStackPreserved()
    {
        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);

        await AssertStackTrace(s => s.ExecuteAsync(_ => MyThrowingMethod()));
        await AssertStackTrace(s => s.ExecuteAsync(_ => MyThrowingMethod(), context));
        await AssertStackTrace(s => s.ExecuteAsync((_, _) => MyThrowingMethod(), context, "state"));
        await AssertStackTrace(s => s.ExecuteAsync((_, _) => MyThrowingMethod(), "state"));

        static async ValueTask AssertStackTrace(Func<ResiliencePipeline, ValueTask<string>> execute)
        {
            var strategy = new TestResilienceStrategy().AsPipeline();

            var error = await Should.ThrowAsync<FormatException>(() => execute(strategy).AsTask());

            error.StackTrace.ShouldNotBeNull();
            error.StackTrace.ShouldContain(nameof(MyThrowingMethod));
        }

        static ValueTask<string> MyThrowingMethod() => throw new FormatException();
    }

    [Fact]
    public async Task ExecuteOutcomeAsync_Ok()
    {
        var result = await ResiliencePipeline.Empty.ExecuteOutcomeAsync((context, state) =>
        {
            state.ShouldBe("state");
            context.IsSynchronous.ShouldBeFalse();
            context.ResultType.ShouldBe(typeof(int));
            return Outcome.FromResultAsValueTask(12345);
        },
        ResilienceContextPool.Shared.Get(TestCancellation.Token),
        "state");

        result.Result.ShouldBe(12345);
    }

    [Fact]
    public async Task ExecuteAsync_T_AsynchronousCallback_Ok()
    {
        await AssertCompletesAsynchronously(static (s, t) => s.ExecuteAsync(_ => new ValueTask<string>(t)));
        await AssertCompletesAsynchronously(static (s, t) => s.ExecuteAsync(_ => new ValueTask<string>(t), ResilienceContextPool.Shared.Get(TestCancellation.Token)));
        await AssertCompletesAsynchronously(static (s, t) => s.ExecuteAsync((_, _) => new ValueTask<string>(t), ResilienceContextPool.Shared.Get(TestCancellation.Token), "state"));
        await AssertCompletesAsynchronously(static (s, t) => s.ExecuteAsync((_, _) => new ValueTask<string>(t), "state"));

        static async Task AssertCompletesAsynchronously(Func<ResiliencePipeline, Task<string>, ValueTask<string>> execute)
        {
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            var strategy = new TestResilienceStrategy().AsPipeline();

            var task = execute(strategy, tcs.Task);

            task.IsCompleted.ShouldBeFalse();

            tcs.SetResult("dummy");

            var result = await task;

            result.ShouldBe("dummy");
        }
    }

    [Fact]
    public async Task ExecuteAsync_T_AsynchronousCallback_EnsureExceptionRethrown()
    {
        await AssertThrowsAsynchronously(static (s, t) => s.ExecuteAsync(_ => new ValueTask<string>(t)));
        await AssertThrowsAsynchronously(static (s, t) => s.ExecuteAsync(_ => new ValueTask<string>(t), ResilienceContextPool.Shared.Get(TestCancellation.Token)));
        await AssertThrowsAsynchronously(static (s, t) => s.ExecuteAsync((_, _) => new ValueTask<string>(t), ResilienceContextPool.Shared.Get(TestCancellation.Token), "state"));
        await AssertThrowsAsynchronously(static (s, t) => s.ExecuteAsync((_, _) => new ValueTask<string>(t), "state"));

        static async Task AssertThrowsAsynchronously(Func<ResiliencePipeline, Task<string>, ValueTask<string>> execute)
        {
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            var exception = new FormatException("Dummy message.");
            var strategy = new TestResilienceStrategy().AsPipeline();

            var task = execute(strategy, tcs.Task).AsTask();

            task.IsCompleted.ShouldBeFalse();

            tcs.SetException(exception);

            var error = await Should.ThrowAsync<FormatException>(() => task);

            error.ShouldBeSameAs(exception);
        }
    }
}
