using System.ComponentModel.DataAnnotations;
using Polly.Hedging;
using Polly.Utils;

namespace Polly.Core.Tests.Hedging;

public class HedgingStrategyOptionsTests
{
    [Fact]
    public void Ctor_EnsureDefaults()
    {
        var options = new HedgingStrategyOptions<int>();

        options.ShouldHandle.Should().NotBeNull();
        options.HedgingActionGenerator.Should().NotBeNull();
        options.HedgingDelay.Should().Be(TimeSpan.FromSeconds(2));
        options.MaxHedgedAttempts.Should().Be(2);
        options.OnHedging.Should().BeNull();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task HedgingActionGenerator_EnsureDefaults(bool synchronous)
    {
        var options = new HedgingStrategyOptions<int>();
        var context = ResilienceContextPool.Shared.Get().Initialize<int>(synchronous);
        var threadId = Thread.CurrentThread.ManagedThreadId;

        var action = options.HedgingActionGenerator(new HedgingActionGeneratorArguments<int>(context, context, 1, c =>
        {
            if (synchronous)
            {
                Thread.CurrentThread.ManagedThreadId.Should().NotBe(threadId);
            }
            else
            {
                Thread.CurrentThread.ManagedThreadId.Should().Be(threadId);
            }

            return Outcome.FromResultAsTask(99);
        }))!;

        action.Should().NotBeNull();
        (await action()).Result.Should().Be(99);
    }

    [Fact]
    public async Task ShouldHandle_EnsureDefaults()
    {
        var options = new HedgingStrategyOptions<int>();
        var args = new HedgingPredicateArguments();
        var context = ResilienceContextPool.Shared.Get();

        (await options.ShouldHandle(new(context, Outcome.FromResult(0), args))).Should().Be(false);
        (await options.ShouldHandle(new(context, Outcome.FromException<int>(new OperationCanceledException()), args))).Should().Be(false);
        (await options.ShouldHandle(new(context, Outcome.FromException<int>(new InvalidOperationException()), args))).Should().Be(true);
    }

    [Fact]
    public void Validation()
    {
        var options = new HedgingStrategyOptions<int>
        {
            HedgingDelayGenerator = null!,
            ShouldHandle = null!,
            MaxHedgedAttempts = -1,
            OnHedging = null!,
            HedgingActionGenerator = null!
        };

        options
            .Invoking(o => ValidationHelper.ValidateObject(new(o, "Invalid.")))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            Invalid.

            Validation Errors:
            The field MaxHedgedAttempts must be between 2 and 10.
            The ShouldHandle field is required.
            The HedgingActionGenerator field is required.
            """);
    }
}
