using System.Threading.Tasks;

namespace Polly.Strategy;

internal abstract class GeneratorInvoker<TArgs, TValue>
{
    public static GeneratorInvoker<TArgs, TValue>? Create<TResult>(
        Func<OutcomeArguments<TResult, TArgs>, ValueTask<TValue>>? generator,
        TValue defaultValue,
        bool isGeneric) => generator switch
        {
            Func<OutcomeArguments<object, TArgs>, ValueTask<TValue>> objectGenerator when !isGeneric => new NonGenericGeneratorInvoker(objectGenerator),
            { } => new GenericGeneratorInvoker<TResult>(generator, defaultValue),
            _ => null
        };

    public abstract ValueTask<TValue> HandleAsync<TResult>(OutcomeArguments<TResult, TArgs> args);

    private sealed class NonGenericGeneratorInvoker : GeneratorInvoker<TArgs, TValue>
    {
        private readonly Func<OutcomeArguments<object, TArgs>, ValueTask<TValue>> _generator;

        public NonGenericGeneratorInvoker(Func<OutcomeArguments<object, TArgs>, ValueTask<TValue>> generator) => _generator = generator;

        public override ValueTask<TValue> HandleAsync<TResult>(OutcomeArguments<TResult, TArgs> args) => _generator(args.AsObjectArguments());
    }

    private sealed class GenericGeneratorInvoker<T> : GeneratorInvoker<TArgs, TValue>
    {
        private readonly object _generator;
        private readonly TValue _defaultValue;

        public GenericGeneratorInvoker(Func<OutcomeArguments<T, TArgs>, ValueTask<TValue>> generator, TValue defaultValue)
        {
            _generator = generator;
            _defaultValue = defaultValue;
        }

        public override ValueTask<TValue> HandleAsync<TResult>(OutcomeArguments<TResult, TArgs> args)
        {
            if (typeof(TResult) == typeof(T))
            {
                return ((Func<OutcomeArguments<TResult, TArgs>, ValueTask<TValue>>)_generator)(args);
            }

            return new(_defaultValue);
        }
    }
}
