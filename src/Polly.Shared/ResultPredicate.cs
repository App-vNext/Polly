namespace Polly
{
    /// <summary>
    /// Predicate for validating the result value and optional value predicate
    /// </summary>
    /// <typeparam name="TResult">Type of result</typeparam>
    /// <param name="result">Result value to validate</param>
    /// <returns>True if valid</returns>
    public delegate bool ResultPredicate<TResult>(TResult result);
}