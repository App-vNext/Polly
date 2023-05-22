using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;

public class OutcomeGeneratorTests
{
    private readonly OutcomeGenerator<TestArguments, GeneratedValue> _sut = new();

    [Fact]
    public void Empty_Ok()
    {
        _sut.IsEmpty.Should().BeTrue();

        _sut.SetGenerator<int>((_, _) => GeneratedValue.Invalid);

        _sut.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void CreateHandler_Empty_ReturnsNull()
    {
        CreateHandler(_sut).Should().BeNull();
    }

    public static readonly TheoryData<Action<OutcomeGenerator<TestArguments, GeneratedValue>>> Data = new()
    {
        sut =>
        {
            sut.SetGenerator<int>((_, _) => GeneratedValue.Valid1);
            InvokeHandler(sut, new Outcome<int>(0), GeneratedValue.Valid1);
        },
        sut =>
        {
            sut.SetGenerator<int>((_, _) => new ValueTask<GeneratedValue>(GeneratedValue.Valid1));
            InvokeHandler(sut, new Outcome<int>(0), GeneratedValue.Valid1);
        },
        sut =>
        {
            sut.SetGenerator<int>((_, _) => GeneratedValue.Valid1);
            InvokeHandler(sut, new Outcome<bool>(true), GeneratedValue.Default);
        },
        sut =>
        {
            sut.SetGenerator<int>((_, _) => new ValueTask<GeneratedValue>(GeneratedValue.Valid1));
            InvokeHandler(sut, new Outcome<bool>(true), GeneratedValue.Default);
        },
        sut =>
        {
            sut.SetGenerator<int>((_, _) => GeneratedValue.Invalid);
            InvokeHandler(sut, new Outcome<int>(0), GeneratedValue.Default);
        },
        sut =>
        {
            sut.SetGenerator<int>((_, _) => new ValueTask<GeneratedValue>(GeneratedValue.Invalid));
            InvokeHandler(sut, new Outcome<int>(0), GeneratedValue.Default);
        },
        sut =>
        {
            sut.SetGenerator<int>((_, _) => GeneratedValue.Valid1);
            sut.SetGenerator<int>((_, _) => GeneratedValue.Valid2);
            InvokeHandler(sut, new Outcome<int>(0), GeneratedValue.Valid2);
        },
        sut =>
        {
            sut.SetGenerator<int>((_, _) => new ValueTask<GeneratedValue>(GeneratedValue.Valid1));
            sut.SetGenerator<int>((_, _) => new ValueTask<GeneratedValue>(GeneratedValue.Valid2));
            InvokeHandler(sut, new Outcome<int>(0), GeneratedValue.Valid2);
        },
        sut =>
        {
            sut.SetGenerator<int>((_, _) => GeneratedValue.Valid1);
            sut.SetGenerator<bool>((_, _) => GeneratedValue.Valid2);
            InvokeHandler(sut, new Outcome<int>(0), GeneratedValue.Valid1);
            InvokeHandler(sut, new Outcome<bool>(true), GeneratedValue.Valid2);
        },
        sut =>
        {
            sut.SetGenerator<int>((_, _) => GeneratedValue.Valid1);
            sut.SetGenerator<double>((_, _) => GeneratedValue.Valid1);
            InvokeHandler(sut, new Outcome<bool>(true), GeneratedValue.Default);
        },
        sut =>
        {
            sut.SetGenerator<object>((_, _) => GeneratedValue.Valid1);
            InvokeHandler(sut, new Outcome<bool>(true), GeneratedValue.Valid1);
            InvokeHandler(sut, new Outcome<int>(0), GeneratedValue.Valid1);
        },
        sut =>
        {
            sut.SetGenerator<int>((_, _) => GeneratedValue.Valid1);
            sut.SetGenerator((_, _) => GeneratedValue.Valid2);
            InvokeHandler(sut, new Outcome<int>(10), GeneratedValue.Valid1);
        },
        sut =>
        {
            sut.SetGenerator<int>((_, _) => GeneratedValue.Invalid);
            sut.SetGenerator((_, _) => GeneratedValue.Valid2);
            InvokeHandler(sut, new Outcome<int>(10), GeneratedValue.Valid2);
        },
        sut =>
        {
            sut.SetGenerator((_, _) => GeneratedValue.Invalid);
            InvokeHandler(sut, new Outcome<int>(10), GeneratedValue.Default);
        },
        sut =>
        {
            sut.SetGenerator<bool>((_, _) => GeneratedValue.Valid1);
            sut.SetGenerator((_, _) => GeneratedValue.Valid2);
            InvokeHandler(sut, new Outcome<int>(10), GeneratedValue.Valid2);
        },
        sut =>
        {
            sut.SetGenerator((_, _) => GeneratedValue.Valid1);
            InvokeHandler(sut, new Outcome<int>(10), GeneratedValue.Valid1);
        },
        sut =>
        {
            sut.SetGenerator((outcome, _) =>
            {
                outcome.Result.Should().Be(10);
                return GeneratedValue.Valid1;
            });
            InvokeHandler(sut, new Outcome<int>(10), GeneratedValue.Valid1);
        },
        sut =>
        {
            sut.SetGenerator((_, _) => new ValueTask<GeneratedValue>(GeneratedValue.Valid1));
            InvokeHandler(sut, new Outcome<int>(10), GeneratedValue.Valid1);
        },
        sut =>
        {
            sut.SetVoidGenerator((_, _) => new ValueTask<GeneratedValue>(GeneratedValue.Valid1));
            InvokeHandler(sut, new Outcome<VoidResult>(VoidResult.Instance), GeneratedValue.Valid1);
        },
        sut =>
        {
            sut.SetVoidGenerator((_, _) => GeneratedValue.Valid1);
            InvokeHandler(sut, new Outcome<VoidResult>(VoidResult.Instance), GeneratedValue.Valid1);
        },
        sut =>
        {
            sut.SetVoidGenerator((_, _) => new ValueTask<GeneratedValue>(GeneratedValue.Valid1));
            InvokeHandler(sut, new Outcome<int>(10), GeneratedValue.Default);
        },
        sut =>
        {
            sut.SetVoidGenerator((_, _) => GeneratedValue.Valid1);
            InvokeHandler(sut, new Outcome<int>(10), GeneratedValue.Default);
        },
    };

    [MemberData(nameof(Data))]
    [Theory]
    public void ResultHandler_SinglePredicate_Ok(Action<OutcomeGenerator<TestArguments, GeneratedValue>> callback)
    {
        _sut.Invoking(s => callback(s)).Should().NotThrow();
        callback(_sut);
    }

    [Fact]
    public void AddResultHandlers_DifferentResultType_NotInvoked()
    {
        var callbacks = new List<int>();

        for (var i = 0; i < 10; i++)
        {
            var index = i;

            _sut.SetGenerator<int>((_, _) =>
            {
                callbacks.Add(index);
                return GeneratedValue.Valid1;
            });

            _sut.SetGenerator<bool>((_, _) =>
            {
                callbacks.Add(index);
                return GeneratedValue.Valid1;
            });
        }

        InvokeHandler(_sut, new Outcome<int>(1), GeneratedValue.Valid1);

        callbacks.Distinct().Should().HaveCount(1);
    }

    private static void InvokeHandler<T>(OutcomeGenerator<TestArguments, GeneratedValue> sut, Outcome<T> outcome, GeneratedValue expectedResult)
    {
        OutcomeGenerator<TestArguments, GeneratedValue>.Handler? handler = CreateHandler(sut);

        if (handler == null)
        {
            expectedResult.Should().Be(GeneratedValue.Default);
            return;
        }

        handler.GenerateAsync(outcome, new TestArguments()).AsTask().Result.Should().Be(expectedResult);
    }

    private static OutcomeGenerator<TestArguments, GeneratedValue>.Handler? CreateHandler(OutcomeGenerator<TestArguments, GeneratedValue> generator)
    {
        return generator.CreateHandler(GeneratedValue.Default, value => value == GeneratedValue.Valid1 || value == GeneratedValue.Valid2);
    }

    public enum GeneratedValue
    {
        Default,
        Valid1,
        Valid2,
        Invalid
    }
}
