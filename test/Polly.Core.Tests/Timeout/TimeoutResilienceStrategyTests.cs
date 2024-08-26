using System.Runtime.InteropServices;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
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
    private readonly List<TelemetryEventArguments<object, object>> _args = [];

    public TimeoutResilienceStrategyTests()
    {
        _telemetry = TestUtilities.CreateResilienceTelemetry(_args.Add);
        _options = new TimeoutStrategyOptions();
        _cancellationSource = new CancellationTokenSource();
    }

#pragma warning disable IDE0028 // Simplify collection initialization
    public static TheoryData<Func<TimeSpan>> Execute_NoTimeout_Data() => new()
    {
        () => TimeSpan.Zero,
        () => TimeSpan.FromMilliseconds(-1),
        () => System.Threading.Timeout.InfiniteTimeSpan,
    };
#pragma warning restore IDE0028 // Simplify collection initialization

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
        var called = false;
        SetTimeout(_delay);

        var executionTime = _delay + TimeSpan.FromSeconds(1);
        _options.OnTimeout = args =>
        {
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

        _args.Should().HaveCount(1);
        _args[0].Arguments.Should().BeOfType<OnTimeoutArguments>();
    }

    [MemberData(nameof(Execute_NoTimeout_Data))]
    [Theory]
    public void Execute_NoTimeout(Func<TimeSpan> timeout)
    {
        var called = false;
        SetTimeout(timeout());
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
    public async Task Execute_TimeoutGeneratorIsNull_FallsBackToTimeout()
    {
        var called = false;
        var timeout = TimeSpan.FromMilliseconds(10);
        _options.TimeoutGenerator = null;
        _options.Timeout = timeout;

        _options.OnTimeout = args =>
        {
            called = true;
            args.Timeout.Should().Be(timeout);
            return default;
        };

        var sut = CreateSut();
        await sut
        .Invoking(s => s.ExecuteAsync(async token =>
        {
            var delay = _timeProvider.Delay(TimeSpan.FromMilliseconds(50), token);
            _timeProvider.Advance(timeout);
            await delay;
        },
        CancellationToken.None)
        .AsTask())
        .Should().ThrowAsync<TimeoutRejectedException>();

        called.Should().BeTrue();
    }

    [Fact]
    public async Task Execute_Timeout_EnsureStackTrace()
    {
        SetTimeout(TimeSpan.FromSeconds(2));
        var sut = CreateSut();

        var outcome = await sut.ExecuteOutcomeAsync(
            async (c, _) =>
            {
                var delay = _timeProvider.Delay(TimeSpan.FromSeconds(4), c.CancellationToken);
                _timeProvider.Advance(TimeSpan.FromSeconds(2));
                await delay;

                return Outcome.FromResult("dummy");
            },
            ResilienceContextPool.Shared.Get(),
            "state");

        outcome.Exception.Should().BeOfType<TimeoutRejectedException>();

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            outcome.Exception!.StackTrace.Should().NotBeEmpty();
        }
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

        _args.Should().HaveCount(0);
    }

    [Fact]
    public async Task Execute_NoTimeoutOrCancellation_EnsureCancellationTokenRestored()
    {
        var delay = TimeSpan.FromSeconds(10);

        using var cts = new CancellationTokenSource();
        SetTimeout(TimeSpan.FromSeconds(10));
        _timeProvider.Advance(delay);

        var sut = CreateSut();

        var context = ResilienceContextPool.Shared.Get();
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

        var mockSynchronizationContext = Substitute.For<SynchronizationContext>();
        mockSynchronizationContext
            .When(x => x.Post(Arg.Any<SendOrPostCallback>(), Arg.Any<object>()))
            .Do((p) => ((SendOrPostCallback)p[1])(p[2]));

        mockSynchronizationContext.CreateCopy()
            .Returns(mockSynchronizationContext);

        SynchronizationContext.SetSynchronizationContext(mockSynchronizationContext);

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
        catch (OperationCanceledException)
        {
            // ok
        }

        // Assert
        mockSynchronizationContext.DidNotReceiveWithAnyArgs().Post(default!, default);
    }

    private void SetTimeout(TimeSpan timeout) => _options.TimeoutGenerator = args => new ValueTask<TimeSpan>(timeout);

    private ResiliencePipeline CreateSut() => new TimeoutResilienceStrategy(_options, _timeProvider, _telemetry).AsPipeline();
}
