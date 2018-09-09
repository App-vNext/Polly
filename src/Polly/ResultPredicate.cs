namespace Polly
{
    /// <summary>
    /// A predicate that can be run against a passed result value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <param name="result">The passed result, against which to evaluate the predicate.</param>
    /// <typeparam name="TResult">The type of results which this predicate can evaluate.</typeparam>
    /// <returns>True if the passed <paramref name="result"/> matched the predicate; otherwise, false.</returns>

    public delegate bool ResultPredicate<in TResult>(TResult result);
}