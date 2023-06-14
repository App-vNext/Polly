using Moq;
using Polly.Retry;
using Polly.Telemetry;
using Polly.Utils;

namespace Polly.Core.Tests.Retry;

public class RetryResilienceStrategyTests
{
    private readonly RetryStrategyOptions _options = new();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly Mock<DiagnosticSource> _diagnosticSource = new();

    public RetryResilienceStrategyTests()
    {
        _telemetry = TestUtilities.CreateResilienceTelemetry(_diagnosticSource.Object);
        _options.ShouldRetry = _ => new ValueTask<bool>(false);
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
        var context = ResilienceContext.Get();
        context.CancellationToken = cancellationToken.Token;
        var executed = false;

        var result = await sut.ExecuteOutcomeAsync((_, _) => { executed = true; return "dummy".AsOutcomeAsync(); }, context, "state");
        result.Exception.Should().BeOfType<OperationCanceledException>();
        executed.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_CancellationRequestedAfterCallback_EnsureNotRetried()
    {
        using var cancellationToken = new CancellationTokenSource();

        _options.ShouldRetry = _ => PredicateResult.True;
        _options.OnRetry = _ =>
        {
            cancellationToken.Cancel();
            return default;
        };

        var sut = CreateSut(TimeProvider.System);
        var context = ResilienceContext.Get();
        context.CancellationToken = cancellationToken.Token;
        var executed = false;

        var result = await sut.ExecuteOutcomeAsync((_, _) => { executed = true; return "dummy".AsOutcomeAsync(); }, context, "state");
        result.Exception.Should().BeOfType<OperationCanceledException>();
        executed.Should().BeTrue();
    }

    [Fact]
    public void ExecuteAsync_MultipleRetries_EnsureDiscardedResultsDisposed()
    {
        // arrange
        _options.RetryCount = 5;
        SetupNoDelay();
        _timeProvider.SetupAnyDelay();
        _options.ShouldRetry = _ => PredicateResult.True;
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
        _options.ShouldRetry = args => args.Outcome.ResultPredicateAsync(0);
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

        _options.ShouldRetry = args => args.Outcome.ExceptionPredicateAsync<InvalidOperationException>();
        _options.RetryCount = 3;
        SetupNoDelay();
        var sut = CreateSut();

        Assert.Throws<InvalidOperationException>(() => sut.Execute<int>(() => throw new InvalidOperationException()));

        calls.Should().Be(3);
    }

    [Fact]
    public void Retry_Infinite_Respected()
    {
        int calls = 0;
        _options.BackoffType = RetryBackoffType.Constant;
        _options.OnRetry = args =>
        {
            if (args.Arguments.Attempt > RetryConstants.MaxRetryCount)
            {
                throw new InvalidOperationException();
            }

            calls++;
            return default;
        };
        _options.ShouldRetry = args => args.Outcome.ResultPredicateAsync(0);
        _options.RetryCount = RetryStrategyOptions.InfiniteRetryCount;
        SetupNoDelay();
        var sut = CreateSut();

        Assert.Throws<InvalidOperationException>(() => sut.Execute(() => 0));

        calls.Should().Be(RetryConstants.MaxRetryCount + 1);
    }

    [Fact]
    public void RetryDelayGenerator_Respected()
    {
        int calls = 0;
        _options.OnRetry = _ => { calls++; return default; };
        _options.ShouldRetry = args => args.Outcome.ResultPredicateAsync(0);
        _options.RetryCount = 3;
        _options.BackoffType = RetryBackoffType.Constant;
        _options.RetryDelayGenerator = _ => new ValueTask<TimeSpan>(TimeSpan.FromMilliseconds(123));
        _timeProvider.SetupDelay(TimeSpan.FromMilliseconds(123));

        var sut = CreateSut();

        sut.Execute(() => 0);

        _timeProvider.Verify(v => v.Delay(TimeSpan.FromMilliseconds(123), default), Times.Exactly(3));
    }

    [Fact]
    public void OnRetry_EnsureCorrectArguments()
    {
        var attempts = new List<int>();
        var delays = new List<TimeSpan>();
        _options.OnRetry = args =>
        {
            attempts.Add(args.Arguments.Attempt);
            delays.Add(args.Arguments.RetryDelay);

            args.Exception.Should().BeNull();
            args.Result.Should().Be(0);
            return default;
        };

        _options.ShouldRetry = args => args.Outcome.ResultPredicateAsync(0);
        _options.RetryCount = 3;
        _options.BackoffType = RetryBackoffType.Linear;
        _timeProvider.SetupAnyDelay();

        var sut = CreateSut();

        sut.Execute(() => 0);

        attempts.Should().HaveCount(3);
        attempts[0].Should().Be(0);
        attempts[1].Should().Be(1);
        attempts[2].Should().Be(2);

        delays[0].Should().Be(TimeSpan.FromSeconds(2));
        delays[1].Should().Be(TimeSpan.FromSeconds(4));
        delays[2].Should().Be(TimeSpan.FromSeconds(6));
    }

    [Fact]
    public void OnRetry_EnsureTelemetry()
    {
        var attempts = new List<int>();
        var delays = new List<TimeSpan>();

        _diagnosticSource.Setup(v => v.IsEnabled("OnRetry")).Returns(true);

        _options.ShouldRetry = args => args.Outcome.ResultPredicateAsync(0);
        _options.RetryCount = 3;
        _options.BackoffType = RetryBackoffType.Linear;
        _timeProvider.SetupAnyDelay();

        var sut = CreateSut();

        sut.Execute(() => 0);

        _diagnosticSource.VerifyAll();
    }

    [Fact]
    public void RetryDelayGenerator_EnsureCorrectArguments()
    {
        var attempts = new List<int>();
        var hints = new List<TimeSpan>();
        _options.RetryDelayGenerator = args =>
        {
            attempts.Add(args.Arguments.Attempt);
            hints.Add(args.Arguments.DelayHint);

            args.Exception.Should().BeNull();
            args.Result.Should().Be(0);

            return new ValueTask<TimeSpan>(TimeSpan.Zero);
        };

        _options.ShouldRetry = args => args.Outcome.ResultPredicateAsync(0);
        _options.RetryCount = 3;
        _options.BackoffType = RetryBackoffType.Linear;
        _timeProvider.SetupAnyDelay();

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

    private RetryResilienceStrategy CreateSut(TimeProvider? timeProvider = null)
    {
        return new RetryResilienceStrategy(
            _options.BaseDelay,
            _options.BackoffType,
            _options.RetryCount,
            PredicateInvoker<ShouldRetryArguments>.Create(_options.ShouldRetry!, false)!,
            EventInvoker<OnRetryArguments>.Create(_options.OnRetry, false),
            GeneratorInvoker<RetryDelayArguments, TimeSpan>.Create(_options.RetryDelayGenerator, TimeSpan.MinValue, false),
            timeProvider ?? _timeProvider.Object,
            _telemetry,
            RandomUtil.Instance);
    }
}
