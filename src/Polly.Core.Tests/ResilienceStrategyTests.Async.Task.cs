using FluentAssertions;
using Polly.Core.Tests.Utils;
using Xunit;

namespace Polly.Core.Tests;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

public partial class ResilienceStrategyTests
{
    public static IEnumerable<object[]> ExecuteAsTaskAsync_EnsureCorrectBehavior_Data()
    {
        return ConvertExecuteParameters(ExecuteAsTaskAsync_EnsureCorrectBehavior_ExecuteParameters);
    }

    private static IEnumerable<ExecuteParameters> ExecuteAsTaskAsync_EnsureCorrectBehavior_ExecuteParameters()
    {
        yield return new ExecuteParameters(r => r.ExecuteAsTaskAsync(async _ => { }))
        {
            Caption = "ExecuteAsTaskAsync_NoCancellation",
            AssertContext = AssertResilienceContext,
            AssertContextAfter = AssertContextNotInitialized,
        };

        yield return new ExecuteParameters(r => r.ExecuteAsTaskAsync(async t => { t.Should().Be(CancellationToken); }, CancellationToken))
        {
            Caption = "ExecuteAsTaskAsync_Cancellation",
            AssertContext = AssertResilienceContextAndToken,
            AssertContextAfter = AssertContextNotInitialized,
        };

        yield return new ExecuteParameters(r => r.ExecuteAsTaskAsync(async (_, s) => { s.Should().Be("dummy-state"); }, ResilienceContext.Get(), "dummy-state"))
        {
            Caption = "ExecuteAsTaskAsync_ResilienceContextAndState",
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

        static void AssertContextNotInitialized(ResilienceContext context) => context.IsInitialized.Should().BeFalse();

        static void AssertContextInitialized(ResilienceContext context) => context.IsInitialized.Should().BeTrue();
    }

    [MemberData(nameof(ExecuteAsTaskAsync_EnsureCorrectBehavior_Data))]
    [Theory]
    public async Task ExecuteAsTaskAsync_Ok(ExecuteParameters parameters)
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
