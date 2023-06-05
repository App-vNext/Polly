using System.Threading.Tasks;
using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;

public class GeneratorInvokerTests
{
    [Fact]
    public void NullCallback_Ok()
    {
        GeneratorInvoker<TestArguments, string>.Create<string>(null, "default", false).Should().BeNull();
        GeneratorInvoker<TestArguments, string>.Create<string>(null, "default", true).Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_NonGeneric_Ok()
    {
        var context = ResilienceContext.Get();
        var args = new TestArguments();
        var invoker = GeneratorInvoker<TestArguments, string>.Create<object>(args =>
        {
            args.Context.Should().NotBeNull();
            args.Result.Should().Be(10);

            return new ValueTask<string>("generated-value");
        },
        "default",
        false)!;

        var outcomeArgs = new OutcomeArguments<int, TestArguments>(context, new Outcome<int>(10), args);
        (await invoker.HandleAsync(outcomeArgs)).Should().Be("generated-value");
    }

    [Fact]
    public async Task HandleAsync_Generic_Ok()
    {
        var context = ResilienceContext.Get();
        var args = new TestArguments();
        var invoker = GeneratorInvoker<TestArguments, string>.Create<int>(args =>
        {
            args.Context.Should().NotBeNull();
            args.Result.Should().Be(10);

            return new ValueTask<string>("generated-value");
        },
        "default",
        true)!;

        (await invoker.HandleAsync<int>(new(context, new Outcome<int>(10), args))).Should().Be("generated-value");
        (await invoker.HandleAsync<string>(new(context, new Outcome<string>("dummy"), args))).Should().Be("default");

        invoker = GeneratorInvoker<TestArguments, string>.Create<object>(_ => new ValueTask<string>("dummy"), "default", true);
        (await invoker!.HandleAsync<string>(new(context, new Outcome<string>("dummy"), args))).Should().Be("default");
        (await invoker!.HandleAsync<object>(new(context, new Outcome<object>("dummy"), args))).Should().Be("dummy");
    }

    [Fact]
    public async Task HandleAsync_GenericObject_Ok()
    {
        var context = ResilienceContext.Get();

        var args = new TestArguments();
        var invoker = GeneratorInvoker<TestArguments, string>.Create<object>(_ => new ValueTask<string>("dummy"), "default", true);
        (await invoker!.HandleAsync<string>(new(context, new Outcome<string>("dummy"), args))).Should().Be("default");
        (await invoker!.HandleAsync<object>(new(context, new Outcome<object>("dummy"), args))).Should().Be("dummy");
    }
}
