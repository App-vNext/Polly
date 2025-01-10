namespace Polly.Core.Tests;

public partial class ResiliencePipelineTests
{
    public static IEnumerable<object[]> ExecuteT_EnsureCorrectBehavior_Data() =>
        ConvertExecuteParameters(ExecuteT_EnsureCorrectBehavior_ExecuteParameters);

    private static IEnumerable<ExecuteParameters> ExecuteT_EnsureCorrectBehavior_ExecuteParameters()
    {
        long result = 12345;

        yield return new ExecuteParameters<long>(r => r.Execute(t => result), result)
        {
            Caption = "ExecuteT_NoCancellation",
            AssertContext = AssertResilienceContext,
        };

        yield return new ExecuteParameters<long>(r => r.Execute(t => { t.Should().Be(CancellationToken); return result; }, CancellationToken), result)
        {
            Caption = "ExecuteT_Cancellation",
            AssertContext = AssertResilienceContextAndToken,
        };

        yield return new ExecuteParameters<long>(r => r.Execute(() => result), result)
        {
            Caption = "ExecuteT",
            AssertContext = AssertResilienceContext,
        };

        yield return new ExecuteParameters<long>(r => r.Execute((state) => { state.Should().Be("state"); return result; }, "state"), result)
        {
            Caption = "ExecuteT_State",
            AssertContext = AssertResilienceContext,
        };

        yield return new ExecuteParameters<long>(r => r.Execute(_ => result, ResilienceContextPool.Shared.Get(CancellationToken)), result)
        {
            Caption = "ExecuteT_ResilienceContext",
            AssertContext = AssertResilienceContext,
            AssertContextAfter = AssertContextInitialized
        };

        yield return new ExecuteParameters<long>(r => r.Execute((_, s) => { s.Should().Be("dummy-state"); return result; }, ResilienceContextPool.Shared.Get(CancellationToken), "dummy-state"), result)
        {
            Caption = "ExecuteT_ResilienceContext",
            AssertContext = AssertResilienceContext,
        };

        yield return new ExecuteParameters<long>(r => r.Execute((state, _) => { state.Should().Be("dummy-state"); return result; }, "dummy-state", CancellationToken), result)
        {
            Caption = "ExecuteT_StateAndCancellation",
            AssertContext = AssertResilienceContextAndToken,
        };

        static void AssertResilienceContext(ResilienceContext context)
        {
            context.IsSynchronous.Should().BeTrue();
            context.IsVoid.Should().BeFalse();
            context.ResultType.Should().Be<long>();
            context.ContinueOnCapturedContext.Should().BeFalse();
        }

        static void AssertResilienceContextAndToken(ResilienceContext context)
        {
            AssertResilienceContext(context);
            context.CancellationToken.Should().Be(CancellationToken);
        }

        static void AssertContextInitialized(ResilienceContext context) => context.IsInitialized.Should().BeTrue();
    }

    [Theory]
#pragma warning disable xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
    [MemberData(nameof(ExecuteT_EnsureCorrectBehavior_Data))]
#pragma warning restore xUnit1042 // The member referenced by the MemberData attribute returns untyped data rows
    public async Task ExecuteT_Ok(ExecuteParameters parameters)
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
    public void Execute_T_EnsureCallStackPreserved()
    {
        var context = ResilienceContextPool.Shared.Get(CancellationToken);

        AssertStackTrace(s => s.Execute(() => MyThrowingMethod()));
        AssertStackTrace(s => s.Execute(_ => MyThrowingMethod()));
        AssertStackTrace(s => s.Execute((_, _) => MyThrowingMethod(), context));
        AssertStackTrace(s => s.Execute((_) => MyThrowingMethod(), context));
        AssertStackTrace(s => s.Execute((_, _) => MyThrowingMethod(), context, "state"));
        AssertStackTrace(s => s.Execute((_, _) => MyThrowingMethod(), "state"));
        AssertStackTrace(s => s.Execute((_) => MyThrowingMethod(), "state"));

        static void AssertStackTrace(Func<ResiliencePipeline, string> execute)
        {
            var strategy = new TestResilienceStrategy().AsPipeline();

            var error = strategy
                .Invoking(s => execute(s))
                .Should()
                .Throw<FormatException>();

            error.And.StackTrace.Should().Contain(nameof(MyThrowingMethod));
        }

        static string MyThrowingMethod() => throw new FormatException();
    }
}
