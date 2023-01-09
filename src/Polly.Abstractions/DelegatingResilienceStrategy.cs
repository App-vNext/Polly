using Resilience;

namespace Polly;

public abstract class DelegatingResilienceStrategy : IResilienceStrategy
{
    public IResilienceStrategy Next { get; set; } = NullResilienceStrategy.Instance;

    public virtual ValueTask<T> ExecuteAsync<T, TState>(Func<ResilienceContext, TState, ValueTask<T>> execution, ResilienceContext context, TState state)
    {
        return Next.ExecuteAsync(execution, context, state);
    }
}
