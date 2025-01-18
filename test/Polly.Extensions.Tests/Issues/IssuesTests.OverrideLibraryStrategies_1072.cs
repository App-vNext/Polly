using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly.Registry;

namespace Polly.Extensions.Tests.Issues;

public partial class IssuesTests
{
    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void OverrideLibraryStrategies_1072(bool overrideStrategy)
    {
        // arrange
        var services = new ServiceCollection();
        var failFirstCall = true;

        if (overrideStrategy)
        {
            // This call overrides the pipeline that the library uses. The last call to AddResiliencePipeline wins.
            services.AddResiliencePipeline("library-pipeline", builder => builder.AddRetry(new()
            {
                ShouldHandle = args => args.Outcome.Exception switch
                {
                    InvalidOperationException => PredicateResult.True(),
                    SocketException => PredicateResult.True(),
                    _ => PredicateResult.False()
                },
                Delay = TimeSpan.Zero
            }));
        }

        AddLibraryServices(services);

        var serviceProvider = services.BuildServiceProvider();
        var api = serviceProvider.GetRequiredService<LibraryApi>();

        // act && assert
        if (overrideStrategy)
        {
            // The library now also handles SocketException.
            Should.NotThrow(() => api.ExecuteLibrary(UnstableCall));
        }
        else
        {
            // Originally, the library pipeline only handled InvalidOperationException.
            Should.Throw<SocketException>(() => api.ExecuteLibrary(UnstableCall));
        }

        void UnstableCall()
        {
            if (failFirstCall)
            {
                failFirstCall = false;

                // This exception was not originally handled by pipeline.
                throw new SocketException();
            }
        }
    }

    private static void AddLibraryServices(IServiceCollection services)
    {
        services.TryAddSingleton<LibraryApi>();
        services.AddResiliencePipeline("library-pipeline", builder => builder.AddRetry(new()
        {
            ShouldHandle = args => args.Outcome.Exception switch
            {
                InvalidOperationException => PredicateResult.True(),
                _ => PredicateResult.False()
            }
        }));
    }

    public class LibraryApi(ResiliencePipelineProvider<string> provider)
    {
        private readonly ResiliencePipeline _pipeline = provider.GetPipeline("library-pipeline");

        public void ExecuteLibrary(Action execute) => _pipeline.Execute(execute);
    }
}
