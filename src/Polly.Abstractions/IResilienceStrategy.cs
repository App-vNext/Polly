namespace Polly;

public interface IResilienceStrategy
{
    ValueTask<T> ExecuteAsync<T, TState>(Func<ResilienceContext, TState, ValueTask<T>> execution, ResilienceContext context, TState state);
}
