namespace Polly.Core.Tests;

public partial class ResilienceStrategyTests
{
    public static IEnumerable<object[]> Execute_EnsureCorrectBehavior_Data()
    {
        return ConvertExecuteParameters(Execute_EnsureCorrectBehavior_ExecuteParameters);
    }

    private static IEnumerable<ExecuteParameters> Execute_EnsureCorrectBehavior_ExecuteParameters()
    {
        yield return new ExecuteParameters(r => r.Execute(_ => { }))
        {
            Caption = "Execute_NoCancellation",
            AssertContext = AssertResilienceContext,
        };

        yield return new ExecuteParameters(r => r.Execute(t => { t.Should().Be(CancellationToken); }, CancellationToken))
        {
            Caption = "Execute_Cancellation",
            AssertContext = AssertResilienceContextAndToken,
        };

        yield return new ExecuteParameters(r => r.Execute((state, t) => { state.Should().Be("state"); t.Should().Be(CancellationToken); }, "state", CancellationToken))
        {
            Caption = "Execute_StateAndCancellation",
            AssertContext = AssertResilienceContextAndToken,
        };

        yield return new ExecuteParameters(r => r.Execute(() => { }))
        {
            Caption = "Execute",
            AssertContext = AssertResilienceContext,
        };

        yield return new ExecuteParameters(r => r.Execute(state => { state.Should().Be("state"); }, "state"))
        {
            Caption = "ExecuteAndState",
            AssertContext = AssertResilienceContext,
        };

        yield return new ExecuteParameters(r => r.Execute(context => { }, ResilienceContextPool.Shared.Get()))
        {
            Caption = "ResilienceContext",
            AssertContext = AssertResilienceContext,
            AssertContextAfter = AssertContextInitialized,
        };

        yield return new ExecuteParameters(r => r.Execute((_, s) => { s.Should().Be("dummy-state"); }, ResilienceContextPool.Shared.Get(), "dummy-state"))
        {
            Caption = "Execute_ResilienceContextAndState",
            AssertContext = AssertResilienceContext,
            AssertContextAfter = AssertContextInitialized,
        };

        static void AssertResilienceContext(ResilienceContext context)
        {
            context.IsSynchronous.Should().BeTrue();
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

    [MemberData(nameof(Execute_EnsureCorrectBehavior_Data))]
    [Theory]
    public async Task Execute_Ok(ExecuteParameters parameters)
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
    public void Execute_EnsureCallStackPreserved()
    {
        AssertStackTrace(s => s.Execute(() => MyThrowingMethod()));
        AssertStackTrace(s => s.Execute(_ => MyThrowingMethod()));
        AssertStackTrace(s => s.Execute((_) => MyThrowingMethod(), ResilienceContextPool.Shared.Get()));
        AssertStackTrace(s => s.Execute((_, _) => MyThrowingMethod(), ResilienceContextPool.Shared.Get()));
        AssertStackTrace(s => s.Execute((_, _) => MyThrowingMethod(), ResilienceContextPool.Shared.Get(), "state"));
        AssertStackTrace(s => s.Execute((_, _) => MyThrowingMethod(), "state"));
        AssertStackTrace(s => s.Execute((_) => MyThrowingMethod(), "state"));

        static void AssertStackTrace(Action<ResilienceStrategy> execute)
        {
            var strategy = new TestResilienceStrategy();

            var error = strategy
                .Invoking(s => execute(s))
                .Should()
                .Throw<FormatException>();

            error.And.StackTrace.Should().Contain(nameof(MyThrowingMethod));
        }

        static void MyThrowingMethod() => throw new FormatException();
    }
}
