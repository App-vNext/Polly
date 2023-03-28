using Polly.DependencyInjection;

namespace Polly.Hosting.Tests;

public class PollyDependencyInjectionKeysTests
{
    [Fact]
    public void ServiceProvider_Ok()
    {
        PollyDependencyInjectionKeys.ServiceProvider.Key.Should().Be("Polly.DependencyInjection.ServiceProvider");
    }
}
