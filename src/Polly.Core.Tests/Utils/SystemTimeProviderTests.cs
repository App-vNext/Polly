using FluentAssertions;
using Polly.Utils;
using Xunit;

namespace Polly.Core.Tests.Utils;

public class SystemTimeProviderTests
{
    [Fact]
    public void TimestampFrequency_Ok()
    {
        TimeProvider.System.TimestampFrequency.Should().Be(Stopwatch.Frequency);
    }

    [Fact]
    public async Task CancelAfter_Ok()
    {
        await TestUtils.AssertWithTimeoutAsync(async () =>
        {
            using var cts = new CancellationTokenSource();
            TimeProvider.System.CancelAfter(cts, TimeSpan.FromMilliseconds(10));
            cts.IsCancellationRequested.Should().BeFalse();
            await Task.Delay(10);
            cts.Token.IsCancellationRequested.Should().BeTrue();
        });
    }

    [Fact]
    public async Task Delay_Ok()
    {
        using var cts = new CancellationTokenSource();

        await TestUtils.AssertWithTimeoutAsync(() =>
        {
            TimeProvider.System.Delay(TimeSpan.FromMilliseconds(10)).IsCompleted.Should().BeFalse();
        });
    }

    [Fact]
    public void Delay_NoDelay_Ok()
    {
        using var cts = new CancellationTokenSource();

        TimeProvider.System.Delay(TimeSpan.Zero).IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetElapsedTime_Ok()
    {
        var delay = TimeSpan.FromMilliseconds(10);
        var delayWithTolerance = TimeSpan.FromMilliseconds(30);

        await TestUtils.AssertWithTimeoutAsync(async () =>
        {
            var stamp1 = TimeProvider.System.GetTimestamp();
            await Task.Delay(10);
            var stamp2 = TimeProvider.System.GetTimestamp();

            var elapsed = TimeProvider.System.GetElapsedTime(stamp1, stamp2);

            elapsed.Should().BeGreaterThanOrEqualTo(delay);
            elapsed.Should().BeLessThan(delayWithTolerance);
        });
    }

    [Fact]
    public void GetElapsedTime_Mocked_Ok()
    {
        var provider = new FakeTimeProvider(40);
        provider.SetupSequence(v => v.GetTimestamp()).Returns(120000).Returns(480000);

        var stamp1 = provider.Object.GetTimestamp();
        var stamp2 = provider.Object.GetTimestamp();

        var delay = provider.Object.GetElapsedTime(stamp1, stamp2);

        var tickFrequency = (double)TimeSpan.TicksPerSecond / 40;
        var expected = new TimeSpan((long)((stamp2 - stamp1) * tickFrequency));

        delay.Should().Be(expected);
    }

    [Fact]
    public async Task UtcNow_Ok()
    {
        await TestUtils.AssertWithTimeoutAsync(() =>
        {
            var now = TimeProvider.System.UtcNow;
            (DateTimeOffset.UtcNow - now).Should().BeLessThanOrEqualTo(TimeSpan.FromMilliseconds(10));
        });
    }

}
