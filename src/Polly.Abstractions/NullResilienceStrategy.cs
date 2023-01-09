namespace Polly;

public sealed class NullResilienceStrategy : IResilienceStrategy
{
    public static readonly NullResilienceStrategy Instance = new();

    private NullResilienceStrategy()
    {
    }

    public ValueTask<T> ExecuteAsync<T, TState>(Func<ResilienceContext, TState, ValueTask<T>> execution, ResilienceContext context, TState state) => execution(context, state);
}
