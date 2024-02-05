namespace Polly.Specs.NoOp;

public class NoOpTResultSpecs
{
    [Fact]
    public void Should_execute_user_delegate()
    {
        NoOpPolicy<int> policy = Policy.NoOp<int>();
        int? result = null;

        policy.Invoking(x => result = x.Execute(() => 10))
            .Should().NotThrow();

        result.HasValue.Should().BeTrue();
        result.Should().Be(10);
    }

    [Fact]
    public void Should_execute_user_delegate_without_adding_extra_cancellation_behaviour()
    {
        NoOpPolicy<int> policy = Policy.NoOp<int>();
        int? result = null;

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            policy.Invoking(p => result = p.Execute(_ => 10, cts.Token))
               .Should().NotThrow();
        }

        result.HasValue.Should().BeTrue();
        result.Should().Be(10);
    }
}
