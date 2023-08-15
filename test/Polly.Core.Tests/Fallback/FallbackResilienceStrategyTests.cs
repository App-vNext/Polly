using Polly.Fallback;
using Polly.Telemetry;

namespace Polly.Core.Tests.Fallback;

public class FallbackResilienceStrategyTests
{
    private readonly FallbackStrategyOptions<string> _options = new();
    private readonly List<TelemetryEventArguments<object, object>> _args = new();
    private readonly ResilienceStrategyTelemetry _telemetry;
    private FallbackHandler<string>? _handler;

    public FallbackResilienceStrategyTests() => _telemetry = TestUtilities.CreateResilienceTelemetry(_args.Add);

    [Fact]
    public void Ctor_Ok()
    {
        Create().Should().NotBeNull();
    }

    [Fact]
    public void Handle_Result_Ok()
    {
        var called = false;
        _options.OnFallback = _ => { called = true; return default; };
        SetHandler(outcome => outcome.Result == "error", () => Outcome.FromResult("success"));

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

        SetHandler(outcome => outcome.Exception is InvalidOperationException, () => Outcome.FromResult("secondary"));
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
        SetHandler(outcome => outcome.Exception is InvalidOperationException, () => { fallbackActionCalled = true; return Outcome.FromResult("secondary"); });

        Create().Invoking(s => s.Execute<string>(_ => throw new ArgumentException())).Should().Throw<ArgumentException>();

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
        SetHandler(outcome => false, () => Outcome.FromResult("secondary"));

        Create().Execute(_ => "primary").Should().Be("primary");
        _args.Should().BeEmpty();
        called.Should().BeFalse();
        fallbackActionCalled.Should().BeFalse();
    }

    private void SetHandler(
        Func<Outcome<string>, bool> shouldHandle,
        Func<Outcome<string>> fallback)
    {
        _handler = FallbackHelper.CreateHandler(shouldHandle, fallback);
    }

    private ResiliencePipeline<string> Create()
        => new FallbackResilienceStrategy<string>(_handler!, _options.OnFallback, _telemetry).AsPipeline();
}
