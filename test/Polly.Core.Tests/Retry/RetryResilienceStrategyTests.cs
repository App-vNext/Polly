using FluentAssertions.Execution;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Polly.Retry;
using Polly.Telemetry;
using Polly.Utils;

namespace Polly.Core.Tests.Retry;

public class RetryResilienceStrategyTests
{
    private readonly RetryStrategyOptions _options = new();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly Mock<DiagnosticSource> _diagnosticSource = new();
    private ResilienceStrategyTelemetry _telemetry;

    public RetryResilienceStrategyTests()
    {
        _telemetry = TestUtilities.CreateResilienceTelemetry(_diagnosticSource.Object);
        _options.ShouldHandle = _ => new ValueTask<bool>(false);

    }

    [Fact]
    public void ExecuteAsync_EnsureResultNotDisposed()
    {
        SetupNoDelay();
        var sut = CreateSut();

        var result = sut.Execute(() => new DisposableResult());
        result.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_CancellationRequested_EnsureNotRetried()
    {
        SetupNoDelay();
        var sut = CreateSut();
        using var cancellationToken = new CancellationTokenSource();
        cancellationToken.Cancel();
        var context = ResilienceContextPool.Shared.Get();
        context.CancellationToken = cancellationToken.Token;
        var executed = false;

        var result = await sut.ExecuteOutcomeAsync((_, _) => { executed = true; return Outcome.FromResultAsTask("dummy"); }, context, "state");
        result.Exception.Should().BeOfType<OperationCanceledException>();
        executed.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_CancellationRequestedAfterCallback_EnsureNotRetried()
    {
        using var cancellationToken = new CancellationTokenSource();

        _options.ShouldHandle = _ => PredicateResult.True;
        _options.OnRetry = _ =>
        {
            cancellationToken.Cancel();
            return default;
        };

        var sut = CreateSut(TimeProvider.System);
        var context = ResilienceContextPool.Shared.Get();
        context.CancellationToken = cancellationToken.Token;
        var executed = false;

        var result = await sut.ExecuteOutcomeAsync((_, _) => { executed = true; return Outcome.FromResultAsTask("dummy"); }, context, "state");
        result.Exception.Should().BeOfType<OperationCanceledException>();
        executed.Should().BeTrue();
    }

    [Fact]
    public void ExecuteAsync_MultipleRetries_EnsureDiscardedResultsDisposed()
    {
        // arrange
        _options.RetryCount = 5;
        SetupNoDelay();
        _options.ShouldHandle = _ => PredicateResult.True;
        var results = new List<DisposableResult>();
        var sut = CreateSut();

        // act
        var result = sut.Execute(_ =>
        {
            var r = new DisposableResult();
            results.Add(r);
            return r;
        });

        // assert
        result.IsDisposed.Should().BeFalse();
        results.Count.Should().Be(_options.RetryCount + 1);
        results.Last().IsDisposed.Should().BeFalse();

        results.Remove(results.Last());
        results.Should().AllSatisfy(r => r.IsDisposed.Should().BeTrue());
    }

    [Fact]
    public void Retry_RetryCount_Respected()
    {
        int calls = 0;
        _options.OnRetry = _ => { calls++; return default; };
        _options.ShouldHandle = args => args.Outcome.ResultPredicateAsync(0);
        _options.RetryCount = 12;
        SetupNoDelay();
        var sut = CreateSut();

        sut.Execute(() => 0);

        calls.Should().Be(12);
    }

    [Fact]
    public void RetryException_RetryCount_Respected()
    {
        int calls = 0;
        _options.OnRetry = args =>
        {
            args.Exception.Should().BeOfType<InvalidOperationException>();
            calls++;
            return default;
        };

        _options.ShouldHandle = args => args.Outcome.ExceptionPredicateAsync<InvalidOperationException>();
        _options.RetryCount = 3;
        SetupNoDelay();
        var sut = CreateSut();

        Assert.Throws<InvalidOperationException>(() => sut.Execute<int>(() => throw new InvalidOperationException()));

        calls.Should().Be(3);
    }

    [Fact]
    public void RetryDelayGenerator_Respected()
    {
        var retries = 0;
        var generatedValues = 0;
        var delay = TimeSpan.FromMilliseconds(120);

        _options.ShouldHandle = _ => PredicateResult.True;
        _options.RetryCount = 3;
        _options.BackoffType = RetryBackoffType.Constant;

        _options.OnRetry = args =>
        {
            retries++;
            args.Arguments.RetryDelay.Should().Be(delay);
            return default;
        };
        _options.RetryDelayGenerator = _ =>
        {
            generatedValues++;
            return new ValueTask<TimeSpan>(delay);
        };

        CreateSut(TimeProvider.System).Execute(_ => { });

        retries.Should().Be(3);
        generatedValues.Should().Be(3);
    }

    [Fact]
    public async Task RetryDelayGenerator_ZeroDelay_NoTimeProviderCalls()
    {
        int retries = 0;
        int generatedValues = 0;

        var delay = TimeSpan.Zero;
        var provider = new ThrowingFakeTimeProvider();

        _options.ShouldHandle = _ => PredicateResult.True;
        _options.RetryCount = 3;
        _options.BackoffType = RetryBackoffType.Constant;

        _options.OnRetry = _ =>
        {
            retries++;
            return default;
        };
        _options.RetryDelayGenerator = _ =>
        {
            generatedValues++;
            return new ValueTask<TimeSpan>(delay);
        };

        var sut = CreateSut(provider);
        await sut.ExecuteAsync(_ => default);

        retries.Should().Be(3);
        generatedValues.Should().Be(3);
    }

    private sealed class ThrowingFakeTimeProvider : FakeTimeProvider
    {
        public override DateTimeOffset GetUtcNow() => throw new AssertionFailedException("TimeProvider should not be used.");

        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
            => throw new AssertionFailedException("TimeProvider should not be used.");
    }

    [Fact]
    public async void OnRetry_EnsureCorrectArguments()
    {
        var attempts = new List<int>();
        var delays = new List<TimeSpan>();
        _options.OnRetry = args =>
        {
            attempts.Add(args.Arguments.AttemptNumber);
            delays.Add(args.Arguments.RetryDelay);

            args.Exception.Should().BeNull();
            args.Result.Should().Be(0);
            return default;
        };

        _options.ShouldHandle = args => PredicateResult.True;
        _options.RetryCount = 3;
        _options.BackoffType = RetryBackoffType.Linear;

        var sut = CreateSut();

        var executing = ExecuteAndAdvance(sut);

        await executing;

        attempts.Should().HaveCount(3);
        attempts[0].Should().Be(0);
        attempts[1].Should().Be(1);
        attempts[2].Should().Be(2);

        delays[0].Should().Be(TimeSpan.FromSeconds(2));
        delays[1].Should().Be(TimeSpan.FromSeconds(4));
        delays[2].Should().Be(TimeSpan.FromSeconds(6));
    }

    [Fact]
    public async Task OnRetry_EnsureExecutionTime()
    {
        _options.OnRetry = args =>
        {
            args.Arguments.ExecutionTime.Should().Be(TimeSpan.FromMinutes(1));

            return default;
        };

        _options.ShouldHandle = _ => PredicateResult.True;
        _options.RetryCount = 1;
        _options.BackoffType = RetryBackoffType.Constant;
        _options.BaseDelay = TimeSpan.Zero;

        var sut = CreateSut();

        await sut.ExecuteAsync(_ =>
        {
            _timeProvider.Advance(TimeSpan.FromMinutes(1));
            return new ValueTask<int>(0);
        }).AsTask();
    }

    [Fact]
    public void Execute_EnsureAttemptReported()
    {
        var called = false;
        _telemetry = TestUtilities.CreateResilienceTelemetry(args =>
        {
            var attempt = args.Arguments.Should().BeOfType<ExecutionAttemptArguments>().Subject;

            attempt.Handled.Should().BeFalse();
            attempt.AttemptNumber.Should().Be(0);
            attempt.ExecutionTime.Should().Be(TimeSpan.FromSeconds(1));
            called = true;
        });

        var sut = CreateSut();

        sut.Execute(() =>
        {
            _timeProvider.Advance(TimeSpan.FromSeconds(1));
            return 0;
        });

        called.Should().BeTrue();
    }

    [Fact]
    public async Task OnRetry_EnsureTelemetry()
    {
        var attempts = new List<int>();
        var delays = new List<TimeSpan>();

        _diagnosticSource.Setup(v => v.IsEnabled("OnRetry")).Returns(true);

        _options.ShouldHandle = args => args.Outcome.ResultPredicateAsync(0);
        _options.RetryCount = 3;
        _options.BackoffType = RetryBackoffType.Linear;

        var sut = CreateSut();

        await ExecuteAndAdvance(sut);

        _diagnosticSource.VerifyAll();
    }

    [Fact]
    public void RetryDelayGenerator_EnsureCorrectArguments()
    {
        var attempts = new List<int>();
        var hints = new List<TimeSpan>();
        _options.RetryDelayGenerator = args =>
        {
            attempts.Add(args.Arguments.AttemptNumber);
            hints.Add(args.Arguments.DelayHint);

            args.Exception.Should().BeNull();
            args.Result.Should().Be(0);

            return new ValueTask<TimeSpan>(TimeSpan.Zero);
        };

        _options.ShouldHandle = args => args.Outcome.ResultPredicateAsync(0);
        _options.RetryCount = 3;
        _options.BackoffType = RetryBackoffType.Linear;

        var sut = CreateSut();

        sut.Execute(() => 0);

        attempts.Should().HaveCount(3);
        attempts[0].Should().Be(0);
        attempts[1].Should().Be(1);
        attempts[2].Should().Be(2);

        hints[0].Should().Be(TimeSpan.FromSeconds(2));
        hints[1].Should().Be(TimeSpan.FromSeconds(4));
        hints[2].Should().Be(TimeSpan.FromSeconds(6));
    }

    private void SetupNoDelay() => _options.RetryDelayGenerator = _ => new ValueTask<TimeSpan>(TimeSpan.Zero);

    private async ValueTask<int> ExecuteAndAdvance(ReactiveResilienceStrategyBridge<object> sut)
    {
        var executing = sut.ExecuteAsync(_ => new ValueTask<int>(0)).AsTask();

        while (!executing.IsCompleted)
        {
            _timeProvider.Advance(TimeSpan.FromMinutes(1));
        }

        return await executing;
    }

    private ReactiveResilienceStrategyBridge<object> CreateSut(TimeProvider? timeProvider = null) =>
        new(new RetryResilienceStrategy<object>(_options,
            timeProvider ?? _timeProvider,
            _telemetry,
            () => 1.0));
}
