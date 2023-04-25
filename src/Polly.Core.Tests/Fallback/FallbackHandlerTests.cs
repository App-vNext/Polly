using System.ComponentModel.DataAnnotations;
using Polly.Fallback;
using Polly.Strategy;

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
    public void SetFallback_Empty_Discarded()
    {
        var handler = new FallbackHandler()
            .SetFallback<int>(handler =>
            {
                handler.FallbackAction = (_, _) => new ValueTask<int>(0);
            })
            .SetVoidFallback(handler =>
            {
                handler.FallbackAction = (_, _) => default;
            });

        handler.IsEmpty.Should().BeTrue();
        handler.CreateHandler().Should().BeNull();
    }

    [Fact]
    public async Task SetFallback_Ok()
    {
        var handler = new FallbackHandler()
            .SetFallback<int>(handler =>
            {
                handler.FallbackAction = (_, _) => new ValueTask<int>(0);
                handler.ShouldHandle.HandleResult(-1);
            })
            .CreateHandler();

        var args = new HandleFallbackArguments(ResilienceContext.Get());
        handler.Should().NotBeNull();
        var action = await handler!.ShouldHandleAsync(new Outcome<int>(-1), args);
        (await action!(new Outcome<int>(-1), args)).Should().Be(0);

        action = await handler!.ShouldHandleAsync(new Outcome<int>(0), args);
        action.Should().BeNull();
    }

    [Fact]
    public async Task SetVoidFallback_Ok()
    {
        var handler = new FallbackHandler()
            .SetVoidFallback(handler =>
            {
                handler.FallbackAction = (_, _) => default;
                handler.ShouldHandle.HandleException<InvalidOperationException>();
            })
            .CreateHandler();

        var args = new HandleFallbackArguments(ResilienceContext.Get());
        handler.Should().NotBeNull();
        var action = await handler!.ShouldHandleAsync(new Outcome<VoidResult>(new InvalidOperationException()), args);
        action.Should().NotBeNull();
        (await action!(new Outcome<VoidResult>(new InvalidOperationException()), args)).Should().Be(VoidResult.Instance);

        action = await handler!.ShouldHandleAsync(new Outcome<VoidResult>(new ArgumentNullException()), args);
        action.Should().BeNull();
    }

    [Fact]
    public async Task ShouldHandleAsync_UnknownResultType_Null()
    {
        var handler = new FallbackHandler()
            .SetFallback<int>(handler =>
            {
                handler.FallbackAction = (_, _) => default;
                handler.ShouldHandle.HandleException<InvalidOperationException>();
            })
            .SetFallback<string>(handler =>
            {
                handler.FallbackAction = (_, _) => default;
            })
            .CreateHandler();

        var args = new HandleFallbackArguments(ResilienceContext.Get());
        var action = await handler!.ShouldHandleAsync(new Outcome<double>(new InvalidOperationException()), args);
        action.Should().BeNull();
    }
}
