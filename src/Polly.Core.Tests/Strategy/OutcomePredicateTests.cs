using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;

public class OutcomePredicateTests
{
    private readonly DummyPredicate _sut = new();

    [Fact]
    public void Empty_Ok()
    {
        _sut.IsEmpty.Should().BeTrue();

        _sut.Exception<Exception>();

        _sut.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void CreateHandler_Empty_ReturnsNull()
    {
        _sut.CreateHandler().Should().BeNull();
    }

    public static readonly TheoryData<Action<DummyPredicate>> ExceptionPredicates = new()
    {
        sut =>
        {
            sut.Exception<InvalidOperationException>();
            InvokeHandler(sut, new InvalidOperationException("dummy"), true);
        },
        sut =>
        {
            sut.Exception<InvalidOperationException>(_=> true);
            InvokeHandler(sut, new ArgumentNullException("dummy"), false);
        },
        sut =>
        {
            sut.Exception<InvalidOperationException>((_, _)=> true);
            InvokeHandler(sut, new ArgumentNullException("dummy"), false);
        },
        sut =>
        {
            sut.Exception<InvalidOperationException>((_, _)=> new ValueTask<bool>(true));
            InvokeHandler(sut, new ArgumentNullException("dummy"), false);
        },
        sut =>
        {
            sut.Exception<InvalidOperationException>();
            InvokeHandler(sut, new ArgumentNullException("dummy"), false);
        },
        sut =>
        {
            sut.Exception<InvalidOperationException>();
            InvokeHandler(sut, new InvalidOperationException("dummy"), true);
        },
        sut =>
        {
            sut.Exception<InvalidOperationException>(e => e.Message.Contains("dummy"));
            InvokeHandler(sut, new InvalidOperationException("other message"), false);
        },
        sut =>
        {
            sut.Exception<InvalidOperationException>((e, _) => e.Message.Contains("dummy"));
            InvokeHandler(sut, new InvalidOperationException("other message"), false);
        },
        sut =>
        {
            sut.Exception<InvalidOperationException>((e, _) => e.Message.Contains("dummy"));
            InvokeHandler(sut, new InvalidOperationException("dummy"), true);
        },
        sut =>
        {
            sut.Exception<InvalidOperationException>((e, _) => new ValueTask<bool>(e.Message.Contains("dummy")));
            InvokeHandler(sut, new InvalidOperationException("other message"), false);
        },
        sut =>
        {
            sut.Exception<InvalidOperationException>((e, _) => new ValueTask<bool>(e.Message.Contains("dummy")));
            InvokeHandler(sut, new InvalidOperationException("dummy"), true);
        },
    };

    [MemberData(nameof(ExceptionPredicates))]
    [Theory]
    public void ExceptionHandler_SinglePredicate_Ok(Action<DummyPredicate> execute)
    {
        _sut.Invoking(s => execute(s)).Should().NotThrow();
        execute(_sut);
    }

    [MemberData(nameof(ExceptionPredicates))]
    [Theory]
    public void ExceptionHandler_MultiplePredicates_Ok(Action<DummyPredicate> execute)
    {
        _sut.Exception<Exception>(e => false);
        _sut.Invoking(s => execute(s)).Should().NotThrow();
    }

    public static readonly TheoryData<Action<DummyPredicate>> ResultPredicates = new()
    {
        sut =>
        {
            sut.Result(10);
            InvokeResultHandler(sut, 10, true);
        },
        sut =>
        {
            sut.Result(11);
            InvokeResultHandler(sut, 10, false);
        },
        sut =>
        {
            sut.Result(10);
            InvokeResultHandler(sut, 10.0, false);
        },
        sut =>
        {
            sut.Result<int>(result => result == 5);
            InvokeResultHandler(sut, 5, true);
        },
        sut =>
        {
            sut.Result<int>(result => result == 5);
            InvokeResultHandler(sut, 6, false);
        },
        sut =>
        {
            sut.Result<int>((result, _) => result == 5);
            InvokeResultHandler(sut, 5, true);
        },
        sut =>
        {
            sut.Result<int>((result, _) => result == 5);
            InvokeResultHandler(sut, 6, false);
        },
        sut =>
        {
            sut.Result<int>((result, _) => new ValueTask<bool>(result == 5));
            InvokeResultHandler(sut, 5, true);
        },
        sut =>
        {
            sut.Result<int>((result, _) => new ValueTask<bool>(result == 5));
            InvokeResultHandler(sut, 6, false);
        },
        sut =>
        {
            sut.Result(10);
            sut.Result(11);
            sut.Result("dummy-1");
            sut.Result("dummy-2");

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
            sut.Outcome<int>((outcome, _) => new ValueTask<bool>(outcome.Result == 5));
            InvokeResultHandler(sut, 6, false);
        },
        sut =>
        {
            sut.Outcome<int>((outcome, _) => new ValueTask<bool>(outcome.Result == 6));
            InvokeResultHandler(sut, 6, true);
        },
        sut =>
        {
            sut.Outcome<int>((outcome, _) => outcome.Result == 5);
            InvokeResultHandler(sut, 6, false);
        },
        sut =>
        {
            sut.Outcome<int>((outcome, _) => outcome.Result == 6);
            InvokeResultHandler(sut, 6, true);
        },
        sut =>
        {
            sut.Result<int>((result, _) => result == 6);
            InvokeHandler(sut, new InvalidOperationException(), false);
            InvokeHandler<int>(sut, new InvalidOperationException(), false);
        },
        sut =>
        {
            sut.Result<int>((result, _) => new ValueTask<bool>(result == 6));
            InvokeHandler(sut, new InvalidOperationException(), false);
            InvokeHandler<int>(sut, new InvalidOperationException(), false);
        },
        sut =>
        {
            sut.Outcome<int>((outcome, _) => outcome.Exception is InvalidOperationException);
            InvokeHandler<int>(sut, new InvalidOperationException(), true);
        },
        sut =>
        {
            sut.Outcome<int>((outcome, _) => new ValueTask<bool>(outcome.Exception is InvalidOperationException));
            InvokeHandler<int>(sut, new InvalidOperationException(), true);
        }
    };

    [MemberData(nameof(ResultPredicates))]
    [Theory]
    public void ResultHandler_SinglePredicate_Ok(Action<DummyPredicate> executeResult)
    {
        _sut.Invoking(s => executeResult(s)).Should().NotThrow();
        executeResult(_sut);
    }

    [MemberData(nameof(ResultPredicates))]
    [Theory]
    public void ResultHandler_MultiplePredicates_Ok(Action<DummyPredicate> executeResult)
    {
        _sut.Exception<Exception>(e => false);
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

            _sut.Result<int>(result =>
            {
                callbacks.Add(index);
                return false;
            });

            if (differentTypes)
            {
                _sut.Result<string>(result =>
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

            _sut.Result<int>(result =>
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

            _sut.Result<int>(result =>
            {
                callbacks.Add(index);
                return index > 5;
            });
        }

        InvokeResultHandler(_sut, 1, true);
        callbacks.Distinct().Should().HaveCount(7);
    }

    private static void InvokeHandler<TResult>(DummyPredicate sut, Exception exception, bool expectedResult)
    {
        var args = new Args();
        sut.CreateHandler()!.ShouldHandle(new Outcome<TResult>(exception), args).AsTask().Result.Should().Be(expectedResult);
    }

    private static void InvokeHandler(DummyPredicate sut, Exception exception, bool expectedResult)
    {
        var args = new Args();

        sut.CreateHandler()!.ShouldHandle(new Outcome<object>(exception), args).AsTask().Result.Should().Be(expectedResult);

        // again with void result
        sut.CreateHandler()!.ShouldHandle(new Outcome<VoidResult>(exception), args).AsTask().Result.Should().Be(expectedResult);

        // again with result
        sut.Result(12345);
        sut.CreateHandler()!.ShouldHandle(new Outcome<int>(10), args).AsTask().Result.Should().Be(false);
    }

    private static void InvokeResultHandler<T>(DummyPredicate sut, T result, bool expectedResult)
    {
        var args = new Args();
        sut.CreateHandler()!.ShouldHandle<T>(new Outcome<T>(result), args).AsTask().Result.Should().Be(expectedResult);
    }

    public sealed class DummyPredicate : OutcomePredicate<Args, DummyPredicate>
    {
    }

    public class Args : IResilienceArguments
    {
        public Args() => Context = ResilienceContext.Get();

        public ResilienceContext Context { get; private set; }
    }
}
