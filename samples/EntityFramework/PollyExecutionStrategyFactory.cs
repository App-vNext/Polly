using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;

public class PollyExecutionStrategyFactory : IExecutionStrategyFactory
{
    private readonly ExecutionStrategyDependencies dependencies;
    private readonly ResilienceStrategy resilienceStrategy;


    public PollyExecutionStrategyFactory(ExecutionStrategyDependencies dependencies, ILoggerFactory loggerFactory)
    {
        this.dependencies = dependencies;
        resilienceStrategy = new ResilienceStrategyBuilder()
            .AddRetry(new()
            {
                BackoffType = Polly.Retry.RetryBackoffType.Constant,
                RetryCount = 3,
                ShouldHandle = args => args.Exception switch
                {
                    InvalidOperationException => PredicateResult.True,
                    TimeoutRejectedException => PredicateResult.True,
                    _ => PredicateResult.False
                }
            })
            .AddTimeout(TimeSpan.FromSeconds(1))
            .ConfigureTelemetry(loggerFactory)
            .Build();
    }

    public IExecutionStrategy Create() => new PollyExecutionStrategy(dependencies, resilienceStrategy);
}
