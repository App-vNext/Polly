using Polly.Fallback;
using Polly.Telemetry;

namespace Polly.Core.Tests.Fallback;

public class FallbackResilienceStrategyTests
{
    private readonly FallbackStrategyOptions<string> _options = new();
    private readonly List<TelemetryEventArguments<object, object>> _args = [];
    private readonly ResilienceStrategyTelemetry _telemetry;
    private FallbackHandler<string>? _handler;

    public FallbackResilienceStrategyTests() => _telemetry = TestUtilities.CreateResilienceTelemetry(_args.Add);

    [Fact]
    public void Ctor_Ok() =>
        Create().ShouldNotBeNull();

    [Fact]
    public void Handle_Result_Ok()
    {
        var called = false;
        _options.OnFallback = _ => { called = true; return default; };
        SetHandler(outcome => outcome.Result == "error", () => Outcome.FromResult("success"));

        Create().Execute(_ => "error").ShouldBe("success");

        _args.Count(v => v.Arguments is OnFallbackArguments<string>).ShouldBe(1);
        called.ShouldBeTrue();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void ShouldHandle_ArgumentsSetCorrectly(bool handle)
    {
        var called = 0;

        _handler = new FallbackHandler<string>(
            args =>
            {
                args.Outcome.Result.ShouldBe("ok");
                args.Context.ShouldNotBeNull();
                called++;

                return new ValueTask<bool>(handle);
            },
            args =>
            {
                args.Outcome.Result.ShouldBe("ok");
                args.Context.ShouldNotBeNull();
                called++;
                return Outcome.FromResultAsValueTask("fallback");
            });

        var result = Create().Execute(_ => "ok");

        if (handle)
        {
            result.ShouldBe("fallback");
            called.ShouldBe(2);
        }
        else
        {
            result.ShouldBe("ok");
            called.ShouldBe(1);
        }
    }

    [Fact]
    public void Handle_Result_FallbackActionThrows()
    {
        SetHandler(_ => true, () => throw new InvalidOperationException());
        Should.Throw<InvalidOperationException>(() => Create().Execute(_ => "dummy"));
    }

    [Fact]
    public void Handle_Exception_Ok()
    {
        var called = false;
        _options.OnFallback = _ => { called = true; return default; };

        SetHandler(outcome => outcome.Exception is InvalidOperationException, () => Outcome.FromResult("secondary"));
        Create().Execute<string>(_ => throw new InvalidOperationException()).ShouldBe("secondary");

        _args.Count(v => v.Arguments is OnFallbackArguments<string>).ShouldBe(1);
        called.ShouldBeTrue();
    }

    [Fact]
    public void Handle_UnhandledException_Ok()
    {
        var called = false;
        var fallbackActionCalled = false;

        _options.OnFallback = _ => { called = true; return default; };
        SetHandler(outcome => outcome.Exception is InvalidOperationException, () => { fallbackActionCalled = true; return Outcome.FromResult("secondary"); });

        Should.Throw<ArgumentException>(() => Create().Execute<string>(_ => throw new ArgumentException()));

        _args.ShouldBeEmpty();
        called.ShouldBeFalse();
        fallbackActionCalled.ShouldBeFalse();
    }

    [Fact]
    public void Handle_UnhandledResult_Ok()
    {
        var called = false;
        var fallbackActionCalled = false;

        _options.OnFallback = _ => { called = true; return default; };
        SetHandler(outcome => false, () => Outcome.FromResult("secondary"));

        Create().Execute(_ => "primary").ShouldBe("primary");
        _args.ShouldBeEmpty();
        called.ShouldBeFalse();
        fallbackActionCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteOutcomeAsync_UnhandledException_Ok()
    {
        var called = false;
        var fallbackActionCalled = false;

        _options.OnFallback = _ => { called = true; return default; };
        SetHandler(outcome => outcome.Exception is InvalidOperationException, () => { fallbackActionCalled = true; return Outcome.FromResult("secondary"); });

        var outcome = await Create().ExecuteOutcomeAsync<string, string>((_, _) => throw new ArgumentException(), new(), "dummy-state");
        outcome.Exception.ShouldBeOfType<ArgumentException>();

        _args.ShouldBeEmpty();
        called.ShouldBeFalse();
        fallbackActionCalled.ShouldBeFalse();
    }

    private void SetHandler(
        Func<Outcome<string>, bool> shouldHandle,
        Func<Outcome<string>> fallback) =>
        _handler = FallbackHelper.CreateHandler(shouldHandle, fallback);

    private ResiliencePipeline<string> Create()
        => new FallbackResilienceStrategy<string>(_handler!, _options.OnFallback, _telemetry).AsPipeline();
}
