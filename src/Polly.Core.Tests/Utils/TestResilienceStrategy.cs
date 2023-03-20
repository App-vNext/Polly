namespace Polly.Core.Tests.Utils;

public class TestResilienceStrategy : ResilienceStrategy
{
    public Action<ResilienceContext, object?>? Before { get; set; }

    public Action<object?, Exception?>? After { get; set; }

    public Func<ResilienceContext, object?, Task>? OnExecute { get; set; }

    protected internal override async ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
    {
        Before?.Invoke(context, state);

        try
        {
            if (OnExecute != null)
            {
                await OnExecute(context, state);
            }

            var result = await callback(context, state);

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
