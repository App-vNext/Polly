using Microsoft.Extensions.DependencyInjection;
using Polly.CircuitBreaker;
using Polly.Extensions.DependencyInjection;
using Polly.Registry;

namespace Polly.Extensions.Tests.Issues;

public partial class IssuesTests
{
    [Fact]
    public async Task OnCircuitBreakWithServiceProvider_796()
    {
        var contextChecked = false;

        // create the strategy
        var serviceCollection = new ServiceCollection().AddResilienceStrategy("my-strategy", (builder, context) =>
        {
            builder
                .AddStrategy(new ServiceProviderStrategy(context.ServiceProvider))
                .AddAdvancedCircuitBreaker(new AdvancedCircuitBreakerStrategyOptions
                {
                    FailureThreshold = 1,
                    MinimumThroughput = 10,
                    OnOpened = async args =>
                    {
                        args.Context.Properties.GetValue(PollyDependencyInjectionKeys.ServiceProvider, null!).Should().NotBeNull();
                        contextChecked = true;

                        // do asynchronous call
                        await Task.Yield();
                    },
                    ShouldHandle = args => args.Result switch
                    {
                        string result when result == "error" => PredicateResult.True,
                        _ => PredicateResult.False
                    }
                });
        });

        // retrieve the provider
        var strategyProvider = serviceCollection.BuildServiceProvider().GetRequiredService<ResilienceStrategyProvider<string>>();
        var strategy = strategyProvider.GetStrategy("my-strategy");

        // now trigger the circuit breaker by evaluating multiple result types
        for (int i = 0; i < 10; i++)
        {
            await strategy.ExecuteAsync(_ => new ValueTask<string>("error"));
        }

        // now the circuit breaker should be open
        await strategy.Invoking(s => s.ExecuteAsync(_ => new ValueTask<string>("valid-result")).AsTask()).Should().ThrowAsync<BrokenCircuitException>();

        // check that service provider was received in the context
        contextChecked.Should().BeTrue();
    }

    private class ServiceProviderStrategy : ResilienceStrategy
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderStrategy(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        protected override ValueTask<Outcome<TResult>> ExecuteCoreAsync<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state)
        {
            context.Properties.Set(PollyDependencyInjectionKeys.ServiceProvider, _serviceProvider);
            return callback(context, state);
        }
    }
}
