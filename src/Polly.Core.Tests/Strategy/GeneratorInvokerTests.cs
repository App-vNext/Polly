using System.Threading.Tasks;
using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;

public class GeneratorInvokerTests
{
    [Fact]
    public void NullCallback_Ok()
    {
        GeneratorInvoker<TestArguments, string>.NonGeneric(null).Should().BeNull();
        GeneratorInvoker<TestArguments, string>.Generic<string>(null, "default").Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_NonGeneric_Ok()
    {
        var args = new TestArguments(ResilienceContext.Get());
        var invoker = GeneratorInvoker<TestArguments, string>.NonGeneric((outcome, args) =>
        {
            args.Context.Should().NotBeNull();
            outcome.Result.Should().Be(10);

            return new ValueTask<string>("generated-value");
        })!;

        (await invoker.HandleAsync(new Outcome<int>(10), args)).Should().Be("generated-value");
    }

    [Fact]
    public async Task HandleAsync_Generic_Ok()
    {
        var args = new TestArguments(ResilienceContext.Get());
        var invoker = GeneratorInvoker<TestArguments, string>.Generic<int>((outcome, args) =>
        {
            args.Context.Should().NotBeNull();
            outcome.Result.Should().Be(10);

            return new ValueTask<string>("generated-value");
        },
        "default")!;

        (await invoker.HandleAsync(new Outcome<int>(10), args)).Should().Be("generated-value");
        (await invoker.HandleAsync(new Outcome<string>("dummy"), args)).Should().Be("default");
    }
}
