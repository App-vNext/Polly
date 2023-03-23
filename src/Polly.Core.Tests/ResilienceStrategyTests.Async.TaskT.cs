namespace Polly.Core.Tests;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

public partial class ResilienceStrategyTests
{
    public static IEnumerable<object[]> ExecuteAsTaskAsyncT_EnsureCorrectBehavior_Data()
    {
        return ConvertExecuteParameters(ExecuteAsTaskAsyncT_EnsureCorrectBehavior_ExecuteParameters);
    }

    private static IEnumerable<ExecuteParameters> ExecuteAsTaskAsyncT_EnsureCorrectBehavior_ExecuteParameters()
    {
        long result = 12345;

        yield return new ExecuteParameters<long>(r => r.ExecuteTaskAsync(async t => result), result)
        {
            Caption = "ExecuteAsTaskAsyncT_NoCancellation",
            AssertContext = AssertResilienceContext,
            AssertContextAfter = AssertContextNotInitialized,
        };

        yield return new ExecuteParameters<long>(r => r.ExecuteTaskAsync(async t => { t.Should().Be(CancellationToken); return result; }, CancellationToken), result)
        {
            Caption = "ExecuteAsTaskAsyncT_Cancellation",
            AssertContext = AssertResilienceContextAndToken,
            AssertContextAfter = AssertContextNotInitialized,
        };

        yield return new ExecuteParameters<long>(r => r.ExecuteTaskAsync(async (_, s) => { s.Should().Be("dummy-state"); return result; }, ResilienceContext.Get(), "dummy-state"), result)
        {
            Caption = "ExecuteAsTaskAsyncT_ResilienceContextAndState",
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

        static void AssertContextNotInitialized(ResilienceContext context) => context.IsInitialized.Should().BeFalse();

        static void AssertContextInitialized(ResilienceContext context) => context.IsInitialized.Should().BeTrue();
    }

    [MemberData(nameof(ExecuteAsTaskAsyncT_EnsureCorrectBehavior_Data))]
    [Theory]
    public async Task ExecuteAsTaskAsyncT_Ok(ExecuteParameters parameters)
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
}
