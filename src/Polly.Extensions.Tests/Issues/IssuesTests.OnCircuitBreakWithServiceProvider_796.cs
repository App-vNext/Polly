using Microsoft.Extensions.DependencyInjection;
using Polly.CircuitBreaker;
using Polly.Extensions.DependencyInjection;
using Polly.Registry;

namespace Polly.Core.Tests.Issues;

public partial class IssuesTests
{
    [Fact]
    public async Task OnCircuitBreakWithServiceProvider_796()
    {
        var contextChecked = false;
        var options = new AdvancedCircuitBreakerStrategyOptions
        {
            FailureThreshold = 1,
            MinimumThroughput = 10,
        };

        options.OnOpened.Register(async (_, args) =>
        {
            args.Context.Properties.GetValue(PollyDependencyInjectionKeys.ServiceProvider, null!).Should().NotBeNull();
            contextChecked = true;

            // do asynchronous call
            await Task.Yield();
        });

        // handle string results
        options.ShouldHandle.HandleResult("error");

        // create the strategy
        var serviceCollection = new ServiceCollection().AddResilienceStrategy("my-strategy", (builder, context) =>
        {
            builder
                .AddStrategy(new ServiceProviderStrategy(context.ServiceProvider))
                .AddAdvancedCircuitBreaker(options);
        });

        // retrieve the provider
        var strategyProvider = serviceCollection.BuildServiceProvider().GetRequiredService<ResilienceStrategyProvider<string>>();
        var strategy = strategyProvider.Get("my-strategy");

        // now trigger the circuit breaker by evaluating multiple result types
        for (int i = 0; i < 10; i++)
        {
            await strategy.ExecuteAsync(_ => Task.FromResult("error"));
        }

        // now the circuit breaker should be open
        await strategy.Invoking(s => s.ExecuteAsync(_ => Task.FromResult("valid-result"))).Should().ThrowAsync<BrokenCircuitException>();

        // check that service provider was received in the context
        contextChecked.Should().BeTrue();
    }

    private class ServiceProviderStrategy : ResilienceStrategy
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderStrategy(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        protected override ValueTask<TResult> ExecuteCoreAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
        {
            context.Properties.Set(PollyDependencyInjectionKeys.ServiceProvider, _serviceProvider);
            return callback(context, state);
        }
    }
}
