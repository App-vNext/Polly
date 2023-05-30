using System.Threading.Tasks;

namespace Polly.Strategy;

internal abstract class GeneratorInvoker<TArgs, TValue>
    where TArgs : IResilienceArguments
{
    public static GeneratorInvoker<TArgs, TValue>? Create<TResult>(
        Func<Outcome<TResult>, TArgs, ValueTask<TValue>>? generator,
        TValue defaultValue,
        bool isGeneric) => generator switch
        {
            null => null,
            Func<Outcome<object>, TArgs, ValueTask<TValue>> objectGenerator when !isGeneric => new NonGenericGeneratorInvoker(objectGenerator),
            _ => new GenericGeneratorInvoker<TResult>(generator, defaultValue)
        };

    public abstract ValueTask<TValue> HandleAsync<TResult>(Outcome<TResult> outcome, TArgs args);

    private sealed class NonGenericGeneratorInvoker : GeneratorInvoker<TArgs, TValue>
    {
        private readonly Func<Outcome<object>, TArgs, ValueTask<TValue>> _generator;

        public NonGenericGeneratorInvoker(Func<Outcome<object>, TArgs, ValueTask<TValue>> generator) => _generator = generator;

        public override ValueTask<TValue> HandleAsync<TResult>(Outcome<TResult> outcome, TArgs args) => _generator(outcome.AsOutcome(), args);
    }

    private sealed class GenericGeneratorInvoker<T> : GeneratorInvoker<TArgs, TValue>
    {
        private readonly object _generator;
        private readonly TValue _defaultValue;

        public GenericGeneratorInvoker(Func<Outcome<T>, TArgs, ValueTask<TValue>> generator, TValue defaultValue)
        {
            _generator = generator;
            _defaultValue = defaultValue;
        }

        public override ValueTask<TValue> HandleAsync<TResult>(Outcome<TResult> outcome, TArgs args)
        {
            if (typeof(TResult) == typeof(T))
            {
                return ((Func<Outcome<TResult>, TArgs, ValueTask<TValue>>)_generator)(outcome, args);
            }

            return new(_defaultValue);
        }
    }
}
