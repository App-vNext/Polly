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
        var context = ResilienceContext.Get();
        var args = new TestArguments();
        var called = false;
        var invoker = EventInvoker<TestArguments>.Create<object>(args =>
        {
            args.Result.Should().Be(10);
            args.Context.Should().NotBeNull();
            called = true;
            return default;
        },
        false)!;

        await invoker.HandleAsync<int>(new(context, new Outcome<int>(10), args));
        called.Should().Be(true);
    }

    [Fact]
    public async Task HandleAsync_Generic_Ok()
    {
        var context = ResilienceContext.Get();
        var args = new TestArguments();
        var called = false;
        var invoker = EventInvoker<TestArguments>.Create<int>(args =>
        {
            args.Context.Should().NotBeNull();
            args.Result.Should().Be(10);
            called = true;
            return default;
        },
        true)!;

        await invoker.HandleAsync<int>(new(context, new Outcome<int>(10), args));
        called.Should().Be(true);

        called = false;
        await invoker.HandleAsync<string>(new(ResilienceContext.Get(), new Outcome<string>("dummy"), args));
        called.Should().Be(false);
    }

    [Fact]
    public async Task HandleAsync_GenericObject_Ok()
    {
        var context = ResilienceContext.Get();
        var called = false;
        var args = new TestArguments();
        var invoker = EventInvoker<TestArguments>.Create<object>(_ => { called = true; return default; }, true);
        await invoker!.HandleAsync<string>(new(context, new Outcome<string>("dummy"), args));
        called.Should().BeFalse();

        await invoker!.HandleAsync<object>(new(context, new Outcome<object>("dummy"), args));
        called.Should().BeTrue();
    }
}
