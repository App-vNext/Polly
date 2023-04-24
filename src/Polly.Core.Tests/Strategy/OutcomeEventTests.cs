using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;

public class OutcomeEventTests
{
    private readonly OutcomeEvent<TestArguments> _sut = new();

    [Fact]
    public void Empty_Ok()
    {
        _sut.IsEmpty.Should().BeTrue();

        _sut.Register<int>(() => { });

        _sut.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void CreateHandler_Empty_ReturnsNull()
    {
        _sut.CreateHandler().Should().BeNull();
    }

    public static readonly TheoryData<Action<OutcomeEvent<TestArguments>>> Data = new()
    {
        sut =>
        {
            bool called = false;
            sut.Register<int>((_, _) => { called = true; });
            InvokeHandler(sut, new Outcome<int>(0));
            called.Should().BeTrue();
        },
        sut =>
        {
            bool called = false;
            sut.Register<int>(_ => { called = true; });
            InvokeHandler(sut, new Outcome<int>(0));
            called.Should().BeTrue();
        },
        sut =>
        {
            bool called = false;
            sut.Register<int>(() => { called = true; });
            InvokeHandler(sut, new Outcome<int>(0));
            called.Should().BeTrue();
        },
        sut =>
        {
            bool called = false;
            sut.Register<int>((_, _) => { called = true; return default; });
            InvokeHandler(sut, new Outcome<int>(0));
            called.Should().BeTrue();
        },
        sut =>
        {
            int calls = 0;
            sut.Register<int>((_, _) => { calls++; });
            sut.Register<int>((_, _) => { calls++; });

            InvokeHandler(sut, new Outcome<int>(0));

            calls.Should().Be(2);
        },
        sut =>
        {
            bool called = false;
            sut.Register<int>((_, _) => { called = true; return default; });
            sut.Register<double>((_, _) => { called = true; return default; });
            InvokeHandler(sut, new Outcome<bool>(true));
            called.Should().BeFalse();
        },
        sut =>
        {
            bool called = false;
            sut.Register<double>((_, _) => { called = true; return default; });
            InvokeHandler(sut, new Outcome<bool>(true));
            called.Should().BeFalse();
        },
        sut =>
        {
            bool called = false;
            sut.Register<double>((_, _) => { called = true; return default; });
            sut.SetCallbacks(new OutcomeEvent<TestArguments, double>());
            InvokeHandler(sut, new Outcome<double>(1.0));
            called.Should().BeFalse();
        },
        sut =>
        {
            int called = 0;
            sut.Register<double>((_, _) => { called ++; return default; });
            sut.RegisterVoid((_, _) => { called ++; return default; });
            InvokeHandler(sut, new Outcome<VoidResult>(VoidResult.Instance));
            called.Should().Be(1);
        },
        sut =>
        {
            int called = 0;
            sut.RegisterVoid((_) => called ++);
            InvokeHandler(sut, new Outcome<VoidResult>(VoidResult.Instance));
            called.Should().Be(1);
        },
        sut =>
        {
            int called = 0;
            sut.RegisterVoid(() => called ++);
            InvokeHandler(sut, new Outcome<VoidResult>(VoidResult.Instance));
            called.Should().Be(1);
        },
        sut =>
        {
            int called = 0;
            sut.RegisterVoid((_, _) => called ++);
            InvokeHandler(sut, new Outcome<VoidResult>(VoidResult.Instance));
            called.Should().Be(1);
        },
        sut =>
        {
            int called = 0;
            sut.RegisterVoid((_, _) => called ++);
            sut.RegisterVoid((_, _) => called ++);
            InvokeHandler(sut, new Outcome<VoidResult>(VoidResult.Instance));
            called.Should().Be(2);
        },
        sut =>
        {
            sut.SetVoidCallbacks(new VoidOutcomeEvent<TestArguments>());
            InvokeHandler(sut, new Outcome<VoidResult>(VoidResult.Instance));
        },
        sut =>
        {
            int called = 0;
            sut.Register(() => called ++);
            InvokeHandler(sut, new Outcome<int>(10));
            called.Should().Be(1);
        },
        sut =>
        {
            int called = 0;
            sut.Register(() => called ++);
            sut.Register(() => called ++);
            InvokeHandler(sut, new Outcome<int>(10));
            called.Should().Be(2);
        },
        sut =>
        {
            int called = 0;
            sut.Register(() => called ++);
            sut.Register(() => called ++);
            sut.Register<int>(() => called ++);
            InvokeHandler(sut, new Outcome<int>(10));
            called.Should().Be(3);
        },
        sut =>
        {
            int called = 0;
            sut.Register(o => { o.Result.Should().Be(10); called ++; });
            InvokeHandler(sut, new Outcome<int>(10));
            called.Should().Be(1);
        },
        sut =>
        {
            int called = 0;
            sut.Register(o => { o.Exception.Should().BeOfType<InvalidOperationException>(); called ++; });
            InvokeHandler(sut, new Outcome<int>(new InvalidOperationException()));
            called.Should().Be(1);
        },
        sut =>
        {
            int called = 0;
            sut.Register((_, _) => called ++);
            InvokeHandler(sut, new Outcome<int>(10));
            called.Should().Be(1);
        },
        sut =>
        {
            int called = 0;
            sut.Register((_, _) => { called ++; return default; });
            InvokeHandler(sut, new Outcome<int>(10));
            called.Should().Be(1);
        },
    };

    [MemberData(nameof(Data))]
    [Theory]
    public void InvokeCallbacks_Ok(Action<OutcomeEvent<TestArguments>> callback)
    {
        _sut.Invoking(s => callback(s)).Should().NotThrow();
        callback(_sut);
    }

    [Fact]
    public void Register_DifferentResultType_NotInvoked()
    {
        var callbacks = new List<int>();

        for (var i = 0; i < 10; i++)
        {
            var index = i;

            _sut.Register<int>((_, _) =>
            {
                callbacks.Add(index);
            });

            _sut.Register<bool>((_, _) =>
            {
                callbacks.Add(index);
            });
        }

        InvokeHandler(_sut, new Outcome<int>(1));

        callbacks.Distinct().Should().HaveCount(10);
    }

    private static void InvokeHandler<T>(OutcomeEvent<TestArguments> sut, Outcome<T> outcome)
    {
        sut.CreateHandler()?.HandleAsync(outcome, new TestArguments()).AsTask().Wait();
    }
}
