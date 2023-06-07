using System.ComponentModel.DataAnnotations;
using Polly.Fallback;

namespace Polly.Core.Tests.Fallback;

public class FallbackHandlerTests
{
    [Fact]
    public void SetFallback_ConfigureAsInvalid_Throws()
    {
        var handler = new FallbackHandler();

        handler
            .Invoking(h => h.SetFallback<int>(handler =>
            {
                handler.FallbackAction = null!;
                handler.ShouldHandle = null!;
            }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            The fallback handler configuration is invalid.

            Validation Errors:
            The ShouldHandle field is required.
            The FallbackAction field is required.
            """);
    }

    [Fact]
    public void SetVoidFallback_ConfigureAsInvalid_Throws()
    {
        var handler = new FallbackHandler();

        handler
            .Invoking(h => h.SetVoidFallback(handler =>
            {
                handler.FallbackAction = null!;
                handler.ShouldHandle = null!;
            }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("""
            The fallback handler configuration is invalid.

            Validation Errors:
            The ShouldHandle field is required.
            The FallbackAction field is required.
            """);
    }

    [Fact]
    public void SetFallback_Empty_HandlerCreated()
    {
        var handler = new FallbackHandler();
        handler.IsEmpty.Should().BeTrue();
        handler.CreateHandler().Should().NotBeNull();
    }

    [Fact]
    public async Task SetFallback_Ok()
    {
        var handler = new FallbackHandler()
            .SetFallback<int>(handler =>
            {
                handler.FallbackAction = _ => new ValueTask<int>(0);
                handler.ShouldHandle = args => new ValueTask<bool>(args.Result == -1);
            })
            .CreateHandler();

        var args = new HandleFallbackArguments();
        handler.Should().NotBeNull();
        var action = await handler!.ShouldHandleAsync<int>(new(ResilienceContext.Get(), new Outcome<int>(-1), args));
        (await action!(new(ResilienceContext.Get(), new Outcome<int>(-1), args))).Should().Be(0);

        action = await handler!.ShouldHandleAsync<int>(new(ResilienceContext.Get(), new Outcome<int>(0), args));
        action.Should().BeNull();
    }

    [Fact]
    public async Task SetVoidFallback_Ok()
    {
        var handler = new FallbackHandler()
            .SetVoidFallback(handler =>
            {
                handler.FallbackAction = _ => default;
                handler.ShouldHandle = args => new ValueTask<bool>(args.Exception is InvalidOperationException);
            })
            .CreateHandler();

        var args = new HandleFallbackArguments();
        handler.Should().NotBeNull();
        var action = await handler!.ShouldHandleAsync<VoidResult>(new(ResilienceContext.Get(), new Outcome<VoidResult>(new InvalidOperationException()), args));
        action.Should().NotBeNull();
        (await action!(new(ResilienceContext.Get(), new Outcome<VoidResult>(new InvalidOperationException()), args))).Should().Be(VoidResult.Instance);

        action = await handler!.ShouldHandleAsync<VoidResult>(new(ResilienceContext.Get(), new Outcome<VoidResult>(new ArgumentNullException()), args));
        action.Should().BeNull();
    }

    [Fact]
    public async Task ShouldHandleAsync_UnknownResultType_Null()
    {
        var handler = new FallbackHandler()
            .SetFallback<int>(handler =>
            {
                handler.FallbackAction = _ => default;
                handler.ShouldHandle = args => new ValueTask<bool>(args.Exception is InvalidOperationException);
            })
            .SetFallback<string>(handler =>
            {
                handler.FallbackAction = _ => default;
                handler.ShouldHandle = _ => PredicateResult.True;
            })
            .CreateHandler();

        var context = ResilienceContext.Get();
        var args = new HandleFallbackArguments();
        var action = await handler!.ShouldHandleAsync<double>(new(context, new Outcome<double>(new InvalidOperationException()), args));
        action.Should().BeNull();
    }
}
