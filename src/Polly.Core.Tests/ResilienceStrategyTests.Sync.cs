using System.Threading.Tasks;
using FluentAssertions;
using Polly.Core.Tests.Utils;
using Xunit;

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
            AssertContextAfter = AssertContextNotInitialized,
        };

        yield return new ExecuteParameters(r => r.Execute(t => { t.Should().Be(CancellationToken); }, CancellationToken))
        {
            Caption = "Execute_Cancellation",
            AssertContext = AssertResilienceContextAndToken,
            AssertContextAfter = AssertContextNotInitialized,
        };

        yield return new ExecuteParameters(r => r.Execute((_, s) => { s.Should().Be("dummy-state"); }, ResilienceContext.Get(), "dummy-state"))
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

        static void AssertContextNotInitialized(ResilienceContext context) => context.IsInitialized.Should().BeFalse();

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
}
