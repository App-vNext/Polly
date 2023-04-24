
using Polly.Fallback;
using Polly.Strategy;

namespace Polly.Core.Tests.Fallback;

public class FallbackResilienceStrategyTests
{
    private readonly FallbackStrategyOptions _options = new();
    private readonly List<IResilienceArguments> _args = new();
    private readonly ResilienceStrategyTelemetry _telemetry;

    public FallbackResilienceStrategyTests() => _telemetry = TestUtilities.CreateResilienceTelemetry(args => _args.Add(args));

    [Fact]
    public void Ctor_Ok()
    {
        Create().Should().NotBeNull();
    }

    [Fact]
    public void NoHandler_Skips()
    {
        Create().Execute(_ => { });

        _args.Should().BeEmpty();
    }

    [Fact]
    public void Handle_Result_Ok()
    {
        var called = false;
        _options.OnFallback.Register(() => called = true);
        _options.Handler.SetFallback<int>(handler =>
        {
            handler.ShouldHandle.HandleResult(-1);
            handler.FallbackAction = (outcome, args) =>
            {
                outcome.Result.Should().Be(-1);
                args.Context.Should().NotBeNull();
                return new ValueTask<int>(0);
            };
        });

        Create().Execute(_ => -1).Should().Be(0);

        _args.Should().ContainSingle(v => v is HandleFallbackArguments);
        called.Should().BeTrue();
    }

    [Fact]
    public void Handle_Exception_Ok()
    {
        var called = false;
        _options.OnFallback.Register(() => called = true);
        _options.Handler.SetFallback<int>(handler =>
        {
            handler.ShouldHandle.HandleException<InvalidOperationException>();
            handler.FallbackAction = (outcome, args) =>
            {
                outcome.Exception.Should().BeOfType<InvalidOperationException>();
                args.Context.Should().NotBeNull();
                return new ValueTask<int>(0);
            };
        });

        Create().Execute<int>(_ => throw new InvalidOperationException()).Should().Be(0);

        _args.Should().ContainSingle(v => v is HandleFallbackArguments);
        called.Should().BeTrue();
    }

    [Fact]
    public void Handle_UnhandledException_Ok()
    {
        var called = false;
        var fallbackActionCalled = false;

        _options.OnFallback.Register(() => called = true);
        _options.Handler.SetFallback<int>(handler =>
        {
            handler.ShouldHandle.HandleException<InvalidOperationException>();
            handler.FallbackAction = (_, _) =>
            {
                fallbackActionCalled = true;
                return new ValueTask<int>(0);
            };
        });

        Create().Invoking(s => s.Execute<int>(_ => throw new ArgumentException())).Should().Throw<ArgumentException>();

        _args.Should().BeEmpty();
        called.Should().BeFalse();
        fallbackActionCalled.Should().BeFalse();
    }

    [Fact]
    public void Handle_UnhandledResult_Ok()
    {
        var called = false;
        var fallbackActionCalled = false;

        _options.OnFallback.Register(() => called = true);
        _options.Handler.SetFallback<int>(handler =>
        {
            handler.ShouldHandle.HandleResult(-1);
            handler.FallbackAction = (_, _) =>
            {
                fallbackActionCalled = true;
                return new ValueTask<int>(0);
            };
        });

        Create().Execute(_ => 0).Should().Be(0);
        _args.Should().BeEmpty();
        called.Should().BeFalse();
        fallbackActionCalled.Should().BeFalse();
    }

    private FallbackResilienceStrategy Create() => new(_options, _telemetry);
}
