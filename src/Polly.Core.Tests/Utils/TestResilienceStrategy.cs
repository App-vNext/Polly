namespace Polly.Core.Tests.Utils;

public class TestResilienceStrategy : DelegatingResilienceStrategy
{
    public Action<ResilienceContext, object?>? Before { get; set; }

    public Action<object?, Exception?>? After { get; set; }

    public Func<ResilienceContext, object?, Task>? OnExecute { get; set; }

    protected override async ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> execution, ResilienceContext context, TState state)
    {
        Before?.Invoke(context, state);

        try
        {
            if (OnExecute != null)
            {
                await OnExecute(context, state);
            }

            var result = await base.ExecuteCoreAsync(execution, context, state);

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
