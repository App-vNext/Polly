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
        var context = ResilienceContext.Get();
        var args = new TestArguments();
        var called = false;
        var invoker = PredicateInvoker<TestArguments>.Create<object>(args =>
        {
            args.Result.Should().Be(10);
            args.Context.Should().NotBeNull();
            called = true;
            return new ValueTask<bool>(true);
        },
        false);

        (await invoker!.HandleAsync<int>(new(context, new Outcome<int>(10), args))).Should().Be(true);
        called.Should().Be(true);
    }

    [Fact]
    public async Task HandleAsync_Generic_Ok()
    {
        var context = ResilienceContext.Get();
        var args = new TestArguments();
        var called = false;
        var invoker = PredicateInvoker<TestArguments>.Create<int>(args =>
        {
            args.Result.Should().Be(10);
            args.Context.Should().NotBeNull();
            called = true;
            return new ValueTask<bool>(true);
        },
        true);

        (await invoker!.HandleAsync<int>(new(context, new Outcome<int>(10), args))).Should().Be(true);
        called.Should().Be(true);

        called = false;
        (await invoker.HandleAsync<string>(new(context, new Outcome<string>("dummy"), args))).Should().Be(false);
        called.Should().Be(false);
    }

    [Fact]
    public async Task HandleAsync_GenericObject_Ok()
    {
        var context = ResilienceContext.Get();
        var args = new TestArguments();
        var invoker = PredicateInvoker<TestArguments>.Create<object>(_ => PredicateResult.True, true);
        (await invoker!.HandleAsync<string>(new(context, new Outcome<string>("dummy"), args))).Should().BeFalse();
        (await invoker!.HandleAsync<object>(new(context, new Outcome<object>("dummy"), args))).Should().BeTrue();
    }
}
