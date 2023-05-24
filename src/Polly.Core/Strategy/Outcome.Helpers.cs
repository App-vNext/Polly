namespace Polly.Strategy;

public readonly partial struct Outcome
{
    internal ValueTask<bool> ResultPredicateAsync<TResult>(TResult result) => new(ResultPredicate(result));

    internal ValueTask<bool> ResultPredicateAsync<TResult>(Predicate<TResult> predicate) => new(ResultPredicate(predicate));

    internal bool ResultPredicate<TResult>(TResult result)
    {
        return ResultPredicate<TResult>(r => EqualityComparer<TResult>.Default.Equals(r, result));
    }

    internal bool ResultPredicate<TResult>(Predicate<TResult> predicate)
    {
        if (TryGetResult(out var result) && result is TResult typedResult)
        {
            return predicate(typedResult);
        }

        return false;
    }

    internal ValueTask<bool> ExceptionPredicateAsync<TException>()
        where TException : Exception => new(ExceptionPredicate<TException>());

    internal ValueTask<bool> ExceptionPredicateAsync<TException>(Predicate<TException> predicate)
        where TException : Exception => new(ExceptionPredicate(predicate));

    internal bool ExceptionPredicate<TException>()
        where TException : Exception => ExceptionPredicate<TException>(_ => true);

    internal bool ExceptionPredicate<TException>(Predicate<TException> predicate)
        where TException : Exception => Exception is TException typedException && predicate(typedException);
}
