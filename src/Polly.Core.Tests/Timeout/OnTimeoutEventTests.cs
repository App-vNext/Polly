using Polly.Timeout;

namespace Polly.Core.Tests.Timeout;

public class OnTimeoutEventTests
{
    [Fact]
    public async Task Add_EnsureOrdering()
    {
        var ev = new OnTimeoutEvent();
        List<int> raisedEvents = new List<int>();

        ev.Add(() => raisedEvents.Add(1));
        ev.Add(args => raisedEvents.Add(2));
        ev.Add(args => { raisedEvents.Add(3); return default; });

        var handler = ev.CreateHandler()!;

        await handler(TimeoutTestUtils.OnTimeoutArguments());

        raisedEvents.Should().HaveCount(3);
        raisedEvents.Should().BeInAscendingOrder();
    }

    [Fact]
    public void Add_EnsureValidation()
    {
        var ev = new OnTimeoutEvent();

        Assert.Throws<ArgumentNullException>(() => ev.Add((Action)null!));
        Assert.Throws<ArgumentNullException>(() => ev.Add((Action<OnTimeoutArguments>)null!));
        Assert.Throws<ArgumentNullException>(() => ev.Add(null!));
    }

    [InlineData(1)]
    [InlineData(2)]
    [InlineData(100)]
    [Theory]
    public async Task CreateHandler_Ok(int count)
    {
        var ev = new OnTimeoutEvent();
        var events = new List<int>();

        for (int i = 0; i < count; i++)
        {
            ev.Add(() => events.Add(i));
        }

        await ev.CreateHandler()!(TimeoutTestUtils.OnTimeoutArguments());

        events.Should().HaveCount(count);
        events.Should().BeInAscendingOrder();
    }

    [Fact]
    public void CreateHandler_NoEvents_Null()
    {
        var ev = new OnTimeoutEvent();

        var handler = ev.CreateHandler();

        handler.Should().BeNull();
    }
}
