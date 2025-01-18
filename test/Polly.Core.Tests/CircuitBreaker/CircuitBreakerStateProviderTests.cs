using Polly.CircuitBreaker;

namespace Polly.Core.Tests.CircuitBreaker;

public class CircuitBreakerStateProviderTests
{
    [Fact]
    public void Ctor_Ok()
    {
        var provider = new CircuitBreakerStateProvider();

        provider.IsInitialized.ShouldBeFalse();
    }

    [Fact]
    public void NotInitialized_EnsureDefaults()
    {
        var provider = new CircuitBreakerStateProvider();

        provider.CircuitState.ShouldBe(CircuitState.Closed);
    }

    [Fact]
    public async Task ResetAsync_NotInitialized_Throws()
    {
        var control = new CircuitBreakerManualControl();

        await Should.NotThrowAsync(() => control.CloseAsync(CancellationToken.None));
    }

    [Fact]
    public void Initialize_Twice_Throws()
    {
        var provider = new CircuitBreakerStateProvider();
        provider.Initialize(() => CircuitState.Closed);

        Should.Throw<InvalidOperationException>(() => provider.Initialize(() => CircuitState.Closed));
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

        provider.CircuitState.ShouldBe(CircuitState.HalfOpen);

        stateCalled.ShouldBeTrue();
    }
}
