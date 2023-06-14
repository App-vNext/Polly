using Polly.Fallback;
using Polly.Telemetry;
using Polly.Utils;

namespace Polly.Core.Tests.Fallback;

public class FallbackResilienceStrategyTests
{
    private readonly FallbackStrategyOptions _options = new();
    private readonly List<TelemetryEventArguments> _args = new();
    private readonly ResilienceStrategyTelemetry _telemetry;
    private FallbackHandler<string>? _handler;

    public FallbackResilienceStrategyTests() => _telemetry = TestUtilities.CreateResilienceTelemetry(_args.Add);

    [Fact]
    public void Ctor_Ok()
    {
        Create().Should().NotBeNull();
    }

    [Fact]
    public void DoesntHandle_Skips()
    {
        SetHandler(_ => true, () => "dummy".AsOutcome(), true);

        Create().Execute(() => -1).Should().Be(-1);

        _args.Should().BeEmpty();
    }

    [Fact]
    public void Handle_Result_Ok()
    {
        var called = false;
        _options.OnFallback = _ => { called = true; return default; };
        SetHandler(outcome => outcome.Result == "error", () => "success".AsOutcome());

        Create().Execute(_ => "error").Should().Be("success");

        _args.Should().ContainSingle(v => v.Arguments is OnFallbackArguments);
        called.Should().BeTrue();
    }

    [Fact]
    public void Handle_Result_FallbackActionThrows()
    {
        SetHandler(_ => true, () => throw new InvalidOperationException());
        Create().Invoking(s => s.Execute(_ => "dummy")).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Handle_Exception_Ok()
    {
        var called = false;
        _options.OnFallback = _ => { called = true; return default; };

        SetHandler(outcome => outcome.Exception is InvalidOperationException, () => "secondary".AsOutcome());
        Create().Execute<string>(_ => throw new InvalidOperationException()).Should().Be("secondary");

        _args.Should().ContainSingle(v => v.Arguments is OnFallbackArguments);
        called.Should().BeTrue();
    }

    [Fact]
    public void Handle_UnhandledException_Ok()
    {
        var called = false;
        var fallbackActionCalled = false;

        _options.OnFallback = _ => { called = true; return default; };
        SetHandler(outcome => outcome.Exception is InvalidOperationException, () => { fallbackActionCalled = true; return "secondary".AsOutcome(); });

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

        _options.OnFallback = _ => { called = true; return default; };
        SetHandler(outcome => false, () => "secondary".AsOutcome());

        Create().Execute(_ => "primary").Should().Be("primary");
        _args.Should().BeEmpty();
        called.Should().BeFalse();
        fallbackActionCalled.Should().BeFalse();
    }

    private void SetHandler(
        Func<Outcome<string>, bool> shouldHandle,
        Func<Outcome<string>> fallback,
        bool isGeneric = true)
    {
        _handler = FallbackHelper.CreateHandler(shouldHandle, fallback, isGeneric);
    }

    private FallbackResilienceStrategy<string> Create() => new(
        _handler!,
        EventInvoker<OnFallbackArguments>.Create(_options.OnFallback, false),
        _telemetry);
}
