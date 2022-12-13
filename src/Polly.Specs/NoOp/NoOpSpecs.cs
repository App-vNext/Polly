using System.Threading;
using FluentAssertions;
using Polly.NoOp;
using Xunit;

namespace Polly.Specs.NoOp;

public class NoOpSpecs
{
    [Fact]
    public void Should_execute_user_delegate()
    {
        var policy = Policy.NoOp();
        var executed = false;

        policy.Invoking(x => x.Execute(() => { executed = true; }))
            .Should().NotThrow();

        executed.Should().BeTrue();
    }

    [Fact]
    public void Should_execute_user_delegate_without_adding_extra_cancellation_behaviour()
    {
        var policy = Policy.NoOp();
        var executed = false;

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            policy.Invoking(p => p.Execute(_ => { executed = true; }, cts.Token))
                .Should().NotThrow();
        }

        executed.Should().BeTrue();
    }
}