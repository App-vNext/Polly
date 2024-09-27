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
        options.ActionGenerator.Should().NotBeNull();
        options.Delay.Should().Be(TimeSpan.FromSeconds(2));
        options.MaxHedgedAttempts.Should().Be(1);
        options.OnHedging.Should().BeNull();
        options.Name.Should().Be("Hedging");
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task HedgingActionGenerator_EnsureDefaults(bool synchronous)
    {
        var options = new HedgingStrategyOptions<int>();
        var context = ResilienceContextPool.Shared.Get().Initialize<int>(synchronous);
        var threadId = Thread.CurrentThread.ManagedThreadId;
        using var semaphore = new SemaphoreSlim(0);

        var action = options.ActionGenerator(new HedgingActionGeneratorArguments<int>(context, context, 1, c =>
        {
            if (synchronous)
            {
                Thread.CurrentThread.ManagedThreadId.Should().NotBe(threadId);
            }
            else
            {
                Thread.CurrentThread.ManagedThreadId.Should().Be(threadId);
            }

            semaphore.Release();
            return Outcome.FromResultAsValueTask(99);
        }))!;

        var task = action();
        semaphore
            .Wait(TimeSpan.FromSeconds(20))
            .Should()
            .BeTrue($"The test thread failed to enter the {nameof(semaphore)}, the hedging callback didn't executed");
        (await task).Result.Should().Be(99);
    }

    [Fact]
    public async Task ShouldHandle_EnsureDefaults()
    {
        var options = new HedgingStrategyOptions<int>();
        var context = ResilienceContextPool.Shared.Get();

        (await options.ShouldHandle(new HedgingPredicateArguments<int>(context, Outcome.FromResult(0)))).Should().Be(false);
        (await options.ShouldHandle(new HedgingPredicateArguments<int>(context, Outcome.FromException<int>(new OperationCanceledException())))).Should().Be(false);
        (await options.ShouldHandle(new HedgingPredicateArguments<int>(context, Outcome.FromException<int>(new InvalidOperationException())))).Should().Be(true);
    }

    [Fact]
    public void Validation()
    {
        var options = new HedgingStrategyOptions<int>
        {
            DelayGenerator = null!,
            ShouldHandle = null!,
            MaxHedgedAttempts = -1,
            OnHedging = null!,
            ActionGenerator = null!
        };

        options
            .Invoking(o => ValidationHelper.ValidateObject(new(o, "Invalid.")))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            Invalid.

            Validation Errors:
            The field MaxHedgedAttempts must be between 1 and 10.
            The ShouldHandle field is required.
            The ActionGenerator field is required.
            """);
    }
}
