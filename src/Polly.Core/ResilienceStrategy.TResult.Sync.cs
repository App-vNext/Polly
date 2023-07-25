namespace Polly;

#pragma warning disable RS0027 // API with optional parameter(s) should have the most parameters amongst its public overloads

public partial class ResilienceStrategy<TResult>
{
    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <returns>An instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
    public T Execute<T, TState>(
        Func<ResilienceContext, TState, T> callback,
        ResilienceContext context,
        TState state)
        where T : TResult
    {
        Guard.NotNull(callback);
        Guard.NotNull(context);

        return Strategy.Execute(callback, context, state);
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <returns>An instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
    public T Execute<T>(
        Func<ResilienceContext, T> callback,
        ResilienceContext context)
        where T : TResult
    {
        Guard.NotNull(callback);
        Guard.NotNull(context);

        return Strategy.Execute(callback, context);
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the callback.</param>
    /// <returns>An instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public T Execute<T>(
        Func<CancellationToken, T> callback,
        CancellationToken cancellationToken = default)
        where T : TResult
    {
        Guard.NotNull(callback);

        return Strategy.Execute(callback, cancellationToken);
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <returns>An instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public T Execute<T>(Func<T> callback)
        where T : TResult
    {
        Guard.NotNull(callback);

        return Strategy.Execute(callback);
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <returns>An instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public T Execute<T, TState>(Func<TState, T> callback, TState state)
        where T : TResult
    {
        Guard.NotNull(callback);

        return Strategy.Execute(callback, state);
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the callback.</param>
    /// <returns>An instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public T Execute<T, TState>(
        Func<TState, CancellationToken, T> callback,
        TState state,
        CancellationToken cancellationToken = default)
        where T : TResult
    {
        Guard.NotNull(callback);

        return Strategy.Execute(callback, state, cancellationToken);
    }
}
