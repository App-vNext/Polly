using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;

public class NoOutcomeEventTests
{
    [Fact]
    public async Task Add_EnsureOrdering()
    {
        var ev = new NoOutcomeEvent<TestArguments>();
        var raisedEvents = new List<int>();

        ev.Register(() => raisedEvents.Add(1));
        ev.Register(args => raisedEvents.Add(2));
        ev.Register(args => { raisedEvents.Add(3); return default; });

        var handler = ev.CreateHandler()!;

        await handler(new TestArguments());

        raisedEvents.Should().HaveCount(3);
        raisedEvents.Should().BeInAscendingOrder();
    }

    [Fact]
    public void Add_EnsureValidation()
    {
        var ev = new NoOutcomeEvent<TestArguments>();

        Assert.Throws<ArgumentNullException>(() => ev.Register((Action)null!));
        Assert.Throws<ArgumentNullException>(() => ev.Register((Action<TestArguments>)null!));
        Assert.Throws<ArgumentNullException>(() => ev.Register(null!));
    }

    [InlineData(1)]
    [InlineData(2)]
    [InlineData(100)]
    [Theory]
    public async Task CreateHandler_Ok(int count)
    {
        var ev = new NoOutcomeEvent<TestArguments>();
        var events = new List<int>();

        for (var i = 0; i < count; i++)
        {
            ev.Register(() => events.Add(i));
        }

        await ev.CreateHandler()!(new TestArguments());

        events.Should().HaveCount(count);
        events.Should().BeInAscendingOrder();
    }

    [Fact]
    public void CreateHandler_NoEvents_Null()
    {
        var ev = new NoOutcomeEvent<TestArguments>();

        var handler = ev.CreateHandler();

        handler.Should().BeNull();
    }
}
