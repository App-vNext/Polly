using Polly.Strategy;

namespace Polly.Core.Tests.Timeout;

public class NoOutcomeGeneratorTests
{
    [Fact]
    public void Default_EnsureCorrectValue()
    {
        var generator = new NoOutcomeGenerator<TestArguments, TimeSpan>();

        generator.CreateHandler(default, _ => true).Should().BeNull();
    }

    [Fact]
    public async Task SetGenerator_Callback_Ok()
    {
        var generator = new NoOutcomeGenerator<TestArguments, TimeSpan>();
        var delay = TimeSpan.FromSeconds(1);

        generator.SetGenerator(args => delay);

        var actualDelay = await generator.CreateHandler(default, _ => true)!(new TestArguments());

        actualDelay.Should().Be(delay);
    }

    [Fact]
    public async Task SetGenerator_AsyncCallback_Ok()
    {
        var generator = new NoOutcomeGenerator<TestArguments, TimeSpan>();
        var delay = TimeSpan.FromSeconds(1);

        generator.SetGenerator(args => new ValueTask<TimeSpan>(delay));

        var actualDelay = await generator.CreateHandler(default, _ => true)!(new TestArguments());

        actualDelay.Should().Be(delay);
    }

    [Fact]
    public async Task CreateHandler_EnsureValidation()
    {
        var generator = new NoOutcomeGenerator<TestArguments, TimeSpan>();
        var delay = TimeSpan.FromSeconds(1);

        generator.SetGenerator(args => new ValueTask<TimeSpan>(delay));

        var actualDelay = await generator.CreateHandler(TimeSpan.FromMilliseconds(123), _ => false)!(new TestArguments());

        actualDelay.Should().Be(TimeSpan.FromMilliseconds(123));
    }

    [Fact]
    public void ArgValidation_EnsureThrows()
    {
        var generator = new NoOutcomeGenerator<TestArguments, TimeSpan>();

        Assert.Throws<ArgumentNullException>(() => generator.SetGenerator((Func<TestArguments, TimeSpan>)null!));
        Assert.Throws<ArgumentNullException>(() => generator.SetGenerator((Func<TestArguments, ValueTask<TimeSpan>>)null!));
    }
}
