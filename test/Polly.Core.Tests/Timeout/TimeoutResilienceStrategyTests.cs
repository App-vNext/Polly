using Microsoft.Extensions.Time.Testing;
using Moq;
using Polly.Telemetry;
using Polly.Timeout;

namespace Polly.Core.Tests.Timeout;

public class TimeoutResilienceStrategyTests : IDisposable
{
    private readonly ResilienceStrategyTelemetry _telemetry;
    private readonly FakeTimeProvider _timeProvider = new();
    private readonly TimeoutStrategyOptions _options;
    private readonly CancellationTokenSource _cancellationSource;
    private readonly TimeSpan _delay = TimeSpan.FromSeconds(12);

    private readonly Mock<DiagnosticSource> _diagnosticSource = new();

    public TimeoutResilienceStrategyTests()
    {
        _telemetry = TestUtilities.CreateResilienceTelemetry(_diagnosticSource.Object);
        _options = new TimeoutStrategyOptions();
        _cancellationSource = new CancellationTokenSource();
    }

    public static TheoryData<TimeSpan> Execute_NoTimeout_Data() => new()
    {
        TimeSpan.Zero,
        TimeSpan.FromMilliseconds(-1),
        System.Threading.Timeout.InfiniteTimeSpan,
    };

    public void Dispose() => _cancellationSource.Dispose();

    [Fact]
    public void Execute_EnsureTimeoutGeneratorCalled()
    {
        var called = false;
        _options.TimeoutGenerator = args =>
        {
            args.Context.Should().NotBeNull();
            called = true;
            return new ValueTask<TimeSpan>(TimeSpan.Zero);
        };

        var sut = CreateSut();

        sut.Execute(_ => { });

        called.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_EnsureOnTimeoutCalled()
    {
        _diagnosticSource.Setup(v => v.IsEnabled("OnTimeout")).Returns(true);

        var called = false;
        SetTimeout(_delay);

        var executionTime = _delay + TimeSpan.FromSeconds(1);
        _options.OnTimeout = args =>
        {
            args.Exception.Should().BeAssignableTo<OperationCanceledException>();
            args.Timeout.Should().Be(_delay);
            args.Context.Should().NotBeNull();
            args.Context.CancellationToken.IsCancellationRequested.Should().BeFalse();
            called = true;
            return default;
        };

        var sut = CreateSut();
        await sut.Invoking(s => sut.ExecuteAsync(async token =>
        {
            var delay = _timeProvider.Delay(executionTime, token);
            _timeProvider.Advance(_delay);
            await delay;
        })
        .AsTask()).Should().ThrowAsync<TimeoutRejectedException>();

        called.Should().BeTrue();
        _diagnosticSource.VerifyAll();
    }

    [MemberData(nameof(Execute_NoTimeout_Data))]
    [Theory]
    public void Execute_NoTimeout(TimeSpan timeout)
    {
        var called = false;
        SetTimeout(timeout);
        var sut = CreateSut();
        sut.Execute(_ => { });

        called.Should().BeFalse();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task Execute_Timeout(bool defaultCancellationToken)
    {
        using var cts = new CancellationTokenSource();
        CancellationToken token = defaultCancellationToken ? default : cts.Token;
        SetTimeout(TimeSpan.FromSeconds(2));
        var sut = CreateSut();

        await sut
            .Invoking(s => s.ExecuteAsync(async token =>
            {
                var delay = _timeProvider.Delay(TimeSpan.FromSeconds(4), token);
                _timeProvider.Advance(TimeSpan.FromSeconds(2));
                await delay;
            },
            token)
            .AsTask())
            .Should().ThrowAsync<TimeoutRejectedException>()
            .WithMessage("The operation didn't complete within the allowed timeout of '00:00:02'.");
    }

    [Fact]
    public async Task Execute_Timeout_EnsureStackTrace()
    {
        SetTimeout(TimeSpan.FromSeconds(2));
        var sut = CreateSut();

        var outcome = await sut.ExecuteOutcomeAsync(async (c, _) =>
        {
            var delay = _timeProvider.Delay(TimeSpan.FromSeconds(4), c.CancellationToken);
            _timeProvider.Advance(TimeSpan.FromSeconds(2));
            await delay;

            return Outcome.FromResult("dummy");
        },
        ResilienceContext.Get(),
        "state");
        outcome.Exception.Should().BeOfType<TimeoutRejectedException>();
        outcome.Exception!.StackTrace.Should().Contain("Execute_Timeout_EnsureStackTrace");
    }

    [Fact]
    public async Task Execute_Cancelled_EnsureNoTimeout()
    {
        var delay = TimeSpan.FromSeconds(10);

        var onTimeoutCalled = false;
        using var cts = new CancellationTokenSource();
        SetTimeout(TimeSpan.FromSeconds(10));
        _options.OnTimeout = args =>
        {
            onTimeoutCalled = true;
            return default;
        };

        var sut = CreateSut();

        await sut.Invoking(s => s.ExecuteAsync(async token =>
        {
            var task = _timeProvider.Delay(delay, token);
            cts.Cancel();
            await task;
        },
        cts.Token).AsTask())
        .Should()
        .ThrowAsync<OperationCanceledException>();

        onTimeoutCalled.Should().BeFalse();

        _diagnosticSource.Verify(v => v.IsEnabled("OnTimeout"), Times.Never());
    }

    [Fact]
    public async Task Execute_NoTimeoutOrCancellation_EnsureCancellationTokenRestored()
    {
        var delay = TimeSpan.FromSeconds(10);

        using var cts = new CancellationTokenSource();
        SetTimeout(TimeSpan.FromSeconds(10));
        _timeProvider.Advance(delay);

        var sut = CreateSut();

        var context = ResilienceContext.Get();
        context.CancellationToken = cts.Token;

        await sut.ExecuteAsync(
            (r, _) =>
            {
                r.CancellationToken.Should().NotBe(cts.Token);
                return default;
            },
            context,
            string.Empty);

        context.CancellationToken.Should().Be(cts.Token);
    }

    [Fact]
    public async Task Execute_EnsureCancellationTokenRegistrationNotExecutedOnSynchronizationContext()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        SetTimeout(TimeSpan.FromSeconds(10));

        var sut = CreateSut();

        var mockSynchronizationContext = new Mock<SynchronizationContext>(MockBehavior.Strict);
        mockSynchronizationContext
            .Setup(x => x.Post(It.IsAny<SendOrPostCallback>(), It.IsAny<object>()))
            .Callback<SendOrPostCallback, object>((callback, state) => callback(state));

        mockSynchronizationContext
            .Setup(x => x.CreateCopy())
            .Returns(mockSynchronizationContext.Object);

        SynchronizationContext.SetSynchronizationContext(mockSynchronizationContext.Object);

        // Act
        try
        {
            await sut.ExecuteAsync(async token =>
            {
                Task delayTask = Task.Delay(TimeSpan.FromSeconds(10), token);
                cts.Cancel();
                await delayTask;
            },
            cts.Token);
        }
        catch (TaskCanceledException)
        {
            // ok
        }

        // Assert
        mockSynchronizationContext.Verify(x => x.Post(It.IsAny<SendOrPostCallback>(), It.IsAny<object>()), Times.Never());
    }

    private void SetTimeout(TimeSpan timeout) => _options.TimeoutGenerator = args => new ValueTask<TimeSpan>(timeout);

    private TimeoutResilienceStrategy CreateSut() => new(_options, _timeProvider, _telemetry);
}
