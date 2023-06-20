namespace Polly.Core.Tests;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

public partial class ResilienceStrategyTests
{
    public static IEnumerable<object[]> ExecuteAsyncT_EnsureCorrectBehavior_Data()
    {
        return ConvertExecuteParameters(ExecuteAsyncT_EnsureCorrectBehavior_ExecuteParameters);
    }

    private static IEnumerable<ExecuteParameters> ExecuteAsyncT_EnsureCorrectBehavior_ExecuteParameters()
    {
        long result = 12345;

        yield return new ExecuteParameters<long>(r => r.ExecuteAsync(async t => result), result)
        {
            Caption = "ExecuteAsyncT_NoCancellation",
            AssertContext = AssertResilienceContext,
        };

        yield return new ExecuteParameters<long>(r => r.ExecuteAsync(async t => { t.Should().Be(CancellationToken); return result; }, CancellationToken), result)
        {
            Caption = "ExecuteAsyncT_Cancellation",
            AssertContext = AssertResilienceContextAndToken,
        };

        yield return new ExecuteParameters<long>(r => r.ExecuteAsync(async (state, t) =>
        {
            state.Should().Be("state");
            t.Should().Be(CancellationToken);
            return result;
        }, "state", CancellationToken), result)
        {
            Caption = "ExecuteAsyncT_StateAndCancellation",
            AssertContext = AssertResilienceContextAndToken,
        };

        yield return new ExecuteParameters<long>(r => r.ExecuteAsync(async (_, s) => { s.Should().Be("dummy-state"); return result; }, ResilienceContext.Get(), "dummy-state"), result)
        {
            Caption = "ExecuteAsyncT_ResilienceContextAndState",
            AssertContext = AssertResilienceContext,
            AssertContextAfter = AssertContextInitialized,
        };

        yield return new ExecuteParameters<long>(r => r.ExecuteAsync(async _ => result, ResilienceContext.Get()), result)
        {
            Caption = "ExecuteAsyncT_ResilienceContext",
            AssertContext = AssertResilienceContext,
            AssertContextAfter = AssertContextInitialized,
        };

        static void AssertResilienceContext(ResilienceContext context)
        {
            context.IsSynchronous.Should().BeFalse();
            context.IsVoid.Should().BeFalse();
            context.ResultType.Should().Be(typeof(long));
            context.ContinueOnCapturedContext.Should().BeFalse();
        }

        static void AssertResilienceContextAndToken(ResilienceContext context)
        {
            AssertResilienceContext(context);
            context.CancellationToken.Should().Be(CancellationToken);
        }

        static void AssertContextInitialized(ResilienceContext context) => context.IsInitialized.Should().BeTrue();
    }

    [MemberData(nameof(ExecuteAsyncT_EnsureCorrectBehavior_Data))]
    [Theory]
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

        var result = await parameters.Execute(strategy);

        parameters.AssertContextAfter(context!);
        parameters.AssertResult(result);
    }

    [Fact]
    public async Task ExecuteAsync_T_EnsureCallStackPreserved()
    {
        await AssertStackTrace(s => s.ExecuteAsync(_ => MyThrowingMethod()));
        await AssertStackTrace(s => s.ExecuteAsync(_ => MyThrowingMethod(), ResilienceContext.Get()));
        await AssertStackTrace(s => s.ExecuteAsync((_, _) => MyThrowingMethod(), ResilienceContext.Get(), "state"));
        await AssertStackTrace(s => s.ExecuteAsync((_, _) => MyThrowingMethod(), "state"));

        static async ValueTask AssertStackTrace(Func<ResilienceStrategy, ValueTask<string>> execute)
        {
            var strategy = new TestResilienceStrategy();

            var error = await strategy
                .Invoking(s =>
                {
                    return execute(s).AsTask();
                })
                .Should()
                .ThrowAsync<FormatException>();

            error.And.StackTrace.Should().Contain(nameof(MyThrowingMethod));
        }

        static ValueTask<string> MyThrowingMethod() => throw new FormatException();
    }

    [Fact]
    public async Task ExecuteOutcomeAsync_Ok()
    {
        var result = await NullResilienceStrategy.Instance.ExecuteOutcomeAsync((context, state) =>
        {
            state.Should().Be("state");
            context.IsSynchronous.Should().BeFalse();
            context.ResultType.Should().Be(typeof(int));
            return Outcome.FromResultAsTask(12345);
        },
        ResilienceContext.Get(),
        "state");

        result.Result.Should().Be(12345);
    }
}
