namespace Polly.TestUtils;

public class TestResilienceStrategy : ResilienceStrategy
{
    public Action<ResilienceContext, object?>? Before { get; set; }

    public Action<object?, Exception?>? After { get; set; }

    public Func<ResilienceContext, object?, Task>? OnExecute { get; set; }

    protected internal override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        if (callback is null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        return ExecuteCoreInternal(callback, context, state);
    }

    private async ValueTask<Outcome<TResult>> ExecuteCoreInternal<TResult, TState>(
        Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
        ResilienceContext context,
        TState state)
    {
        Before?.Invoke(context, state);

        try
        {
            if (OnExecute != null)
            {
                await OnExecute(context, state).ConfigureAwait(false);
            }

            var result = await callback(context, state).ConfigureAwait(false);

            After?.Invoke(result, null);

            return result;
        }
        catch (Exception e)
        {
            After?.Invoke(null, e);

            throw;
        }
    }
}
