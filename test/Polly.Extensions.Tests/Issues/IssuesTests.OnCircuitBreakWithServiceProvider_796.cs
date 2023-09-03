using Microsoft.Extensions.DependencyInjection;
using Polly.CircuitBreaker;
using Polly.DependencyInjection;
using Polly.Registry;

namespace Polly.Extensions.Tests.Issues;

public partial class IssuesTests
{
    private static readonly ResiliencePropertyKey<IServiceProvider> ServiceProviderKey = new("ServiceProvider");

    [Fact]
    public async Task OnCircuitBreakWithServiceProvider_796()
    {
        var contextChecked = false;

        // create the pipeline
        var serviceCollection = new ServiceCollection().AddResiliencePipeline("my-pipeline", (builder, context) =>
        {
            builder
                .AddStrategy(new ServiceProviderStrategy(context.ServiceProvider))
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 1,
                    MinimumThroughput = 10,
                    OnOpened = async args =>
                    {
                        args.Context.Properties.GetValue(ServiceProviderKey, null!).Should().NotBeNull();
                        contextChecked = true;

                        // do asynchronous call
                        await Task.Yield();
                    },
                    ShouldHandle = args => args.Outcome.Result switch
                    {
                        string result when result == "error" => PredicateResult.True(),
                        _ => PredicateResult.False()
                    }
                });
        });

        // retrieve the provider
        var pipelineProvider = serviceCollection.BuildServiceProvider().GetRequiredService<ResiliencePipelineProvider<string>>();
        var pipeline = pipelineProvider.GetPipeline("my-pipeline");

        // now trigger the circuit breaker by evaluating multiple result types
        for (int i = 0; i < 10; i++)
        {
            await pipeline.ExecuteAsync(_ => new ValueTask<string>("error"));
        }

        // now the circuit breaker should be open
        await pipeline.Invoking(s => s.ExecuteAsync(_ => new ValueTask<string>("valid-result")).AsTask()).Should().ThrowAsync<BrokenCircuitException>();

        // check that service provider was received in the context
        contextChecked.Should().BeTrue();
    }

    private class ServiceProviderStrategy : ResilienceStrategy
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderStrategy(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        protected override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state)
        {
            context.Properties.Set(ServiceProviderKey, _serviceProvider);
            return callback(context, state);
        }
    }
}
