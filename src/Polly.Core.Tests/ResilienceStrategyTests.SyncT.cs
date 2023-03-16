using FluentAssertions;
using Polly.Core.Tests.Utils;
using Xunit;

namespace Polly.Core.Tests;

public partial class ResilienceStrategyTests
{
    public static IEnumerable<object[]> ExecuteT_EnsureCorrectBehavior_Data()
    {
        return ConvertExecuteParameters(ExecuteT_EnsureCorrectBehavior_ExecuteParameters);
    }

    private static IEnumerable<ExecuteParameters> ExecuteT_EnsureCorrectBehavior_ExecuteParameters()
    {
        long result = 12345;

        yield return new ExecuteParameters<long>(r => r.Execute(t => result), result)
        {
            Caption = "ExecuteT_NoCancellation",
            AssertContext = AssertResilienceContext,
            AssertContextAfter = AssertContextNotInitialized,
        };

        yield return new ExecuteParameters<long>(r => r.Execute(t => { t.Should().Be(CancellationToken); return result; }, CancellationToken), result)
        {
            Caption = "ExecuteT_Cancellation",
            AssertContext = AssertResilienceContextAndToken,
            AssertContextAfter = AssertContextNotInitialized,
        };

        yield return new ExecuteParameters<long>(r => r.Execute((_, s) => { s.Should().Be("dummy-state"); return result; }, ResilienceContext.Get(), "dummy-state"), result)
        {
            Caption = "ExecuteT_ResilienceContextAndState",
            AssertContext = AssertResilienceContext,
            AssertContextAfter = AssertContextInitialized,
        };

        static void AssertResilienceContext(ResilienceContext context)
        {
            context.IsSynchronous.Should().BeTrue();
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

    [MemberData(nameof(ExecuteT_EnsureCorrectBehavior_Data))]
    [Theory]
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
        };

        var result = await parameters.Execute(strategy);

        parameters.AssertContextAfter(context!);
        parameters.AssertResult(result);
    }
}
