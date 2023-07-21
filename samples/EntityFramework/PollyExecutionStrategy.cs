using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Polly;

public class PollyExecutionStrategy : IExecutionStrategy
{
    private readonly ExecutionStrategyDependencies dependencies;
    private readonly ResilienceStrategy resilienceStrategy;

    public PollyExecutionStrategy(ExecutionStrategyDependencies dependencies, ResilienceStrategy resilienceStrategy)
    {
        this.dependencies = dependencies;
        this.resilienceStrategy = resilienceStrategy;
    }

    public bool RetriesOnFailure => true;

    public TResult Execute<TState, TResult>(
        TState state,
        Func<DbContext, TState, TResult> operation,
        Func<DbContext, TState, ExecutionResult<TResult>>? verifySucceeded)
        => resilienceStrategy.Execute(() => operation(dependencies.CurrentContext.Context, state));

    public async Task<TResult> ExecuteAsync<TState, TResult>(
        TState state,
        Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
        Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>>? verifySucceeded,
        CancellationToken cancellationToken = default)
        => await resilienceStrategy.ExecuteAsync(
            async (token) => await operation(dependencies.CurrentContext.Context, state, token),
            cancellationToken);
}
