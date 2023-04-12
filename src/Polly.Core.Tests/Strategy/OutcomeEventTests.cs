using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;

public class OutcomeEventTests
{
    private readonly DummyEvent _sut = new();

    [Fact]
    public void Empty_Ok()
    {
        _sut.IsEmpty.Should().BeTrue();

        _sut.Add<int>(() => { });

        _sut.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void CreateHandler_Empty_ReturnsNull()
    {
        _sut.CreateHandler().Should().BeNull();
    }

    public static readonly TheoryData<Action<DummyEvent>> Data = new()
    {
        sut =>
        {
            bool called = false;
            sut.Add<int>((_, _) => { called = true; });
            InvokeHandler(sut, new Outcome<int>(0));
            called.Should().BeTrue();
        },
        sut =>
        {
            bool called = false;
            sut.Add<int>(_ => { called = true; });
            InvokeHandler(sut, new Outcome<int>(0));
            called.Should().BeTrue();
        },
        sut =>
        {
            bool called = false;
            sut.Add<int>(() => { called = true; });
            InvokeHandler(sut, new Outcome<int>(0));
            called.Should().BeTrue();
        },
        sut =>
        {
            bool called = false;
            sut.Add<int>((_, _) => { called = true; return default; });
            InvokeHandler(sut, new Outcome<int>(0));
            called.Should().BeTrue();
        },
        sut =>
        {
            int calls = 0;
            sut.Add<int>((_, _) => { calls++; });
            sut.Add<int>((_, _) => { calls++; });

            InvokeHandler(sut, new Outcome<int>(0));

            calls.Should().Be(2);
        },
        sut =>
        {
            bool called = false;
            sut.Add<int>((_, _) => { called = true; return default; });
            sut.Add<double>((_, _) => { called = true; return default; });
            InvokeHandler(sut, new Outcome<bool>(true));
            called.Should().BeFalse();
        },
        sut =>
        {
            bool called = false;
            sut.Add<double>((_, _) => { called = true; return default; });
            InvokeHandler(sut, new Outcome<bool>(true));
            called.Should().BeFalse();
        },
    };

    [MemberData(nameof(Data))]
    [Theory]
    public void InvokeCallbacks_Ok(Action<DummyEvent> callback)
    {
        _sut.Invoking(s => callback(s)).Should().NotThrow();
        callback(_sut);
    }

    [Fact]
    public void AddCallback_DifferentResultType_NotInvoked()
    {
        var callbacks = new List<int>();

        for (var i = 0; i < 10; i++)
        {
            var index = i;

            _sut.Add<int>((_, _) =>
            {
                callbacks.Add(index);
            });

            _sut.Add<bool>((_, _) =>
            {
                callbacks.Add(index);
            });
        }

        InvokeHandler(_sut, new Outcome<int>(1));

        callbacks.Distinct().Should().HaveCount(10);
    }

    private static void InvokeHandler<T>(DummyEvent sut, Outcome<T> outcome)
    {
        sut.CreateHandler()!.Handle(outcome, new TestArguments()).AsTask().Wait();
    }

    public sealed class DummyEvent : OutcomeEvent<TestArguments, DummyEvent>
    {
    }
}
