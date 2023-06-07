namespace Polly.Utils;

internal abstract class PredicateInvoker<TArgs>
{
    public static PredicateInvoker<TArgs>? Create<TResult>(Func<OutcomeArguments<TResult, TArgs>, ValueTask<bool>>? predicate, bool isGeneric) => predicate switch
    {
        Func<OutcomeArguments<object, TArgs>, ValueTask<bool>> objectPredicate when !isGeneric => new NonGenericPredicateInvoker(objectPredicate),
        { } => new GenericPredicateInvoker<TResult>(predicate),
        _ => null,
    };

    public abstract ValueTask<bool> HandleAsync<TResult>(OutcomeArguments<TResult, TArgs> args);

    private sealed class NonGenericPredicateInvoker : PredicateInvoker<TArgs>
    {
        private readonly Func<OutcomeArguments<object, TArgs>, ValueTask<bool>> _predicate;

        public NonGenericPredicateInvoker(Func<OutcomeArguments<object, TArgs>, ValueTask<bool>> predicate) => _predicate = predicate;

        public override ValueTask<bool> HandleAsync<TResult>(OutcomeArguments<TResult, TArgs> args) => _predicate(args.AsObjectArguments());
    }

    private sealed class GenericPredicateInvoker<T> : PredicateInvoker<TArgs>
    {
        private readonly object _predicate;

        public GenericPredicateInvoker(Func<OutcomeArguments<T, TArgs>, ValueTask<bool>> predicate) => _predicate = predicate;

        public override ValueTask<bool> HandleAsync<TResult>(OutcomeArguments<TResult, TArgs> args)
        {
            if (typeof(TResult) == typeof(T))
            {
                return ((Func<OutcomeArguments<TResult, TArgs>, ValueTask<bool>>)_predicate)(args);
            }

            return PredicateResult.False;
        }
    }
}
