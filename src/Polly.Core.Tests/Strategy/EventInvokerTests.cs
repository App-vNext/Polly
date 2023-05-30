using System;
using System.Threading.Tasks;
using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;

public class EventInvokerTests
{
    [Fact]
    public void NullCallback_Ok()
    {
        EventInvoker<TestArguments>.Create<string>(null, isGeneric: true).Should().BeNull();
        EventInvoker<TestArguments>.Create<object>(null, isGeneric: true).Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_NonGeneric_Ok()
    {
        var args = new TestArguments(ResilienceContext.Get());
        var called = false;
        var invoker = EventInvoker<TestArguments>.Create<object>((outcome, args) =>
        {
            outcome.Result.Should().Be(10);
            args.Context.Should().NotBeNull();
            called = true;
            return default;
        },
        false)!;

        await invoker.HandleAsync(new Outcome<int>(10), args);
        called.Should().Be(true);
    }

    [Fact]
    public async Task HandleAsync_Generic_Ok()
    {
        var args = new TestArguments(ResilienceContext.Get());
        var called = false;
        var invoker = EventInvoker<TestArguments>.Create<int>((outcome, args) =>
        {
            args.Context.Should().NotBeNull();
            outcome.Result.Should().Be(10);
            called = true;
            return default;
        },
        true)!;

        await invoker.HandleAsync(new Outcome<int>(10), args);
        called.Should().Be(true);

        called = false;
        await invoker.HandleAsync(new Outcome<string>("dummy"), args);
        called.Should().Be(false);
    }

    [Fact]
    public async Task HandleAsync_GenericObject_Ok()
    {
        var called = false;
        var args = new TestArguments(ResilienceContext.Get());
        var invoker = EventInvoker<TestArguments>.Create<object>((_, _) => { called = true; return default; }, true);
        await invoker!.HandleAsync(new Outcome<string>("dummy"), args);
        called.Should().BeFalse();

        await invoker!.HandleAsync(new Outcome<object>("dummy"), args);
        called.Should().BeTrue();
    }
}
