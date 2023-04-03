using System;
using System.Threading.Tasks;
using Polly.Strategy;

namespace Polly.Core.Tests.Strategy;

public class OutcomeGeneratorTests
{
    private readonly DummyGenerator _sut = new();

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
        _sut.CreateHandler().Should().BeNull();
    }

    public static readonly TheoryData<Action<DummyGenerator>> Data = new()
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
    };

    [MemberData(nameof(Data))]
    [Theory]
    public void ResultHandler_SinglePredicate_Ok(Action<DummyGenerator> callback)
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

    private static void InvokeHandler<T>(DummyGenerator sut, Outcome<T> outcome, GeneratedValue expectedResult)
    {
        var args = new Args();
        sut.CreateHandler()!.Generate(outcome, args).AsTask().Result.Should().Be(expectedResult);
    }

    public sealed class DummyGenerator : OutcomeGenerator<GeneratedValue, Args, DummyGenerator>
    {
        protected override GeneratedValue DefaultValue => GeneratedValue.Default;

        protected override bool IsValid(GeneratedValue value) => value == GeneratedValue.Valid1 || value == GeneratedValue.Valid2;
    }

    public enum GeneratedValue
    {
        Default,
        Valid1,
        Valid2,
        Invalid
    }

    public class Args : IResilienceArguments
    {
        public Args() => Context = ResilienceContext.Get();

        public ResilienceContext Context { get; private set; }
    }
}
