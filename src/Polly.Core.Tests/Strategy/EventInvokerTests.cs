using System;
using System.Threading.Tasks;
using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;

public class EventInvokerTests
{
    [Fact]
    public void NullCallback_Ok()
    {
        EventInvoker<TestArguments>.NonGeneric(null).Should().BeNull();
        EventInvoker<TestArguments>.Generic<string>(null).Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_NonGeneric_Ok()
    {
        var args = new TestArguments(ResilienceContext.Get());
        var called = false;
        var invoker = EventInvoker<TestArguments>.NonGeneric((outcome, args) =>
        {
            outcome.Result.Should().Be(10);
            args.Context.Should().NotBeNull();
            called = true;
            return default;
        })!;

        await invoker.HandleAsync(new Outcome<int>(10), args);
        called.Should().Be(true);
    }

    [Fact]
    public async Task HandleAsync_Generic_Ok()
    {
        var args = new TestArguments(ResilienceContext.Get());
        var called = false;
        var invoker = EventInvoker<TestArguments>.Generic<int>((outcome, args) =>
        {
            args.Context.Should().NotBeNull();
            outcome.Result.Should().Be(10);
            called = true;
            return default;
        })!;

        await invoker.HandleAsync(new Outcome<int>(10), args);
        called.Should().Be(true);

        called = false;
        await invoker.HandleAsync(new Outcome<string>("dummy"), args);
        called.Should().Be(false);
    }
}
