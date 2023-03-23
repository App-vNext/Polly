using Polly.Timeout;

namespace Polly.Core.Tests.Timeout;

public class TimeoutGeneratorTests
{
    [Fact]
    public async Task Default_EnsureCorrectValue()
    {
        var generator = new TimeoutGenerator();

        var actualDelay = await generator.CreateHandler()(TimeoutTestUtils.TimeoutGeneratorArguments());

        actualDelay.Should().Be(TimeoutConstants.InfiniteTimeout);
    }

    [Fact]
    public async Task SetTimeout_Ok()
    {
        var generator = new TimeoutGenerator();
        var delay = TimeSpan.FromSeconds(1);

        generator.SetTimeout(TimeSpan.FromSeconds(1));

        var actualDelay = await generator.CreateHandler()(TimeoutTestUtils.TimeoutGeneratorArguments());

        actualDelay.Should().Be(delay);
    }

    [Fact]
    public void SetTimeout_Validation_Ok()
    {
        var generator = new TimeoutGenerator();

        Assert.Throws<ArgumentOutOfRangeException>(() => generator.SetTimeout(TimeSpan.FromSeconds(0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.SetTimeout(TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public async Task SetTimeout_Callback_Ok()
    {
        var generator = new TimeoutGenerator();
        var delay = TimeSpan.FromSeconds(1);

        generator.SetTimeout(args => delay);

        var actualDelay = await generator.CreateHandler()(TimeoutTestUtils.TimeoutGeneratorArguments());

        actualDelay.Should().Be(delay);
    }

    [Fact]
    public async Task SetTimeout_AsyncCallback_Ok()
    {
        var generator = new TimeoutGenerator();
        var delay = TimeSpan.FromSeconds(1);

        generator.SetTimeout(args => new ValueTask<TimeSpan>(delay));

        var actualDelay = await generator.CreateHandler()(TimeoutTestUtils.TimeoutGeneratorArguments());

        actualDelay.Should().Be(delay);
    }

    [Fact]
    public void ArgValidation_EnsureThrows()
    {
        var generator = new TimeoutGenerator();

        Assert.Throws<ArgumentNullException>(() => generator.SetTimeout((Func<TimeoutGeneratorArguments, TimeSpan>)null!));
        Assert.Throws<ArgumentNullException>(() => generator.SetTimeout((Func<TimeoutGeneratorArguments, ValueTask<TimeSpan>>)null!));
    }
}
