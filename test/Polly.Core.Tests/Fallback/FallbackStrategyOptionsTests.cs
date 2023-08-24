using System.ComponentModel.DataAnnotations;
using Polly.Fallback;
using Polly.Utils;

namespace Polly.Core.Tests.Fallback;

public class FallbackStrategyOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var options = new FallbackStrategyOptions<int>();

        options.ShouldHandle.Should().NotBeNull();
        options.OnFallback.Should().BeNull();
        options.FallbackAction.Should().BeNull();
        options.Name.Should().Be("Fallback");
    }

    [Fact]
    public async Task ShouldHandle_EnsureDefaults()
    {
        var options = new FallbackStrategyOptions<int>();
        var context = ResilienceContextPool.Shared.Get();

        (await options.ShouldHandle(new FallbackPredicateArguments<int>(context, Outcome.FromResult(0)))).Should().Be(false);
        (await options.ShouldHandle(new FallbackPredicateArguments<int>(context, Outcome.FromException<int>(new OperationCanceledException())))).Should().Be(false);
        (await options.ShouldHandle(new FallbackPredicateArguments<int>(context, Outcome.FromException<int>(new InvalidOperationException())))).Should().Be(true);
    }

    [Fact]
    public void Validation()
    {
        var options = new FallbackStrategyOptions<int>
        {
            ShouldHandle = null!
        };

        options
            .Invoking(o => ValidationHelper.ValidateObject(new(o, "Invalid.")))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            Invalid.

            Validation Errors:
            The ShouldHandle field is required.
            The FallbackAction field is required.
            """);
    }
}
