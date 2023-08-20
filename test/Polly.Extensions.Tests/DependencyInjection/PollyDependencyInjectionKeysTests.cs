using Polly.DependencyInjection;

namespace Polly.Extensions.Tests.DependencyInjection;

public class PollyDependencyInjectionKeysTests
{
    [Fact]
    public void ServiceProvider_Ok()
    {
        PollyDependencyInjectionKeys.ServiceProvider.Key.Should().Be("Polly.DependencyInjection.ServiceProvider");
    }
}
