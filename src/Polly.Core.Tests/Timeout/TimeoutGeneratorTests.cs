using Polly.Timeout;

namespace Polly.Core.Tests.Timeout;

public class TimeoutGeneratorTests
{
    [Fact]
    public void Default_EnsureCorrectValue()
    {
        var generator = new TimeoutGenerator();

        generator.CreateHandler().Should().BeNull();
    }

    [Fact]
    public async Task SetTimeout_Callback_Ok()
    {
        var generator = new TimeoutGenerator();
        var delay = TimeSpan.FromSeconds(1);

        generator.SetTimeout(args => delay);

        var actualDelay = await generator.CreateHandler()!(TimeoutTestUtils.TimeoutGeneratorArguments());

        actualDelay.Should().Be(delay);
    }

    [Fact]
    public async Task SetTimeout_AsyncCallback_Ok()
    {
        var generator = new TimeoutGenerator();
        var delay = TimeSpan.FromSeconds(1);

        generator.SetTimeout(args => new ValueTask<TimeSpan>(delay));

        var actualDelay = await generator.CreateHandler()!(TimeoutTestUtils.TimeoutGeneratorArguments());

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
