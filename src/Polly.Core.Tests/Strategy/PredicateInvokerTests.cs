using System;
using System.Threading.Tasks;
using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;

public class PredicateInvokerTests
{
    [Fact]
    public async Task HandleAsync_NonGeneric_Ok()
    {
        var args = new TestArguments(ResilienceContext.Get());
        var called = false;
        var invoker = PredicateInvoker<TestArguments>.NonGeneric((outcome, _) =>
        {
            outcome.Result.Should().Be(10);
            args.Context.Should().NotBeNull();
            called = true;
            return new ValueTask<bool>(true);
        });

        (await invoker.HandleAsync(new Outcome<int>(10), args)).Should().Be(true);
        called.Should().Be(true);
    }

    [Fact]
    public async Task HandleAsync_Generic_Ok()
    {
        var args = new TestArguments(ResilienceContext.Get());
        var called = false;
        var invoker = PredicateInvoker<TestArguments>.Generic<int>((outcome, args) =>
        {
            outcome.Result.Should().Be(10);
            args.Context.Should().NotBeNull();
            called = true;
            return new ValueTask<bool>(true);
        });

        (await invoker.HandleAsync(new Outcome<int>(10), args)).Should().Be(true);
        called.Should().Be(true);

        called = false;
        (await invoker.HandleAsync(new Outcome<string>("dummy"), args)).Should().Be(false);
        called.Should().Be(false);
    }
}
