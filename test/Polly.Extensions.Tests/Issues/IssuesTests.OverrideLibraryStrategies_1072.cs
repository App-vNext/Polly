using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly.Registry;
using Polly.Retry;

namespace Polly.Extensions.Tests.Issues;

public partial class IssuesTests
{
    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void OverrideLibraryStrategies_898(bool overrideStrategy)
    {
        // arrange
        var services = new ServiceCollection();
        var failFirstCall = true;
        AddLibraryServices(services);

        if (overrideStrategy)
        {
            // This call overrides the strategy that the library uses. The last call to AddResilienceStrategy wins.
            services.AddResilienceStrategy("library-strategy", builder => builder.AddRetry(new()
            {
                ShouldHandle = args => args.Exception switch
                {
                    InvalidOperationException => PredicateResult.True,
                    SocketException => PredicateResult.True,
                    _ => PredicateResult.False
                },
                BaseDelay = TimeSpan.Zero
            }));
        }

        var serviceProvider = services.BuildServiceProvider();
        var api = serviceProvider.GetRequiredService<LibraryApi>();

        // act && assert
        if (overrideStrategy)
        {
            // The library now also handles SocketException.
            api.Invoking(a => a.ExecuteLibrary(UnstableCall)).Should().NotThrow();

        }
        else
        {
            // Originally, the library strategy only handled InvalidOperationException.
            api.Invoking(a => a.ExecuteLibrary(UnstableCall)).Should().Throw<SocketException>();
        }

        void UnstableCall()
        {
            if (failFirstCall)
            {
                failFirstCall = false;

                // This exception was not originally handled by strategy.
                throw new SocketException();
            }
        }
    }

    private static void AddLibraryServices(IServiceCollection services)
    {
        services.TryAddSingleton<LibraryApi>();
        services.AddResilienceStrategy("library-strategy", builder => builder.AddRetry(new()
        {
            ShouldHandle = args => args.Exception switch
            {
                InvalidOperationException => PredicateResult.True,
                _ => PredicateResult.False
            }
        }));
    }

    public class LibraryApi
    {
        private readonly ResilienceStrategy _strategy;

        public LibraryApi(ResilienceStrategyProvider<string> provider) => _strategy = provider.Get("library-strategy");

        public void ExecuteLibrary(Action execute) => _strategy.Execute(execute);
    }
}
