using FluentAssertions;
using Polly.Core.Tests.Utils;
using Xunit;

namespace Polly.Core.Tests;

public class DelegatingResilienceStrategyTests
{
    [Fact]
    public void Next_Change_Ok()
    {
        var next = new TestResilienceStrategy();
        var strategy = new TestResilienceStrategy
        {
            Next = next
        };

        strategy.Next.Should().Be(next);
    }

    [Fact]
    public void Next_ChangeToNull_Throws()
    {
        var strategy = new TestResilienceStrategy();

        strategy.Invoking(s => s.Next = null!).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Next_ChangeAfterExecuted_Throws()
    {
        var strategy = new TestResilienceStrategy();

        strategy.Execute(_ => { }, default);

        strategy
            .Invoking(s => s.Next = NullResilienceStrategy.Instance)
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("The delegating resilience strategy is already frozen and changing the value of 'Next' property is not allowed.");
    }

    [Fact]
    public void Execute_HasNext_EnsureExecuteOrder()
    {
        List<int> executions = new();

        var strategy = new TestResilienceStrategy
        {
            Before = (_, _) => executions.Add(1),
            After = (_, _) => executions.Add(5),
        };

        var next = new TestResilienceStrategy
        {
            Before = (_, _) => executions.Add(2),
            After = (_, _) => executions.Add(4),
        };

        strategy.Next = next;
        strategy.Execute(_ => executions.Add(3), default);

        executions.Should().BeInAscendingOrder();
        executions.Should().HaveCount(5);
    }
}
