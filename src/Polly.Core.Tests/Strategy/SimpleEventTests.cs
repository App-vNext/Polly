using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;

public class SimpleEventTests
{
    [Fact]
    public async Task Add_EnsureOrdering()
    {
        var ev = new DummyEvent();
        var raisedEvents = new List<int>();

        ev.Add(() => raisedEvents.Add(1));
        ev.Add(args => raisedEvents.Add(2));
        ev.Add(args => { raisedEvents.Add(3); return default; });

        var handler = ev.CreateHandler()!;

        await handler(new TestArguments());

        raisedEvents.Should().HaveCount(3);
        raisedEvents.Should().BeInAscendingOrder();
    }

    [Fact]
    public void Add_EnsureValidation()
    {
        var ev = new DummyEvent();

        Assert.Throws<ArgumentNullException>(() => ev.Add((Action)null!));
        Assert.Throws<ArgumentNullException>(() => ev.Add((Action<TestArguments>)null!));
        Assert.Throws<ArgumentNullException>(() => ev.Add(null!));
    }

    [InlineData(1)]
    [InlineData(2)]
    [InlineData(100)]
    [Theory]
    public async Task CreateHandler_Ok(int count)
    {
        var ev = new DummyEvent();
        var events = new List<int>();

        for (var i = 0; i < count; i++)
        {
            ev.Add(() => events.Add(i));
        }

        await ev.CreateHandler()!(new TestArguments());

        events.Should().HaveCount(count);
        events.Should().BeInAscendingOrder();
    }

    [Fact]
    public void CreateHandler_NoEvents_Null()
    {
        var ev = new DummyEvent();

        var handler = ev.CreateHandler();

        handler.Should().BeNull();
    }

    private class DummyEvent : SimpleEvent<TestArguments, DummyEvent>
    {
    }
}
