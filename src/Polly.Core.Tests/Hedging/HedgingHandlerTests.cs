using System.ComponentModel.DataAnnotations;
using Polly.Hedging;

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
                handler.ShouldHandle = _ => PredicateResult.True;
                handler.HedgingActionGenerator = args => () => 10.AsOutcomeAsync();
            })
            .SetVoidHedging(handler =>
            {
                handler.ShouldHandle = _ => PredicateResult.True;
                handler.HedgingActionGenerator = args => () => default;
            });

        handler.IsEmpty.Should().BeFalse();
        handler.CreateHandler().Should().NotBeNull();
    }

    [Fact]
    public async Task SetHedging_Ok()
    {
        var context = ResilienceContext.Get();
        var handler = new HedgingHandler()
            .SetHedging<int>(handler =>
            {
                handler.HedgingActionGenerator = args => () => 0.AsOutcomeAsync();
                handler.ShouldHandle = args => new ValueTask<bool>(args.Result == -1);
            })
            .CreateHandler();

        var args = new HandleHedgingArguments();
        handler.Should().NotBeNull();
        var result = await handler!.ShouldHandleAsync<int>(new(context, new Outcome<int>(-1), args));
        result.Should().BeTrue();

        handler.HandlesHedging<int>().Should().BeTrue();

        var action = handler.TryCreateHedgedAction(context, 0, context => 0.AsOutcomeAsync());
        action.Should().NotBeNull();
        (await (action!()!)).Result.Should().Be(0);

        handler.TryCreateHedgedAction(context, 0, context => 0.0.AsOutcomeAsync());
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task SetVoidHedging_Ok(bool returnsNullAction)
    {
        var context = ResilienceContext.Get();
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

                    return () => default;
                };
                handler.ShouldHandle = args => new ValueTask<bool>(args.Exception is InvalidOperationException);
            })
            .CreateHandler();

        var args = new HandleHedgingArguments();
        handler.Should().NotBeNull();
        var result = await handler!.ShouldHandleAsync<VoidResult>(new(context, new Outcome<VoidResult>(new InvalidOperationException()), args));
        result.Should().BeTrue();

        handler.HandlesHedging<VoidResult>().Should().BeTrue();

        var action = handler.TryCreateHedgedAction<VoidResult>(ResilienceContext.Get(), 0, _ => VoidResult.Instance.AsOutcomeAsync());
        if (returnsNullAction)
        {
            action.Should().BeNull();
        }
        else
        {
            action.Should().NotBeNull();
            (await (action!()!)).Result.Should().Be(VoidResult.Instance);
        }
    }

    [Fact]
    public async Task ShouldHandleAsync_UnknownResultType_Null()
    {
        var context = ResilienceContext.Get();
        var handler = new HedgingHandler()
            .SetHedging<int>(handler =>
            {
                handler.HedgingActionGenerator = args => () => new Outcome<int>(0).AsValueTask();
                handler.ShouldHandle = args => new ValueTask<bool>(args.Exception is InvalidOperationException);
            })
            .SetHedging<string>(handler =>
            {
                handler.HedgingActionGenerator = args =>
                {
                    args.Context.Should().NotBeNull();
                    return () => "dummy".AsOutcomeAsync();
                };

                handler.ShouldHandle = args => new ValueTask<bool>(args.Exception is InvalidOperationException);
            })
            .CreateHandler();

        var args = new HandleHedgingArguments();
        (await handler!.ShouldHandleAsync<double>(new(context, new Outcome<double>(new InvalidOperationException()), args))).Should().BeFalse();
        handler.HandlesHedging<double>().Should().BeFalse();
        handler.TryCreateHedgedAction<double>(ResilienceContext.Get(), 0, _ => 0.0.AsOutcomeAsync()).Should().BeNull();
    }
}
