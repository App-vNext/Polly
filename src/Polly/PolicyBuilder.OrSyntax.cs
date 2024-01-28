namespace Polly;

public partial class PolicyBuilder
{
    #region Add exception predicates to exception-filtering policy

    /// <summary>
    /// Specifies the type of exception that this policy can handle.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to handle.</typeparam>
    /// <returns>The PolicyBuilder instance.</returns>
    public PolicyBuilder Or<TException>()
        where TException : Exception
    {
        ExceptionPredicates.Add(exception => exception is TException ? exception : null);
        return this;
    }

    /// <summary>
    /// Specifies the type of exception that this policy can handle with additional filters on this exception type.
    /// </summary>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
    /// <returns>The PolicyBuilder instance.</returns>
    public PolicyBuilder Or<TException>(Func<TException, bool> exceptionPredicate)
        where TException : Exception
    {
        ExceptionPredicates.Add(exception => exception is TException texception && exceptionPredicate(texception) ? exception : null);
        return this;
    }

    /// <summary>
    /// Specifies the type of exception that this policy can handle if found as an InnerException of a regular <see cref="Exception"/>, or at any level of nesting within an <see cref="AggregateException"/>.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to handle.</typeparam>
    /// <returns>The PolicyBuilder instance, for fluent chaining.</returns>
    public PolicyBuilder OrInner<TException>()
        where TException : Exception
    {
        ExceptionPredicates.Add(HandleInner(ex => ex is TException));
        return this;
    }

    /// <summary>
    /// Specifies the type of exception that this policy can handle, with additional filters on this exception type, if found as an InnerException of a regular <see cref="Exception"/>, or at any level of nesting within an <see cref="AggregateException"/>.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to handle.</typeparam>
    /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
    /// <returns>The PolicyBuilder instance, for fluent chaining.</returns>
    public PolicyBuilder OrInner<TException>(Func<TException, bool> exceptionPredicate)
        where TException : Exception
    {
        ExceptionPredicates.Add(HandleInner(exception => exception is TException innerEx && exceptionPredicate(innerEx)));
        return this;
    }

    internal static ExceptionPredicate HandleInner(Func<Exception, bool> predicate) =>
        exception =>
        {
            if (exception is AggregateException aggregateException)
            {
                // search all inner exceptions wrapped inside the AggregateException recursively
                foreach (var innerException in aggregateException.Flatten().InnerExceptions)
                {
                    var matchedInAggregate = HandleInnerNested(predicate, innerException);
                    if (matchedInAggregate != null)
                        return matchedInAggregate;
                }
            }

            return HandleInnerNested(predicate, exception);
        };

    private static Exception HandleInnerNested(Func<Exception, bool> predicate, Exception current)
    {
        if (current == null) return null;
        if (predicate(current)) return current;
        return HandleInnerNested(predicate, current.InnerException);
    }

    #endregion

    #region Add result predicates to exception-filtering policy

    /// <summary>
    /// Specifies the type of result that this policy can handle with additional filters on the result.
    /// </summary>
    /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
    /// <param name="resultPredicate">The predicate to filter the results this policy will handle.</param>
    /// <returns>The PolicyBuilder instance.</returns>
    public PolicyBuilder<TResult> OrResult<TResult>(Func<TResult, bool> resultPredicate) =>
        new PolicyBuilder<TResult>(ExceptionPredicates).OrResult(resultPredicate);

    /// <summary>
    /// Specifies a result value which the policy will handle.
    /// </summary>
    /// <typeparam name="TResult">The type of return values this policy will handle.</typeparam>
    /// <param name="result">The TResult value this policy will handle.</param>
    /// <remarks>This policy filter matches the <paramref name="result"/> value returned using .Equals(), ideally suited for value types such as int and enum.  To match characteristics of class return types, consider the overload taking a result predicate.</remarks>
    /// <returns>The PolicyBuilder instance.</returns>
    public PolicyBuilder<TResult> OrResult<TResult>(TResult result) =>
        OrResult<TResult>(r => (r != null && r.Equals(result)) || (r == null && result == null));

    #endregion
}

public partial class PolicyBuilder<TResult>
{
    #region Add result predicates to result-filtering policy

    /// <summary>
    /// Specifies the type of result that this policy can handle with additional filters on the result.
    /// </summary>
    /// <param name="resultPredicate">The predicate to filter the results this policy will handle.</param>
    /// <returns>The PolicyBuilder instance.</returns>
    public PolicyBuilder<TResult> OrResult(Func<TResult, bool> resultPredicate)
    {
        ResultPredicate<TResult> predicate = result => resultPredicate(result);
        ResultPredicates.Add(predicate);
        return this;
    }

    /// <summary>
    /// Specifies a result value which the policy will handle.
    /// </summary>
    /// <param name="result">The TResult value this policy will handle.</param>
    /// <remarks>This policy filter matches the <paramref name="result"/> value returned using .Equals(), ideally suited for value types such as int and enum.  To match characteristics of class return types, consider the overload taking a result predicate.</remarks>
    /// <returns>The PolicyBuilder instance.</returns>
    public PolicyBuilder<TResult> OrResult(TResult result) =>
        OrResult(r => (r != null && r.Equals(result)) || (r == null && result == null));

    #endregion

    #region Add exception predicates to result-filtering policy

    /// <summary>
    /// Specifies the type of exception that this policy can handle.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to handle.</typeparam>
    /// <returns>The PolicyBuilder instance.</returns>
    public PolicyBuilder<TResult> Or<TException>()
        where TException : Exception
    {
        ExceptionPredicates.Add(exception => exception is TException ? exception : null);
        return this;
    }

    /// <summary>
    /// Specifies the type of exception that this policy can handle with additional filters on this exception type.
    /// </summary>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
    /// <returns>The PolicyBuilder instance.</returns>
    public PolicyBuilder<TResult> Or<TException>(Func<TException, bool> exceptionPredicate)
        where TException : Exception
    {
        ExceptionPredicates.Add(exception => exception is TException texception && exceptionPredicate(texception) ? exception : null);
        return this;
    }

    /// <summary>
    /// Specifies the type of exception that this policy can handle if found as an InnerException of a regular <see cref="Exception"/>, or at any level of nesting within an <see cref="AggregateException"/>.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to handle.</typeparam>
    /// <returns>The PolicyBuilder instance, for fluent chaining.</returns>
    public PolicyBuilder<TResult> OrInner<TException>()
        where TException : Exception
    {
        ExceptionPredicates.Add(PolicyBuilder.HandleInner(ex => ex is TException));
        return this;
    }

    /// <summary>
    /// Specifies the type of exception that this policy can handle, with additional filters on this exception type, if found as an InnerException of a regular <see cref="Exception"/>, or at any level of nesting within an <see cref="AggregateException"/>.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to handle.</typeparam>
    /// <param name="exceptionPredicate">The exception predicate to filter the type of exception this policy can handle.</param>
    /// <returns>The PolicyBuilder instance, for fluent chaining.</returns>
    public PolicyBuilder<TResult> OrInner<TException>(Func<TException, bool> exceptionPredicate)
        where TException : Exception
    {
        ExceptionPredicates.Add(PolicyBuilder.HandleInner(ex => ex is TException texception && exceptionPredicate(texception)));
        return this;
    }

    #endregion
}
