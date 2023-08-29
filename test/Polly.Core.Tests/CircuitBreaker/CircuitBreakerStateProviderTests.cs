using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class CircuitBreakerStateProviderTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var provider = new CircuitBreakerStateProvider();

        provider.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public void NotInitialized_EnsureDefaults()
    {
        var provider = new CircuitBreakerStateProvider();

        provider.CircuitState.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public async Task ResetAsync_NotInitialized_Throws()
    {
        var control = new CircuitBreakerManualControl();

        await control
            .Invoking(c => c.CloseAsync(CancellationToken.None))
            .Should()
            .NotThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void Initialize_Twice_Throws()
    {
        var provider = new CircuitBreakerStateProvider();
        provider.Initialize(() => CircuitState.Closed);

        provider
            .Invoking(c => c.Initialize(() => CircuitState.Closed))
            .Should()
            .Throw<InvalidOperationException>();
    }

    [Fact]
    public void Initialize_Ok()
    {
        var provider = new CircuitBreakerStateProvider();
        var stateCalled = false;

        provider.Initialize(
            () =>
            {
                stateCalled = true;
                return CircuitState.HalfOpen;
            });

        provider.CircuitState.Should().Be(CircuitState.HalfOpen);

        stateCalled.Should().BeTrue();
    }
}
