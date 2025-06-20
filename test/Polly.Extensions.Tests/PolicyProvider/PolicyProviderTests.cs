using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly.PolicyProvider;

namespace Polly.Extensions.Tests.PolicyProvider;

public class PolicyProviderTests
{
    [Fact]
    public void AddPolicyProvider_WithoutConfiguration_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPolicyProvider();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var policyProvider = serviceProvider.GetService<IPolicyProvider>();
        
        policyProvider.Should().NotBeNull();
        policyProvider.Should().BeOfType<DefaultPolicyProvider>();
    }

    [Fact]
    public void AddPolicyProvider_WithConfiguration_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPolicyProvider(options =>
        {
            options.HttpClientRetryAttempts = 5;
            options.HttpClientTimeout = TimeSpan.FromSeconds(20);
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var policyProvider = serviceProvider.GetService<IPolicyProvider>();
        var options = serviceProvider.GetService<IOptions<PolicyProviderOptions>>();
        
        policyProvider.Should().NotBeNull();
        options.Should().NotBeNull();
        options!.Value.HttpClientRetryAttempts.Should().Be(5);
        options.Value.HttpClientTimeout.Should().Be(TimeSpan.FromSeconds(20));
    }

    [Fact]
    public void GetPolicy_HttpClientPolicy_ShouldReturnResiliencePipeline()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPolicyProvider();
        var serviceProvider = services.BuildServiceProvider();
        var policyProvider = serviceProvider.GetRequiredService<IPolicyProvider>();

        // Act
        var policy = policyProvider.GetPolicy(PolicyType.HttpClient);

        // Assert
        policy.Should().NotBeNull();
    }

    [Fact]
    public void GetPolicy_UnsupportedPolicyType_ShouldThrowArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPolicyProvider();
        var serviceProvider = services.BuildServiceProvider();
        var policyProvider = serviceProvider.GetRequiredService<IPolicyProvider>();

        // Act & Assert
        var act = () => policyProvider.GetPolicy((PolicyType)999);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetPolicy_CalledMultipleTimes_ShouldReturnSameInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPolicyProvider();
        var serviceProvider = services.BuildServiceProvider();
        var policyProvider = serviceProvider.GetRequiredService<IPolicyProvider>();

        // Act
        var policy1 = policyProvider.GetPolicy(PolicyType.HttpClient);
        var policy2 = policyProvider.GetPolicy(PolicyType.HttpClient);

        // Assert
        policy1.Should().BeSameAs(policy2);
    }

    [Fact]
    public void PolicyProvider_WhenDisposed_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddPolicyProvider();
        var serviceProvider = services.BuildServiceProvider();
        var policyProvider = (DefaultPolicyProvider)serviceProvider.GetRequiredService<IPolicyProvider>();

        // Act
        policyProvider.Dispose();

        // Assert
        var act = () => policyProvider.GetPolicy(PolicyType.HttpClient);
        act.Should().Throw<ObjectDisposedException>();
    }
}