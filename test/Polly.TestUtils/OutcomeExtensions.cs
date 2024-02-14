namespace Polly.TestUtils;

public static class OutcomeExtensions
{
    public static ValueTask<bool> ResultPredicateAsync<TResult>(this Outcome<object> outcome, TResult result) => new(outcome.ResultPredicate(result));

    public static ValueTask<bool> ResultPredicateAsync<TResult>(this Outcome<object> outcome, Predicate<TResult> predicate) => new(outcome.ResultPredicate(predicate));

    public static bool ResultPredicate<TResult>(this Outcome<object> outcome, TResult result)
        => outcome.ResultPredicate<TResult>(r => EqualityComparer<TResult>.Default.Equals(r, result));

    public static bool ResultPredicate<TResult>(this Outcome<object> outcome, Predicate<TResult> predicate)
    {
        if (outcome.TryGetResult(out var result) && result is TResult typedResult)
        {
            return predicate(typedResult);
        }

        return false;
    }

    public static ValueTask<bool> ExceptionPredicateAsync<TException>(this Outcome<object> outcome)
        where TException : Exception => new(outcome.ExceptionPredicate<TException>());

    public static ValueTask<bool> ExceptionPredicateAsync<TException>(this Outcome<object> outcome, Predicate<TException> predicate)
        where TException : Exception => new(outcome.ExceptionPredicate(predicate));

    public static bool ExceptionPredicate<TException>(this Outcome<object> outcome)
        where TException : Exception => outcome.ExceptionPredicate<TException>(_ => true);

    public static bool ExceptionPredicate<TException>(this Outcome<object> outcome, Predicate<TException> predicate)
        where TException : Exception => outcome.Exception is TException typedException && predicate(typedException);
}
