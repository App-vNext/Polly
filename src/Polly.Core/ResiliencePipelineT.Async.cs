namespace Polly;

#pragma warning disable RS0027 // API with optional parameter(s) should have the most parameters amongst its public overloads

public partial class ResiliencePipeline<T>
{
    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
    public ValueTask<TResult> ExecuteAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<TResult>> callback,
        ResilienceContext context,
        TState state)
        where TResult : T
        => Pipeline.ExecuteAsync(callback, context, state);

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
    public ValueTask<TResult> ExecuteAsync<TResult>(
        Func<ResilienceContext, ValueTask<TResult>> callback,
        ResilienceContext context)
        where TResult : T
        => Pipeline.ExecuteAsync(callback, context);

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the callback.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public ValueTask<TResult> ExecuteAsync<TResult, TState>(
        Func<TState, CancellationToken, ValueTask<TResult>> callback,
        TState state,
        CancellationToken cancellationToken = default)
        where TResult : T
        => Pipeline.ExecuteAsync(callback, state, cancellationToken);

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the callback.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> is <see langword="null"/>.</exception>
    public ValueTask<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, ValueTask<TResult>> callback,
        CancellationToken cancellationToken = default)
        where TResult : T
        => Pipeline.ExecuteAsync(callback, cancellationToken);

    /// <summary>
    /// Executes the specified outcome-based callback.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents the asynchronous execution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="callback"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para><strong>Important:</strong> This method targets advanced, low-allocation scenarios. The user callback
    /// must not throw an exception. Wrap your code and return <see cref="Outcome{TResult}"/>:
    /// use <see cref="Outcome.FromResult{TResult}(TResult)"/> on success, or <see cref="Outcome.FromException{TResult}(Exception)"/> on failure.
    /// Do not rely on strategies to catch your exceptions; any such behavior is an implementation detail and
    /// is not guaranteed across strategies or future versions.</para>
    /// </remarks>
    public ValueTask<Outcome<TResult>> ExecuteOutcomeAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
        where TResult : T
        => Pipeline.ExecuteOutcomeAsync(callback, context, state);
}
