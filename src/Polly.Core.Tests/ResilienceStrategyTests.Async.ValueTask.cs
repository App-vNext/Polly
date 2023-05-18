namespace Polly.Core.Tests;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

public partial class ResilienceStrategyTests
{
    public static IEnumerable<object[]> ExecuteAsync_EnsureCorrectBehavior_Data()
    {
        return ConvertExecuteParameters(ExecuteAsync_EnsureCorrectBehavior_ExecuteParameters);
    }

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

        yield return new ExecuteParameters(r => r.ExecuteAsync(async (_, s) => { s.Should().Be("dummy-state"); }, ResilienceContext.Get(), "dummy-state"))
        {
            Caption = "ExecuteAsync_ResilienceContextAndState",
            AssertContext = AssertResilienceContext,
            AssertContextAfter = AssertContextInitialized,
        };

        yield return new ExecuteParameters(r => r.ExecuteAsync(context => default, ResilienceContext.Get()))
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
        };

        var result = await parameters.Execute(strategy);

        parameters.AssertContextAfter(context!);
        parameters.AssertResult(result);
    }
}
