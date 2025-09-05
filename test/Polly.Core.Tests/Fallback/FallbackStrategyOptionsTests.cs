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

        options.ShouldHandle.ShouldNotBeNull();
        options.OnFallback.ShouldBeNull();
        options.FallbackAction.ShouldBeNull();
        options.Name.ShouldBe("Fallback");
    }

    [Fact]
    public async Task ShouldHandle_EnsureDefaults()
    {
        var options = new FallbackStrategyOptions<int>();
        var context = ResilienceContextPool.Shared.Get(TestCancellation.Token);

        (await options.ShouldHandle(new FallbackPredicateArguments<int>(context, Outcome.FromResult(0)))).ShouldBe(false);
        (await options.ShouldHandle(new FallbackPredicateArguments<int>(context, Outcome.FromException<int>(new OperationCanceledException())))).ShouldBe(false);
        (await options.ShouldHandle(new FallbackPredicateArguments<int>(context, Outcome.FromException<int>(new InvalidOperationException())))).ShouldBe(true);
    }

    [Fact]
    public void Validation()
    {
        var options = new FallbackStrategyOptions<int>
        {
            ShouldHandle = null!
        };

        var exception = Should.Throw<ValidationException>(() => ValidationHelper.ValidateObject(new(options, "Invalid.")));
        exception.Message.Trim().ShouldBe("""
            Invalid.
            Validation Errors:
            The ShouldHandle field is required.
            The FallbackAction field is required.
            """,
            StringCompareShould.IgnoreLineEndings);
    }
}
