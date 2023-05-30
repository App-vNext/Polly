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
        var args = new TestArguments(ResilienceContext.Get());
        var invoker = GeneratorInvoker<TestArguments, string>.Create<object>((outcome, args) =>
        {
            args.Context.Should().NotBeNull();
            outcome.Result.Should().Be(10);

            return new ValueTask<string>("generated-value");
        },
        "default",
        false)!;

        (await invoker.HandleAsync(new Outcome<int>(10), args)).Should().Be("generated-value");
    }

    [Fact]
    public async Task HandleAsync_Generic_Ok()
    {
        var args = new TestArguments(ResilienceContext.Get());
        var invoker = GeneratorInvoker<TestArguments, string>.Create<int>((outcome, args) =>
        {
            args.Context.Should().NotBeNull();
            outcome.Result.Should().Be(10);

            return new ValueTask<string>("generated-value");
        },
        "default",
        true)!;

        (await invoker.HandleAsync(new Outcome<int>(10), args)).Should().Be("generated-value");
        (await invoker.HandleAsync(new Outcome<string>("dummy"), args)).Should().Be("default");

        invoker = GeneratorInvoker<TestArguments, string>.Create<object>((_, _) => new ValueTask<string>("dummy"), "default", true);
        (await invoker!.HandleAsync(new Outcome<string>("dummy"), args)).Should().Be("default");
        (await invoker!.HandleAsync(new Outcome<object>("dummy"), args)).Should().Be("dummy");
    }

    [Fact]
    public async Task HandleAsync_GenericObject_Ok()
    {
        var args = new TestArguments(ResilienceContext.Get());
        var invoker = GeneratorInvoker<TestArguments, string>.Create<object>((_, _) => new ValueTask<string>("dummy"), "default", true);
        (await invoker!.HandleAsync(new Outcome<string>("dummy"), args)).Should().Be("default");
        (await invoker!.HandleAsync(new Outcome<object>("dummy"), args)).Should().Be("dummy");
    }
}
