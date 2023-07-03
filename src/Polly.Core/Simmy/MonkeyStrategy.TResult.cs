namespace Polly.Simmy;

/// <summary>
/// Contains common functionality for chaos strategies which intentionally disrupt executions - which monkey around with calls.
/// </summary>
/// <typeparam name="T">The type of result this strategy supports.</typeparam>
public abstract class MonkeyStrategy<T> : MonkeyStrategy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MonkeyStrategy{TResult}"/> class.
    /// </summary>
    /// <param name="options">The chaos strategy options.</param>
    protected MonkeyStrategy(MonkeyStrategyOptions options)
        : base(options)
    {
    }

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <typeparam name="TState">The type of state associated with the callback.</typeparam>
    /// <typeparam name="T">The type of result returned by the callback.</typeparam>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="context">The context associated with the callback.</param>
    /// <param name="state">The state associated with the callback.</param>
    /// <returns>An instance of <see cref="ValueTask"/> that represents an asynchronous callback.</returns>
    /// <remarks>
    /// This method is called by various methods exposed on <see cref="ResilienceStrategy"/>. These methods make sure that
    /// <paramref name="context"/> is properly initialized with details about the execution mode.
    /// <para>
    /// The provided callback never throws an exception. Instead, the exception is captured and converted to an <see cref="Outcome{TResult}"/>.
    /// </para>
    /// <para>
    /// Do not throw exceptions from your strategy implementation. Instead, return an exception instance as <see cref="Outcome{TResult}"/>.
    /// </para>
    /// </remarks>
    protected internal abstract ValueTask<Outcome<T>> ExecuteCoreAsync<TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<T>>> callback,
        ResilienceContext context,
        TState state);

    /// <inheritdoc/>
    protected internal sealed override ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        Guard.NotNull(callback);

        if (typeof(TResult) != typeof(T))
        {
            return callback(context, state);
        }

        // cast is safe here, because TResult and T are the same type
        var callbackCasted = (Func<ResilienceContext, TState, ValueTask<Outcome<T>>>)(object)callback;
        var valueTask = ExecuteCoreAsync(callbackCasted, context, state);

        return ConvertValueTask<TResult>(valueTask, context);
    }

    // TODO: Consider abstract this out as an utility? it's also being used in OutcomeResilienceStrategy
    private static ValueTask<Outcome<TResult>> ConvertValueTask<TResult>(ValueTask<Outcome<T>> valueTask, ResilienceContext resilienceContext)
    {
        if (valueTask.IsCompletedSuccessfully)
        {
            return new ValueTask<Outcome<TResult>>(valueTask.Result.AsOutcome<TResult>());
        }

        return ConvertValueTaskAsync(valueTask, resilienceContext);

        static async ValueTask<Outcome<TResult>> ConvertValueTaskAsync(ValueTask<Outcome<T>> valueTask, ResilienceContext resilienceContext)
        {
            var outcome = await valueTask.ConfigureAwait(resilienceContext.ContinueOnCapturedContext);
            return outcome.AsOutcome<TResult>();
        }
    }
}
