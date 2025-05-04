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

        options.ShouldHandle.ShouldNotBeNull();
        options.ActionGenerator.ShouldNotBeNull();
        options.Delay.ShouldBe(TimeSpan.FromSeconds(2));
        options.MaxHedgedAttempts.ShouldBe(1);
        options.OnHedging.ShouldBeNull();
        options.Name.ShouldBe("Hedging");
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
                Thread.CurrentThread.ManagedThreadId.ShouldNotBe(threadId);
            }
            else
            {
                Thread.CurrentThread.ManagedThreadId.ShouldBe(threadId);
            }

            semaphore.Release();
            return Outcome.FromResultAsValueTask(99);
        }))!;

        var task = action();
        semaphore
            .Wait(TimeSpan.FromSeconds(20))
            .ShouldBeTrue("The test thread failed to complete within the timeout");
        (await task).Result.ShouldBe(99);
    }

    [Fact]
    public async Task ShouldHandle_EnsureDefaults()
    {
        var options = new HedgingStrategyOptions<int>();
        var context = ResilienceContextPool.Shared.Get();

        (await options.ShouldHandle(new HedgingPredicateArguments<int>(context, Outcome.FromResult(0), 0))).ShouldBe(false);
        (await options.ShouldHandle(new HedgingPredicateArguments<int>(context, Outcome.FromException<int>(new OperationCanceledException()), 1))).ShouldBe(false);
        (await options.ShouldHandle(new HedgingPredicateArguments<int>(context, Outcome.FromException<int>(new InvalidOperationException()), 2))).ShouldBe(true);
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

        var exception = Should.Throw<ValidationException>(() => ValidationHelper.ValidateObject(new(options, "Invalid.")));
        exception.Message.Trim().ShouldBe("""
            Invalid.
            Validation Errors:
            The field MaxHedgedAttempts must be between 1 and 10.
            The ShouldHandle field is required.
            The ActionGenerator field is required.
            """,
            StringCompareShould.IgnoreLineEndings);
    }
}
