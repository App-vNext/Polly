using System.ComponentModel.DataAnnotations;
using Polly.Hedging;
using Polly.Strategy;

namespace Polly.Core.Tests.Hedging;

public class HedgingHandlerTests
{
    [Fact]
    public void SetHedging_ConfigureAsInvalid_Throws()
    {
        var handler = new HedgingHandler();

        handler
            .Invoking(h => h.SetHedging<int>(handler =>
            {
                handler.HedgingActionGenerator = null!;
                handler.ShouldHandle = null!;
            }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            The hedging handler configuration is invalid.

            Validation Errors:
            The ShouldHandle field is required.
            The HedgingActionGenerator field is required.
            """);
    }

    [Fact]
    public void SetVoidHedging_ConfigureAsInvalid_Throws()
    {
        var handler = new HedgingHandler();

        handler
            .Invoking(h => h.SetVoidHedging(handler =>
            {
                handler.HedgingActionGenerator = null!;
                handler.ShouldHandle = null!;
            }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            The hedging handler configuration is invalid.

            Validation Errors:
            The ShouldHandle field is required.
            The HedgingActionGenerator field is required.
            """);
    }

    [Fact]
    public void SetHedging_Empty_Discarded()
    {
        var handler = new HedgingHandler()
            .SetHedging<int>(handler =>
            {
                handler.ShouldHandle = (_, _) => PredicateResult.True;
                handler.HedgingActionGenerator = args => () => Task.FromResult(10);
            })
            .SetVoidHedging(handler =>
            {
                handler.ShouldHandle = (_, _) => PredicateResult.True;
                handler.HedgingActionGenerator = args => () => Task.CompletedTask;
            });

        handler.IsEmpty.Should().BeFalse();
        handler.CreateHandler().Should().NotBeNull();
    }

    [Fact]
    public async Task SetHedging_Ok()
    {
        var handler = new HedgingHandler()
            .SetHedging<int>(handler =>
            {
                handler.HedgingActionGenerator = args => () => Task.FromResult(0);
                handler.ShouldHandle = (outcome, _) => new ValueTask<bool>(outcome.Result == -1);
            })
            .CreateHandler();

        var args = new HandleHedgingArguments(ResilienceContext.Get());
        handler.Should().NotBeNull();
        var result = await handler!.ShouldHandleAsync(new Outcome<int>(-1), args);
        result.Should().BeTrue();

        handler.HandlesHedging<int>().Should().BeTrue();

        var action = handler.TryCreateHedgedAction<int>(ResilienceContext.Get(), 0);
        action.Should().NotBeNull();
        (await (action!()!)).Should().Be(0);

        handler.TryCreateHedgedAction<double>(ResilienceContext.Get(), 0).Should().BeNull();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task SetVoidHedging_Ok(bool returnsNullAction)
    {
        var handler = new HedgingHandler()
            .SetVoidHedging(handler =>
            {
                handler.HedgingActionGenerator = args =>
                {
                    args.Context.Should().NotBeNull();
                    if (returnsNullAction)
                    {
                        return null;
                    }

                    return () => Task.CompletedTask;
                };
                handler.ShouldHandle = (outcome, _) => new ValueTask<bool>(outcome.Exception is InvalidOperationException);
            })
            .CreateHandler();

        var args = new HandleHedgingArguments(ResilienceContext.Get());
        handler.Should().NotBeNull();
        var result = await handler!.ShouldHandleAsync(new Outcome<VoidResult>(new InvalidOperationException()), args);
        result.Should().BeTrue();

        handler.HandlesHedging<VoidResult>().Should().BeTrue();

        var action = handler.TryCreateHedgedAction<VoidResult>(ResilienceContext.Get(), 0);
        if (returnsNullAction)
        {
            action.Should().BeNull();
        }
        else
        {
            action.Should().NotBeNull();
            (await (action!()!)).Should().Be(VoidResult.Instance);
        }
    }

    [Fact]
    public async Task ShouldHandleAsync_UnknownResultType_Null()
    {
        var handler = new HedgingHandler()
            .SetHedging<int>(handler =>
            {
                handler.HedgingActionGenerator = args => () => Task.FromResult(0);
                handler.ShouldHandle = (outcome, _) => new ValueTask<bool>(outcome.Exception is InvalidOperationException);
            })
            .SetHedging<string>(handler =>
            {
                handler.HedgingActionGenerator = args =>
                {
                    args.Context.Should().NotBeNull();
                    return () => Task.FromResult("dummy");
                };

                handler.ShouldHandle = (outcome, _) => new ValueTask<bool>(outcome.Exception is InvalidOperationException);
            })
            .CreateHandler();

        var args = new HandleHedgingArguments(ResilienceContext.Get());
        (await handler!.ShouldHandleAsync(new Outcome<double>(new InvalidOperationException()), args)).Should().BeFalse();
        handler.HandlesHedging<double>().Should().BeFalse();
        handler.TryCreateHedgedAction<double>(ResilienceContext.Get(), 0).Should().BeNull();
    }
}
