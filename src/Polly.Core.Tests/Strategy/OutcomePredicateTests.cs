using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;

public class OutcomePredicateTests
{
    private readonly OutcomePredicate<TestArguments> _sut = new();

    [Fact]
    public void Empty_Ok()
    {
        _sut.IsEmpty.Should().BeTrue();

        _sut.HandleException<Exception>();

        _sut.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Empty_ConfigurePredicates_Ok()
    {
        _sut.IsEmpty.Should().BeTrue();
        _sut.ConfigurePredicates<int>(p => p.IsEmpty.Should().BeTrue());
        _sut.ConfigurePredicates<int>(p => p.IsEmpty.Should().BeTrue());
        _sut.IsEmpty.Should().BeFalse();
        _sut.CreateHandler().Should().BeNull();
    }

    [Fact]
    public void CreateHandler_Empty_ReturnsNull()
    {
        _sut.CreateHandler().Should().BeNull();
    }

    public static readonly TheoryData<Action<OutcomePredicate<TestArguments>>> ExceptionPredicates = new()
    {
        sut =>
        {
            sut.HandleException<InvalidOperationException>();
            InvokeHandler(sut, new InvalidOperationException("dummy"), true);
        },
        sut =>
        {
            sut.HandleException<InvalidOperationException>(_=> true);
            InvokeHandler(sut, new ArgumentNullException("dummy"), false);
        },
        sut =>
        {
            sut.HandleException<InvalidOperationException>((_, _)=> true);
            InvokeHandler(sut, new ArgumentNullException("dummy"), false);
        },
        sut =>
        {
            sut.HandleException<InvalidOperationException>((_, _)=> new ValueTask<bool>(true));
            InvokeHandler(sut, new ArgumentNullException("dummy"), false);
        },
        sut =>
        {
            sut.HandleException<InvalidOperationException>();
            InvokeHandler(sut, new ArgumentNullException("dummy"), false);
        },
        sut =>
        {
            sut.HandleException<InvalidOperationException>();
            InvokeHandler(sut, new InvalidOperationException("dummy"), true);
        },
        sut =>
        {
            sut.HandleException<InvalidOperationException>(e => e.Message.Contains("dummy"));
            InvokeHandler(sut, new InvalidOperationException("other message"), false);
        },
        sut =>
        {
            sut.HandleException<InvalidOperationException>((e, _) => e.Message.Contains("dummy"));
            InvokeHandler(sut, new InvalidOperationException("other message"), false);
        },
        sut =>
        {
            sut.HandleException<InvalidOperationException>((e, _) => e.Message.Contains("dummy"));
            InvokeHandler(sut, new InvalidOperationException("dummy"), true);
        },
        sut =>
        {
            sut.HandleException<InvalidOperationException>((e, _) => new ValueTask<bool>(e.Message.Contains("dummy")));
            InvokeHandler(sut, new InvalidOperationException("other message"), false);
        },
        sut =>
        {
            sut.HandleException<InvalidOperationException>((e, _) => new ValueTask<bool>(e.Message.Contains("dummy")));
            InvokeHandler(sut, new InvalidOperationException("dummy"), true);
        },
    };

    [MemberData(nameof(ExceptionPredicates))]
    [Theory]
    public void ExceptionHandler_SinglePredicate_Ok(Action<OutcomePredicate<TestArguments>> execute)
    {
        _sut.Invoking(s => execute(s)).Should().NotThrow();
        execute(_sut);
    }

    [MemberData(nameof(ExceptionPredicates))]
    [Theory]
    public void ExceptionHandler_MultiplePredicates_Ok(Action<OutcomePredicate<TestArguments>> execute)
    {
        _sut.HandleException<Exception>(e => false);
        _sut.Invoking(s => execute(s)).Should().NotThrow();
    }

    public static readonly TheoryData<Action<OutcomePredicate<TestArguments>>> ResultPredicates = new()
    {
        sut =>
        {
            sut.HandleResult(10);
            InvokeResultHandler(sut, 10, true);
        },
        sut =>
        {
            sut.HandleResult(11);
            InvokeResultHandler(sut, 10, false);
        },
        sut =>
        {
            sut.HandleResult(10);
            InvokeResultHandler(sut, 10.0, false);
        },
        sut =>
        {
            sut.HandleResult<int>(result => result == 5);
            InvokeResultHandler(sut, 5, true);
        },
        sut =>
        {
            sut.HandleResult<int>(result => result == 5);
            InvokeResultHandler(sut, 6, false);
        },
        sut =>
        {
            sut.HandleResult<int>((result, _) => result == 5);
            InvokeResultHandler(sut, 5, true);
        },
        sut =>
        {
            sut.HandleResult<int>((result, _) => result == 5);
            InvokeResultHandler(sut, 6, false);
        },
        sut =>
        {
            sut.HandleResult<int>((result, _) => new ValueTask<bool>(result == 5));
            InvokeResultHandler(sut, 5, true);
        },
        sut =>
        {
            sut.HandleResult<int>((result, _) => new ValueTask<bool>(result == 5));
            InvokeResultHandler(sut, 6, false);
        },
        sut =>
        {
            sut.HandleResult(10);
            sut.HandleResult(11);
            sut.HandleResult("dummy-1");
            sut.HandleResult("dummy-2");

            InvokeResultHandler(sut, "dummy", false);
            InvokeResultHandler(sut, "dummy-1", true);
            InvokeResultHandler(sut, "dummy-2", true);

            InvokeResultHandler(sut, 10, true);
            InvokeResultHandler(sut, 11, true);
            InvokeResultHandler(sut, 12, false);

            InvokeResultHandler(sut, new object(), false);
        },
        sut =>
        {
            sut.HandleOutcome<int>((outcome, _) => new ValueTask<bool>(outcome.Result == 5));
            InvokeResultHandler(sut, 6, false);
        },
        sut =>
        {
            sut.HandleOutcome<int>((outcome, _) => new ValueTask<bool>(outcome.Result == 6));
            InvokeResultHandler(sut, 6, true);
        },
        sut =>
        {
            sut.HandleOutcome<int>((outcome, _) => outcome.Result == 5);
            InvokeResultHandler(sut, 6, false);
        },
        sut =>
        {
            sut.HandleOutcome<int>((outcome, _) => outcome.Result == 6);
            InvokeResultHandler(sut, 6, true);
        },
        sut =>
        {
            sut.HandleResult<int>((result, _) => result == 6);
            InvokeHandler(sut, new InvalidOperationException(), false);
            InvokeHandler<int>(sut, new InvalidOperationException(), false);
        },
        sut =>
        {
            sut.HandleResult<int>((result, _) => new ValueTask<bool>(result == 6));
            InvokeHandler(sut, new InvalidOperationException(), false);
            InvokeHandler<int>(sut, new InvalidOperationException(), false);
        },
        sut =>
        {
            sut.HandleOutcome<int>((outcome, _) => outcome.Exception is InvalidOperationException);
            InvokeHandler<int>(sut, new InvalidOperationException(), true);
        },
        sut =>
        {
            sut.HandleOutcome<int>((outcome, _) => new ValueTask<bool>(outcome.Exception is InvalidOperationException));
            InvokeHandler<int>(sut, new InvalidOperationException(), true);
        }
    };

    [MemberData(nameof(ResultPredicates))]
    [Theory]
    public void ResultHandler_SinglePredicate_Ok(Action<OutcomePredicate<TestArguments>> executeResult)
    {
        _sut.Invoking(s => executeResult(s)).Should().NotThrow();
        executeResult(_sut);
    }

    [MemberData(nameof(ResultPredicates))]
    [Theory]
    public void ResultHandler_MultiplePredicates_Ok(Action<OutcomePredicate<TestArguments>> executeResult)
    {
        _sut.HandleException<Exception>(e => false);
        _sut.Invoking(s => executeResult(s)).Should().NotThrow();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void AddResultHandlers_Multiple(bool differentTypes)
    {
        var callbacks = new List<int>();

        for (var i = 0; i < 10; i++)
        {
            var index = i;

            _sut.HandleResult<int>(result =>
            {
                callbacks.Add(index);
                return false;
            });

            if (differentTypes)
            {
                _sut.HandleResult<string>(result =>
                {
                    callbacks.Add(index);
                    return false;
                });
            }
        }

        InvokeResultHandler(_sut, 10, false);
        callbacks.Distinct().Should().HaveCount(10);
    }

    [Fact]
    public void AddResultHandlers_DifferentResultType_NotInvoked()
    {
        var callbacks = new List<int>();

        for (var i = 0; i < 10; i++)
        {
            var index = i;

            _sut.HandleResult<int>(result =>
            {
                callbacks.Add(index);
                return false;
            });
        }

        InvokeResultHandler(_sut, new object(), false);

        callbacks.Distinct().Should().HaveCount(0);
    }

    [Fact]
    public void AddResultHandlers_StopOnTrue()
    {
        var callbacks = new List<int>();

        for (var i = 0; i < 10; i++)
        {
            var index = i;

            _sut.HandleResult<int>(result =>
            {
                callbacks.Add(index);
                return index > 5;
            });
        }

        InvokeResultHandler(_sut, 1, true);
        callbacks.Distinct().Should().HaveCount(7);
    }

    private static void InvokeHandler<TResult>(OutcomePredicate<TestArguments> sut, Exception exception, bool expectedResult)
    {
        var args = new TestArguments();
        sut.CreateHandler()!.ShouldHandle(new Outcome<TResult>(exception), args).AsTask().Result.Should().Be(expectedResult);
    }

    private static void InvokeHandler(OutcomePredicate<TestArguments> sut, Exception exception, bool expectedResult)
    {
        var args = new TestArguments();

        sut.CreateHandler()!.ShouldHandle(new Outcome<object>(exception), args).AsTask().Result.Should().Be(expectedResult);

        // again with void result
        sut.CreateHandler()!.ShouldHandle(new Outcome<VoidResult>(exception), args).AsTask().Result.Should().Be(expectedResult);

        // again with result
        sut.HandleResult(12345);
        sut.CreateHandler()!.ShouldHandle(new Outcome<int>(10), args).AsTask().Result.Should().Be(false);
    }

    private static void InvokeResultHandler<T>(OutcomePredicate<TestArguments> sut, T result, bool expectedResult)
    {
        sut.CreateHandler()!.ShouldHandle<T>(new Outcome<T>(result), new TestArguments()).AsTask().Result.Should().Be(expectedResult);
    }
}
