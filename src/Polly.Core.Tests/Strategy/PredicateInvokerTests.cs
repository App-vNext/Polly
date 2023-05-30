using System;
using System.Threading.Tasks;
using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;

public class PredicateInvokerTests
{
    [Fact]
    public void NullCallback_Ok()
    {
        PredicateInvoker<TestArguments>.Create<string>(null, isGeneric: true).Should().BeNull();
        PredicateInvoker<TestArguments>.Create<object>(null, isGeneric: true).Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_NonGeneric_Ok()
    {
        var args = new TestArguments(ResilienceContext.Get());
        var called = false;
        var invoker = PredicateInvoker<TestArguments>.Create<object>((outcome, _) =>
        {
            outcome.Result.Should().Be(10);
            args.Context.Should().NotBeNull();
            called = true;
            return new ValueTask<bool>(true);
        },
        false);

        (await invoker!.HandleAsync(new Outcome<int>(10), args)).Should().Be(true);
        called.Should().Be(true);
    }

    [Fact]
    public async Task HandleAsync_Generic_Ok()
    {
        var args = new TestArguments(ResilienceContext.Get());
        var called = false;
        var invoker = PredicateInvoker<TestArguments>.Create<int>((outcome, args) =>
        {
            outcome.Result.Should().Be(10);
            args.Context.Should().NotBeNull();
            called = true;
            return new ValueTask<bool>(true);
        },
        true);

        (await invoker!.HandleAsync(new Outcome<int>(10), args)).Should().Be(true);
        called.Should().Be(true);

        called = false;
        (await invoker.HandleAsync(new Outcome<string>("dummy"), args)).Should().Be(false);
        called.Should().Be(false);
    }
}
