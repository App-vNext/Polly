using Moq;
using Polly.Retry;
using Polly.Telemetry;

namespace Polly.Core.Tests.Retry;

public class RetryResilienceStrategyTests
{
    private readonly RetryStrategyOptions _options = new();
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly ResilienceTelemetry _telemetry;
    private readonly Mock<DiagnosticSource> _diagnosticSource = new();

    public RetryResilienceStrategyTests() => _telemetry = TestUtils.CreateResilienceTelemetry(_diagnosticSource.Object);

    [Fact]
    public void ShouldRetryEmpty_Skipped()
    {
        bool called = false;
        _options.OnRetry.Add<int>(() => called = true);
        SetupNoDelay();
        var sut = CreateSut();

        sut.Execute(_ => 0, default);

        called.Should().BeFalse();
    }

    [Fact]
    public void Retry_RetryCount_Respected()
    {
        int calls = 0;
        _options.OnRetry.Add<int>(() => calls++);
        _options.ShouldRetry.Result<int>(0);
        _options.RetryCount = 12;
        SetupNoDelay();
        var sut = CreateSut();

        sut.Execute(_ => 0, default);

        calls.Should().Be(12);
    }

    [Fact]
    public void RetryException_RetryCount_Respected()
    {
        int calls = 0;
        _options.OnRetry.Add<int>((args, _) =>
        {
            args.Exception.Should().BeOfType<InvalidOperationException>();
            calls++;
        });
        _options.ShouldRetry.Exception<InvalidOperationException>();
        _options.RetryCount = 3;
        SetupNoDelay();
        var sut = CreateSut();

        Assert.Throws<InvalidOperationException>(() => sut.Execute<int>(_ => throw new InvalidOperationException(), default));

        calls.Should().Be(3);
    }

    [Fact]
    public void Retry_Infinite_Respected()
    {
        int calls = 0;
        _options.BackoffType = RetryBackoffType.Constant;
        _options.OnRetry.Add<int>((_, args) =>
        {
            if (args.Attempt > RetryConstants.MaxRetryCount)
            {
                throw new InvalidOperationException();
            }

            calls++;
        });
        _options.ShouldRetry.Result(0);
        _options.RetryCount = RetryStrategyOptions.InfiniteRetryCount;
        SetupNoDelay();
        var sut = CreateSut();

        Assert.Throws<InvalidOperationException>(() => sut.Execute(_ => 0, default));

        calls.Should().Be(RetryConstants.MaxRetryCount + 1);
    }

    [Fact]
    public void RetryDelayGenerator_Respected()
    {
        int calls = 0;
        _options.OnRetry.Add<int>(() => calls++);
        _options.ShouldRetry.Result<int>(0);
        _options.RetryCount = 3;
        _options.BackoffType = RetryBackoffType.Constant;
        _options.RetryDelayGenerator.SetGenerator<int>((_, _) => TimeSpan.FromMilliseconds(123));
        _timeProvider.SetupDelay(TimeSpan.FromMilliseconds(123));

        var sut = CreateSut();

        sut.Execute(_ => 0, default);

        _timeProvider.Verify(v => v.Delay(TimeSpan.FromMilliseconds(123), default), Times.Exactly(3));
    }

    [Fact]
    public void OnRetry_EnsureCorrectArguments()
    {
        var attempts = new List<int>();
        var delays = new List<TimeSpan>();
        _options.OnRetry.Add<int>((outcome, args) =>
        {
            attempts.Add(args.Attempt);
            delays.Add(args.RetryDelay);

            outcome.Exception.Should().BeNull();
            outcome.Result.Should().Be(0);
        });

        _options.ShouldRetry.Result<int>(0);
        _options.RetryCount = 3;
        _options.BackoffType = RetryBackoffType.Linear;
        _timeProvider.SetupAnyDelay();

        var sut = CreateSut();

        sut.Execute(_ => 0, default);

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

        _options.ShouldRetry.Result<int>(0);
        _options.RetryCount = 3;
        _options.BackoffType = RetryBackoffType.Linear;
        _timeProvider.SetupAnyDelay();

        var sut = CreateSut();

        sut.Execute(_ => 0, default);

        _diagnosticSource.VerifyAll();
    }

    [Fact]
    public void RetryDelayGenerator_EnsureCorrectArguments()
    {
        var attempts = new List<int>();
        var hints = new List<TimeSpan>();
        _options.RetryDelayGenerator.SetGenerator<int>((outcome, args) =>
        {
            attempts.Add(args.Attempt);
            hints.Add(args.DelayHint);

            outcome.Exception.Should().BeNull();
            outcome.Result.Should().Be(0);

            return TimeSpan.Zero;
        });

        _options.ShouldRetry.Result<int>(0);
        _options.RetryCount = 3;
        _options.BackoffType = RetryBackoffType.Linear;
        _timeProvider.SetupAnyDelay();

        var sut = CreateSut();

        sut.Execute(_ => 0, default);

        attempts.Should().HaveCount(3);
        attempts[0].Should().Be(0);
        attempts[1].Should().Be(1);
        attempts[2].Should().Be(2);

        hints[0].Should().Be(TimeSpan.FromSeconds(2));
        hints[1].Should().Be(TimeSpan.FromSeconds(4));
        hints[2].Should().Be(TimeSpan.FromSeconds(6));
    }

    private void SetupNoDelay() => _options.RetryDelayGenerator.SetGenerator<int>((_, _) => TimeSpan.Zero);

    private RetryResilienceStrategy CreateSut()
    {
        return new RetryResilienceStrategy(_options, _timeProvider.Object, _telemetry);
    }
}
